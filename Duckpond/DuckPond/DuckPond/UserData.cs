using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckPond
{
    class UserData
    {
        private String username;
        private String password;

        //Debug Constructor, replace later
        public UserData(String username, String password)
        {
            this.username = username;
            this.password = password;
        }

        public String getUsername()
        {
            return username;
        }

        public String getPassword()
        {
            return password;
        }

        public void setPassword(string pass)
        {
            this.password = pass;
        }
    }
}
