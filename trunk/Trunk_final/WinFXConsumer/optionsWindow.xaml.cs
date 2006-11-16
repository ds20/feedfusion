using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WinFXConsumer
{
    /// <summary>
    /// Interaction logic for optionsWindow.xaml
    /// </summary>

    public partial class optionsWindow : System.Windows.Window
    {
        Indexer.FeedDB db;  
        public optionsWindow(Indexer.FeedDB db):this() 
        {
            this.db = db;
        } 

        public optionsWindow()
        {
            InitializeComponent();
            btndelete.Click += new RoutedEventHandler(btndelete_Click);  
            txtInterval.Text = Properties.Settings.Default.autorefreshInterval.ToString(); 
            btnApply.Click += new RoutedEventHandler(btnApply_Click);
            chkDelete.IsChecked = Properties.Settings.Default.deleteHistoryOnExit;
            chkDelete.Checked += new RoutedEventHandler(chkDelete_Checked);
            openWindows.IsChecked = Properties.Settings.Default.openLinksInNewWindows;
            openWindows.Checked += new RoutedEventHandler(openWindows_Checked);
        }

        void openWindows_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.openLinksInNewWindows = (bool)openWindows.IsChecked;
            Properties.Settings.Default.Save();
        }

        void btndelete_Click(object sender, RoutedEventArgs e)
        {
            db.removeAllHistory(); 
        }

        void chkDelete_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.deleteHistoryOnExit = (bool)chkDelete.IsChecked;
            Properties.Settings.Default.Save(); 
        }

        void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.autorefreshInterval != uint.Parse(txtInterval.Text))
            {
                Properties.Settings.Default.autorefreshInterval = uint.Parse(txtInterval.Text );
                Properties.Settings.Default.Save();
                MessageBox.Show("The feed autorefresh interval will change the next time you start feedfusion.","Settings changed",MessageBoxButton.OK,MessageBoxImage.Information);
                this.Close(); 
            }
        }

    }
}