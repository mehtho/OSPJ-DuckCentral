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
using System.Reflection;
using DuckPond.Models.Whitelists;
using DuckPond.Resources;
using InstantMessenger;

namespace DuckServer
{
    public class ServiceConn
    {
        public ServiceConn(Service p, TcpClient c)
        {
            prog = p;
            client = c;
            dboss = new List<DatabaseObject>();

            // Handle client in another thread.
            (new Thread(new ThreadStart(SetupConn))).Start();
        }

        public ServiceConn()
        {
        }

        public string Server;  // Address of server. In this case - local IP address.
        public int Port = 25568;
        Service prog;
        public TcpClient client;
        public NetworkStream netStream;  // Raw-data stream of connection.
        public SslStream ssl;            // Encrypts connection using SSL.
        public BinaryReader br;
        public BinaryWriter bw;
        List<DatabaseObject> dboss;

        public void setConnParams(String s, int p)
        {
            Server = s;
            Port = p;
        }

        void SetupConn()  // Setup connection and login or register.
        {
            try
            {
                netStream = client.GetStream();
                ssl = new SslStream(netStream, false);
                ssl.AuthenticateAsServer(prog.cert, false, SslProtocols.Tls, true);
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
                    HandleLog(logMode);
                    
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

        private void HandleLog(int mode)
        {
            SQLiteClass sql = new SQLiteClass(ProgramFilesx86() + "\\DuckServer\\Information.dat");
            MSSQL msl;

            switch (mode)
            {
                case IM_Event:
                    String eventString = br.ReadString();
                    Events ev = DeserializeXMLFileToObject<Events>(eventString);
                    sql.CloseCon();
                    msl = new MSSQL();
                    msl.AddEvent(ev);
                    break;
                case IM_NewIdentity:
                    try
                    {
                        var socketIp = GetOriginIP();

                        String k = br.ReadString();
                        KnownHost kh = DeserializeXMLFileToObject<KnownHost>(k);

                        kh.hostIP = socketIp;
                        kh.status = KnownHost.STATE_ONLINE;
                        kh.dateAdded = DateTime.Now;

                        msl = new MSSQL();
                        msl.AddKnownHost(kh);
                        bw.Write(IM_OK);
                    }
                    catch
                    {
                        bw.Write(IM_Bad_Credentials);
                    }
                    break;
                case IM_GetIdentity:
                    
                    break;
                case IM_NewVersions:
                    IMClient imc = new IMClient();
                    imc.setConnParams(GetOriginIP(),25567);
                    imc.SetupConn();
                    imc.SendSignal(IM_NewVersions, Service.DoSerialize(new DateTimeVersions { ServiceVersion = sql.GetServices(), WhitelistVersion = sql.GetWhitelists(), ServiceDateTime = sql.GetLastUpdated(SQLiteClass.GET_SERVICE_LIST), WhitelistDateTime = sql.GetLastUpdated(SQLiteClass.GET_WHITELIST_LIST) }));
                    break;
                case IM_AddDatabases:
                    sql.AddDatabase(new DatabaseObject(br.ReadString(), br.ReadInt32()));
                    sql.CloseCon();
                    break;
                case IM_GetDatabases:
                    List<DatabaseObject> dbos = sql.GetConnections();
                    foreach (DatabaseObject dbo in dbos)
                    {
                        bw.Write(dbo.ConnectionString);
                        bw.Write(dbo.Preference);
                    }
                    bw.Write(IM_OK);
                    sql.CloseCon();
                    break;
                case IM_NewDatabases:
                    List<DatabaseObject> dbs = DeserializeXMLFileToObject<List<DatabaseObject>>(br.ReadString());
                    sql.NewConnections(dbs);
                    sql.CloseCon();
                    break;
                case IM_NewServiceList:
                    List<ServicesObject> sros = DeserializeXMLFileToObject<List<ServicesObject>>(br.ReadString());
                    sql.NewServices(sros);
                    sql.CloseCon();

                    //Port scan
                    //Broadcast
                    break;
                case IM_NewWhitelists:
                    List<Whitelists> wls = DeserializeXMLFileToObject<List<Whitelists>>(br.ReadString());
                    sql.NewWhitelists(wls);
                    sql.CloseCon();
                    break;
                case IM_AddWhiteList:
                    break;
                case IM_RemoveWhitelist:
                    break;
                case IM_Debug:
                    Console.WriteLine(br.ReadString());
                    bw.Write(IM_OK);
                    break;
                default:
                    break;
            }
        }

        public String RequestParam(byte code)
        {
            bw.Write(code);
            return br.ReadString();
        }


        public struct DateTimeVersions
        {
            public DateTime ServiceDateTime;
            public DateTime WhitelistDateTime;
            public List<ServicesObject> ServiceVersion;
            public List<Whitelists> WhitelistVersion;
        }

        public String GetOriginIP()
        {
            var pi = netStream.GetType().GetProperty("Socket", BindingFlags.NonPublic | BindingFlags.Instance);
            var socketIp = ((Socket)pi.GetValue(netStream, null)).RemoteEndPoint.ToString();
            int index = socketIp.IndexOf(":");
            if (index > 0)
                socketIp = socketIp.Substring(0, index);

            return socketIp;
        }
        /*public List<ServicesObject> GetActiveServices()
        {
            MSSQL ms = new MSSQL();
            List<ServicesObject> sos = ms.GetServices();
            List<String> ips = ms.GetIPs();

            IPPS = new List<IPPlusStatus>();
            count = 0;
            max = ips.Count;

            foreach (String ip in ips)
            {
                IPPlusStatus ipps = new IPPlusStatus { IP = "0.0.0.0", Status = KnownHost.STATE_UNKNOWN };

                Thread t = new Thread(() => ipps = HostScan.run(ip, 25568));
                t.Start();
                t.Join();
                if (!ipps.IP.Equals("0.0.0.0"))
                {
                    IPPS.Add(ipps);
                }

                count++;
                if (count == max)
                {
                    foreach (IPPlusStatus iss in IPPS)
                    {
                        if (iss.Status == KnownHost.STATE_ONLINE)
                        {
                            try
                            {
                                ServiceConn iMC = new ServiceConn();
                                iMC.setConnParams(iss.IP, 25568);
                                iMC.SetupConn();

                                string guid = iMC.RequestParam(IM_GetIdentity);

                                foreach (ServicesObject so in sos)
                                {
                                    if (kh.GUID.Trim().Equals(guid))
                                    {
                                        kh.status = KnownHost.STATE_ONLINE;
                                        kh.version = version;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                Console.WriteLine(e.Source);
                                Console.WriteLine(e.StackTrace);
                            }
                        }
                    }
                }
            }
            Console.Write("Finished the procedure");
            ms.CloseCon();
            return khs;
        }*/

        public const int IM_Hello = 25050520;      // Hello
        public const byte IM_OK = 0;           // OK
        public const byte IM_Login = 1;        // Login
        public const byte IM_Bad_Credentials = 2;     // Bad Cred
        public const byte IM_Event = 4;  // Event log to server
        public const byte IM_NewIdentity = 30;
        public const byte IM_GetIdentity = 31;
        public const byte IM_GetVersion = 32;
        public const byte IM_GetMAC = 33;
        public const byte IM_RegistrationDone = 34;
        public const byte IM_NewVersionsCheck = 50;
        public const byte IM_NewVersions = 51;
        public const byte IM_AddDatabases = 62;
        public const byte IM_GetDatabases = 63;
        public const byte IM_NewDatabases = 65;
        public const byte IM_NoMoreDatabases = 66;
        public const byte IM_NewServiceList = 70;
        public const byte IM_NewWhitelists = 80;
        public const byte IM_AddWhiteList = 81;
        public const byte IM_RemoveWhitelist = 82;
        public const byte IM_Debug = 99;
    }
}
