using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckPond.Models;
using Microsoft.SqlServer;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace DuckPond.Resources
{
    class DatabaseBackup
    {
        private Server srv;
        private Database dtb;

        public DatabaseBackup()
        {
            //Get connection string
            SQLiteClass sql = new SQLiteClass();
            List<DatabaseObject> dbos = sql.GetConnections();
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(dbos[0].ConnectionString);
            srv = new Server(new ServerConnection(conn));

            //List down all the databases on the server
            foreach (Database myDatabase in srv.Databases)
            {
                Console.WriteLine(myDatabase.Name);
                if (myDatabase.Name.Equals("DuckpondOne"))
                {
                    dtb = myDatabase;
                }
            }
        }

        public void DoBackup()
        {
            SQLiteClass sql = new SQLiteClass();

            Backup bkpDBFull = new Backup();
            bkpDBFull.Action = BackupActionType.Database;
            bkpDBFull.Database = dtb.Name;

            bkpDBFull.Devices.AddDevice(sql.GetBackupLocation()+"\\"+DateTime.Now.ToString("ddMMyyyy hhmmss")+".bak", DeviceType.File);
            bkpDBFull.BackupSetName = "Duckpond Suite Database Backup " + DateTime.Now;
            bkpDBFull.BackupSetDescription = "Duckpond Suite Database - Full Backup";

            bkpDBFull.ExpirationDate = DateTime.Today.AddDays(28);

            bkpDBFull.Initialize = false;

            try
            {
                bkpDBFull.SqlBackup(srv);
            }
            catch
            {
                throw new FailedOperationException();
            }
            
        }
    }
}
