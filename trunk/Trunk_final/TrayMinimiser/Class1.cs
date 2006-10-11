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

        public void TrayMinimiser()
        {}


        public void feedChanged(string name, string category)
        { }

        private class Events : PluginInterface.EventsClass
        {
            public void FeedDownloaded(string feed) 
            {
                DiffuseDlgDemo.Notification nt = new DiffuseDlgDemo.Notification("Feedfuion has an incoming news article.");
                nt.Show();
            }
            public void NewFeedAdded(string feed) 
            {
                DiffuseDlgDemo.Notification nt = new DiffuseDlgDemo.Notification("A new news source has successfully been added to FeedFusion.");
                nt.Show();
            }
            public void CategoryAdded(string cat) 
            {
                DiffuseDlgDemo.Notification nt = new DiffuseDlgDemo.Notification("A new category has successfully been added to FeedFusion.");
                nt.Show(); 
            }
        }

        public void getDataBase(DataBaseEngine data)
        {

            data.RegisterEventHandler(new Events()); 
        }
  

        public void addToToolbar(System.Windows.Controls.ToolBar ToolBar)
        { 
        
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
                ico.Icon = new Icon(Environment.CurrentDirectory + @"\Icons\icon.ico");
                ico.Text = "FeedFusion RSS Reader is up and running"; 
                ico.Visible = true;
                ico.DoubleClick += new EventHandler(ico_DoubleClick);
            }
            else
            {
                if (ico != null)
                    ico.Visible = false;
                owner.ShowInTaskbar = true; 

                //ico = null;
            }

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
            
        }

        public void addToToolbar()
        { }

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
