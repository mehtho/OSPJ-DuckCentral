using DuckPond;
using DuckPond.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckServer
{
    class ServiceConnectionDelegator
    {
        public static ServicesObject GetService()
        {
            //Work on later for cascade
            SQLiteClass sql = new SQLiteClass(SQLiteClass.ProgramFilesx86() + "\\DuckClient\\Information.dat");
            List<ServicesObject> srvs = sql.GetServices();
            //DEBUG CODE
            foreach(ServicesObject srv in srvs)
            {
                if (srv.Preference == 1)
                {
                    return srv;
                }
            }
            return null;
            //DEBUG CODE
        }
    }
}
