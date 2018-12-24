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

namespace DuckPond.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        DatabaseObject dtb1;
        DatabaseObject dtb2;

        public Settings()
        {
            InitializeComponent();

            SQLiteClass sql = new SQLiteClass();

            dtb1 = sql.GetDatabase(1);
            if (dtb1.ConnectionString != null)
            {
                if (dtb1.ConnectionString.Length >= 10)
                    DBAddress1.Text = dtb1.ConnectionString.Substring(0, 10) + "...";
            }

            dtb2 = sql.GetDatabase(2);
            if (dtb2.ConnectionString != null)
            {
                if (dtb2.ConnectionString.Length >= 10)
                    DBAddress2.Text = dtb2.ConnectionString.Substring(0, 10) + "...";
            }

            sql.CloseCon();
        }

        private void BtnSetDB1_Click(object sender, RoutedEventArgs e)
        {
            if (DBAddress1.Text.Trim().Length!=0||!DBAddress1.Text.Equals(dtb1.ConnectionString.Substring(0, 10) + "..."))
            {
                DatabaseObject dtb = new DatabaseObject(DBAddress1.Text,1);

                SQLiteClass sql = new SQLiteClass();
                sql.SetDatabase(dtb);
                sql.CloseCon();

                ErrorDB1.Content = "Sucess!";
            }
            else
            {
                ErrorDB2.Content = "Please Enter a Connection String";
            }

        }

        private void BtnSetDB2_Click(object sender, RoutedEventArgs e)
        {
            if (DBAddress2.Text.Trim().Length != 0 || !DBAddress2.Text.Equals(dtb2.ConnectionString.Substring(0, 10) + "..."))
            {
                DatabaseObject dtb = new DatabaseObject(DBAddress2.Text,2);

                SQLiteClass sql = new SQLiteClass();
                sql.SetDatabase(dtb);
                sql.CloseCon();

                ErrorDB2.Content = "Success!";
            }
            else
            {
                ErrorDB2.Content = "Please Enter a Connection String";
            }
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
    }
}
