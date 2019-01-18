using DuckPond.Models;
using DuckServer;
using InstantMessenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuckPond.Resources
{
    public class HostScan
    {
        private string host;
        private PortList portList;

        public HostScan(string host, int portStart, int portStop)
        {
            this.host = host;
            this.portList = new PortList(portStart, portStop);
        }

        public HostScan(string host)
            : this(host, 1, 65535)
        {
        }

        public HostScan()
            : this("127.0.0.1")
        {
        }

        public static void run(ServicesObject so)
        {
            List<int> toRet = new List<int>();
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(so.IPAddress.Trim(), so.port, null, null);

                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4));

                    if (success)
                    {
                        IMClient imc = new IMClient();
                        imc.setConnParams(so.IPAddress.Trim(),so.port);
                        try
                        {
                            imc.SetupConn();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.Source);
                            Console.WriteLine(e.StackTrace);
                        }
                        
                        Console.WriteLine(so.IPAddress);
                        ServiceConnectionDelegator.spps.Add(new ServicePlusStatus { service = so, Status = ServicesObject.STATUS_ONLINE });
                    }
                    else
                    {
                        ServiceConnectionDelegator.spps.Add(new ServicePlusStatus { service = so, Status = ServicesObject.STATUS_OFFLINE });
                    }
                }
            }
            catch (SocketException)
            {
                ServiceConnectionDelegator.spps.Add(new ServicePlusStatus { service = so, Status = ServicesObject.STATUS_OFFLINE });
            }
            catch (Exception e)
            {
                ServiceConnectionDelegator.spps.Add(new ServicePlusStatus { service = so, Status = ServicesObject.STATUS_OFFLINE });
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.StackTrace);
            }
        }
    }

    public struct ServicePlusStatus
    {
        public ServicesObject service { set; get; }
        public byte Status { set; get; }
    }

    public class PortList
    {
        private int start;
        private int stop;
        private int ptr;

        public PortList(int start, int stop)
        {
            this.start = start;
            this.stop = stop;
            this.ptr = start;
        }
        public PortList() : this(1, 65535)
        {
        }

        public bool hasMore()
        {
            return (stop - ptr) >= 0;
        }
        public int getNext()
        {
            if (hasMore())
                return ptr++;
            return -1;
        }
    }
}