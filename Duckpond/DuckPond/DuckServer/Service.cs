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

        String GetIPFromConfig()
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
        }

        private static List<GUIDMACVersionIP> GetActiveHosts()
        {
            MSSQL ms = new MSSQL();
            List<KnownHost> khs = new List<KnownHost>();
            List<String> ips = ms.GetIPs();
            List<GUIDMACVersionIP> gmvis = new List<GUIDMACVersionIP>();

            IPPS = new List<IPPlusStatus>();
            count = 0;
            max = ips.Count;

            foreach (String ip in ips)
            {
                IPPlusStatus ipps = new IPPlusStatus { IP = "0.0.0.0", Status = KnownHost.STATE_UNKNOWN };

                Thread t = new Thread(() => ipps = HostScan.run(ip, 25567));
                t.Start();
                t.Join();
                if (!ipps.IP.Equals("0.0.0.0"))
                {
                    IPPS.Add(ipps);
                }
            }

            foreach (IPPlusStatus iss in IPPS)
            {
                if (iss.Status == KnownHost.STATE_ONLINE)
                {
                    try
                    {
                        IMClient iMC = new IMClient();
                        iMC.setConnParams(iss.IP, 25567);

                        iMC.SetupConn();
                        string guid = iMC.RequestParam(ServiceConn.IM_GetIdentity);
                        iMC.CloseConn();
                        iMC.SetupConn();

                        string version = iMC.RequestParam(ServiceConn.IM_GetVersion);
                        iMC.CloseConn();
                        iMC.SetupConn();

                        string mac = iMC.RequestParam(ServiceConn.IM_GetMAC);

                        GUIDMACVersionIP gmvi = new GUIDMACVersionIP
                        {
                            GUID = guid, IP=iss.IP, MAC=mac, Version=version
                        };
                        gmvis.Add(gmvi);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.Source);
                        Console.WriteLine(e.StackTrace);
                    }
                }
            }
            
            Console.Write("Finished the procedure");
            return gmvis;
        }

        private static void RoutineCheck()
        {
            if (!MSSQL.ConnectionsExist())
            {
                Console.WriteLine("No database connections configured");
                return;
            }


            Console.WriteLine("ROUTINE CHECK");
            List<GUIDMACVersionIP> gmvis = GetActiveHosts();
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
                    if (!found)
                    {
                        ms.AddKnownHost(new KnownHost(gmvi.MAC, gmvi.IP, gmvi.Version, DateTime.Now, gmvi.GUID));
                    }
                }
            }
        }
    }
}
