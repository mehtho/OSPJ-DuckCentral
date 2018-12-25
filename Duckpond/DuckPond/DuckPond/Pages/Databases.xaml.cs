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
                Console.WriteLine("Ran for a row");
                DataGridRow dataGridRow = (DataGridRow)DatabaseTable.ItemContainerGenerator.ContainerFromIndex(i);
                DBTableRow db = (DBTableRow)dataGridRow.Item;
                if (db.ConnectionString.Trim().Length != 0)
                {
                    dbos.Add(new DatabaseObject(db.ConnectionString,db.Preference));
                    Console.WriteLine("Trap");
                }
                else
                {
                    Console.WriteLine("Escape");
                }
            }

            dbos = dbos.OrderBy(x => x.Preference).ToList();
            DatabaseTable.ItemsSource = new List<DBTableRow>();
            clean = new List<DatabaseObject>();

            int a = 1;
            this.dtrs = new List<DBTableRow>();

            Console.WriteLine("101DBOCOUNT "+dbos.Count);

            foreach (DatabaseObject dbo in dbos)
            {  
                clean.Add(new DatabaseObject(dbo.ConnectionString,a));
                Console.WriteLine("ADDED TO CLEAN, NOW LONG AS " + clean.Count);
                dtrs.Add(new DBTableRow { ConnectionString = dbo.ConnectionString, Preference = a });

                DatabaseTable.ItemsSource = dtrs;
                DatabaseTable.Items.Refresh();

                a++;
            }

            return true;
        }

        public void Commit()
        {
            SQLiteClass sql = new SQLiteClass();
            Console.WriteLine("CLEANCOUNT "+clean.Count);
            sql.NewConnections(clean);

            sql.CloseCon();
        }
    }
}
