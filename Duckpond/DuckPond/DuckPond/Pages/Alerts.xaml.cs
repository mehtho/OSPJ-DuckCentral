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
    /// Interaction logic for Alerts.xaml
    /// </summary>
    public partial class Alerts : Page
    {
        public Alerts()
        {
            InitializeComponent();

            ColSeverity.Binding = new Binding("Severity");
            ColMessage.Binding = new Binding("Message");
            ColGUID.Binding = new Binding("GUID");
            ColIP.Binding = new Binding("IP");
            ColCode.Binding = new Binding("Code");
            ColDate.Binding = new Binding("Date");

            loadTable();
        }

        public struct EVTableRow
        {
            public String Date { set; get; }
            public String Code { set; get; }
            public String Message { set; get; }
            public String GUID { set; get; }
            public String IP { set; get; }
            public String Severity { set; get; }
        }

        private void loadTable()
        {
            MSSQL ms = new MSSQL();
            List<Events> evs = ms.GetEvents();

            foreach (Events ev in evs)
            {
                String el = "";
                switch (ev.eventLevel)
                {
                    case 0:
                        el = "0: Normal";
                        break;
                    case 1:
                        el = "1: Concern";
                        break;
                    case 2:
                        el = "2: Warning";
                        break;
                    case 3:
                        el = "3: Critical";
                        break;
                }
                EventsTable.Items.Add(new EVTableRow { Code = ev.eventCode, Date = ev.eventDate.ToString("dd/MMM/yyyy hh:mm:ss tt"), Message = ev.eventMessage, IP = ev.eventIP, GUID = ev.eventGUID, Severity = el});
            }
            ms.CloseCon();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            DoFilter();
        }

        private void DoFilter()
        {
            List<Events> wls = new List<Events>();
            String acode = "";
            String amessage = "";
            String aip = "";
            String aguid = "";
            DateTime adt1 = DateTime.Parse("1/1/2000 12:00:00 AM",
                          System.Globalization.CultureInfo.InvariantCulture);
            DateTime adt2 = DateTime.Now;
            if (FilterAlertCode.Text.Equals("") && FilterMessage.Text.Equals("") && FilterIP.Text.Equals("") && FilterGUID.Text.Equals("") 
                && FilterDate1.Value == null && FilterDate2.Value == null 
                && (bool)BtnL0.IsChecked && (bool)BtnL1.IsChecked && (bool)BtnL2.IsChecked && (bool)BtnL3.IsChecked)
            {
                MSSQL ms = new MSSQL();
                List<Events> evss = ms.GetEvents();

                foreach (Events ev in evss)
                {
                    String el = "";
                    switch (ev.eventLevel)
                    {
                        case 0:
                            el = "0: Normal";
                            break;
                        case 1:
                            el = "1: Concern";
                            break;
                        case 2:
                            el = "2: Warning";
                            break;
                        case 3:
                            el = "3: Critical";
                            break;
                    }
                    EventsTable.Items.Clear();
                    EventsTable.Items.Add(new EVTableRow { Code = ev.eventCode, Date = ev.eventDate.ToString("dd/MMM/yyyy hh:mm:ss tt"), Message = ev.eventMessage, IP = ev.eventIP, GUID = ev.eventGUID, Severity = el });
                }
                ms.CloseCon();
            }
            else
            {
                if (!FilterAlertCode.Text.Equals(""))
                {
                    acode = FilterAlertCode.Text;
                }
                if (!FilterMessage.Text.Equals(""))
                {
                    amessage = FilterMessage.Text;
                }
                if (!FilterIP.Text.Equals(""))
                {
                    aip = FilterIP.Text;
                }
                if (!FilterGUID.Text.Equals(""))
                {
                    aguid = FilterGUID.Text;
                }
                if (!(FilterDate1.Value == null))
                {
                    adt1 = (DateTime)FilterDate1.Value;
                }
                if (!(FilterDate2.Value == null))
                {
                    adt2 = (DateTime)FilterDate2.Value;
                }

                List<int> alevels = new List<int>();

                if ((bool)BtnL0.IsChecked)
                {
                    alevels.Add(0);
                }
                if ((bool)BtnL1.IsChecked)
                {
                    alevels.Add(1);
                }
                if ((bool)BtnL2.IsChecked)
                {
                    alevels.Add(2);
                }
                if ((bool)BtnL3.IsChecked)
                {
                    alevels.Add(3);
                }

                MSSQL ms = new MSSQL();
                List<Events> evss = ms.FilteredEvents(acode, amessage, alevels.ToArray(), aip, aguid, adt1, adt2);

                EventsTable.Items.Clear();

                foreach (Events ev in evss)
                {
                    String el = "";
                    switch (ev.eventLevel)
                    {
                        case 0:
                            el = "0: Normal";
                            break;
                        case 1:
                            el = "1: Concern";
                            break;
                        case 2:
                            el = "2: Warning";
                            break;
                        case 3:
                            el = "3: Critical";
                            break;
                    }
                    EventsTable.Items.Add(new EVTableRow { Code = ev.eventCode, Date = ev.eventDate.ToString("dd/MMM/yyyy hh:mm:ss tt"), Message = ev.eventMessage, IP = ev.eventIP, GUID = ev.eventGUID, Severity = el });
                }

                ms.CloseCon();
            }
        }
    }
}
