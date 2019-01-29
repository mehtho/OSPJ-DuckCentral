using DuckPond.Resources;
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
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.SqlServer.Management.Smo;

namespace DuckPond.Pages
{
    /// <summary>
    /// Interaction logic for Backup.xaml
    /// </summary>
    public partial class Backup : Page
    {
        SQLiteClass sql;

        public Backup()
        {
            sql = new SQLiteClass();
            InitializeComponent();
            LblLocation.Content = sql.GetBackupLocation();
        }

        private void BtnSetLocation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result==CommonFileDialogResult.Ok)
            {
                sql.SetBackupLocation(dialog.FileName);
                LblLocation.Content = dialog.FileName;
            }
        }

        private void BtnDoBackup_Click(object sender, RoutedEventArgs e)
        {
            DatabaseBackup dtbk = new DatabaseBackup();
            try
            {
                dtbk.DoBackup();
            }
            catch (FailedOperationException)
            {
                LblError.Content = "Cannot backup Azure Database, use Azure portal instead.";
            }
        }
    }
}
