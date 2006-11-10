using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PluginInterface;
namespace syncroniserFusion
{
    public class syncroniser:PluginInterface.rssInterface  
    {

        localhost1.Service srv;

        Window win;
        Button btnExport;
        Button btnImport;
        TextBox txtUserName;
        TextBox txtPassword;
        CheckBox chkCreateAccount;
        Window owner;
        System.Windows.Forms.NotifyIcon ico;



        public void setOpml(Opml opml) { }  
        public void loadWindow()
        {
            srv = new syncroniserFusion.localhost1.Service();
            win = new System.Windows.Window();
            win.Width = 200;
            win.Height = 200;

            Label lblUser = new Label();
            lblUser.Content = "Username:";

            txtUserName = new TextBox();
            txtUserName.Text = "FeedFusion"; 
            txtUserName.Width = 100;
            txtUserName.Height = 20;

            Label lblPassword = new Label();
            lblPassword.Content = "Password";

            txtPassword = new TextBox();
            txtPassword.Text = "root123"; 
            txtPassword.Width = 100;
            txtPassword.Height = 20;

            chkCreateAccount = new CheckBox();
            chkCreateAccount.Content = "New Account";
            chkCreateAccount.Click += new RoutedEventHandler(chkCreateAccount_Click);

            btnImport = new Button();
            btnImport.Content = "Import";
            btnImport.Visibility = Visibility.Visible;
            btnImport.Width = 100;
            btnImport.Height = 30;
            btnImport.Click += new RoutedEventHandler(btnImport_Click);

            btnExport = new Button();
            btnExport.Content = "Export";
            btnExport.Visibility = Visibility.Visible;
            btnExport.Width = 100;
            btnExport.Height = 30;
            btnExport.Click += new RoutedEventHandler(btnExport_Click);

            StackPanel rootPanel = new StackPanel();
            win.Content = rootPanel;
            rootPanel.Children.Add(lblUser);
            rootPanel.Children.Add(txtUserName);
            rootPanel.Children.Add(lblPassword);
            rootPanel.Children.Add(txtPassword);
            rootPanel.Children.Add(chkCreateAccount);
            rootPanel.Children.Add(btnImport);
            rootPanel.Children.Add(btnExport);
            win.Show();


        }

        void chkCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            btnImport.IsEnabled = !btnImport.IsEnabled;
        }

        //Export
        void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (true == chkCreateAccount.IsChecked)
            {
                int i = srv.NewUser(txtUserName.Text, txtPassword.Text, "text de opml");
                if (i > 0)
                {
                    MessageBox.Show("User sucessfully created.");
                }
                else
                {
                    MessageBox.Show("There was an error in the process. Probably the username already exists.");
                }
            }
            else
            {
                int i = srv.UpdateFeed(txtUserName.Text, txtPassword.Text, "text de opml nou");
                if (i > 0)
                {
                    MessageBox.Show("Sucessfully updated.");
                }
                else
                {
                    MessageBox.Show("There was an error in the process. Wrong username or password.");
                }
            }
        }

        //Import
        void btnImport_Click(object sender, RoutedEventArgs e)
        {
            string opml = srv.GetFeed(txtUserName.Text, txtPassword.Text);
            MessageBox.Show(opml);
        }

        public void feedChanged(string name, string category)
        { }

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
            loadWindow(); 
        }
        private void bt_Click(object Sender, System.Windows.RoutedEventArgs e)
        {
        }


        public void addToToolbar(System.Windows.Controls.ToolBar ToolBar)
        {
            System.Windows.Controls.Button btn = new System.Windows.Controls.Button();
            btn.ToolTip = "Opens FusionSyncroniser Plugin Configuration Window";
            
            Image im = new Image();
            im.Width = 30;
            im.Height = 30;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 30;
            bi.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\icons\\bomb.png");
            bi.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            im.Source = bi;


            btn.Content = im;
            btn.Click += new System.Windows.RoutedEventHandler(btn_Click);
            ToolBar.Items.Add(btn); 
        }
        private void btn_Click(object Sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                string[] cats = data.getCategories();
                System.Windows.Forms.MessageBox.Show(cats[0]);
            }
            catch { }
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


        #region rssInterface Members


        public void setOwner(Window window)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
