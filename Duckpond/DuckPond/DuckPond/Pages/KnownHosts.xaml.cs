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
    /// Interaction logic for KnownHosts.xaml
    /// </summary>
    public partial class KnownHosts : Page
    {
        private List<KHTableRow> khrs;

        public KnownHosts()
        {
            InitializeComponent();

            ColDate.Binding = new Binding("DateAdded");
            ColGUID.Binding = new Binding("GUID");
            ColIP.Binding = new Binding("IP");
            ColMac.Binding = new Binding("MAC");
            ColVersion.Binding = new Binding("Version");
            ColStatus.Binding = new Binding("Status");

            KnownHostTable.CanUserAddRows = false;

            LoadTable();
        }

        private void BtnCloseAddHosts_Click(object sender, RoutedEventArgs e)
        {
            BtnCloseAddHosts.Visibility = Visibility.Hidden;
            AddHost_Frame.Visibility = Visibility.Hidden;
        }

        private void BtnAddHost_Click(object sender, RoutedEventArgs e)
        {
            BtnCloseAddHosts.Visibility = Visibility.Visible;
            AddHost_Frame.Visibility = Visibility.Visible;
        }

        private void BtnRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            BtnCloseAddHosts.Visibility = Visibility.Visible;
            BtnCloseAddHosts.Content = "Back";
            AddHost_Frame.Visibility = Visibility.Visible;
            AddHost_Frame.Navigate(new AreYouSure());
        }

        public struct KHTableRow
        {
            public DateTime dateAdded;
            public String GUID;
            public String IP;
            public String MAC;
            public String Version;
            public String Status;
        }


        private void LoadTable()
        {
            MSSQL ms = new MSSQL();
            List<KnownHost> khs = ms.GetKnownHosts();
            khrs = new List<KHTableRow>();

            //Integrate portscan results here

            foreach (KnownHost kh in khs)
            {
                khrs.Add(new KHTableRow { GUID=kh.GUID, dateAdded=kh.dateAdded, IP=kh.hostIP, MAC=kh.hostMAC, Status=KnownHost.ByteToString(kh.status), Version=""});
            }
            KnownHostTable.ItemsSource = khrs;
            KnownHostTable.Items.Refresh();
            ms.CloseCon();
        }
    }
}
