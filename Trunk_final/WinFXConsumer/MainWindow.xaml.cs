using System;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.IO;
using System.Runtime.Remoting;
using System.Xml;
using System.Threading;
using System.Xml.XPath;
using System.Xml.Xsl;
using Indexer;
using PluginInterface;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;   
using System.Windows.Media.Media3D;

using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Media.Imaging;  

namespace WinFXConsumer
{

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class Window1 : Window,PluginInterface.EventsClass  
    {
        private FeedTree feedTree;
        private string FeedWithNewArticle;
        private Random r = new Random();
        private FeedDB dataBase;
        private pluginManager pManager;
        private int simultaneousDownloads;
        private Queue<String> waitQueue;
        private List<String> downloadList;
        private Boolean closeAllThreads = false;
        private Object lockProgressBar = new Object(), lockLogFile = new Object();
        public delegate void NoArgDelegate();
        public delegate void OneArgDelegate(Object arg);
        private delegate void DelegShowhist();
        private DelegShowhist delegShowHist;
        private Image imgStatus;
        private double catListSize=-1;
        private double progressBarIncrement;
        protected string[] _styleList;
        protected int _styleIndex = 0;
#region styles
        protected void DiscoverStyles()
        {
            _styleList = Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory+@"\Styles\", "*_style.xaml");
        }

        protected void btnO_Click(object sender, RoutedEventArgs args)
        {
            // To detect new styles...
            this.DiscoverStyles();
            if (this._styleList == null || this._styleList.Length == 0)
            {
                //MessageBox.Show("No skins available");
                return;
            }

            this._styleIndex++;
            if (this._styleIndex > this._styleList.Length - 1)
                this._styleIndex = 0;
            
            this.ApplyStyle(this._styleIndex);
        }

        protected void ApplyStyle(string styleName)
        {           
            try
            {
                if (styleName.IndexOf("_style.xaml") == -1)
                {
                    styleName += "_style.xaml";
                    styleName = AppDomain.CurrentDomain.BaseDirectory + "\\" + styleName;
                }

                if (File.Exists(styleName) == false)
                {
                    return;
                }


                using (FileStream fs = new FileStream(styleName, FileMode.Open, FileAccess.Read))
                {
                    ResourceDictionary dictionary = (ResourceDictionary)XamlReader.Load(fs);

                    this.Resources = dictionary;

                    fs.Close();
                }
                
                /*Stream stream = File.OpenRead(styleName);
                FrameworkElement resourceElement = (FrameworkElement)Parser.LoadXml(stream);
                resourceElement.Resources.Seal();
                this.Application.Resources = resourceElement.Resources;//Who is Application??????!!!!???!!!!!
                stream.Close();*/
            }

            catch (Exception ex)
            {
                //ErrorLog.LogHandledException(ex);
            }

        }

        protected void ApplyStyle(int styleIndex)
        {
            try
            {
                this.ApplyStyle(this._styleList[styleIndex]);
            }

            catch { }
        }

        protected void ApplyStyle()
        {
            this.ApplyStyle(this._styleIndex);
        }
#endregion
#region feedTree
        void populateCategoryList()
        {
            feedTree.Background = Brushes.Transparent;
            feedTree.BorderThickness = new Thickness(0);  
            TreeViewItem father=new TreeViewItem();
            string[] caty = dataBase.getCategories();  
            foreach (string s in dataBase.getCategories())
            {

                TreeViewItem catNode = new TreeViewItem();
                catNode.Tag = s;
                father.Items.Add(catNode);
                foreach (XmlFeed feed in dataBase.getFeeds(s))
                {
                    TreeViewItem feedNode = new TreeViewItem();
                    feedNode.Tag = feed;
                    catNode.Items.Add(feedNode);

                }
            }
            feedTree.Populate(father,caty );
        }

        void feedTree_categoryNameChanged(string cat, string newCat)
        {
            dataBase.renameCategory(cat, newCat);  
        }


        void categoryList_viewFeed(XmlFeed f)
        {
            XmlHistory[] hist=dataBase.getHistory(f.url);
            if ((hist!=null) && (hist.Length>0)) 
            {
                String histContents = (hist[0].contents);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(histContents);
                findPluginFor(doc);
                Thread t_nou = new Thread(new ParameterizedThreadStart(getHistoryAsync));
                t_nou.Start(f.url);
            }
        }

        void categoryList_feedRename(XmlFeed f, string newName)
        {
            dataBase.renameFeed(f.url, newName);   
        }

        void categoryList_deleteClick(XmlFeed f)
        {
            dataBase.removeFeed(f.url);   
        }

        private void ListRefresh()
        {
            feedTree.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,new NoArgDelegate(ListRefreshUIThread));
        }

        private void ListRefreshUIThread()
        {
            feedTree.Items.Clear();
            populateCategoryList();

        }



#endregion
        public Window1()
        {
            InitializeComponent();
            InitializeImages();
          
            feedTree=new FeedTree();
            feedTree.categoryNameChanged += new FeedTree.categoryNameChange(feedTree_categoryNameChanged);
            feedTree.deleteClick += new FeedTree.deleteFeedDelegate(categoryList_deleteClick);
            feedTree.feedRename += new FeedTree.feedRenameDelegate(categoryList_feedRename);
            feedTree.viewFeed += new FeedTree.viewFeedDelegate(categoryList_viewFeed);
            cats.Content = feedTree; 
            System.Windows.Data.Binding  b= new System.Windows.Data.Binding("Width");
            b.Source = columnLeft;
            feedTree.SetBinding(FeedTree.WidthProperty,b);    

            this.ContentRendered += Window1_ContentRendered;
            this.Closing += Window1_Closing;

            dataBase = new FeedDB();
            dataBase.RegisterEventHandler(this);
            dataBase.getHistory("there isn't any");
            dataBase.delegCatFeedChanged += ListRefresh;

            btnAddFeed.Click += button5_Click;
            btnPluginOptions.Click += button6_Click;
            btnOpml.Click += button7_Click;
            btnAbout.Click += button8_Click;
            btnProgramOptions.Click += new RoutedEventHandler(btnProgramOptions_Click);
            btnNotes.Click += new RoutedEventHandler(btnNotes_Click);  
            btnO.Click += new RoutedEventHandler(btnO_Click);
            listBox1.SelectionChanged += listBox1_SelectionChanged;
            txtSearch.KeyDown += txtSearch_KeyDown;
            btnSearch.Click += Button_Click;
            latestPosts.SelectionChanged += new SelectionChangedEventHandler(latestPosts_SelectionChanged);
            populateCategoryList();
            progressBarIncrement = this.Width / dataBase.getAllFeeds().Length;
            //progressBar.Width = 0;
        
        }

        void btnNotes_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)+"\\release_notes.txt");  
        }


        void latestPosts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String histContents = (dataBase.getHistory((string)latestPosts.SelectedItem)[0].contents);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(histContents);
            findPluginFor(doc); 
        }

        private void InitializeImages()
        {
            Image myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\search.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            WrapPanel wp = new WrapPanel();
            Label l = new Label();
            l.Content = "Search";
            wp.Children.Add(myImage);
            wp.Children.Add(l);
            hist.Header = wp;

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\cat.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            WrapPanel wp2 = new WrapPanel();
            Label l2 = new Label();
            l2.Content = "Categories";
            wp2.Children.Add(myImage);
            wp2.Children.Add(l2);
            cats.Header = wp2;

            myImage = new Image();
            myImage.Width = 18;
            myImage.Height = 18;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\searchButtonImage.png");
            myBitmapImage.DecodePixelWidth = 18;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            wp2 = new WrapPanel();
            l2 = new Label();
            l2.Height = 33;
            l2.Content = "Go";
            wp2.Children.Add(myImage);
            wp2.Children.Add(l2);
            btnSearch.Content = wp2;

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\latest.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            wp2 = new WrapPanel();
            l2 = new Label();
            l2.Content = "Latest";
            wp2.Children.Add(myImage);
            wp2.Children.Add(l2);
            latest.Header = wp2;

            myImage = new Image();
            myImage.Width = 30;
            myImage.Height = 30;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\reload.png");
            myBitmapImage.DecodePixelWidth = 30;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            wp2 = new WrapPanel();
            //wp2.Children.Add(myImage);
            btnRefresh.Content = myImage;

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\cat.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            btnAddFeed.Icon = myImage;

            imgStatus = new Image();
            imgStatus.Width = 15;
            imgStatus.Height = 15;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\statusOK.png");
            myBitmapImage.DecodePixelWidth = 15;
            myBitmapImage.EndInit();
            imgStatus.Source = myBitmapImage;
            statusIconPanel.Children.Add(imgStatus);  

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\user.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            btnProgramOptions.Icon = myImage;

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\import.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            btnOpml.Icon = myImage;

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\skin.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            btnO.Icon = myImage;

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\bomb.png");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            btnPluginOptions.Icon = myImage;

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\user.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            btnProgramOptions.Icon = myImage;

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\about.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            btnAbout.Icon = myImage;
        }

        public void FeedDownloaded(string feed)
        {
            feedTree.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new OneArgDelegate(feedTree.MarkUnreadFeed),feed);
            latestPosts.Items.Insert(0, feed);
        }



        public void CategoryAdded(string c)
        { }

        public void NewFeedAdded(string feed)
        { }  


        void btnProgramOptions_Click(object sender, RoutedEventArgs e)
        {
            optionsWindow ow = new optionsWindow(dataBase);
            ow.ShowDialog(); 
        }

        void Window1_ContentRendered(Object sender, EventArgs e)
        {
            SetFrameBackground();
            pManager = new pluginManager(ToolBar, dataBase, this,new opml() );
            for (int i = 0; i < pManager.number; i++) pManager.plugins[i].addToToolbar(this.ToolBar);
            ToolBar.Items.Refresh();
            this.WindowState = WindowState.Normal;      
            this.DiscoverStyles();
            if (this._styleList == null || this._styleList.Length == 0)
            {
                return;
            }

            this._styleIndex++;
            if (this._styleIndex > this._styleList.Length - 1)
                this._styleIndex = 0;

            //this.ApplyStyle(this._styleIndex);
            this.InvalidateVisual();
            this.Width += 1;
            Timer t = new Timer(new TimerCallback(refreshTimerCallback), null, 0, 60 * Properties.Settings.Default.autorefreshInterval * 1000);
            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 1)
            {
                Window2 addWindow = new Window2(dataBase, this._styleList[_styleIndex], arguments[0]);
                addWindow.ShowDialog();
            }
      
        }
        void refreshTimerCallback(object sender)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,new  NoArgDelegate(DownloadAll));     
        }
        private void Refresh(object sender, RoutedEventArgs e)
        { DownloadAll(); }

        private void DownloadAll()
        {
            simultaneousDownloads = 4;
            downloadList = new List<String>(simultaneousDownloads);
            waitQueue = new Queue<String>();
            progressBar.Visibility = Visibility.Visible;
            progressBar.Value = 0; 
            XmlFeed[] allFeeds = dataBase.getAllFeeds();
            progressBar.Minimum = 0;  
            progressBar.Maximum = allFeeds.Length;
            //progressBar.Width = 0;
            foreach (XmlFeed feed in allFeeds)
                //ThreadPool.QueueUserWorkItem(new WaitCallback(downloadByDownList), feed.url);
                waitQueue.Enqueue(feed.url);
            Thread tWaitQueue = new Thread(new ThreadStart(checkWaitQueue));
            tWaitQueue.Start();
        }

        private void SetFrameBackground()
        {
            ImageSource s = new BitmapImage(new Uri( System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Icons\\bal.png"));
            ImageBrush ib = new ImageBrush(s);
            ib.Opacity = 0.5;
            ib.Stretch = Stretch.Fill;
            this.Background = ib;
        }

        void txtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key.CompareTo(System.Windows.Input.Key.Enter) == 0)&&(txtSearch.Text!=""))
            {
                XmlHistory[] hist = dataBase.searchHistory(txtSearch.Text);
                putHistoryInList(hist);
            }
        }

        void Button_Click(Object sender, EventArgs e)
        {
            if (txtSearch.Text != "")
            {
                XmlHistory[] hist = dataBase.searchHistory(txtSearch.Text);
                putHistoryInList(hist);
            }
        }


        //dld feed
        public static XmlDocument dldFeed(string url)
        {
            XmlDocument xDoc = new XmlDocument();
            
            
            xDoc.Load(Validator.ParseURL(url));

            //MessageBox.Show("S-a dld feedul.");
            return xDoc;
        }

        //gaseste plugin pt feed
        public void findPluginFor(XmlDocument rssDocument)
        {
            int i = 0;
            bool gasit = false;
            while ((i < pManager.plugins.Count) && (!gasit))
            {
                pManager.plugins[i].setDocument(rssDocument);
                if (pManager.plugins[i].canParse())
                {
                    gasit = true;
                    //MessageBox.Show("Pluginul care parseaza este " + pManager.plugins[i].description());
                    imgStatus.ToolTip = "Parsing with the " +pManager.plugins[i].name()+  " plugin";  
                    String parsedHTML = pManager.plugins[i].parsedHTML();
                    setBrowserDocumentText(parsedHTML);
                }
                i++;
            }
            if (!gasit) setBrowserDocumentText("<html><body><h2><font color=\"red\">No plugin.</font>There is no plugin installed for this RSS type.</h2></body></html>");
        }

        delegate void DelegSetDocumentText(Object parsedHTML_o);
        void setBrowserDocumentText(Object parsedHTML_o)
        {

            double d = r.NextDouble();
            string htmlFile = Path.GetTempPath()  + d.ToString() + ".htm";
            TextWriter  f = new StreamWriter(htmlFile);
            f.Write((string)parsedHTML_o);
            f.Close();
            browser.Navigate(new Uri(htmlFile)); 
            SetFrameBackground();
        }

        private XmlDocument downloadFeed2(String url)
        {
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.Load(url);
                    XmlHistory[] h = dataBase.getHistory(url);
                    if (h.Length == 0 || h[0].contents.CompareTo(xDoc.OuterXml) != 0)
                        dataBase.addHistory(url, xDoc.OuterXml, DateTime.Now);
            }
            catch (Exception e)
            {
                setBrowserDocumentText("<html><body><h2>" + url + "</h2><h3>" + e.Message + "</h3></body></html>");
                xDoc = null;
            }
            return xDoc;
        }

        /// <summary>
        /// Downloads the XML Feed and refreshes its history
        /// </summary>
        /// <param name="url">The url of the feed</param>
        /// <returns>An XML document with the downloaded feed</returns>
        private void justDownloadFeed(String url)
        {
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.Load(url);
                    XmlHistory[] h = dataBase.getHistory(url);
                    if (h.Length == 0 || h[0].contents.CompareTo(xDoc.OuterXml) != 0)
                        dataBase.addHistory(url, xDoc.OuterXml, DateTime.Now);
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, url);
                statusBar.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new OneArgDelegate(addStatusBarItem), url + "   " + e.Message);
                WriteToLogFileAsync(DateTime.Now + "::" + url + "::" + e.Message + "\r\n");
                xDoc = null;
            }
        }

        private void addStatusBarItem(Object msg_o)
        {
            statusBar.ToolTip=msg_o;
        }
        
        private void WriteToLogFileAsync(String msg)
        {
            Thread t = new Thread(new ParameterizedThreadStart(WriteToLogFile));
            t.Start(msg);
        }

        private void WriteToLogFile(Object msg_o){
            lock (lockLogFile)
            {
                FileStream f = new FileStream("feedfusion.log", FileMode.Append);
                char[] caractere = ((String)msg_o).ToCharArray();
                System.Text.Encoder enc = System.Text.Encoding.Default.GetEncoder();
                byte[] bitiDeScris = new byte[2048];
                int codificate = enc.GetBytes(caractere, 0, caractere.Length, bitiDeScris, 0, false);
                f.Write(bitiDeScris, 0, codificate);
                f.Close();
            }
        }

        private void downloadFeed(Object url_o)
        {
            String url = (String)url_o;
            XmlDocument xDoc = downloadFeed2(url);
            if (xDoc != null)
                findPluginFor(xDoc);
        }
        
        


        private void button5_Click(object sender, RoutedEventArgs e)
        {
            Window2 addWindow = new Window2(dataBase, this._styleList[_styleIndex]);
            addWindow.ShowDialog(); 

        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            pluginConfigWindow w = new pluginConfigWindow(dataBase, pManager, this._styleList[_styleIndex]);
            w.ShowDialog();
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            OpmlWindow w = new OpmlWindow(dataBase, this._styleList[_styleIndex]);
            try { w.ShowDialog(); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            Window3 w = new Window3(this._styleList[_styleIndex]);
            w.ShowDialog();
        }

        
        delegate void DelegPutHistoryInList(Object hist_o_vect);

        private void putHistoryInList(Object hist_o_vect)
        {
            listBox1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new OneArgDelegate(pugHistoryInListUIThread), hist_o_vect);
        }

        private void pugHistoryInListUIThread(Object hist_o_vect)
        {
            listBox1.Items.Clear();
            XmlHistory[] hist = (XmlHistory[])hist_o_vect;
            foreach (XmlHistory h in hist)
                listBox1.Items.Add(h);
        }
        
        private void getHistoryAsync(Object feed_o)
        {
            String feed = (String)feed_o;
            if (dataBase.feedExists(feed))
            {
                XmlHistory[] hist = dataBase.getHistory(feed);
                putHistoryInList(hist);
            }
        }

        private void showHistoryInBrowser()
        {
            if (listBox1.SelectedIndex == -1) return;
            String histContents = ((XmlHistory)listBox1.Items[listBox1.SelectedIndex]).contents;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(histContents);
            findPluginFor(doc);
        }

        private void listBox1_SelectionChanged(Object sender, EventArgs e)
        {
            listBox1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new NoArgDelegate(showHistoryInBrowser));
        }

        private void checkWaitQueue()
        {
            while (!closeAllThreads && waitQueue.Count>0)
                if (downloadList.Count < simultaneousDownloads)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(downloadByDownList));
                    t.ApartmentState = ApartmentState.STA;  
                    t.IsBackground = true;
                    t.Start(waitQueue.Dequeue());
                }
                else
                    Thread.Sleep(500);
        }

        private void downloadByDownList(Object url_o)
        {
            String url = (String)url_o;
            downloadList.Add(url);
            justDownloadFeed(url);
            downloadList.Remove(url);
            progressBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Render,
                new NoArgDelegate(IncrementProgressBar));
        }

        private void IncrementProgressBar()
        {
            //lock (lockProgressBar)
            //{
                progressBar.Value = progressBar.Value + 1;
                //progressBar.Width+=progressBarIncrement;
                progressBar.UpdateLayout();
                progressBar.InvalidateVisual(); 
                if (progressBar.Maximum == progressBar.Value)
                    progressBar.Visibility  = Visibility.Hidden ;
            //}
        }


        private void window1_close(Object sender, EventArgs e)
        {
            closeAllThreads = true;
            dataBase.Optimize();
        }

        void Window1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.Settings.Default.deleteHistoryOnExit) dataBase.removeAllHistory();   
            foreach (Window f in Application.Current.Windows) if (f != this) f.Close();
            Application.Current.Shutdown();
        }

    }
}

