using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckPond.Models
{
    [Serializable] public class Events
    {
        public String eventCode;
        public String eventMessage;
        public int eventLevel;
        public String eventIP;
        public String eventGUID;
        public DateTime eventDate;
        public String comment;

        public Events(String eCode, String eMessage, int eLevel, String eIP, String eGUID, DateTime eDate, string eComment)
        {
            this.eventCode = eCode;
            this.eventMessage = eMessage;
            this.eventLevel = eLevel;
            this.eventIP = eIP;
            this.eventGUID = eGUID;
            this.eventDate = eDate;
            this.comment = eComment;
        }

        public Events()
        {

        }
    }
}
