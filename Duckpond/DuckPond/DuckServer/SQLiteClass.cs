using DuckPond.Models;
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
                Console.WriteLine("Writing a new DB at "+FileLocation);
                SQLiteConnection.CreateFile(FileLocation);
                m_dbConnection = new SQLiteConnection("Data Source=" + FileLocation + ";Version=3;");
                m_dbConnection.Open();

                string sql = "CREATE TABLE BigDatabase (ConnectionString TEXT, Preference INT PRIMARY KEY NOT NULL )";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
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
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return reader.GetString(0);
            }

            return "";
        }

        public List<DatabaseObject> GetConnections()
        {
            String sql = "SELECT * FROM BigDatabase";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            SQLiteDataReader reader = command.ExecuteReader();

            List<DatabaseObject> dbs = new List<DatabaseObject>();

            while (reader.Read())
            {
                dbs.Add(new DatabaseObject(reader.GetString(0),reader.GetInt32(1)));
            }

            return dbs;
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
                "(ConnectionString, Preference) values ($connectionString, $preference) ", m_dbConnection);

            cmd.Parameters.AddWithValue("$connectionString", dtb.ConnectionString);
            cmd.Parameters.AddWithValue("$preference", dtb.Preference);

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
    }
}
