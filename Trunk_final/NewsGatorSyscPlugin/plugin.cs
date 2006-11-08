using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System.IO;
using PluginInterface;
namespace NewsGatorSyscPlugin
{
    public class plugin:rssInterface  
    {
        DataBaseEngine db;
        Window loginWindow;
        TextBox txtAccountName;
        TextBox txtAccountPassword;
        #region rssInterface Members

        void rssInterface.setDocument(System.Xml.XmlDocument doc)
        {
            
        }

        bool rssInterface.canParse()
        {
            return false;
        }

        string rssInterface.parsedHTML()
        {
            return "";
        }

        void rssInterface.showConfiguration()
        {
            
        }

        void rssInterface.addToToolbar(System.Windows.Controls.ToolBar ToolBar)
        {
            System.Windows.Controls.Button toolbtn = new System.Windows.Controls.Button();
            toolbtn.ToolTip = "Sync with NewsGator account";

            Image im = new Image();
            im.Width = 30;
            im.Height = 30;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 30;
            bi.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\icons\\newsgator.png");
            bi.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            im.Source = bi;
            toolbtn.Content = im;
            toolbtn.Click += new RoutedEventHandler(toolbtn_Click);
            ToolBar.Items.Add(toolbtn);

            loginWindow = new Window();
            loginWindow.Width = 260;
            loginWindow.Height = 120; 
            StackPanel sp = new StackPanel();
            WrapPanel wpAccountName = new WrapPanel();
            Label lblAccountName = new Label();
            lblAccountName.Content = "Account name:";
            lblAccountName.Width = 120;  
            txtAccountName = new TextBox();
            txtAccountName.Width = 130;
            wpAccountName.Children.Add(lblAccountName);
            wpAccountName.Children.Add(txtAccountName);

            WrapPanel wpAccountPassword = new WrapPanel();
            Label lblAccountPassword = new Label();
            lblAccountPassword.Content="Password";
            lblAccountPassword.Width = 120;  
            txtAccountPassword = new TextBox();
            txtAccountPassword.Width = 130;
            wpAccountPassword.Children.Add(lblAccountPassword);
            wpAccountPassword.Children.Add(txtAccountPassword);

            Button btnSync = new Button();
            btnSync.Content = "Sync!";
            btnSync.Click += new RoutedEventHandler(btnSync_Click);  
            StackPanel spMain=new StackPanel();
            spMain.Children.Add(wpAccountName );
            spMain.Children.Add(wpAccountPassword );
            spMain.Children.Add(btnSync); 
            loginWindow.Content = spMain;
        }

        void btnSync_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(txtAccountName.Text+ " "+txtAccountPassword.Text   );
            LocationService.LocationWebService ls = new LocationService.LocationWebService();
            ls.Credentials = new NetworkCredential(txtAccountName.Text, txtAccountPassword.Text);
            LocationService.Location[] locs = ls.GetLocations();
            MessageBox.Show(locs[0].name);
            SubscriptionService.SubscriptionWebService s = new SubscriptionService.SubscriptionWebService();
            s.Credentials = new NetworkCredential(txtAccountName.Text, txtAccountPassword.Text);
            s.GetSubscriptionListCompleted += new NewsGatorSyscPlugin.SubscriptionService.GetSubscriptionListCompletedEventHandler(s_GetSubscriptionListCompleted);
            s.GetSubscriptionListAsync(locs[0].name, null);
        }

        void s_GetSubscriptionListCompleted(object sender, NewsGatorSyscPlugin.SubscriptionService.GetSubscriptionListCompletedEventArgs e)
        {
            if (e.Cancelled || (e.Error != null))
            {
                MessageBox.Show("An unexpected error ocurred. Operation might not have finished corectly.");
            }
            else
            {
                

                System.Xml.XmlTextWriter writer = new  System.Xml.XmlTextWriter(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\opml.opml",null);
                e.Result.WriteTo(writer);
                writer.Flush();
                writer.Close();  
            }
        }

        void toolbtn_Click(object sender, RoutedEventArgs e)
        {
            loginWindow.Show(); 
        }

        string rssInterface.description()
        {
            return "The Newsgator plugin allows syncronisation with the online rss reader NewsGator.";
        }

        string rssInterface.name()
        {
            return "Newsgator Sync Plugin";
        }

        void rssInterface.getDataBase(DataBaseEngine data)
        {
            db = data;
        }

        void rssInterface.setOwner(System.Windows.Window window)
        {
            
        }

        void rssInterface.feedChanged(string name, string category)
        {
           
        }

        #endregion
    }
}
