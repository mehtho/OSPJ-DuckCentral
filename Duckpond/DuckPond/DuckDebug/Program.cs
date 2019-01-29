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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DuckDebug
{
    class Program
    {
        static int Main(string[] args)
        {
            SQLiteClass sql = new SQLiteClass(SQLiteClass.ProgramFilesx86() + "\\DuckClient\\Information.dat");

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(20);

            var timer = new Timer((e) =>
            {
                RoutineCheck();

            }, null, startTimeSpan, periodTimeSpan);

            Service s = new Service();
            Console.ReadLine();
            return 0;
        }



        private static void RoutineCheck()
        {
            SQLiteClass sql = new SQLiteClass(SQLiteClass.ProgramFilesx86() + "\\DuckClient\\Information.dat");
            List<SQLiteClass.CachedMessage> cms = sql.GetCachedMessages();

            Console.WriteLine("Loading servers");
            ServiceConnectionDelegator.LoadServices();
            ServicesObject so = ServiceConnectionDelegator.GetService();
            if (so == null)
            {
                Console.WriteLine("Could not find a server");
            }
            else
            {
                Console.WriteLine("Using server: " + so.IPAddress);
            }
            IMClient.SendEvent(new Events("DEBUG000", "Debug message 0", 0, Service.GetIPFromConfig(), sql.GetGUID(), DateTime.Now));

            foreach (SQLiteClass.CachedMessage cm in cms)
            {
                try
                {
                    IMClient imc = new IMClient();
                    imc.setConnParams(so.IPAddress.Trim(), so.port);
                    Console.WriteLine("Sending a cached event to " + so.IPAddress);
                    imc.SetupConn();
                    imc.SendSignal((byte)cm.code, cm.message);
                    imc.Disconnect();
                    sql.RemoveCachedMessage(cm);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.Source);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        public static VersionGUIDPair GetVersionAndGUID()
        {
            IMClient iM = new IMClient();
            iM.setConnParams(ServiceConnectionDelegator.GetService().IPAddress, 25567);
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
            im.setConnParams(ServiceConnectionDelegator.GetService().IPAddress, 25567);
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
            im.setConnParams(ServiceConnectionDelegator.GetService().IPAddress, 25567);
            im.SetupConn();
            im.SendSignal(IMClient.IM_Event, toSend);
        }

        public void SendEvent(Events ev)
        {
            String toSend = DoSerialize(ev);
            IMClient im = new IMClient();
            im.setConnParams(ServiceConnectionDelegator.GetService().IPAddress, 25567);
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
