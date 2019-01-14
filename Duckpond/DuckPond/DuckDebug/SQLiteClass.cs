using DuckPond.Models;
using DuckPond.Models.Whitelists;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

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
                Directory.CreateDirectory(ProgramFilesx86() + "\\DuckClient");
                Console.WriteLine("Writing a new DB at "+FileLocation);
                SQLiteConnection.CreateFile(FileLocation);
                m_dbConnection = new SQLiteConnection("Data Source=" + FileLocation + ";Version=3;");
                m_dbConnection.Open();

                string sql = "CREATE TABLE Identity (GUID TEXT, REGISTERED BOOL, Version TEXT)";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();

                string sql2 = "INSERT INTO Identity (GUID, REGISTERED, Version) values ($gid, 'false','1.0.0.0');";
                SQLiteCommand command2 = new SQLiteCommand(sql2, m_dbConnection);
                command2.Parameters.AddWithValue("$gid", MakeGUID().ToString());
                command2.ExecuteNonQuery();

                string sql3 = "CREATE TABLE Whitelists (PID TEXT, VID TEXT, Serial TEXT, DateTime TEXT, WhitelistID INT PRIMARY KEY NOT NULL)";
                SQLiteCommand command3 = new SQLiteCommand(sql3, m_dbConnection);
                command3.ExecuteNonQuery();

                string sql4 = "CREATE TABLE Services (IP TEXT, Port INT, Preference INT PRIMARY KEY NOT NULL )";
                SQLiteCommand command4 = new SQLiteCommand(sql4, m_dbConnection);
                command4.ExecuteNonQuery();

                string sql5 = "CREATE TABLE LastUpdatedTable (DateTime TEXT, What TEXT PRIMARY KEY NOT NULL )";
                SQLiteCommand command5 = new SQLiteCommand(sql5, m_dbConnection);
                command5.ExecuteNonQuery();

                string sql6 = "INSERT INTO LastUpdatedTable (DateTime, What) VALUES ($dt,'Whitelist')";
                SQLiteCommand command6 = new SQLiteCommand(sql6, m_dbConnection);
                command6.Parameters.AddWithValue("$dt", DateTime.Parse("1/1/2000 12:00:00 AM", System.Globalization.CultureInfo.InvariantCulture).ToString());
                command6.ExecuteNonQuery();

                string sql7 = "INSERT INTO LastUpdatedTable (DateTime, What) VALUES ($dt,'Service')";
                SQLiteCommand command7 = new SQLiteCommand(sql7, m_dbConnection);
                command7.Parameters.AddWithValue("$dt", DateTime.Parse("1/1/2000 12:00:00 AM", System.Globalization.CultureInfo.InvariantCulture).ToString());
                command7.ExecuteNonQuery();
            }

            m_dbConnection = new SQLiteConnection("Data Source="+FileLocation+";Version=3;");
            m_dbConnection.Open();
        }

        public void CloseCon()
        {
            m_dbConnection.Close();
        }

        public String GetGUID()
        {
            String sql = "SELECT * FROM Identity;";
            using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader["GUID"].ToString();
                    }

                    return "";
                }
            }
        }

        public String GetVersion()
        {
            String sql = "SELECT * FROM Identity;";
            using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader["Version"].ToString();
                    }

                    return "";
                }
            }
        }

        public void SetRegistered(bool b)
        {
            String sql = "Update Identity set REGISTERED = 'true';";
            using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
            {
                command.ExecuteNonQuery();
            }
        }

        public bool GetRegistered()
        {
            String sql = "SELECT * FROM Identity;";

            using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return (bool)reader["Registered"];
                    }

                    return true;
                }
            }
        }

        /*public void NewServices(List<ServicesObject> sros)
        {
            using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
            {
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Services", m_dbConnection);
                Console.WriteLine("125");
                cmd.ExecuteNonQuery();
                Console.WriteLine("126");
                foreach (ServicesObject sro in sros)
                {
                    Console.WriteLine("127");
                    AddService(sro, tr);
                    Console.WriteLine("128");
                }
                Console.WriteLine("129");
                tr.Commit();
                Console.WriteLine("130");
            }
        }*/

        public void NewServices(List<ServicesObject> sros)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Services", m_dbConnection))
            {
                Console.WriteLine("125");
                cmd.ExecuteNonQuery();
                Console.WriteLine("126");
                foreach (ServicesObject sro in sros)
                {
                    Console.WriteLine("127");
                    AddService(sro);
                    Console.WriteLine("128");
                }
                Console.WriteLine("129");
            }
        }

        public void AddService(ServicesObject sro)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Services (IP, Port, Preference) values ($Ip, $Port, $Preference) ", m_dbConnection))
            {
                cmd.Parameters.AddWithValue("$Ip", sro.IPAddress);
                cmd.Parameters.AddWithValue("$Port", sro.port);
                cmd.Parameters.AddWithValue("$Preference", sro.Preference);

                cmd.ExecuteNonQuery();
            }
        }

        public void AddService(ServicesObject sro, SQLiteTransaction sqlt)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Services (Ip, Port, Preference) values ($Ip, $Port, $Preference) ", m_dbConnection))
            {
                Console.WriteLine("154");
                cmd.Parameters.AddWithValue("$Ip", sro.IPAddress);
                cmd.Parameters.AddWithValue("$Port", sro.port);
                cmd.Parameters.AddWithValue("$Preference", sro.Preference);
                cmd.Transaction = sqlt;
                Console.WriteLine("160");
                cmd.ExecuteNonQuery();
                Console.WriteLine("162");
            }
        }

        public List<ServicesObject> GetServices()
        {
            String sql = "SELECT * FROM Services ORDER BY Preference asc";
            using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    List<ServicesObject> sros = new List<ServicesObject>();

                    while (reader.Read())
                    {
                        sros.Add(new ServicesObject(reader["IP"].ToString(), (int)reader["Port"], (int)reader["Preference"]));
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
                    this.AddWhitelist(wl);
                }
                tr.Commit();
            }
        }

        public void AddWhitelist(Whitelists wl)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Whitelists (PID, VID, Serial, WhitelistID, DateTime) values ($PID, $VID, $Serial, $WhitelistID, $DateTime) ", m_dbConnection))
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
            String sql = "SELECT * FROM Whitelists ORDER BY WhitelistID desc";
            using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    List<Whitelists> wls = new List<Whitelists>();

                    while (reader.Read())
                    {
                        wls.Add(new Whitelists((DateTime)reader["DateTime"], reader["Vid"].ToString(), reader["Pid"].ToString(), reader["Serial"].ToString(), (int)reader["WhitelistID"]));
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

        public static Guid MakeGUID()
        {
            Guid g;
            // Create and display the value of two GUIDs.
            g = Guid.NewGuid();
            return g;
        }
    }
}
