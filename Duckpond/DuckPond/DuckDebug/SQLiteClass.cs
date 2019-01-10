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
                Directory.CreateDirectory(ProgramFilesx86() + "\\DuckClient");
                Console.WriteLine("Writing a new DB at "+FileLocation);
                SQLiteConnection.CreateFile(FileLocation);
                m_dbConnection = new SQLiteConnection("Data Source=" + FileLocation + ";Version=3;");
                m_dbConnection.Open();

                string sql = "CREATE TABLE Identity (GUID TEXT, REGISTERED BOOL)";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();

                string sql2 = "INSERT INTO Identity (GUID, REGISTERED) values ($gid, 'false');";
                SQLiteCommand command2 = new SQLiteCommand(sql2, m_dbConnection);
                command2.Parameters.AddWithValue("$gid", MakeGUID().ToString());
                command2.ExecuteNonQuery();
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
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return reader.GetString(0);
            }

            return "";
        }

        public void SetRegistered(bool b)
        {
            String sql = "Update Identity set REGISTERED = 'true';";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        public bool GetRegistered()
        {
            String sql = "SELECT * FROM Identity;";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return (bool)reader["Registered"];
            }

            return true;
        }

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
