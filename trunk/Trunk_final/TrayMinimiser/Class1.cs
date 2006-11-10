using System;
using System.Collections.Generic;
using System.Text;
using PluginInterface;
using System.Xml;
using System.Windows; 
using System.Drawing;
namespace TrayMinimiser
{
    public class Tray:rssInterface
    {

        Window owner;
        System.Windows.Forms.NotifyIcon ico;
        System.Windows.Controls.Button b = new System.Windows.Controls.Button();
        System.Windows.Controls.CheckBox ck1 = new System.Windows.Controls.CheckBox();
        System.Windows.Controls.CheckBox ck2 = new System.Windows.Controls.CheckBox();
        System.Windows.Controls.CheckBox ck3 = new System.Windows.Controls.CheckBox();

        public void TrayMinimiser()
        {}
        public void setOpml(Opml opml) { }  

        public void feedChanged(string name, string category)
        { }
        public delegate void NoArgDelegate();
        public class Events : PluginInterface.EventsClass
        {
            NotificationWindow nDownload = new NotificationWindow("FeedFusion has downloaded a new article.");
            NotificationWindow nNewFeed = new NotificationWindow("A new feed has beed added to the FeedFusion database.");
            NotificationWindow nNewCat = new NotificationWindow("A new category has been added to the FeedFusion database."); 
            public void FeedDownloaded(string feed) 
            {
                nDownload.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(nDownload.Show1));
               
            }


            public void NewFeedAdded(string feed) 
            {
                nNewFeed.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(nNewFeed.Show1)); 
               
            }

 
            public void CategoryAdded(string cat) 
            {
                nNewCat.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(nNewCat.Show1)); 
            }
        }

        [System.STAThreadAttribute] 
        public void getDataBase(DataBaseEngine data)
        {

            data.RegisterEventHandler(new Events()); 
        }
  

        public void addToToolbar(System.Windows.Controls.ToolBar ToolBar)
        {

            System.Windows.Controls.Image myImage = new System.Windows.Controls.Image();
            myImage.Width = 30;
            myImage.Height = 30; 
            // Create source
            System.Windows.Media.Imaging.BitmapImage myBitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri( System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\downarrow.png");
            myBitmapImage.DecodePixelWidth = 30;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            b.Content = myImage; 
            b.ToolTip = "Configures trayMinimiser plugin options.";
            b.Click += new RoutedEventHandler(b_Click);
            ToolBar.Items.Add(b); 
        }

        void b_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This function is not curently implemented", "Info", MessageBoxButton.OK, MessageBoxImage.Stop);      
        }

        public void setOwner(System.Windows.Window window)
        { 
            owner = window;
            window.StateChanged += new EventHandler(window_StateChanged);

        }

        void window_StateChanged(object sender, EventArgs e)
        {

            if (owner.WindowState == WindowState.Minimized)
            {
                //MessageBox.Show("Minimised");
                ico = new System.Windows.Forms.NotifyIcon();
                owner.ShowInTaskbar = false; 
                ico.Icon = new Icon( System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Icons\icon.ico");
                ico.Text = "FeedFusion RSS Reader is up and running"; 
                ico.Visible = true;
                ico.DoubleClick += new EventHandler(ico_DoubleClick);

                System.Windows.Forms.ContextMenu contextMenu1;
                contextMenu1 = new System.Windows.Forms.ContextMenu();
                System.Windows.Forms.MenuItem menuItem1;
                menuItem1 = new System.Windows.Forms.MenuItem();
                System.Windows.Forms.MenuItem menuItem2;
                menuItem2 = new System.Windows.Forms.MenuItem();

                contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { menuItem1, menuItem2 });
                menuItem1.Index = 0;
                menuItem1.Text = "Restore";
                menuItem1.Click += new EventHandler(ico_DoubleClick);
                menuItem2.Index = 1;
                menuItem2.Text = "Close";
                menuItem2.Click += new EventHandler(Close_Click);

                ico.ContextMenu = contextMenu1; 

            }
            else
            {
                if (ico != null)
                    ico.Visible = false;
                owner.ShowInTaskbar = true; 

                //ico = null;
            }

        }

        void Close_Click(object sender, EventArgs e)
        {
            owner.Close();
        }

        void ico_DoubleClick(object sender, EventArgs e)
        {
            owner.WindowState = WindowState.Normal;
        }

        public bool canParse()
        {
            return false;
        }

        public void setDocument(XmlDocument doc)
        {
           
        }

  



        public string parsedHTML()
        {
         
            return "";
        }

        public void showConfiguration()
        {
            System.Windows.Window w = new Window();
            System.Windows.Controls.Canvas c=new System.Windows.Controls.Canvas();
            w.Content = c;
            ck1.Content = "Alert when adding a new category.";
            ck2.Content = "Alert when adding a new feed";
            ck3.Content = "Alert when received news article."; 
            ck1.IsChecked = true;
            ck2.IsChecked = true;
            ck3.IsChecked = true;
 
        }


        public string description()
        {
            return "Offers minimise to tray and notifications functionality.";
        }

        public string name()
        {
            return "TrayMinimiser";
        }



    }
}
