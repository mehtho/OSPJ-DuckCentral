using DuckDebug;
using DuckPond;
using DuckPond.Models;
using InstantMessenger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuckServer
{
    public class Service
    {
        public X509Certificate2 cert = new X509Certificate2(SQLiteClass.ProgramFilesx86()+"\\DuckServer\\server.pfx", "instant");//EX509!!!

        public IPAddress ip;
        public int port = 25567;
        public bool running = true;
        public TcpListener server;

        public Service()
        {
            ip = IPAddress.Parse(GetIPFromConfig());
            server = new TcpListener(ip, port);
            server.Start();

            Console.WriteLine("[{0}] Client Listener is running properly on " + ip+":"+port, DateTime.Now);

            Listen();
        }

        private static void WriteKey(String localIP)
        {
            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).CreateSubKey(@"SOFTWARE", true);

            key.CreateSubKey("DuckClient");
            key = key.OpenSubKey("DuckClient", true);


            key.CreateSubKey("Address");
            key = key.OpenSubKey("Address", true);
            if (key.GetValue("PrefIP") == null)
            {
                key.SetValue("PrefIP", localIP);
            }
        }

        void Listen()  // Listen to incoming connections.
        {
            while (running)
            {
                TcpClient tcpClient = server.AcceptTcpClient();  // Accept incoming connection.
                ServiceConn client = new ServiceConn(this, tcpClient);     // Handle in another thread.
            }
        }

        public static String GetIPFromConfig()
        {
            SQLiteClass sql = new SQLiteClass(SQLiteClass.ProgramFilesx86() + "\\DuckClient\\information.dat");
            List<ServicesObject> sros = sql.GetServices();

            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\DuckClient\Address", true))
            {
                if (key != null)
                {
                    Object o = key.GetValue("PrefIP");
                    if (o != null)
                    {
                        return o.ToString();
                    }
                    else
                    {
                        Console.WriteLine("Registry entry was null");
                    }
                }
                else
                {
                    WriteKey("127.0.0.1");
                }
            }

            ServicesObject inUse =  ServiceConnectionDelegator.GetService();
            Console.WriteLine(inUse);
            try
            {
                return GetIPAddressLike(inUse.IPAddress);
            }
            catch
            {

            }

            Console.WriteLine("Returning default IP");
            return "127.0.0.1";
        }

        public static string GetIPAddressLike(String target)
        {
            Console.WriteLine(target);
            IPHostEntry Host = default(IPHostEntry);
            string Hostname = Environment.MachineName;
            Host = Dns.GetHostEntry(Hostname);
            IPAddress targetIPA = IPAddress.Parse(target.Trim());
            String IPAddresss = "";
            int best = 0;
            foreach (IPAddress IP in Host.AddressList)
            {
                Console.WriteLine(IP);
                if (IP.AddressFamily == AddressFamily.InterNetwork)
                {
                    int matches = 0;
                    for (int i = 0; i < 4; i++)
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


    }
}
