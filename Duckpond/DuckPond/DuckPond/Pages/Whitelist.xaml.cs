﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using DuckPond.Models.Whitelists;
using DuckPond.Resources;
using Xceed.Wpf.Toolkit;

namespace DuckPond.Pages
{
    /// <summary>
    /// Interaction logic for Whitelist.xaml
    /// </summary>
    public partial class Whitelist : Page
    {
        public Whitelist()
        {
            InitializeComponent();
            DateCol.Binding = new Binding("Datetime");
            Pidcol.Binding = new Binding("Pid");
            VidCol.Binding = new Binding("Vid");
            SerialCol.Binding = new Binding("SerialNumber");
            IDCol.Binding = new Binding("WhitelistID");

            LoadTable();
        }

        public struct WLTableRow
        {
            public String Datetime { set; get; }
            public String Pid { set; get; }
            public String Vid { set; get; }
            public String SerialNumber { set; get; }
            public int WhitelistID { set; get; }
        }

        private void LoadTable()
        {
            MSSQL ms = new MSSQL();
            List<Whitelists> wls = ms.GetWhitelists();

            foreach (Whitelists wl in wls)
            {
                WhitelistTable.Items.Add(new WLTableRow { Datetime = wl.Datetime1.ToString("dd/MMM/yyyy hh:mm:ss tt"), Pid = wl.Pid1, Vid = wl.Vid1, SerialNumber = wl.SerialNumber1, WhitelistID = wl.WhitelistID1 });
            }
            ms.closeCon();
        }

        private void DoFilter()
        {
            List<Whitelists> wls = new List<Whitelists>();
            String aserial = "";
            String avid = "";
            String apid = "";
            DateTime adt1 = DateTime.Parse("1/1/2000 12:00:00 AM",
                          System.Globalization.CultureInfo.InvariantCulture);
            DateTime adt2 = DateTime.Now;
            if (FilterPid.Text.Equals("") && FilterVid.Text.Equals("") && FilterSerial.Text.Equals("") && FilterDate1.Value==null && FilterDate2.Value==null)
            {
                MSSQL ms = new MSSQL();
                List<Whitelists> wlss = ms.GetWhitelists();
                WhitelistTable.Items.Clear();


                foreach (Whitelists wl in wlss)
                {
                    WhitelistTable.Items.Add(new WLTableRow { Datetime = wl.Datetime1.ToString("dd/MMM/yyyy hh:mm:ss tt"), Pid = wl.Pid1, Vid = wl.Vid1, SerialNumber = wl.SerialNumber1, WhitelistID = wl.WhitelistID1 });
                }
                ms.closeCon();
            }
            else
            {
                if (!FilterPid.Text.Equals(""))
                {
                    apid = FilterPid.Text;
                }
                if (!FilterVid.Text.Equals(""))
                {
                    avid = FilterVid.Text;
                }
                if (!FilterSerial.Text.Equals(""))
                {
                    aserial = FilterSerial.Text;
                }
                if (!(FilterDate1.Value == null))
                {
                    adt1 = (DateTime)FilterDate1.Value;
                }
                if (!(FilterDate2.Value == null))
                {
                    adt2 = (DateTime)FilterDate2.Value;
                }

                MSSQL ms = new MSSQL();
                Console.WriteLine(apid+avid+aserial+adt1.ToString()+adt2.ToString());
                wls = ms.FilteredWhitelists(apid, avid, aserial, adt1, adt2);

                WhitelistTable.Items.Clear();

                foreach (Whitelists wl in wls)
                {
                    WhitelistTable.Items.Add(new WLTableRow { Datetime = wl.Datetime1.ToString("dd/MMM/yyyy hh:mm:ss tt"), Pid = wl.Pid1, Vid = wl.Vid1, SerialNumber = wl.SerialNumber1, WhitelistID = wl.WhitelistID1 });
                }

                ms.closeCon();
            }
        }

        private void BtnAddOne_Click(object sender, RoutedEventArgs e)
        {
            Whitelists wl;
            if (TextCleaner.CleanHexString(VidAddOne.Text).Length!=4)
            {
                VidAddOne.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }
            else if(TextCleaner.CleanHexString(PidAddOne.Text).Length!=4)
            {
                PidAddOne.BorderBrush = new SolidColorBrush(Colors.Red); 
            }
            else if(SerialAddOne.Text.Trim().Length > 255 || SerialAddOne.Text.Trim().Length ==0)
            {
                SerialAddOne.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                wl = new Whitelists(DateTime.Now, 
                    TextCleaner.CleanHexString(VidAddOne.Text),
                    TextCleaner.CleanHexString(PidAddOne.Text),
                    SerialAddOne.Text.Trim());

                MSSQL ms = new MSSQL();

                if (ms.AddWhitelist(wl)==0)
                {
                    //Set Success
                    VidAddOne.ClearValue(Border.BorderBrushProperty);
                    PidAddOne.ClearValue(Border.BorderBrushProperty);
                    SerialAddOne.ClearValue(Border.BorderBrushProperty);
                    VidAddOne.Text = "";
                    PidAddOne.Text = "";
                    SerialAddOne.Text = "";

                    MsgAddOne.Content = "Success!";
                    WhitelistTable.Items.Clear();
                    LoadTable();
                }
                else if (ms.AddWhitelist(wl)==1)
                {
                    MsgAddOne.Content = "Database Error";
                }
                else
                {
                    MsgAddOne.Content = "Unknown Error";
                }

                ms.closeCon();
            }
        }

        private void BtnFilteredSearch_Click(object sender, RoutedEventArgs e)
        {
            DoFilter();
        }
    }
}