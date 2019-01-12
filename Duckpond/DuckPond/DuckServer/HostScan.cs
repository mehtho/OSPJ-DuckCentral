using DuckPond.Models;
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

        public static IPPlusStatus run(String host, int port)
        {
            List<int> toRet = new List<int>();
            TcpClient tcp = new TcpClient();
                try
                {
                    tcp = new TcpClient(host.Trim(), port);
                    return new IPPlusStatus { IP = host, Status = KnownHost.STATE_ONLINE };
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.Source);
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    try
                    {
                        tcp.Close();
                    }
                    catch { }
                }
             return new IPPlusStatus { IP = host, Status = KnownHost.STATE_OFFLINE };
        }
    }

    public struct IPPlusStatus
    {
        public string IP { set; get; }
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