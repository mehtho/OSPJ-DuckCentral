using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckPond
{
    class Debug
    {
        //Decrypt
        
        public static void Decrypt()
        {
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source=data.db;Version=3;Password=1CF01FBFAA598E96241D4A8D2802E3B39899E34A2B61BC3BEFEEECDCD592A58C4A8E20D54222F9849CE6FEBC2A4CD64E13CE02DAB71CFE4EF7655CF72A28FF06;");
            m_dbConnection.Open();
            m_dbConnection.ChangePassword("");
        }
        

        //Encrypt
        public static void Encrypt()
        {
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source=data.db;Version=3;");
            m_dbConnection.Open();
            m_dbConnection.ChangePassword("1CF01FBFAA598E96241D4A8D2802E3B39899E34A2B61BC3BEFEEECDCD592A58C4A8E20D54222F9849CE6FEBC2A4CD64E13CE02DAB71CFE4EF7655CF72A28FF06"); 
        }
        
    }
}
