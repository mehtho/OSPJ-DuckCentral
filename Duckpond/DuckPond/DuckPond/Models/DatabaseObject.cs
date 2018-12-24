using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckPond.Models
{
    class DatabaseObject
    {
        public DatabaseObject()
        {

        }

        public DatabaseObject(String Conn, int Pref)
        {
            this.ConnectionString = Conn;
            this.Preference = Pref;
        }

        public String ConnectionString;

        public int Preference;
    }
}
