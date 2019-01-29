using DuckPond;
using DuckPond.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckPond.Resources;
using System.Threading;
using InstantMessenger;

namespace DuckServer
{
    class ServiceConnectionDelegator
    {
        private static List<ServicesObject> rawServices = new List<ServicesObject>();
        public static List<ServicePlusStatus> spps = new List<ServicePlusStatus>();
        public static bool start = false;

        public static void LoadServices()
        {
            start = true;
            SQLiteClass sql = new SQLiteClass(SQLiteClass.ProgramFilesx86() + "\\DuckClient\\Information.dat");
            rawServices =  sql.GetServices();

            spps = new List<ServicePlusStatus>();

            List<Thread> threads = new List<Thread>();
            foreach (ServicesObject so in rawServices)
            {
                Thread th = new Thread(() => HostScan.run(so));
                th.Start();
                threads.Add(th);
            }

            foreach (Thread th in threads)
            {
                th.Join();
            }
        }

        public static ServicesObject GetService()
        {
            if (!start)
            {
                LoadServices();
            }
            for(int i= 0 ; i<spps.Count;i++)
            {
                if (spps[i].Status==ServicesObject.STATUS_ONLINE)
                {
                    
                    return spps[i].service;
                }
            }
            return null;
        }
    }
}
