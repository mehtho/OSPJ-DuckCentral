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
using DuckPond.Models.Whitelists;

namespace DuckPond
{
    class MSSQL
    {
        private SqlConnection cnn;
        private int connectedTo;

        //General Methods

        public MSSQL()
        {
            Begin();
        }

        public static bool ConnectionsExist()
        {
            Directory.CreateDirectory(ProgramFilesx86() + "\\DuckServer");
            SQLiteClass sqlite = new SQLiteClass(ProgramFilesx86() + "\\DuckServer\\Information.dat");
            List<DatabaseObject> connections = sqlite.GetConnections();

            if (connections.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Begin()
        {
            Directory.CreateDirectory(ProgramFilesx86() + "\\DuckServer");
            SQLiteClass sqlite = new SQLiteClass(ProgramFilesx86() + "\\DuckServer\\Information.dat");
            List<DatabaseObject> connections = sqlite.GetConnections();

            if (connections.Count > 0)
            {
                //Keep trying and save successful connection
                foreach (DatabaseObject db in connections)
                {
                    try
                    {
                        cnn = new SqlConnection(db.ConnectionString);
                        cnn.Open();
                        cnn.Close();
                        connectedTo = db.Preference;
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Source);
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }

                }
            }
            else
            {
                //Write to events
                Console.WriteLine("No Database configured");
            }
        }

        public void CloseCon()
        {
            cnn.Close();
        }

        private bool OpenCon()
        {
            try
            {
                if (!(cnn.State == ConnectionState.Open))
                {
                    cnn.Open();
                }
                return true;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Source);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.Source);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Source);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
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

        //Event Methods
        public List<Events> GetEvents()
        {
            List<Events> evs = new List<Events>();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT * FROM dbo.Events a INNER JOIN dbo.eventdetails b ON a.code = b.code ORDER BY a.Date desc";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;

            if (!OpenCon())
            {
                return new List<Events>();
            }
            using (reader = cmd.ExecuteReader())
            {
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
                return evs;
            }
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
            if (severity.Length > 0)
            {
                sqltxt += "AND severity IN(";
                bool f = true;
                foreach (int s in severity)
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

            if (!OpenCon())
            {
                return new List<Events>();
            }
            using (reader = cmd.ExecuteReader())
            {
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
                return evs;
            }
        }

        public void AddEvent(Events e)
        {
            Console.WriteLine(e.eventMessage);
            if (OpenCon())
            {
                String q = "INSERT INTO dbo.Events (Code, IP, Date, GUID, Message) Values (@cod, @I, @dat, @gui, @message)";
                SqlCommand comm = new SqlCommand(q, cnn);

                comm.Parameters.AddWithValue("cod", e.eventCode);
                comm.Parameters.AddWithValue("I", e.eventIP);
                comm.Parameters.AddWithValue("dat", e.eventDate);
                comm.Parameters.AddWithValue("gui", e.eventGUID);
                comm.Parameters.AddWithValue("message", e.eventMessage);

                comm.ExecuteNonQuery();
            }
        }

        //Known Host Methods
        public List<KnownHost> GetKnownHosts()
        {
            List<KnownHost> khs = new List<KnownHost>();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT * FROM dbo.Hosts ORDER BY DateAdded desc";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;

            if (!OpenCon())
            {
                return new List<KnownHost>();
            }
            using (reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    KnownHost kh = new KnownHost(
                        reader["MAC"].ToString(),
                        reader["IP"].ToString(),
                        reader["Version"].ToString(),
                        (DateTime)reader["DateAdded"],
                        reader["GUID"].ToString(),
                        reader["Hostname"].ToString()
                        );
                    khs.Add(kh);
                }
                return khs;
            }
        }

        public void AddKnownHost(KnownHost kh)
        {
            if (OpenCon())
            {
                if (KnownHostDupeCheck(kh.GUID))
                {
                    this.UpdateHost(kh);
                }
                else
                {
                    SqlCommand comm = new SqlCommand("INSERT INTO dbo.Hosts (MAC, IP, Version, DateAdded, GUID, Hostname) Values (@m, @i, @v, @da, @g, @h)", cnn);

                    comm.Parameters.AddWithValue("m", kh.hostMAC);
                    comm.Parameters.AddWithValue("i", kh.hostIP.Trim());
                    comm.Parameters.AddWithValue("v", kh.version.Trim());
                    comm.Parameters.AddWithValue("da", kh.dateAdded);
                    comm.Parameters.AddWithValue("g", kh.GUID.Trim());
                    comm.Parameters.AddWithValue("h", kh.hostname);

                    try
                    {
                        comm.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Source);
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
                }
            }
        }

        public bool KnownHostDupeCheck(String GUID)
        {
            if (OpenCon())
            {
                SqlCommand comm = new SqlCommand("SELECT GUID FROM dbo.hosts WHERE GUID = @guid", cnn);
                comm.Parameters.AddWithValue("guid",GUID);

                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<String> GetIPs()
        {
            List<String> ips = new List<string>();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT * FROM dbo.CheckIPs";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;

            if (!OpenCon())
            {
                return new List<String>();
            }
            using (reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    ips.Add(reader["IPAddress"].ToString());
                }
            }
            return ips;
        }

        public void UpdateHost(KnownHost kh)
        {
            if (OpenCon())
            {
                String q = "Update dbo.hosts set MAC = @mac, IP = @ip, version = @version, DateAdded=@dateadded, Hostname= @hostname Where Guid = @guid";
                SqlCommand comm = new SqlCommand(q, cnn);

                comm.Parameters.AddWithValue("mac", kh.hostMAC);
                comm.Parameters.AddWithValue("ip", kh.hostIP.Trim());
                comm.Parameters.AddWithValue("version", kh.version.Trim());
                comm.Parameters.AddWithValue("dateadded", kh.dateAdded);
                comm.Parameters.AddWithValue("guid", kh.GUID.Trim());
                comm.Parameters.AddWithValue("hostname", kh.hostname);

                comm.ExecuteNonQuery();
            }
        }

        //Service Methods

        public List<ServicesObject> GetServices()
        {
            List<ServicesObject> sos = new List<ServicesObject>();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT * FROM dbo.Services";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;

            if (!OpenCon())
            {
                return sos;
            }
            using (reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    sos.Add(new ServicesObject(reader["IP"].ToString(), (int)reader["Port"], (int)reader["Preference"]));
                }

                return sos;
            }
        }

        public DateTime GetLastUpdated(byte b)
        {
            if (!OpenCon())
            {
                return DateTime.Parse("1/1/2000 12:00:00 AM", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                String s;
                if (b == GET_SERVICE_LIST)
                {
                    s = "Service";
                }
                else if (b == GET_WHITELIST_LIST)
                {
                    s = "Whitelist";
                }
                else
                {
                    return DateTime.Parse("1/1/2000 12:00:00 AM", System.Globalization.CultureInfo.InvariantCulture);
                }
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "SELECT LastUpdated FROM dbo.LastUpdated WHERE What = @wt";
                cmd.Parameters.AddWithValue("wt", s);
                cmd.CommandType = CommandType.Text;
                cmd.Connection = cnn;

                using (reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return (DateTime)reader["LastUpdated"];
                    }
                }
                return DateTime.Parse("1/1/2000 12:00:00 AM", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public void SetLastUpdated(DateTime dt, byte b)
        {
            if (!OpenCon())
            {
                return;
            }
            else
            {
                String s;
                if (b == GET_SERVICE_LIST)
                {
                    s = "Service";
                }
                else if (b == GET_WHITELIST_LIST)
                {
                    s = "Whitelist";
                }
                else
                {
                    return;
                }

                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "UPDATE dbo.LastUpdated SET LastUpdated = @dt WHERE What = @wt";
                cmd.Parameters.AddWithValue("wt", s);
                cmd.Parameters.AddWithValue("dt", dt);
                cmd.CommandType = CommandType.Text;
                cmd.Connection = cnn;
                cmd.ExecuteNonQuery();
            }
        }

        public void FixLastUpdated(byte b)
        {
            if (!OpenCon())
            {
                return;
            }
            else
            {
                String s;
                if (b == GET_SERVICE_LIST)
                {
                    s = "Service";
                }
                else if (b == GET_WHITELIST_LIST)
                {
                    s = "Whitelist";
                }
                else
                {
                    return;
                }

                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "INSERT INTO LastUpdated (LastUpdated,What) VALUES (@dt, @wt)";
                cmd.Parameters.AddWithValue("wt", s);
                cmd.Parameters.AddWithValue("dt", DateTime.Parse("1/1/2000 12:00:00 AM", System.Globalization.CultureInfo.InvariantCulture));
                cmd.CommandType = CommandType.Text;
                cmd.Connection = cnn;
                cmd.ExecuteNonQuery();
            }
        }

        public List<Whitelists> GetWhitelists()
        {
            if (!OpenCon())
            {
                return new List<Whitelists>();
            }

            List<Whitelists> wls = new List<Whitelists>();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT * FROM dbo.Whitelist ORDER BY WhitelistID desc";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;

            using (reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Whitelists wl = new Whitelists(
                        (DateTime)reader["Datetime"],
                        reader["Vid"].ToString(),
                        reader["Pid"].ToString(),
                        reader["Serial"].ToString(),
                        (int)reader["WhitelistID"]);

                    wls.Add(wl);
                }
                return wls;
            }
        }

        

        public const byte GET_SERVICE_LIST = 0;
        public const byte GET_WHITELIST_LIST = 2;
    }
}
