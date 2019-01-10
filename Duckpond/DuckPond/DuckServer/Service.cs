using DuckPond;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DuckServer
{
    public class Service
    {
        public static void Main(string[] args)
        {
            MSSQL ms = new MSSQL();
            Service p = new Service(); 
        }

        // Self-signed certificate for SSL encryption.
        // You can generate one using my generate_cert script in tools directory (OpenSSL is required).
        public X509Certificate2 cert = new X509Certificate2(SQLiteClass.ProgramFilesx86()+"\\DuckServer\\server.pfx", "instant");

        // IP of this computer. If you are running all clients at the same computer you can use 127.0.0.1 (localhost). 
        public IPAddress ip;
        public int port = 25567;
        public bool running = true;
        public TcpListener server;

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


    }
}
