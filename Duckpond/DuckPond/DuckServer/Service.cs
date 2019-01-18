using DuckPond;
using DuckPond.Models;
using DuckPond.Resources;
using InstantMessenger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DuckServer
{
    public class Service
    {
        public static void Main(string[] args)
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(20);

            var timer = new Timer((e) =>
            {
                RoutineCheck();

            }, null, startTimeSpan, periodTimeSpan);

            Service p = new Service();
        }

        // Self-signed certificate for SSL encryption.
        // You can generate one using my generate_cert script in tools directory (OpenSSL is required).
        public X509Certificate2 cert = new X509Certificate2(SQLiteClass.ProgramFilesx86()+"\\DuckServer\\server.pfx", "instant");

        public IPAddress ip;
        public int port = 25568;
        public bool running = true;
        public TcpListener server;
        public static int count = 0;
        public static int max = 0;
        public static List<IPPlusStatus> IPPS;
        public static bool shouldIBeRunning;

        public Service()
        {
            Console.Title = "DuckServer";
            Console.WriteLine("----- DuckServer -----");
            Console.WriteLine("[{0}] Starting server...", DateTime.Now);
            ip = IPAddress.Parse(GetIPFromConfig());
            server = new TcpListener(ip, port);
            server.Start();
            Console.WriteLine("[{0}] Server is running properly on " + ip+":"+port, DateTime.Now);

            Listen();
        }

        void Listen()  // Listen to incoming connections.
        {
            while (running)
            {
                TcpClient tcpClient = server.AcceptTcpClient();  // Accept incoming connection.
                ServiceConn client = new ServiceConn(this, tcpClient);     // Handle in another thread.
            }
        }

        static String GetIPFromConfig()
        {
            string path = SQLiteClass.ProgramFilesx86() + "\\DuckServer\\config.cfg";

            if (!File.Exists(path)||new FileInfo(path).Length==0)
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("127.0.0.1");
                }
            }
            
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                return line;
            }
            return "127.0.0.1";
        }

        private struct GUIDMACVersionIP
        {
            public String GUID;
            public String MAC;
            public String Version;
            public String IP;
            public String Hostname;
        }

        private struct IPPSandThread
        {
            public Thread t { get; set; }
            public IPPlusStatus ipps { get; set; }
        }

        private static List<GUIDMACVersionIP> GetActiveHosts()
        {
            MSSQL ms = new MSSQL();
            List<KnownHost> khs = new List<KnownHost>();
            List<String> ips = ms.GetIPs();
            count = ips.Count;
            List<GUIDMACVersionIP> gmvis = new List<GUIDMACVersionIP>();

            IPPS = new List<IPPlusStatus>();
            
            List<Thread> threads = new List<Thread>();
            foreach (String ip in ips)
            {
                Thread th = new Thread(() => HostScan.run(ip, 25567));
                th.Start();
                threads.Add(th);
            }

            foreach(Thread th in threads)
            {
                th.Join();
            }

            Console.WriteLine(IPPS.Count+" live hosts");

            for (int i = 0; i <= count; i++)
            {
                try
                {
                    IPPlusStatus iss = IPPS[i];

                    if (iss.Status == KnownHost.STATE_ONLINE)
                    {
                        IMClient iMC = new IMClient();
                        iMC.setConnParams(iss.IP, 25567);

                        iMC.SetupConn();
                        string guid = iMC.RequestParam(IMClient.IM_GetIdentity);
                        iMC.CloseConn();
                        iMC.SetupConn();

                        string version = iMC.RequestParam(IMClient.IM_GetVersion);
                        iMC.CloseConn();
                        iMC.SetupConn();

                        string mac = iMC.RequestParam(IMClient.IM_GetMAC);
                        iMC.CloseConn();
                        iMC.SetupConn();

                        string hostname = iMC.RequestParam(IMClient.IM_GetHostname);

                        GUIDMACVersionIP gmvi = new GUIDMACVersionIP
                        {
                            GUID = guid,
                            IP = iss.IP,
                            MAC = mac,
                            Version = version,
                            Hostname = hostname
                        };
                        gmvis.Add(gmvi);
                    }
                }
                catch (Exception)
                {

                }
            }
            return gmvis;
        }

        private static void RoutineCheck()
        {
            if (!MSSQL.ConnectionsExist())
            {
                Console.WriteLine("No database connections configured");
                return;
            }

            Console.WriteLine("Getting active hosts");
            List<GUIDMACVersionIP> gmvis = GetActiveHosts();
            Console.WriteLine(gmvis.Count+" running clients");
            MSSQL ms = new MSSQL();
            List<KnownHost> khs = ms.GetKnownHosts();

            foreach(GUIDMACVersionIP gmvi in gmvis)
            {
                bool found = false;
                foreach (KnownHost kh in khs)
                {
                    bool change = false;
                    if (kh.GUID.Equals(gmvi.GUID))
                    {
                        found = true;
                        if (!kh.hostMAC.Equals(gmvi.MAC))
                        {
                            //SendEvent
                            kh.hostMAC = gmvi.MAC;

                            change = true;
                        }
                        if (!kh.version.Equals(gmvi.Version))
                        {
                            //SendEvent
                            kh.version = gmvi.Version;

                            change = true;
                        }
                        if (!kh.hostIP.Equals(gmvi.IP))
                        {
                            //SendEvent
                            kh.hostIP = gmvi.IP;

                            change = true;
                        }
                        if (change)
                        {
                            ms.UpdateHost(kh);
                        }
                        break;
                    }
                }
                if (!found)
                {
                    ms.AddKnownHost(new KnownHost(gmvi.MAC, gmvi.IP, gmvi.Version, DateTime.Now, gmvi.GUID, gmvi.Hostname));
                    IMClient imc = new IMClient();
                    imc.setConnParams(gmvi.IP, 25567);
                    imc.SetupConn();
                    imc.SendSignal(ServiceConn.IM_RegistrationDone, gmvi.GUID);
                    imc.Disconnect();
                }

            }

            //Version broadcast
            
            ms.GetWhitelists();

            SQLiteClass sql = new SQLiteClass(SQLiteClass.ProgramFilesx86() + "\\DuckServer\\Information.dat");
            sql.NewServices(ms.GetServices());
            sql.SetLastUpdated(SQLiteClass.GET_SERVICE_LIST, ms.GetLastUpdated(SQLiteClass.GET_SERVICE_LIST));
            sql.NewWhitelists(ms.GetWhitelists());
            sql.SetLastUpdated(SQLiteClass.GET_WHITELIST_LIST, ms.GetLastUpdated(SQLiteClass.GET_WHITELIST_LIST));

            DateTime serviceTime = sql.GetLastUpdated(SQLiteClass.GET_SERVICE_LIST);
            DateTime whitelistTime = sql.GetLastUpdated(SQLiteClass.GET_WHITELIST_LIST);

            foreach (GUIDMACVersionIP gmvi in gmvis)
            {
                IMClient imclient = new IMClient();
                imclient.setConnParams(gmvi.IP, 25567);
                Thread th = new Thread(() => SendNewDataVersion(imclient, serviceTime, whitelistTime));
                th.Start();
            }
            shouldIBeRunning = ServiceConcurrencyCheck();
        }

        private static bool ServiceConcurrencyCheck()
        {
            SQLiteClass sql = new SQLiteClass(SQLiteClass.ProgramFilesx86() + "\\DuckServer\\Information.dat");
            List<ServicesObject> sros = sql.GetServices();
            String ip = GetIPFromConfig();
            int myPriority = Int32.MaxValue;
            bool found = false;

            foreach (ServicesObject sro in sros)
            {
                if (sro.IPAddress.Trim().Equals(ip.Trim()))
                {
                    myPriority = sro.Preference;
                    if (myPriority - 1 == 0)
                    {
                        return true;
                    }
                    found = true;
                }
            }

            if (!found)
            {
                Console.WriteLine("Could not find "+ip +" in service list");
            }

            foreach (ServicesObject sro in sros)
            {
                //If not me
                if (!sro.IPAddress.Equals(ip)&&sro.Preference==(myPriority-1))
                {
                    try
                    {
                        IMClient imc = new IMClient();
                        imc.setConnParams(sro.IPAddress.Trim(), sro.port);
                        imc.SetupConn();
                        imc.Disconnect();
                        return false;
                    }
                    catch
                    {
                        return true;
                    }
                }
            }
            Console.WriteLine("No server list available");
            return false;
        }

        private static void SendNewDataVersion(IMClient imclient, DateTime st, DateTime wt)
        {
            imclient.SetupConn();
            imclient.SendSignal(ServiceConn.IM_NewVersionsCheck, DoSerialize(new DateTimeVersions {ServiceDateTime = st, WhitelistDateTime = wt })); 
        }

        public struct DateTimeVersions
        {
            public DateTime ServiceDateTime;
            public DateTime WhitelistDateTime;
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
