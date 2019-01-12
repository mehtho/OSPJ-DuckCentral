using DuckPond.Models;
using InstantMessenger;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;
using System.Xml.Serialization;

namespace DuckPond.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Databases : Page
    {
        private List<DatabaseObject> clean;
        private List<DBTableRow> dtrs;

        public Databases()
        {
            InitializeComponent();

            ColPref.Binding = new Binding("Preference");
            ColConn.Binding = new Binding("ConnectionString");

            DatabaseTable.CanUserAddRows = false;
            LoadTable();
        }

        public struct DBTableRow
        {
            public String ConnectionString { set; get; }
            public int Preference { set; get; }
        }


        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            dtrs.Add(new DBTableRow { ConnectionString = "", Preference = dtrs.Count + 1 });
            DatabaseTable.ItemsSource = dtrs;
            DatabaseTable.Items.Refresh();
        }

        private void LoadTable()
        {
            SQLiteClass sql = new SQLiteClass();
            List<DatabaseObject> dbos = sql.GetConnections();
            dtrs = new List<DBTableRow>();

            foreach (DatabaseObject dbo in dbos)
            {
                dtrs.Add(new DBTableRow {ConnectionString=dbo.ConnectionString,Preference=dbo.Preference });
            }
            DatabaseTable.ItemsSource = dtrs;
            DatabaseTable.Items.Refresh();
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
            List<DatabaseObject> dbos = new List<DatabaseObject>();

            for (int i = 0; i < dtrs.Count; i++)
            {
                DataGridRow dataGridRow = (DataGridRow)DatabaseTable.ItemContainerGenerator.ContainerFromIndex(i);
                DBTableRow db = (DBTableRow)dataGridRow.Item;
                if (db.ConnectionString.Trim().Length != 0)
                {
                    dbos.Add(new DatabaseObject(db.ConnectionString,db.Preference));
                }
            }

            dbos = dbos.OrderBy(x => x.Preference).ToList();
            DatabaseTable.ItemsSource = new List<DBTableRow>();
            clean = new List<DatabaseObject>();

            int a = 1;
            this.dtrs = new List<DBTableRow>();

            foreach (DatabaseObject dbo in dbos)
            {  
                clean.Add(new DatabaseObject(dbo.ConnectionString,a));
                dtrs.Add(new DBTableRow { ConnectionString = dbo.ConnectionString, Preference = a });

                DatabaseTable.ItemsSource = dtrs;
                DatabaseTable.Items.Refresh();

                a++;
            }

            return true;
        }

        public void Commit()
        {
            //Maybe make a receipt?
            SQLiteClass sql = new SQLiteClass();
            sql.NewConnections(clean);
            //Get all services and ports
            List<ServicesObject> svcs = sql.GetServices();

            foreach (ServicesObject so in svcs)
            {
                try
                {
                    IMClient im = new IMClient();
                    im.setConnParams(so.IPAddress, so.port);
                    im.SetupConn();

                    im.SendSignal((byte)IMClient.IM_NewDatabases, DoSerialize(clean));
                    im.Disconnect();
                }
                catch(System.Net.Sockets.SocketException)
                {

                }
            }

            sql.CloseCon();
        }

        private void BtnRemoveRow_Click(object sender, RoutedEventArgs e)
        {
            int rowNumber = DatabaseTable.SelectedIndex;
            dtrs.RemoveAt(rowNumber);
            DatabaseTable.Items.Refresh();
        }

        public static String DoSerialize(Object o)
        {
            XmlSerializer xsSubmit = new XmlSerializer(o.GetType());
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, o);
                    xml = sww.ToString(); // Your XML
                    return xml;
                }
            }
        }
    }
}
