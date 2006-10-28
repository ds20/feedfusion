using System;
using System.Collections.Generic;
using System.Text;
using PluginInterface;
using System.Xml;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Shell32;
using Win32;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using System.Collections;



namespace ClipboardMonitor
{
    public class Monitor: System.Windows.Forms.Form ,rssInterface 
    {
        System.Windows.Controls.Button b = new System.Windows.Controls.Button();  

        public  Monitor()
        { 
            RegisterClipboardViewer();
            
        }
        DataBaseEngine data;
        System.Windows.Window win;
        System.Windows.Controls.TextBox txtPassword;
        System.Windows.Controls.Label lblUser;
        System.Windows.Controls.Button btnAdd;
        System.Windows.Controls.ListBox listCat; 

        string[] formatsAll = new string[] 
		{
			DataFormats.Bitmap,
			DataFormats.CommaSeparatedValue,
			DataFormats.Dib,
			DataFormats.Dif,
			DataFormats.EnhancedMetafile,
			DataFormats.FileDrop,
			DataFormats.Html,
			DataFormats.Locale,
			DataFormats.MetafilePict,
			DataFormats.OemText,
			DataFormats.Palette,
			DataFormats.PenData,
			DataFormats.Riff,
			DataFormats.Rtf,
			DataFormats.Serializable,
			DataFormats.StringFormat,
			DataFormats.SymbolicLink,
			DataFormats.Text,
			DataFormats.Tiff,
			DataFormats.UnicodeText,
			DataFormats.WaveAudio
		};

        string[] formatsAllDesc = new String[] 
		{
			"Bitmap",
			"CommaSeparatedValue",
			"Dib",
			"Dif",
			"EnhancedMetafile",
			"FileDrop",
			"Html",
			"Locale",
			"MetafilePict",
			"OemText",
			"Palette",
			"PenData",
			"Riff",
			"Rtf",
			"Serializable",
			"StringFormat",
			"SymbolicLink",
			"Text",
			"Tiff",
			"UnicodeText",
			"WaveAudio"
		};

        IntPtr _ClipboardViewerNext;



        private void RegisterClipboardViewer()
        {
            _ClipboardViewerNext = Win32.User32.SetClipboardViewer(this.Handle);
        }




        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            switch ((Win32.Msgs)m.Msg)
            {
                //
                // The WM_DRAWCLIPBOARD message is sent to the first window 
                // in the clipboard viewer chain when the content of the 
                // clipboard changes. This enables a clipboard viewer 
                // window to display the new content of the clipboard. 
                //
                case Win32.Msgs.WM_DRAWCLIPBOARD:


                    GetClipboardData();
                    //
                    // Each window that receives the WM_DRAWCLIPBOARD message 
                    // must call the SendMessage function to pass the message 
                    // on to the next window in the clipboard viewer chain.
                    //
                    Win32.User32.SendMessage(_ClipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    break;


                //
                // The WM_CHANGECBCHAIN message is sent to the first window 
                // in the clipboard viewer chain when a window is being 
                // removed from the chain. 
                //
                case Win32.Msgs.WM_CHANGECBCHAIN:


                    // When a clipboard viewer window receives the WM_CHANGECBCHAIN message, 
                    // it should call the SendMessage function to pass the message to the 
                    // next window in the chain, unless the next window is the window 
                    // being removed. In this case, the clipboard viewer should save 
                    // the handle specified by the lParam parameter as the next window in the chain. 

                    //
                    // wParam is the Handle to the window being removed from 
                    // the clipboard viewer chain 
                    // lParam is the Handle to the next window in the chain 
                    // following the window being removed. 
                    if (m.WParam == _ClipboardViewerNext)
                    {
                        //
                        // If wParam is the next clipboard viewer then it
                        // is being removed so update pointer to the next
                        // window in the clipboard chain
                        //
                        _ClipboardViewerNext = m.LParam;
                    }
                    else
                    {
                        Win32.User32.SendMessage(_ClipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    }
                    break;

                default:
                    //
                    // Let the form process the messages that we are
                    // not interested in
                    //
                    base.WndProc(ref m);
                    break;

            }
        }



        private void GetClipboardData()
        {
            //
            // Data on the clipboard uses the 
            // IDataObject interface
            //
            System.Windows.Forms.IDataObject iData = new System.Windows.Forms.DataObject();
            string strText = "clipmon";

            try
            {
                iData = Clipboard.GetDataObject();
            }
            catch (System.Runtime.InteropServices.ExternalException externEx)
            {
                // Copying a field definition in Access 2002 causes this sometimes?
 
                return;
            }
            catch (Exception ex)
            {
                return;
            }


            if (iData.GetDataPresent(DataFormats.Text))
            {

            }

            if (ClipboardSearch(iData))
            {

            }

        }

        private bool ClipboardSearch(System.Windows.Forms.IDataObject iData)
        {
            bool FoundNewLinks = false;
            //
            // If it is not text then quit
            // cannot search bitmap etc
            //
            if (!iData.GetDataPresent(DataFormats.Text))
            {
                return false;
            }

            string strClipboardText;

            try
            {
                strClipboardText = (string)iData.GetData(DataFormats.Text);
            }
            catch (Exception ex)
            {
                return false;
            }

            // Hyperlinks e.g. http://www.server.com/folder/file.aspx
            Regex rxURL = new Regex(@"(\b(?:http|https|ftp|file)://[^\s]+)", RegexOptions.IgnoreCase);
            rxURL.Match(strClipboardText);

            foreach (Match rm in rxURL.Matches(strClipboardText))
            {
                if (strClipboardText.Contains(".xml") || strClipboardText.Contains(".rss")) loadWindow(strClipboardText);  
            }



            return FoundNewLinks;
        }

        public bool canParse()
        {return false;}


        public string parsedHTML()
        {return "";}

        public void feedChanged(string name, string category)
        { }

        public void showConfiguration()
        {
            MessageBox.Show("This function is not currently implemented", "Info", MessageBoxButtons.OK, MessageBoxIcon.Stop);   
        }

        public void addToToolbar(System.Windows.Controls.ToolBar ToolBar)
        {
            System.Windows.Controls.Image myImage = new System.Windows.Controls.Image();
            myImage.Width = 30;
            myImage.Height = 30;
            // Create source
            System.Windows.Media.Imaging.BitmapImage myBitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri( System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\circle.png");
            myBitmapImage.DecodePixelWidth = 30;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            b.Content = myImage;
            b.ToolTip = "Configures Clipboard Monitor Plugin options.";
            b.Click += new System.Windows.RoutedEventHandler(b_Click);
            ToolBar.Items.Add(b); 
        
        }

        void b_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            showConfiguration(); 
        }


        public void getDataBase(DataBaseEngine data)
        {
            this.data = data;
        }


        public void setOwner(System.Windows.Window window)
        { }

        public void setDocument(XmlDocument doc)
        { }


        public string description()
        { return "The plugin checks the clipboard for links to rss feeds."; }

        public string name()
        {
            return "Clipboard monitor";
        }



        public void loadWindow(string s)
        {
            
            win = new System.Windows.Window();
            win.Width = 300;
            win.Height = 200;
            win.Title = "Feedfusion Clipboard Monitor";
            System.Windows.Controls.Label lblUser = new System.Windows.Controls.Label();
            lblUser.Content = "Do you want to add this feed to the database?";



            txtPassword = new System.Windows.Controls.TextBox();
            txtPassword.BorderThickness =new  System.Windows.Thickness(0)  ;
            txtPassword.IsEnabled = false;  
            txtPassword.Text = s;
            txtPassword.Width = 280;
            txtPassword.Height = 20;

            System.Windows.Controls.Label lb = new System.Windows.Controls.Label();
            lb.Content = "Category to add to:";
            lb.Width = 280;
            lb.Height = 25;

            listCat = new System.Windows.Controls.ListBox();
            listCat.Width = 280;
            listCat.Height = 25;
            if (data != null)
            {
                string[] cats = data.getCategories();
                foreach (string cat in cats)
                {
                    listCat.Items.Add(cat);
                }
            }

            btnAdd = new System.Windows.Controls.Button();
            btnAdd.Content = "Add";
            btnAdd.Visibility = System.Windows.Visibility.Visible;
            btnAdd.Width = 100;
            btnAdd.Height = 30;
            btnAdd.Click += new System.Windows.RoutedEventHandler(btnAdd_Click);

            StackPanel rootPanel = new StackPanel();
            win.Content = rootPanel;
            rootPanel.Children.Add(lblUser);
            rootPanel.Children.Add(txtPassword);
            rootPanel.Children.Add(lb);
            rootPanel.Children.Add(listCat);
            rootPanel.Children.Add(btnAdd);
            win.Show();


        }

        void btnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                data.addFeed(listCat.SelectedItem.ToString(), txtPassword.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Monitor
            // 
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Name = "Monitor";
            this.Load += new System.EventHandler(this.Monitor_Load);
            this.ResumeLayout(false);

        }

        private void Monitor_Load(object sender, EventArgs e)
        {

        }

 
    }
}
