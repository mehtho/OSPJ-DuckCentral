using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using DuckPond.Models;
using System.Data.SqlClient;
using System.IO;

namespace DuckPond
{
    class SQLiteClass
    {
        private SQLiteConnection m_dbConnection;
        private String password;
        private String fileLocation;

        public SQLiteClass(string password)
        {
            Directory.CreateDirectory(ProgramFilesx86() + "\\DuckPond");
            fileLocation = ProgramFilesx86() + "\\Duckpond\\Information.dat";
            if (File.Exists(fileLocation) && new FileInfo(fileLocation).Length == 0)
            {
                File.Delete(fileLocation);
            }

            if (!File.Exists(fileLocation))
            {
                Console.WriteLine("Writing a new DB at " + fileLocation);
                SQLiteConnection.CreateFile(fileLocation);
                m_dbConnection = new SQLiteConnection("Data Source=" + fileLocation + ";Version=3;");
                m_dbConnection.Open();

                string sql = "CREATE TABLE BigDatabase (ConnectionString TEXT, Preference INT PRIMARY KEY NOT NULL )";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();

                string sql2 = "CREATE TABLE Login (Username TEXT PRIMARY KEY NOT NULL )";
                SQLiteCommand command2 = new SQLiteCommand(sql2, m_dbConnection);
                command2.ExecuteNonQuery();

                string sql3 = "INSERT INTO Login VALUES ('Admin')";
                SQLiteCommand command3 = new SQLiteCommand(sql3, m_dbConnection);
                command3.ExecuteNonQuery();

                string sql4 = "INSERT INTO BigDatabase VALUES ('Connection String Here',1)";
                SQLiteCommand command4 = new SQLiteCommand(sql4, m_dbConnection);
                command4.ExecuteNonQuery();

                string sql5 = "INSERT INTO BigDatabase VALUES ('Connection String Here',2)";
                SQLiteCommand command5 = new SQLiteCommand(sql5, m_dbConnection);
                command5.ExecuteNonQuery();

                string sql6 = "CREATE TABLE Services (IPAddress TEXT, Port INT, Preference INT PRIMARY KEY NOT NULL )";
                SQLiteCommand command6 = new SQLiteCommand(sql6, m_dbConnection);
                command6.ExecuteNonQuery();

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

                m_dbConnection.ChangePassword("1CF01FBFAA598E96241D4A8D2802E3B39899E34A2B61BC3BEFEEECDCD592A58C4A8E20D54222F9849CE6FEBC2A4CD64E13CE02DAB71CFE4EF7655CF72A28FF06");
            }

            fileLocation = ProgramFilesx86() + "\\Duckpond\\Information.dat";
            m_dbConnection = new SQLiteConnection("Data Source=" + fileLocation + ";Version=3;Password=" + password + ";");
            m_dbConnection.SetPassword(password);
            m_dbConnection.Open();
            this.password = password;
            //Debug!
            //m_dbConnection.ChangePassword(password);
        }

        public SQLiteClass()
        {
            fileLocation = ProgramFilesx86() + "\\Duckpond\\Information.dat";
            m_dbConnection = new SQLiteConnection("Data Source="+fileLocation+";Version=3;Password=" + DuckPassword.ud.getPassword() + ";");
            m_dbConnection.SetPassword(DuckPassword.ud.getPassword());
            m_dbConnection.Open();
        }

        public void ChangePassword(String password)
        {
            m_dbConnection.ChangePassword(password);
            DuckPassword.ud.setPassword(password);
        }

        public void CloseCon()
        {
            m_dbConnection.Close();
        }

        public Boolean GetUsernameMatch(string username)
        {
            String sql = "SELECT Username from Login";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            try
            {
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string sample = reader["Username"].ToString();
                    if (sample.ToLower().Equals(username.ToLower()))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (SQLiteException)
            {
                return false;
            }
        }

        public string GetConnectionString(int Preference)
        {
            String sql = "SELECT * FROM BigDatabase WHERE Preference = " + Preference + ";";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return reader.GetString(0);
            }

            return "";
        }

        public void SetDatabase(DatabaseObject dtb)
        {
            SQLiteCommand cmd = new SQLiteCommand("UPDATE BigDatabase " +
                "SET ConnectionString = $connectionstring " +
                "WHERE Preference = $preference", m_dbConnection);

            cmd.Parameters.AddWithValue("$connectionString", dtb.ConnectionString);
            cmd.Parameters.AddWithValue("$preference", dtb.Preference);

            cmd.ExecuteNonQuery();
        }

        public void AddDatabase(DatabaseObject dtb)
        {
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO BigDatabase " +
                "(ConnectionString, Preference) VALUES ($conn,$pref)",m_dbConnection);

            cmd.Parameters.AddWithValue("$conn", dtb.ConnectionString);
            cmd.Parameters.AddWithValue("$pref", dtb.Preference);

            cmd.ExecuteNonQuery();
        }

        public DatabaseObject GetDatabase(int pref)
        {
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM BigDatabase " +
                "WHERE Preference = $preference", m_dbConnection);

            cmd.Parameters.AddWithValue("$preference", pref);
            SQLiteDataReader reader = cmd.ExecuteReader();

            DatabaseObject dtb = new DatabaseObject();
            while (reader.Read())
            {
                dtb.ConnectionString = reader["ConnectionString"].ToString();
                dtb.Preference = Convert.ToInt32(reader["Preference"].ToString());
            }

            return dtb;
        }

        public List<DatabaseObject> GetConnections()
        {
            String sql = "SELECT * FROM BigDatabase ORDER BY PREFERENCE asc";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            SQLiteDataReader reader = command.ExecuteReader();

            List<DatabaseObject> dbs = new List<DatabaseObject>();

            while (reader.Read())
            {
                dbs.Add(new DatabaseObject(reader.GetString(0), reader.GetInt32(1)));
            }

            return dbs;
        }

        public void NewConnections(List<DatabaseObject> dbos)
        {
            using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
            {
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM BigDatabase", m_dbConnection);
                cmd.ExecuteNonQuery();

                foreach (DatabaseObject dbo in dbos)
                {
                    this.AddDatabase(dbo);
                }
                tr.Commit();
            }
        }

        public void NewServices(List<ServicesObject> svos)
        {
            using (SQLiteTransaction tr = m_dbConnection.BeginTransaction())
            {
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Services", m_dbConnection);
                cmd.ExecuteNonQuery();

                foreach (ServicesObject svo in svos)
                {
                    this.AddService(svo);
                }
                tr.Commit();
            }
        }

        public void AddService(ServicesObject svo)
        {
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Services " +
                "(IPAddress, Port, Preference) VALUES ($conn, $port, $pref)", m_dbConnection);

            cmd.Parameters.AddWithValue("$conn", svo.IPAddress);
            cmd.Parameters.AddWithValue("$port", svo.port);
            cmd.Parameters.AddWithValue("$pref", svo.Preference);

            cmd.ExecuteNonQuery();
        }


        public List<ServicesObject> GetServices()
        {
            String sql = "SELECT * FROM Services ORDER BY PREFERENCE asc";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            SQLiteDataReader reader = command.ExecuteReader();

            List<ServicesObject> svos = new List<ServicesObject>();

            while (reader.Read())
            {
                svos.Add(new ServicesObject(reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2)));
            }

            return svos;
        }

        public DateTime GetLastUpdated(byte b)
        {
            String s;
            if (b == GET_SERVICE_LIST)
            {
                s = "SELECT DateTime FROM LastUpdatedTable WHERE What='Service'";
            }
            else if(b == GET_WHITELIST_LIST)
            {
                s = "SELECT DateTime FROM LastUpdatedTable WHERE What='Whitelist'";
            }
            else
            {
                return DateTime.Parse("1/1/2000 12:00:00 AM", System.Globalization.CultureInfo.InvariantCulture);
            }
            SQLiteCommand sql = new SQLiteCommand(s, m_dbConnection);
            SQLiteDataReader reader = sql.ExecuteReader();

            while (reader.Read())
            {
                try
                {
                    return (DateTime)reader["DateTime"];
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
            SQLiteCommand sql = new SQLiteCommand(s, m_dbConnection);
            sql.Parameters.AddWithValue("$dt", dt);
            sql.ExecuteNonQuery();
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
