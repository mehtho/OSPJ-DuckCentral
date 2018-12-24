﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using DuckPond.Models;
using System.Windows;
using System.IO;
using System.Data.SQLite;

namespace DuckPond
{
    class MSSQL
    {
        private SqlConnection cnn;
        private int connectedTo;

        //General Methods

        public MSSQL()
        {
            Directory.CreateDirectory(ProgramFilesx86()+"\\DuckServer");
            SQLiteClass sqlite = new SQLiteClass(ProgramFilesx86()+"\\DuckServer\\Information.dat");
                List<DatabaseObject> connections = sqlite.GetConnections();

                if (connections.Count > 0)
                {
                    foreach(DatabaseObject db in connections)
                    {
                        cnn = new SqlConnection(db.ConnectionString);
                        connectedTo = db.Preference;
                        break;
                    }
                }
                else
                {
                    //Write to events
                } 
        }

        public void closeCon()
        {
            cnn.Close();
        }

        //Event Methods
        public List<Events> GetEvents()
        {
            List<Events> evs = new List<Events>();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT * FROM dbo.Events a INNER JOIN dbo.eventdetails b ON a.code = b.code ORDER BY a.Date desc";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;

            try
            {
                cnn.Open();
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Events ev = new Events(

                        reader["code"].ToString(),
                        reader["message"].ToString(),
                        (int)reader["severity"],
                        reader["IP"].ToString(),
                        reader["GUID"].ToString(),
                        (DateTime)reader["date"]);


                    evs.Add(ev);
                }
            }
            catch(System.Data.SqlClient.SqlException)
            {
                //Write To Event Log("Error connecting to DB!");
            }
            finally
            {
                cnn.Close();
            }

            return evs;
        }

        public List<Events> FilteredEvents(String code, String message, int[] severity, String ip, String guid, DateTime date1, DateTime date2)
        {
            List<Events> evs = new List<Events>();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            String sqltxt = "SELECT * FROM dbo.Events a INNER JOIN dbo.eventdetails b ON a.code = b.code " + 
                "WHERE Date BETWEEN @dt1 AND @dt2 ";

            if (!code.Equals(""))
            {
                sqltxt += "AND code LIKE @code ";
                cmd.CommandText = sqltxt;
                cmd.Parameters.AddWithValue("@code", "%" + code + "%");
            }
            if (!message.Equals(""))
            {
                sqltxt += "AND message LIKE @message ";
                cmd.CommandText = sqltxt;
                cmd.Parameters.AddWithValue("@message", "%" + message + "%");
            }
            if (severity.Length>0)
            {
                sqltxt += "AND severity IN(";
                bool f = true;
                foreach(int s in severity)
                {
                    if (f)
                    {
                        f = false;
                        sqltxt += s + "";
                    }
                    else
                    {
                        sqltxt += "," + s;
                    }
                }
                sqltxt += ") ";
                cmd.CommandText = sqltxt;
            }
            if (!ip.Equals(""))
            {
                sqltxt += "AND ip LIKE @ip ";
                cmd.CommandText = sqltxt;
                cmd.Parameters.AddWithValue("@ip", "%" + ip + "%");
            }
            if (!guid.Equals(""))
            {
                sqltxt += "AND guid LIKE @guid ";
                cmd.CommandText = sqltxt;
                cmd.Parameters.AddWithValue("@guid", "%" + guid + "%");
            }

            SqlParameter dtt1 = new SqlParameter("dt1", SqlDbType.DateTime);
            dtt1.Value = date1;
            cmd.Parameters.Add(dtt1);

            SqlParameter dtt2 = new SqlParameter("dt2", SqlDbType.DateTime);
            dtt2.Value = date2;
            cmd.Parameters.Add(dtt2);

            sqltxt += "ORDER BY a.Date desc";

            cmd.CommandText = sqltxt; 
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;

            cnn.Open();
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Events ev = new Events(

                    reader["code"].ToString(),
                    reader["message"].ToString(),
                    (int)reader["severity"],
                    reader["IP"].ToString(),
                    reader["GUID"].ToString(),
                    (DateTime)reader["date"]);


                evs.Add(ev);
            }
            cnn.Close();
            return evs;
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
    }
}
