using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using DuckPond.Models;
using DuckPond;

namespace DuckServer
{
    public class ServiceConn
    {
        public ServiceConn(Service p, TcpClient c)
        {
            prog = p;
            client = c;

            // Handle client in another thread.
            (new Thread(new ThreadStart(SetupConn))).Start();
        }

        Service prog;
        public TcpClient client;
        public NetworkStream netStream;  // Raw-data stream of connection.
        public SslStream ssl;            // Encrypts connection using SSL.
        public BinaryReader br;
        public BinaryWriter bw;
 
        void SetupConn()  // Setup connection and login or register.
        {
            try
            {
                Console.WriteLine("[{0}] New connection!", DateTime.Now);
                netStream = client.GetStream();
                ssl = new SslStream(netStream, false);
                ssl.AuthenticateAsServer(prog.cert, false, SslProtocols.Tls, true);
                Console.WriteLine("[{0}] Connection authenticated!", DateTime.Now);
                // Now we have encrypted connection.

                br = new BinaryReader(ssl, Encoding.UTF8);
                bw = new BinaryWriter(ssl, Encoding.UTF8);

                // Say "hello".
                bw.Write(IM_Hello);
                bw.Flush();
                int hello = br.ReadInt32();
                if (hello == IM_Hello)
                {
                    // Hello packet is OK. Time to wait for login or register.
                    byte logMode = br.ReadByte();

                    Console.WriteLine("logMode: " + logMode);
                    if (logMode == 99)
                    {
                        Console.WriteLine(br.ReadString());
                        bw.Write(IM_OK);
                    }
                    else if(logMode == 7)
                    {
                        String eventString = br.ReadString();
                        Events ev = DeserializeXMLFileToObject<Events>(eventString);
                        //Write it!
                    }
                    else if (logMode == 62)
                    {
                        SQLiteClass sql = new SQLiteClass(ProgramFilesx86() + "\\DuckServer\\Information.dat");
                        sql.AddDatabase(new DatabaseObject(br.ReadString(), br.ReadInt32()));
                        sql.CloseCon();
                    }
                    else if(logMode == 63)
                    {
                        SQLiteClass sql = new SQLiteClass(ProgramFilesx86() + "\\DuckServer\\Information.dat");
                        List<DatabaseObject> dbos = sql.GetConnections();
                        foreach(DatabaseObject dbo in dbos)
                        {
                            bw.Write(dbo.ConnectionString);
                            bw.Write(dbo.Preference);
                        }
                        bw.Write(IM_OK);
                        sql.CloseCon();
                    }
                    
                }
                //CloseConn();
            }
            catch { CloseConn(); }
        }
        void CloseConn() // Close connection.
        {
            try
            {
                br.Close();
                bw.Close();
                ssl.Close();
                netStream.Close();
                client.Close();
                Console.WriteLine("[{0}] End of connection!", DateTime.Now);
            }
            catch { }
        }

        public static Events DeserializeXMLFileToObject<Events>(string XmlFilename)
        {
            Events returnObject = default(Events);
            if (string.IsNullOrEmpty(XmlFilename)) return default(Events);

            try
            {
                TextReader xmlStream = new StringReader(XmlFilename);
                XmlSerializer serializer = new XmlSerializer(typeof(Events));
                returnObject = (Events)serializer.Deserialize(xmlStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, DateTime.Now);
            }
            return returnObject;
        }

        public static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        public const int IM_Hello = 25050520;      // Hello
        public const byte IM_OK = 0;           // OK
        public const byte IM_Login = 1;        // Login
        public const byte IM_Bad_Credentials = 2;     // Bad Cred
        public const byte IM_Event = 4;  // Event log to server
        public const byte IM_AddDatabase = 62;
        public const byte IM_GetDatabases = 63;
        public const byte IM_RemoveDatase = 64;
        public const byte IM_Debug = 99;
    }
}
