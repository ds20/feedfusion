using System;
using System.Collections.Generic;
using System.Text;
using System.Xml; 

namespace syncroniserFusion
{
    public class syncroniser:PluginInterface.rssInterface  
    {
        PluginInterface.DataBaseEngine data;
        public void setDocument(XmlDocument doc)
        { }

        public bool canParse()
        { 
            return false; 
        }



        public string parsedHTML()
        {
            return "";
        }

        public void showConfiguration()
        {
            Window1 wnd = new Window1();
            wnd.Show();
            

        }
        private void bt_Click(object Sender, System.Windows.RoutedEventArgs e)
        {
        }


        public void addToToolbar(System.Windows.Controls.ToolBar ToolBar)
        {
            System.Windows.Controls.Button btn = new System.Windows.Controls.Button();
            btn.Height = 21;
            btn.Width = 50;
            btn.Content = "Sync";
            btn.Click+= new System.Windows.RoutedEventHandler(btn_Click); 

            ToolBar.Items.Add(btn);  
        }
        private void btn_Click(object Sender, System.Windows.RoutedEventArgs e)
        {
            string[] cats = data.getCategories();
            System.Windows.Forms.MessageBox.Show(cats[0]);
        }

        public string description()
        {
            return "Store your rss feeds remortely on the FeedFusion servers.";
        }

        public string name()
        {
            return "Fusion Syncroniser";
        }

        public void getDataBase(PluginInterface.DataBaseEngine data)
        {
            this.data = data;
            string[] cats=data.getCategories();
            System.Windows.Forms.MessageBox.Show(cats[0]);
        }

    }
}
