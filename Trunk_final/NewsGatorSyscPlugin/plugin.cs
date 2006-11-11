using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System.IO;
using System.Xml;
using PluginInterface;
namespace NewsGatorSyscPlugin
{
    public class plugin:rssInterface  
    {
        Opml op;
        DataBaseEngine db;
        Window loginWindow;
        TextBox txtAccountName;
        TextBox txtAccountPassword;
        SubscriptionService.SubscriptionWebService s;
        string locName;
        #region rssInterface Members

        public void setOpml(Opml opml) 
        {
            op = opml;
        }  

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


        }

        void btnExport_Click(object sender, RoutedEventArgs e)
        {
            op.makeOpml(db); 
            //then send on the server for processing
        }

        void btnSync_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(txtAccountName.Text+ " "+txtAccountPassword.Text   );
            LocationService.LocationWebService ls = new LocationService.LocationWebService();
            ls.Credentials = new NetworkCredential(txtAccountName.Text, txtAccountPassword.Text);
            LocationService.Location[] locs = ls.GetLocations();
            locName=locs[0].name;
            s = new SubscriptionService.SubscriptionWebService();
            s.Credentials = new NetworkCredential(txtAccountName.Text, txtAccountPassword.Text);
            s.GetSubscriptionListCompleted += new NewsGatorSyscPlugin.SubscriptionService.GetSubscriptionListCompletedEventHandler(s_GetSubscriptionListCompleted);
            s.GetSubscriptionListAsync(locName, null);
 
        }

        void s_GetSubscriptionListCompleted(object sender, NewsGatorSyscPlugin.SubscriptionService.GetSubscriptionListCompletedEventArgs e)
        {
            if (e.Cancelled || (e.Error != null))
            {
                MessageBox.Show("An unexpected error ocurred. Operation might not have finished corectly.");
            }
            else
            {
                

                System.Xml.XmlTextWriter writer = new  System.Xml.XmlTextWriter(Path.GetTempPath()+ "\\opml.opml",null);
                e.Result.WriteTo(writer);
                writer.Flush();
                writer.Close();
                op.import(Path.GetTempPath() + "\\opml.opml", db);
                string opmlContent=op.makeOpml(db);
                System.Xml.XmlElement opmlNode = (new System.Xml.XmlDocument()).CreateElement("HH"); ;   

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(opmlContent);
                s.MergeSubscriptionsCompleted += new NewsGatorSyscPlugin.SubscriptionService.MergeSubscriptionsCompletedEventHandler(s_MergeSubscriptionsCompleted);
                s.MergeSubscriptionsAsync(locName, xmlDocument.DocumentElement , false); 
            }
        }

        void s_MergeSubscriptionsCompleted(object sender, NewsGatorSyscPlugin.SubscriptionService.MergeSubscriptionsCompletedEventArgs e)
        {
            if (e.Error != null) MessageBox.Show(e.Error.Message);
            else { MessageBox.Show("Good news! The sync operation was successfully completed."); }
        }

        void toolbtn_Click(object sender, RoutedEventArgs e)
        {
            loginWindow = new Window();
            loginWindow.Width = 280;
            loginWindow.Title = "Login";
            loginWindow.ResizeMode = ResizeMode.NoResize;    
            loginWindow.Height = 120;
            StackPanel sp = new StackPanel();
            WrapPanel wpAccountName = new WrapPanel();
            Label lblAccountName = new Label();
            lblAccountName.Margin = new Thickness(3);   
            lblAccountName.Content = "Account name:";
            lblAccountName.Width = 120;
            txtAccountName = new TextBox();
            txtAccountName.Margin = new Thickness(3);    
            txtAccountName.Width = 130;
            wpAccountName.Children.Add(lblAccountName);
            wpAccountName.Children.Add(txtAccountName);

            WrapPanel wpAccountPassword = new WrapPanel();
            Label lblAccountPassword = new Label();
            lblAccountPassword.Margin = new Thickness(3);    
            lblAccountPassword.Content = "Password";
            lblAccountPassword.Width = 120;
            txtAccountPassword = new TextBox();
            txtAccountPassword.Margin = new Thickness(3);    
            txtAccountPassword.Width = 130;
            wpAccountPassword.Children.Add(lblAccountPassword);
            wpAccountPassword.Children.Add(txtAccountPassword);

            Button btnImport = new Button();
            btnImport.Content = "Login";
            btnImport.Margin = new Thickness(3);    
            btnImport.Click += new RoutedEventHandler(btnSync_Click);
            StackPanel spMain = new StackPanel();
            spMain.Children.Add(wpAccountName);
            spMain.Children.Add(wpAccountPassword);
            spMain.Children.Add(btnImport);
            loginWindow.Content = spMain;
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
