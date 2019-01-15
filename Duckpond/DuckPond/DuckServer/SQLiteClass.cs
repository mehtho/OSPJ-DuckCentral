using DuckPond.Models;
using DuckPond.Models.Whitelists;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace DuckPond
{
    class SQLiteClass
    {
        private SQLiteConnection m_dbConnection;

        public SQLiteClass(string FileLocation)
        {
            if (File.Exists(FileLocation) && new FileInfo(FileLocation).Length == 0)
            {
                File.Delete(FileLocation);
            }

            if (!File.Exists(FileLocation))
            {
                Directory.CreateDirectory(ProgramFilesx86() + "\\DuckServer");
                Console.WriteLine("Writing a new DB at "+FileLocation);
                SQLiteConnection.CreateFile(FileLocation);
                m_dbConnection = new SQLiteConnection("Data Source=" + FileLocation + ";Version=3;");
                m_dbConnection.Open();

                string sql = "CREATE TABLE BigDatabase (ConnectionString TEXT, Preference INT PRIMARY KEY NOT NULL )";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();

                string sql2 = "CREATE TABLE Services (IP TEXT, Port INT, Preference INT PRIMARY KEY NOT NULL )";
                SQLiteCommand command2 = new SQLiteCommand(sql2, m_dbConnection);
                command2.ExecuteNonQuery();

                string sql3 = "CREATE TABLE Whitelists (PID TEXT, VID TEXT, Serial TEXT, DateTime TEXT, WhitelistID INT PRIMARY KEY NOT NULL)";
                SQLiteCommand command3 = new SQLiteCommand(sql3, m_dbConnection);
                command3.ExecuteNonQuery();

                string sql7 = "CREATE TABLE LastUpdatedTable (DateTime TEXT, What TEXT PRIMARY KEY NOT NULL )";
                SQLiteCommand command7 = new SQLiteCommand(sql7, m_dbConnection);
                command7.ExecuteNonQuery();

                string sql8 = "INSERT INTO LastUpdatedTable (DateTime, What) VALUES ($dt,'Whitelist')";
                SQLiteCommand command8 = new SQLiteCommand(sql8, m_dbConnection);
                command8.Parameters.AddWithValue("$dt", DateTime.Parse("1/1/2000 12:00:00 AM", System.Globalization.CultureInfo.InvariantCulture).ToString());
                command8.ExecuteNonQuery();

                string sql9 = "INSERT INTO LastUpdatedTable (DateTime, What) VALUES ($dt,'Service')";
                SQLiteCommand command9 = new SQLiteCommand(sql9, m_dbConnection);
                command9.Parameters.AddWithValue("$dt", DateTime.Parse("1/1/2000 12:00:00 AM", System.Globalization.CultureInfo.InvariantCulture).ToString());
                command9.ExecuteNonQuery();
            }

            m_dbConnection = new SQLiteConnection("Data Source="+FileLocation+";Version=3;");
            m_dbConnection.Open();
        }

        public void CloseCon()
        {
            m_dbConnection.Close();
        }

        public string GetConnectionString(int Preference)
        {
            String sql = "SELECT * FROM BigDatabase WHERE Preference = " + Preference + ";";
           
            using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader.GetString(0);
                    }

                    return "";
                }
            }
        }

        public List<DatabaseObject> GetConnections()
        {
            String sql = "SELECT * FROM BigDatabase";
            using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    List<DatabaseObject> dbs = new List<DatabaseObject>();

                    while (reader.Read())
                    {
                        dbs.Add(new DatabaseObject(reader.GetString(0), reader.GetInt32(1)));
                    }

                    return dbs;
                }
            }
        }

        public void SetDatabase(DatabaseObject dtb)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("UPDATE BigDatabase " +
                "SET ConnectionString = $connectionstring " +
                "WHERE Preference = $preference", m_dbConnection))
            {
                cmd.Parameters.AddWithValue("$connectionString", dtb.ConnectionString);
                cmd.Parameters.AddWithValue("$preference", dtb.Preference);

                cmd.ExecuteNonQuery();
            }
        }

        public void AddDatabase(DatabaseObject dtb)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO BigDatabase " +
                "(ConnectionString, Preference) values ($connectionString, $preference) ", m_dbConnection))
            {
                cmd.Parameters.AddWithValue("$connectionString", dtb.ConnectionString);
                cmd.Parameters.AddWithValue("$preference", dtb.Preference);

                cmd.ExecuteNonQuery();
            }
        }

        public void AddDatabase(DatabaseObject dtb, SQLiteTransaction tr)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO BigDatabase " +
                "(ConnectionString, Preference) values ($connectionString, $preference) ", m_dbConnection))
            {
                cmd.Transaction = tr;
                cmd.Parameters.AddWithValue("$connectionString", dtb.ConnectionString);
                cmd.Parameters.AddWithValue("$preference", dtb.Preference);

                cmd.ExecuteNonQuery();
            }
        }

        public DatabaseObject GetDatabase(int pref)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM BigDatabase " +
                "WHERE Preference = $preference", m_dbConnection))
            {
                cmd.Parameters.AddWithValue("$preference", pref);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    DatabaseObject dtb = new DatabaseObject();
                    while (reader.Read())
                    {
                        dtb.ConnectionString = reader["ConnectionString"].ToString();
                        dtb.Preference = Convert.ToInt32(reader["Preference"].ToString());
                    }

                    return dtb;
                }
            }
        }

        public void NewConnections(List<DatabaseObject> dbos)
        {
            using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
            {
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM BigDatabase", m_dbConnection);
                cmd.ExecuteNonQuery();

                foreach (DatabaseObject dbo in dbos)
                {
                    this.AddDatabase(dbo, tr);
                }
                tr.Commit();
            }
        }

        public void NewServices(List<ServicesObject> sros)
        {
            using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
            {
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Services", m_dbConnection);
                cmd.ExecuteNonQuery();

                foreach (ServicesObject sro in sros)
                {
                    this.AddService(sro);
                }
                tr.Commit();
            }
        }

        public void AddService(ServicesObject sro)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Services " +
                "(Ip, Port, Preference) values ($Ip, $Port, $Preference) ", m_dbConnection))
            {
                cmd.Parameters.AddWithValue("$Ip", sro.IPAddress);
                cmd.Parameters.AddWithValue("$Port", sro.port);
                cmd.Parameters.AddWithValue("$Preference", sro.Preference);

                cmd.ExecuteNonQuery();
            }
        }

        public List<ServicesObject> GetServices()
        {
            List<ServicesObject> sros = new List<ServicesObject>();
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * From Services", m_dbConnection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ServicesObject sro = new ServicesObject(reader["IP"].ToString(), (int)reader["Port"], (int)reader["Preference"]);
                        sros.Add(sro);
                    }

                    return sros; 
                }
            }
        }

        public void NewWhitelists(List<Whitelists> wls)
        {
            using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
            {
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Whitelists", m_dbConnection);
                cmd.ExecuteNonQuery();

                foreach (Whitelists wl in wls)
                {
                    this.AddWhitelist(wl, tr);
                }
                tr.Commit();
            }
        }

        public void AddWhitelist(Whitelists wl, SQLiteTransaction tr)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Whitelists " +
             "(PID, VID, Serial, WhitelistID, DateTime) values ($PID, $VID, $Serial, $WhitelistID, $DateTime) ", m_dbConnection))
            {
                cmd.Transaction = tr;
                cmd.Parameters.AddWithValue("$PID", wl.Pid1);
                cmd.Parameters.AddWithValue("$VID", wl.Vid1);
                cmd.Parameters.AddWithValue("$Serial", wl.SerialNumber1);
                cmd.Parameters.AddWithValue("$WhitelistID", wl.WhitelistID1);
                cmd.Parameters.AddWithValue("$DateTime", wl.Datetime1);

                cmd.ExecuteNonQuery();
            }
        }

        public void AddWhitelist(Whitelists wl)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Whitelists " +
             "(PID, VID, Serial, WhitelistID, DateTime) values ($PID, $VID, $Serial, $WhitelistID, $DateTime) ", m_dbConnection))
            {
                cmd.Parameters.AddWithValue("$PID", wl.Pid1);
                cmd.Parameters.AddWithValue("$VID", wl.Vid1);
                cmd.Parameters.AddWithValue("$Serial", wl.SerialNumber1);
                cmd.Parameters.AddWithValue("$WhitelistID", wl.WhitelistID1);
                cmd.Parameters.AddWithValue("$DateTime", wl.Datetime1);

                cmd.ExecuteNonQuery();
            }
        }

        public List<Whitelists> GetWhitelists()
        {
            List<Whitelists> wls = new List<Whitelists>();
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * From Whitelists", m_dbConnection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Whitelists wl = new Whitelists(DateTime.Parse(reader["DateTime"].ToString(), System.Globalization.CultureInfo.InvariantCulture), reader["Vid"].ToString(), reader["Pid"].ToString(), reader["Serial"].ToString(), (int)reader["WhitelistID"]);
                        wls.Add(wl);
                    }

                    return wls;
                }
            }
        }

        public DateTime GetLastUpdated(byte b)
        {
            String s;
            if (b == GET_SERVICE_LIST)
            {
                s = "SELECT DateTime FROM LastUpdatedTable WHERE What='Service'";
            }
            else if (b == GET_WHITELIST_LIST)
            {
                s = "SELECT DateTime FROM LastUpdatedTable WHERE What='Whitelist'";
            }
            else
            {
                return DateTime.Parse("1/1/2000 12:00:00 AM", System.Globalization.CultureInfo.InvariantCulture);
            }
            
            using (SQLiteCommand sql = new SQLiteCommand(s, m_dbConnection))
            {
                using (SQLiteDataReader reader = sql.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            return DateTime.Parse(reader["DateTime"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.Source);
                            Console.WriteLine(e.StackTrace);
                        }
                    }
                    return DateTime.Parse("1/1/2000 12:00:00 AM", System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }

        public void SetLastUpdated(byte b, DateTime dt)
        {
            String s;
            if (b == GET_SERVICE_LIST)
            {
                s = "UPDATE LastUpdatedTable SET DateTime = $dt WHERE What='Service'";
            }
            else if (b == GET_WHITELIST_LIST)
            {
                s = "UPDATE LastUpdatedTable SET DateTime = $dt WHERE What='Whitelist'";
            }
            else
            {
                return;
            }
            using (SQLiteCommand sql = new SQLiteCommand(s, m_dbConnection))
            {
                sql.Parameters.AddWithValue("$dt", dt);
                sql.ExecuteNonQuery();
            }
        }

        public const byte GET_SERVICE_LIST = 0;
        public const byte GET_WHITELIST_LIST = 2;

        public static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }
    }
}
