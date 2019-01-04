using DuckPond.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DuckPond.Resources;

namespace DuckPond.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (DuckPassword.verifyPassword(CurrentPasswordBox.Password))
            {
                if(Equals(NewPassword1Box.Password, NewPassword2Box.Password))
                {
                    if (DuckPassword.CheckPassword(NewPassword1Box.Password))
                    {
                        SQLiteClass sql = new SQLiteClass();
                        sql.ChangePassword(DuckPassword.PBKDF2HashPassword(NewPassword1Box.Password));

                        ErrorPassword.Content = "Success!";
                    }
                    else
                    {
                        ErrorPassword.Content = "Password too short";
                    }
                }
                else
                {
                    ErrorPassword.Content = "Passwords do not match";
                }
            }
            else
            {
                ErrorPassword.Content = "Incorrect Password";
            }
        }

        private void BtnChangeIPs_Click(object sender, RoutedEventArgs e)
        {
            String include = txtInclude.Text;
            String exclude = txtExclude.Text;

            List<String> ips = IPRange.RangeListStringToIPList(include);
            ips = IPRange.ExcludeFromListString(ips,exclude);

            foreach(String ip in ips)
            {
                Console.WriteLine(ip);
            }

            txtInclude.Text = "";
            txtExclude.Text = "";
        }
    }
}
