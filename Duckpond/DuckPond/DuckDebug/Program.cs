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
            Service s = new Service();
            Console.ReadLine();
            return 0;
        }

        public static VersionGUIDPair GetVersionAndGUID()
        {
            IMClient iM = new IMClient();
            iM.setConnParams(targetIP, 25567);
            iM.SetupConn();
            String aguid = iM.RequestParam(IMClient.IM_GetIdentity);
            iM.Disconnect();
            iM.SetupConn();
            String aversion = iM.RequestParam(IMClient.IM_GetVersion);
            return new VersionGUIDPair { Version = aversion, GUID = aguid };
        }

        public struct VersionGUIDPair
        {
            public String Version { get; set; }
            public String GUID { get; set; }
        }

        public static void SendKnownHost(KnownHost kh)
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

        public void SendEvent(Events ev)
        {
            String toSend = DoSerialize(ev);
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
