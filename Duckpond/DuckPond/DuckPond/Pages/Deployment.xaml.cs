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
    /// Interaction logic for Deployment.xaml
    /// </summary>
    public partial class Deployment : Page
    {
        public Deployment()
        {
            InitializeComponent();
        }

        private void BtnKnownHosts_Click(object sender, RoutedEventArgs e)
        {
            Content_Frame.Navigate(new KnownHosts());
        }
    }
}
