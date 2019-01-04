using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DuckPond
{
    /// <summary>
    /// Interaction logic for LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        public LoginScreen()
        {
            InitializeComponent();
            this.Title = "Duck Pond";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Username = UsernameField.Text.Trim();
            string Password = PasswordField.Password;
            if (Username.Length > 0 && Password.Length > 0)
            {
                if (DuckPassword.DoLogin(Username, Password))
                {
                    Login.Navigate(new Main_Menu());
                }
                else
                {           
                    this.LoginFailText.Content = "Invalid Login Details, Try Again";
                }
            }
        }
    }
}
