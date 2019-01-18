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
using DuckDebug;
using System.Net;
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

        Service prog;
        public TcpClient client;
        public NetworkStream netStream;  // Raw-data stream of connection.
        public SslStream ssl;            // Encrypts connection using SSL.
        public BinaryReader br;
        public BinaryWriter bw;
        List<DatabaseObject> dboss;

        void SetupConn()  // Setup connection and login or register.
        {
            try
            {
                netStream = client.GetStream();
                ssl = new SslStream(netStream, false);
                ssl.AuthenticateAsServer(prog.cert, false, SslProtocols.Tls, true);

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
            SQLiteClass sql = new SQLiteClass(ProgramFilesx86() + "\\DuckClient\\Information.dat");
            switch (mode)
            {
                case IM_GetIdentity:
                    bw.Write(sql.GetGUID());
                    break;
                case IM_GetVersion:
                    bw.Write(sql.GetVersion());
                    break;
                case IM_GetMAC:
                    var s = MACFinder.getMacByIp(GetIPAddress());
                    bw.Write(s);
                    break;
                case IM_GetHostname:
                    bw.Write(Dns.GetHostName());
                    break;
                case IM_RegistrationDone:
                    if (br.ReadString().Equals(sql.GetGUID()))
                    {
                        sql.SetRegistered(true);
                    }
                    break;
                case IM_NewVersionsCheck:
                    DateTimeVersions dtv = DeserializeXMLFileToObject<DateTimeVersions>(br.ReadString());
                    if (DateTime.Compare(dtv.ServiceDateTime, sql.GetLastUpdated(SQLiteClass.GET_SERVICE_LIST)) >0 || DateTime.Compare(dtv.WhitelistDateTime, sql.GetLastUpdated(SQLiteClass.GET_WHITELIST_LIST))>0)
                    {
                        IMClient imc = new IMClient();
                        imc.setConnParams(GetOriginIP(), 25568);
                        imc.SetupConn();
                        imc.SendSignal(IM_NewVersions, "2");
                        imc.Disconnect();
                    }
                    break;
                case IM_NewVersions:
                    DateTimeVersions dtv1 = DeserializeXMLFileToObject<DateTimeVersions>(br.ReadString());
                    UpdateStuff(dtv1);
                    break;
                case IM_NewServiceList:
                    List<ServicesObject> sros = DeserializeXMLFileToObject<List<ServicesObject>>(br.ReadString());
                    sql.NewServices(sros);
                    break;
                case IM_NewWhitelists:
                    List<Whitelists> wls = DeserializeXMLFileToObject<List<Whitelists>>(br.ReadString());
                    sql.NewWhitelists(wls);
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

        public String GetOriginIP()
        {
            var pi = netStream.GetType().GetProperty("Socket", BindingFlags.NonPublic | BindingFlags.Instance);
            var socketIp = ((Socket)pi.GetValue(netStream, null)).RemoteEndPoint.ToString();
            int index = socketIp.IndexOf(":");
            if (index > 0)
                socketIp = socketIp.Substring(0, index);

            return socketIp;
        }

        public static string GetIPAddress()
        {
            IPHostEntry Host = default(IPHostEntry);
            string Hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(Hostname);
            IPAddress targetIPA = IPAddress.Parse(Service.GetIPFromConfig());
            String IPAddresss = "";
            int best = 0;
            foreach (IPAddress IP in Host.AddressList)
            {
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

        public void UpdateStuff(DateTimeVersions dtv1)
        {
            SQLiteClass sql = new SQLiteClass(ProgramFilesx86() + "\\DuckClient\\Information.dat");
            sql.NewServices(dtv1.ServiceVersion);
            sql.NewWhitelists(dtv1.WhitelistVersion);
            sql.SetLastUpdated(SQLiteClass.GET_SERVICE_LIST, dtv1.ServiceDateTime);
            sql.SetLastUpdated(SQLiteClass.GET_WHITELIST_LIST, dtv1.WhitelistDateTime);
        }

        public struct DateTimeVersions
        {
            public DateTime ServiceDateTime;
            public DateTime WhitelistDateTime;
            public List<ServicesObject> ServiceVersion;
            public List<Whitelists> WhitelistVersion;
        }

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
        public const byte IM_GetHostname = 35;
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
