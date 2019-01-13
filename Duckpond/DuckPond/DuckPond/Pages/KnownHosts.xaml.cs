using DuckPond.Models;
using DuckPond.Resources;
using InstantMessenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public static int count = 0;
        public static List<IPPlusStatus> IPPS;

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

        public class KHTableRow
        {
            public String DateAdded { set; get; }
            public String GUID { set; get; }
            public String IP { set; get; }
            public String MAC { set; get; }
            public String Version { set; get; }
            public String Status { set; get; }
        }

        private static List<GUIDMACVersionIP> GetActiveHosts()
        {
            MSSQL ms = new MSSQL();
            List<String> ips = ms.GetIPs();
            count = ips.Count();
            List <GUIDMACVersionIP> gmvis = new List<GUIDMACVersionIP>();

            IPPS = new List<IPPlusStatus>();

            List<Thread> threads = new List<Thread>();
            foreach (String ip in ips)
            {
                Thread th = new Thread(() => HostScan.run(ip, 25567));
                th.Start();
                threads.Add(th);
            }

            foreach (Thread th in threads)
            {
                th.Join();
            }

            if (count != 0)
            {
                Console.WriteLine("Testing waiting");
                Thread.Sleep(500);
            }

            //The response of every ping
            foreach (IPPlusStatus iss in IPPS)
            {
                if (iss.Status == KnownHost.STATE_ONLINE)
                {
                    try
                    {
                        IMClient iMC = new IMClient();
                        iMC.setConnParams(iss.IP, 25567);

                        iMC.SetupConn();
                        string guid = iMC.RequestParam(IMClient.IM_GetIdentity);
                        iMC.CloseConn();
                        iMC.SetupConn();

                        string version = iMC.RequestParam(IMClient.IM_GetVersion);
                        iMC.CloseConn();
                        iMC.SetupConn();

                        string mac = iMC.RequestParam(IMClient.IM_GetMAC);

                        GUIDMACVersionIP gmvi = new GUIDMACVersionIP
                        {
                            GUID = guid,
                            IP = iss.IP,
                            MAC = mac,
                            Version = version
                        };
                        gmvis.Add(gmvi);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return gmvis;
        }

        private struct GUIDMACVersionIP
        {
            public String GUID;
            public String MAC;
            public String Version;
            public String IP;
        }

        private void LoadTable()
        {
            MSSQL ms = new MSSQL();
            List<KnownHost> khs = ms.GetKnownHosts();
            List<String> ips = ms.GetIPs();
            khrs = new List<KHTableRow>();

            List<GUIDMACVersionIP> gmvis = GetActiveHosts();

            foreach(KnownHost kh in khs)
            {
                bool found = false;
                foreach (GUIDMACVersionIP gmvi in gmvis)
                {
                    if (kh.GUID.Trim().Equals(gmvi.GUID.Trim()))
                    {
                        khrs.Add(new KHTableRow { GUID = kh.GUID, DateAdded = kh.dateAdded.ToString(), IP = gmvi.IP, MAC = gmvi.MAC, Status = KnownHost.ByteToString(KnownHost.STATE_ONLINE), Version = gmvi.Version });
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    khrs.Add(new KHTableRow { GUID = kh.GUID, DateAdded = kh.dateAdded.ToString(), IP = kh.hostIP, MAC = kh.hostMAC, Status = KnownHost.ByteToString(kh.status), Version = kh.version });
                }
            }
            KnownHostTable.ItemsSource = khrs;
            KnownHostTable.Items.Refresh();
            ms.CloseCon();

            LastUpdated.Content = DateTime.Now;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTable();
        }
    }
}
