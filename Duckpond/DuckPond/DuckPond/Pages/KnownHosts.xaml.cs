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
        public static int max = 0;
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


        private void LoadTable()
        {
            MSSQL ms = new MSSQL();
            List<KnownHost> khs = ms.GetKnownHosts();
            List<String> ips = ms.GetIPs();
            khrs = new List<KHTableRow>();

            foreach (KnownHost kh in khs)
            {
                khrs.Add(new KHTableRow { GUID = kh.GUID, DateAdded = kh.dateAdded.ToString(), IP = kh.hostIP, MAC = kh.hostMAC, Status = KnownHost.ByteToString(kh.status), Version = "" });
            }


            //Integrate portscan results here
            IPPS = new List<IPPlusStatus>();
            count = 0;
            max = ips.Count;

            foreach(String ip in ips)
            {
                //Make a thread that sends the signal and returns the reply for the next step
                //RUN
                IPPlusStatus ipps = new IPPlusStatus {IP="0.0.0.0",Status=KnownHost.STATE_UNKNOWN };

                Thread t = new Thread(() => ipps = HostScan.run(ip, 25567) );
                t.Start();
                t.Join();
                if (!ipps.IP.Equals("0.0.0.0"))
                {
                    Console.WriteLine("Added an ipps: "+ipps.IP+KnownHost.ByteToString(ipps.Status));
                    IPPS.Add(ipps);
                }
                
                count++;
                Console.WriteLine(count);
                lblLoaded.Content = "Loaded: "+count+" out of "+max;
                if (count == max)
                {
                    Console.WriteLine("MAX COUNT");
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
                                iMC.Disconnect();
                                iMC.SetupConn();
                                string version = iMC.RequestParam(IMClient.IM_GetVersion);

                                Console.WriteLine("KHRS "+khrs.Count);

                                foreach (KHTableRow khr in khrs)
                                {
                                    Console.WriteLine("l1 "+khr.GUID +":"+guid +"/"+(khr.GUID.Trim().Equals(guid)));
                                    if (khr.GUID.Trim().Equals(guid))
                                    {
                                        Console.WriteLine("l2");
                                        khr.Status = KnownHost.ByteToString(KnownHost.STATE_ONLINE);
                                        khr.Version = version;
                                    }
                                }
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine(e.Message);
                                Console.WriteLine(e.Source);
                                Console.WriteLine(e.StackTrace);
                            }
                        }
                    }
                }
            }
            Console.Write("Finished the procedure");
            KnownHostTable.ItemsSource = khrs;
            KnownHostTable.Items.Refresh();
            ms.CloseCon();
        }
    }
}
