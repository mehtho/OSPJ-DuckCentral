using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckPond.Models
{
    [Serializable] class Events
    {
        public String eventCode;
        public String eventMessage;
        public int eventLevel;
        public String eventIP;
        public String eventGUID;
        public DateTime eventDate;

        public Events(String eCode, String eMessage, int eLevel, String eIP, String eGUID, DateTime eDate)
        {
            this.eventCode = eCode;
            this.eventMessage = eMessage;
            this.eventLevel = eLevel;
            this.eventIP = eIP;
            this.eventGUID = eGUID;
            this.eventDate = eDate;
        }

    }
}
