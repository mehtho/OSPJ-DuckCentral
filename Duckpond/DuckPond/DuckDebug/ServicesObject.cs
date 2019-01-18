using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckPond.Models
{
    [Serializable]
    public class ServicesObject
    {
        public ServicesObject()
        {

        }

        public ServicesObject(String Conn, int port, int Pref)
        {
            this.IPAddress = Conn;
            this.port = port;
            this.Preference = Pref;
        }

        public String IPAddress;

        public int port;

        public int Preference;

        public const byte STATUS_UNKNOWN = 0;
        public const byte STATUS_ONLINE = 1;
        public const byte STATUS_OFFLINE = 2;
    }
}
