using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckPond.Models.Whitelists;
using System.Collections;
using DuckPond.Models;
using System.Windows;
using System.Data.SQLite;

namespace DuckPond
{
    class MSSQL
    {
        private SqlConnection cnn;
        private String connectionString;

        //General Methods

        public MSSQL()
        {
            SQLiteClass sqlite = new SQLiteClass(DuckPassword.ud.getPassword());
            this.connectionString = sqlite.GetConnectionString(1);

            try
            {
                cnn = new SqlConnection(connectionString);
            }
            catch (System.ArgumentException)
            {
                SetTopAlert("Error Connecting to DB!");
            }
        }

        private bool OpenCon()
        {
            try
            {
                if (cnn.State == ConnectionState.Closed)
                    cnn.Open();

                SetTopAlert("");
                Console.WriteLine("Opened Connection");
                return true;
            }
            catch (InvalidOperationException)
            {
                SetTopAlert("Error connecting to DB!");
                return false;
            }
            catch (NullReferenceException)
            {
                SetTopAlert("Error connecting to DB!");
                return false;
            }
            catch (Exception e)
            {
                SetTopAlert("Error connecting to DB!");
                Console.WriteLine(e.Source);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        public void CloseCon()
        {
            if (cnn != null && cnn.State == ConnectionState.Open)
                cnn.Close();
        }

        public void SetTopAlert(String str)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(LoginScreen))
                {
                    (window as LoginScreen).AlertMessage.Content = str;
                }
            }
        }

        //Whitelist Methods

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

            reader = cmd.ExecuteReader();

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

            cnn.Close();
            return wls;
        }

        public byte AddWhitelist(Whitelists wl)
        {
            if (!CheckWhitelistDupe(wl))
            {
                if (!OpenCon())
                {
                    return 2;
                }
                String q2 = "INSERT INTO dbo.Whitelist (DateTime,vid,pid,serial) Values (@dt, @v, @p, @cereal2)";
                SqlCommand comm2 = new SqlCommand(q2, cnn);

                SqlParameter datetimeParam = new SqlParameter("dt", SqlDbType.DateTime);
                datetimeParam.Value = wl.Datetime1;
                SqlParameter vParam = new SqlParameter("v", SqlDbType.VarChar);
                vParam.Value = wl.Vid1;
                SqlParameter pParam = new SqlParameter("p", SqlDbType.VarChar);
                pParam.Value = wl.Pid1;
                SqlParameter cereal2Param = new SqlParameter("cereal2", SqlDbType.VarChar);
                cereal2Param.Value = wl.SerialNumber1;

                comm2.Parameters.Add(datetimeParam);
                comm2.Parameters.Add(vParam);
                comm2.Parameters.Add(pParam);
                comm2.Parameters.Add(cereal2Param);

                comm2.ExecuteNonQuery();

                cnn.Close();
                return 0;
            }
            else
            {
                cnn.Close();
                return 1;
            }
        }

        public bool CheckWhitelistDupe(Whitelists wl)
        {
            if (!OpenCon())
            {
                return true;
            }
            String q1 = "SELECT * FROM dbo.Whitelist Where Serial = @cereal";
            SqlParameter serialParam = new SqlParameter("cereal", SqlDbType.VarChar);
            serialParam.Value = wl.SerialNumber1;

            SqlCommand comm = new SqlCommand(q1, cnn);

            comm.Parameters.Add(serialParam);

            var results = comm.ExecuteReader();

            while (results.Read())
            {
                if (results["serial"].ToString().Equals(wl.SerialNumber1))
                {
                    cnn.Close();
                    return true;
                }
            }
            cnn.Close();
            return false;
        }

        public List<Whitelists> FilteredWhitelists(String pid, String vid, String srl, DateTime dt1, DateTime dt2)
        {
            if (!OpenCon())
            {
                return new List<Whitelists>();
            }
            List<Whitelists> wls = new List<Whitelists>();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            String sqltxt = "SELECT * FROM dbo.Whitelist WHERE "
           + "DateTime BETWEEN @dt1 AND @dt2 ";

            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;

            if (!pid.Equals(""))
            {
                sqltxt += "AND Pid LIKE @pid ";
                cmd.CommandText = sqltxt;
                cmd.Parameters.AddWithValue("@pid", "%" + pid + "%");
            }
            if (!vid.Equals(""))
            {
                sqltxt += "AND Vid LIKE @vid ";
                cmd.CommandText = sqltxt;
                cmd.Parameters.AddWithValue("@vid", "%" + vid + "%");
            }
            if (!srl.Equals(""))
            {
                sqltxt += "AND Serial LIKE @srl ";
                cmd.CommandText = sqltxt;
                cmd.Parameters.AddWithValue("@srl", "%" + srl + "%");
            }

            sqltxt += "ORDER BY WhitelistID desc";
            cmd.CommandText = sqltxt;

            SqlParameter dtt1 = new SqlParameter("dt1", SqlDbType.DateTime);
            dtt1.Value = dt1;
            cmd.Parameters.Add(dtt1);

            SqlParameter dtt2 = new SqlParameter("dt2", SqlDbType.DateTime);
            dtt2.Value = dt2;
            cmd.Parameters.Add(dtt2);

            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Whitelists wl = new Whitelists(
                    (DateTime)reader["datetime"],
                    reader["Vid"].ToString(),
                    reader["Pid"].ToString(),
                    reader["Serial"].ToString(),
                    (int)reader["WhitelistID"]);

                wls.Add(wl);
            }

            cnn.Close();
            return wls;
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
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Events ev = new Events(

                    reader["code"].ToString(),
                    reader["message"].ToString(),
                    (int)reader["severity"],
                    reader["IP"].ToString(),
                    reader["GUID"].ToString(),
                    (DateTime)reader["date"],
                    reader["comment"].ToString());

                evs.Add(ev);
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
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Events ev = new Events(

                    reader["code"].ToString(),
                    reader["message"].ToString(),
                    (int)reader["severity"],
                    reader["IP"].ToString(),
                    reader["GUID"].ToString(),
                    (DateTime)reader["date"],
                    reader["comment"].ToString());


                evs.Add(ev);
            }
            cnn.Close();
            return evs;
        }
        //Known Host Methods
        public List<KnownHost> GetKnownHosts()
        {
            try
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
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    KnownHost kh = new KnownHost(
                        reader["MAC"].ToString(),
                        reader["IP"].ToString(),
                        reader["Version"].ToString(),
                        (DateTime)reader["DateAdded"],
                        reader["GUID"].ToString()
                        );
                    khs.Add(kh);
                }

                CloseCon();
                return khs;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.StackTrace);
                return new List<KnownHost>();
            }
        }

        public void AddKnownHost(KnownHost kh)
        {
            String q = "INSERT INTO dbo.Hosts (MAC, IP, Version, DateAdded, GUID) Values (@m, @i, @v, @da, @g)";
            SqlCommand comm = new SqlCommand(q, cnn);

            comm.Parameters.AddWithValue("m", kh.hostMAC);
            comm.Parameters.AddWithValue("i", kh.hostIP);
            comm.Parameters.AddWithValue("v", kh.version);
            comm.Parameters.AddWithValue("da", kh.dateAdded);
            comm.Parameters.AddWithValue("g", kh.GUID);

            comm.ExecuteNonQuery();
        }

        public bool CheckKnownHostDupe(KnownHost kh)
        {
            if (!OpenCon())
            {
                return true;
            }
            String q1 = "SELECT * FROM dbo.Hosts Where GUID = @guid";
            SqlParameter serialParam = new SqlParameter("guid", SqlDbType.VarChar);
            serialParam.Value = kh.GUID;

            SqlCommand comm = new SqlCommand(q1, cnn);

            comm.Parameters.Add(serialParam);

            var results = comm.ExecuteReader();

            while (results.Read())
            {
                if (results["guid"].ToString().Equals(kh.GUID))
                {
                    cnn.Close();
                    return true;
                }
            }
            return false;
        }

        public bool ClearHosts()
        {
            if (!OpenCon())
            {
                return false;
            }
            else
            {
                SqlCommand sql = new SqlCommand("Delete from dbo.Hosts", cnn);
                sql.ExecuteNonQuery();
                return true;
            }
        }

        //IP Methods
        public void AddIPs(List<string> IPs)
        {
            if (OpenCon())
            {
                using (SqlTransaction tr = cnn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand sql1 = new SqlCommand("Delete from CheckIPs", cnn);
                        sql1.Transaction = tr;
                        sql1.ExecuteNonQuery();

                        var dt = new DataTable();
                        dt.Columns.Add("IPAddress");
                        foreach (String IP in IPs)
                        {
                            dt.Rows.Add(IP);
                        }

                        using (var sqlBulk = new SqlBulkCopy(cnn, SqlBulkCopyOptions.KeepIdentity, tr))
                        {
                            sqlBulk.DestinationTableName = "CheckIPs";
                            sqlBulk.WriteToServer(dt);
                        }
                        tr.Commit();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Source);
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        tr.Rollback();
                    }
                }
            }
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
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ips.Add(reader["IPAddress"].ToString());
            }

            return ips;
        }

        //Service Methods

        public List<ServicesObject> GetServices(){
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
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                sos.Add(new ServicesObject(reader["IP"].ToString(), (int)reader["Port"], (int)reader["Preference"]));
            }

            return sos;
        }

        public void AddService(ServicesObject so, SqlTransaction sqlt)
        {
            if (OpenCon())
            {
                String q = "INSERT INTO dbo.Services (IP, Port, Preference) Values (@a, @b, @c)";
                SqlCommand comm = new SqlCommand(q, cnn);
                comm.Transaction = sqlt;
                comm.Parameters.AddWithValue("a", so.IPAddress);
                comm.Parameters.AddWithValue("b", so.port);
                comm.Parameters.AddWithValue("c", so.Preference);

                comm.ExecuteNonQuery();
            }
        }

        public void SetServices(List<ServicesObject> sos)
        {
            if (OpenCon())
            {
                using (SqlTransaction sqlt = cnn.BeginTransaction())
                {
                    String q = "DELETE FROM dbo.Services";
                    SqlCommand comm = new SqlCommand(q, cnn);
                    comm.Transaction = sqlt;
                    comm.ExecuteNonQuery();

                    foreach (ServicesObject so in sos)
                    {
                        AddService(so, sqlt);
                    }
                    sqlt.Commit();
                }
            }
        }

    }
}
