using DuckPond.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public partial class Services : Page
    {
        private List<ServicesObject> clean;
        private List<SVTableRow> svrs;

        public Services()
        {
            InitializeComponent();

            ColPref.Binding = new Binding("Preference");
            ColIP.Binding = new Binding("IPAddress");
            ColPort.Binding = new Binding("Port");

            ServicesTable.CanUserAddRows = false;
            LoadTable();
        }

        public struct SVTableRow
        {
            public String IPAddress { set; get; }
            public int Port { set; get; }
            public int Preference { set; get; }
        }


        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            svrs.Add(new SVTableRow { IPAddress = "", Port=25568, Preference = svrs.Count + 1 });
            ServicesTable.ItemsSource = svrs;
            ServicesTable.Items.Refresh();
        }

        private void LoadTable()
        {
            SQLiteClass sql = new SQLiteClass();
            List<ServicesObject> svos = sql.GetServices();
            svrs = new List<SVTableRow>();

            foreach (ServicesObject svo in svos)
            {
                svrs.Add(new SVTableRow {IPAddress=svo.IPAddress, Port=svo.port, Preference=svo.Preference });
            }
            ServicesTable.ItemsSource = svrs;
            ServicesTable.Items.Refresh();
            sql.CloseCon();
        }

        private void BtnCommit_Click(object sender, RoutedEventArgs e)
        {
            if (AuthTable())
            {
                Commit();
            }
            
        }

        private bool AuthTable()
        {
            List<ServicesObject> svos = new List<ServicesObject>();

            for (int i = 0; i < svrs.Count; i++)
            {
                DataGridRow dataGridRow = (DataGridRow)ServicesTable.ItemContainerGenerator.ContainerFromIndex(i);
                SVTableRow sv = (SVTableRow)dataGridRow.Item;

                if (sv.IPAddress.Trim().Length != 0)
                {
                    svos.Add(new ServicesObject(sv.IPAddress, sv.Port, sv.Preference));
                }
            }

            svos = svos.OrderBy(x => x.Preference).ToList();
            ServicesTable.ItemsSource = new List<SVTableRow>();
            clean = new List<ServicesObject>();

            int a = 1;
            this.svrs = new List<SVTableRow>();

            foreach (ServicesObject svo in svos)
            {
                String ip = KnownHost.ParseAsIP(svo.IPAddress);
                if (!ip.Equals("") && svo.port < 65536 && svo.port > 0)
                {

                    clean.Add(new ServicesObject(svo.IPAddress, svo.port, a));
                    svrs.Add(new SVTableRow { IPAddress = svo.IPAddress, Port = svo.port, Preference = a });

                    ServicesTable.ItemsSource = svrs;
                    ServicesTable.Items.Refresh();

                    a++;
                }
            }

            return true;
        }

        public void Commit()
        {
            SQLiteClass sql = new SQLiteClass();
            sql.NewServices(clean);

            sql.CloseCon();

            MSSQL ms = new MSSQL();
            ms.SetServices(clean);
        }

        private void BtnRemoveRow_Click(object sender, RoutedEventArgs e)
        {
            int rowNumber = ServicesTable.SelectedIndex;
            svrs.RemoveAt(rowNumber);
            ServicesTable.Items.Refresh();
        }
    }
}
