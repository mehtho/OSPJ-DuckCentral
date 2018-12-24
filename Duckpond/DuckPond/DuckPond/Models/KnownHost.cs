using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckPond.Models
{
    class KnownHost
    {
        private string hostMAC;
        private string hostIP;
        private string version;
        private string status;
        private DateTime dateAdded;
        private string GUID;

        public KnownHost(string hMAC, string hIP, string hVer, string hStat, DateTime dAdded, String GUID)
        {
            this.hostMAC = hMAC;
            this.hostIP = hIP;
            this.version = hVer;
            this.status = hStat;
            this.dateAdded = dAdded;
        }

    }
}
