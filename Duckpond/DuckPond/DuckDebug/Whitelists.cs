using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckPond.Models.Whitelists
{
    [Serializable]
    public class Whitelists
    {
        private DateTime Datetime;
        private string Pid;
        private string Vid;
        private string SerialNumber;
        private int WhitelistID;

        public Whitelists()
        {

        }

        public Whitelists(DateTime Datetime, String Vid, String Pid, String SerialNumber, int WhitelistID)
        {
            this.Datetime = Datetime;
            this.Pid = Pid;
            this.Vid = Vid;
            this.SerialNumber = SerialNumber;
            this.WhitelistID = WhitelistID;
        }

        public Whitelists(DateTime Datetime, String Vid, String Pid, String SerialNumber)
        {
            this.Datetime = Datetime;
            this.Pid = Pid;
            this.Vid = Vid;
            this.SerialNumber = SerialNumber;
        }

        public DateTime Datetime1
        {
            get
            {
                return Datetime;
            }

            set
            {
                Datetime = value;
            }
        }

        public String Pid1
        {
            get
            {
                return Pid;
            }

            set
            {
                Pid = value;
            }
        }

        public String Vid1
        {
            get
            {
                return Vid;
            }

            set
            {
                Vid = value;
            }
        }

        public String SerialNumber1
        {
            get
            {
                return SerialNumber;
            }

            set
            {
                SerialNumber = value;
            }
        }

        public int WhitelistID1
        {
            get
            {
                return WhitelistID;
            }

            set
            {
                WhitelistID = value;
            }
        }

        public override String ToString()
        {
            return "Datetime: " + Datetime + " Pid: " + Pid + " Vid: " + Vid + " SerialNumber: " + SerialNumber + " WhitelistID: " + WhitelistID;
        }
    }
}
