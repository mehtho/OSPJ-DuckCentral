using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace InstantMessenger
{
    public class IMClient
    {
        Thread tcpThread;      // Receiver
        bool _conn = false;    // Is connected/connecting?

        public string Server;  // Address of server. In this case - local IP address.
        public int Port = 25567;

        // Start connection thread and login or register.
        void connect(String server, int port)
        {
            if (!_conn)
            {
                _conn = true;
                tcpThread = new Thread(new ThreadStart(SetupConn));
                tcpThread.Start();
            }
        }
        public void Disconnect()
        {
            if (_conn)
                CloseConn();
        }

        public void SendSignal(byte code, string load)
        {
            bw.Write(code);
            bw.Write(load);
        }

        public void SendByte(byte code)
        {
            bw.Write(code);
        }

        public String RequestParam(byte code)
        {
            bw.Write(code);
            return br.ReadString();
        }
 
        TcpClient client;
        NetworkStream netStream;
        SslStream ssl;
        BinaryReader br;
        BinaryWriter bw;

        public void setConnParams(String s, int p)
        {
            Server = s;
            Port = p;
        }

        public void SetupConn()  // Setup connection and login
        {
            Console.WriteLine(Server+":"+Port);
            client = new TcpClient(Server.Trim(), Port);  // Connect to the server.
            netStream = client.GetStream();
            ssl = new SslStream(netStream, false, new RemoteCertificateValidationCallback(ValidateCert));
            ssl.AuthenticateAsClient("DuckServer");
            // Now we have encrypted connection.
            _conn = true;
            br = new BinaryReader(ssl, Encoding.UTF8);
            bw = new BinaryWriter(ssl, Encoding.UTF8);

            // Receive "hello"
            int hello = br.ReadInt32();
            if (hello == IM_Hello)
            {
                // Hello OK, so answer.
                bw.Write(IM_Hello);
            }
            //if (_conn)
                //CloseConn();
        }
        public void CloseConn() // Close connection.
        {
            br.Close();
            bw.Close();
            ssl.Close();
            netStream.Close();
            client.Close();
            _conn = false;
        }

        // Packet types
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
        public const byte IM_AddDatabases = 62;
        public const byte IM_GetDatabases = 63;
        public const byte IM_NewDatabases = 65;
        public const byte IM_NoMoreDatabases = 66;
        public const byte IM_NewServiceList = 70;
        public const byte IM_NewWhitelists = 80;
        public const byte IM_AddWhiteList = 81;
        public const byte IM_RemoveWhitelist = 82;
        public const byte IM_Debug = 99;

        public static bool ValidateCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Uncomment this lines to disallow untrusted certificates.
            //if (sslPolicyErrors == SslPolicyErrors.None)
            //    return true;
            //else
            //    return false;

            return true; // Allow untrusted certificates.
        }
    }
}
