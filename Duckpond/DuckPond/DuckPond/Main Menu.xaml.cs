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
using DuckPond.Pages;

namespace DuckPond
{
    /// <summary>
    /// Interaction logic for Main_Menu.xaml
    /// </summary>
    public partial class Main_Menu : Page
    {
        public Main_Menu()
        {
            InitializeComponent();
        }

        private void BtnWhitelists_Click(object sender, RoutedEventArgs e)
        {
            Content_Frame.Navigate(new Whitelist());
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            Content_Frame.Navigate(new Settings());
        }

        private void BtnDeployment_Click(object sender, RoutedEventArgs e)
        {
            Content_Frame.Navigate(new Deployment());
        }

        private void BtnAlerts_Click(object sender, RoutedEventArgs e)
        {
            Content_Frame.Navigate(new Alerts());
        }

        private void BtnBackups_Click(object sender, RoutedEventArgs e)
        {
            Content_Frame.Navigate(new Backup());
        }

        private void BtnLookup_Click(object sender, RoutedEventArgs e)
        {
            Content_Frame.Navigate(new Lookup());
        }
    }
}
