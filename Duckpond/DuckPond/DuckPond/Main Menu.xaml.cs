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
        private List<Button> sidebuttons;
        public Main_Menu()
        {
            sidebuttons = new List<Button>();
            InitializeComponent();
            sidebuttons.Add(BtnAlerts);
            sidebuttons.Add(BtnBackups);
            sidebuttons.Add(BtnDatabases);
            sidebuttons.Add(BtnDeployment);
            sidebuttons.Add(BtnKnownHosts);
            sidebuttons.Add(BtnServices);
            sidebuttons.Add(BtnWhitelists);
            sidebuttons.Add(BtnSettings);
        }

        private void BtnWhitelists_Click(object sender, RoutedEventArgs e)
        {
            HighlightButtons(BtnWhitelists);
            Content_Frame.Navigate(new Whitelist());
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            HighlightButtons(BtnSettings);
            Content_Frame.Navigate(new Settings());
        }

        private void BtnDeployment_Click(object sender, RoutedEventArgs e)
        {
            HighlightButtons(BtnDeployment);
            Content_Frame.Navigate(new Deployment());
        }

        private void BtnAlerts_Click(object sender, RoutedEventArgs e)
        {
            HighlightButtons(BtnAlerts);
            Content_Frame.Navigate(new Alerts());
        }

        private void BtnBackups_Click(object sender, RoutedEventArgs e)
        {
            HighlightButtons(BtnBackups);
            Content_Frame.Navigate(new Backup());
        }

        private void BtnDatabases_Click(object sender, RoutedEventArgs e)
        {
            HighlightButtons(BtnDatabases);
            Content_Frame.Navigate(new Databases());
        }

        private void BtnService_Click(object sender, RoutedEventArgs e)
        {
            HighlightButtons(BtnServices);
            Content_Frame.Navigate(new Services());
        }

        private void BtnKnownHosts_Click(object sender, RoutedEventArgs e)
        {
            HighlightButtons(BtnKnownHosts);
            Content_Frame.Navigate(new KnownHosts());
        }

        private void HighlightButtons(Button pushed)
        {
            BrushConverter bc = new BrushConverter();
            foreach(Button b in sidebuttons)
            {
                b.Background = (Brush)bc.ConvertFrom("#FFFFDB58");
            }
            pushed.Background = Brushes.Orange;
        }
    }
}
