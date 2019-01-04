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
    /// Interaction logic for AddHost.xaml
    /// </summary>
    public partial class AreYouSure : Page
    {
        public AreYouSure()
        {
            InitializeComponent();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            MSSQL ms = new MSSQL();
            if (ms.ClearHosts())
            {
                BtnYes.Visibility = Visibility.Hidden;
                MessageLabel.Content = "Success";
            }
            else
            {
                BtnYes.Visibility = Visibility.Hidden;
                MessageLabel.Content = "An error occured.";
            }
        }
    }
}
