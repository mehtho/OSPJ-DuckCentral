using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DuckPond.Models
{
    [Serializable]
    public class KnownHost
    {
        public string hostMAC;

        public string hostIP;

        public string version;

        public byte status;

        public DateTime dateAdded;

        public string GUID;

        public string hostname;

        public KnownHost()
        {

        }

        public KnownHost(string hMAC, string hIP, string hVer, byte hStat, DateTime dAdded, String gUID, String hostname)
        {
            this.hostMAC = hMAC;
            this.hostIP = hIP;
            this.version = hVer;
            this.status = hStat;
            this.dateAdded = dAdded;
            this.GUID = gUID;
            this.hostname = hostname;
        }

        public KnownHost(string hMAC, string hIP, string hVer, DateTime dAdded, String gUID, String hostname)
        {
            this.hostMAC = hMAC;
            this.hostIP = hIP;
            this.version = hVer;
            this.dateAdded = dAdded;
            this.GUID = gUID;
            this.status = STATE_UNKNOWN;
            this.hostname = hostname;
        }

        public static String ByteToString(byte b)
        {
            if (b == STATE_UNKNOWN)
            {
                return "Unknown";
            }
            else if (b == STATE_ONLINE)
            {
                return "ONLINE";
            }
            else if(b == STATE_OFFLINE)
            {
                return "Offline";
            }
            else
            {
                return "";
            }
        }


        public static String ParseAsIP(String s)
        {
            try
            {
                IPAddress address = IPAddress.Parse(s);
                return address.ToString();
            }
            catch
            {
                return "";
            }
        }

        public const Byte STATE_UNKNOWN = 0;
        public const Byte STATE_ONLINE = 1;
        public const Byte STATE_OFFLINE = 2;
    }
}
