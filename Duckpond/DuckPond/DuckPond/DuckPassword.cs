using System;
using System.Text;
using SimpleCrypto;

namespace DuckPond
{

    class DuckPassword
    {
        public static UserData ud;

        public static Boolean DoLogin(string username, string password)
        {
            string key = PBKDF2HashPassword(password);
            //HASHED 'password' 1CF01FBFAA598E96241D4A8D2802E3B39899E34A2B61BC3BEFEEECDCD592A58C4A8E20D54222F9849CE6FEBC2A4CD64E13CE02DAB71CFE4EF7655CF72A28FF06
            SQLiteClass sql = new SQLiteClass(key);
            
            if (sql.GetUsernameMatch(username))
            {
                sql.CloseCon();
                ud = new UserData(username, key);
                return true;
            }
            else
            {
                sql.CloseCon();
                return false;
            }
        }

        public static String PBKDF2HashPassword(String password)
        {
            ICryptoService PBKDF2 = new PBKDF2();
            PBKDF2.HashIterations = 100000;

            string salt = "100000.1BCVuzABKz9fnqj/b86+4a7iJ8B0XrRiLPZ6UWCTG6MMPA==";

            byte[] PasswordHash = Convert.FromBase64String(PBKDF2.Compute(password, salt));

            string key = BitConverter.ToString(PasswordHash).Replace("-", string.Empty);

            return key;
        }

        public static bool verifyPassword(String password)
        {
            string key = PBKDF2HashPassword(password);
            //HASHED 'password' 1CF01FBFAA598E96241D4A8D2802E3B39899E34A2B61BC3BEFEEECDCD592A58C4A8E20D54222F9849CE6FEBC2A4CD64E13CE02DAB71CFE4EF7655CF72A28FF06
            if (Equals(key, ud.getPassword()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CheckPassword(String password)
        {
            string pwd = password.Trim();
            if (pwd.Length >= 8)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
