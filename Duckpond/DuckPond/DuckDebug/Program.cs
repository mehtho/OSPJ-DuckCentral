using DuckPond;
using DuckPond.Models;
using DuckServer;
using InstantMessenger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DuckDebug
{
    class Program
    {
        private static String targetIP = "192.168.1.231";
        static int Main(string[] args)
        {
            SendNewHostEntry();
            Console.ReadLine();
            return 0;
        }

        public static void SendNewHostEntry()
        {
            SQLiteClass sql = new SQLiteClass(SQLiteClass.ProgramFilesx86()+"\\DuckClient\\Information.dat");
            if (!sql.GetRegistered())
            {
                KnownHost kh = new KnownHost();
                kh.hostMAC = MACFinder.getMacByIp(GetIPAddress());
                Console.WriteLine("MAC" + kh.hostMAC);
                kh.version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                kh.GUID = sql.GetGUID();

                IMClient iM = new IMClient();
                iM.setConnParams(targetIP, 25567);
                iM.SetupConn();
                if (iM.SendSignalWithRet(IMClient.IM_NewIdentity, DoSerialize(kh)))
                {
                    sql.SetRegistered(true);
                }
                iM.Disconnect();
            }
        }

        public static string GetIPAddress()
        {
            IPHostEntry Host = default(IPHostEntry);
            string Hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(Hostname);
            IPAddress targetIPA = IPAddress.Parse(targetIP);
            String IPAddresss = "";
            int best = 0;
            foreach (IPAddress IP in Host.AddressList)
            {
                if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    int matches = 0;
                    for(int i = 0; i < 4; i++)
                    {
                        if (IP.GetAddressBytes()[i] == targetIPA.GetAddressBytes()[i])
                        {
                            matches++;
                        }
                    }
                    if (matches > best)
                    {
                        best = matches;
                        IPAddresss = Convert.ToString(IP);
                    }
                }
            }
            return IPAddresss;
        }

        public static void GetVersionAndGUID()
        {
            IMClient iM = new IMClient();
            iM.setConnParams(targetIP, 25567);
            iM.SetupConn();
            Console.WriteLine(iM.RequestParam(IMClient.IM_GetIdentity));
            iM.Disconnect();
            iM.SetupConn();
            Console.WriteLine(iM.RequestParam(IMClient.IM_GetVersion));
            Console.ReadLine();
        }

        public static void SendDebugKnownHost(KnownHost kh)
        {
            //Send 1 Debug message
            IMClient im = new IMClient();
            im.setConnParams(targetIP, 25567);
            im.SetupConn();
            Console.WriteLine(DoSerialize(kh));
            im.SendSignal(IMClient.IM_NewIdentity, DoSerialize(kh));
        }

        public void SendDebugEvent()
        {
            Events ev = new Events("Debug000", "The debug message", 2, "123.123.123.123", "hhhhhhhh", DateTime.Now);
            String toSend = DoSerialize(ev);

            Console.WriteLine(toSend);

            //Send 1 Debug message
            IMClient im = new IMClient();
            im.setConnParams(targetIP, 25567);
            im.SetupConn();
            im.SendSignal(IMClient.IM_Event, toSend);
        }

        public static String DoSerialize(Object o)
        {
            XmlSerializer xsSubmit = new XmlSerializer(o.GetType());
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, o);
                    xml = sww.ToString(); // Your XML
                    return xml;
                }
            }
        }
    }
}
