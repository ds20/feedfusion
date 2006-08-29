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
//using System.Drawing;
using System.Windows.Media;
using System.Windows.Markup;


namespace WinFXConsumer
{

    public class ListHeader : StackPanel
    {
        StackPanel sp;
        TextBox l;
        double w;
        public ListHeader(string s, double w)
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;   
            this.Background = Brushes.WhiteSmoke;   
            l=new TextBox();
            l.IsEnabled = false;
            l.Width =w*16/17 ;
            l.HorizontalAlignment = HorizontalAlignment.Center;
            l.HorizontalContentAlignment = HorizontalAlignment.Center; 
            l.Text=s;
            l.Foreground = new SolidColorBrush(Colors.Blue);
            l.FontSize = 22;
            l.FontStyle = FontStyles.Oblique;
            l.MouseEnter += new System.Windows.Input.MouseEventHandler(ListHeader_MouseEnter);
            this.Children.Add(l); 
            sp=new StackPanel();
            sp.Visibility = Visibility.Collapsed;   
            this.Children.Add(sp);  
            this.MouseLeave += new System.Windows.Input.MouseEventHandler(ListHeader_MouseLeave);
            this.MouseEnter += new System.Windows.Input.MouseEventHandler(ListHeader_MouseEnter);
            
        }

        void ListHeader_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            sp.Visibility = Visibility.Visible; 
        }

        void ListHeader_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            sp.Visibility = Visibility.Collapsed;  
        }

        public void Add(XmlFeed feed,System.Windows.Input.MouseButtonEventHandler handler)
        {
            
            Expander exp = new Expander();
            StackPanel itemStack = new StackPanel();
            exp.HorizontalContentAlignment = HorizontalAlignment.Right;
            exp.HorizontalAlignment = HorizontalAlignment.Center ;
            exp.MouseDoubleClick += handler;
            exp.Width = l.Width*4/5 ;
            exp.BorderThickness = new Thickness(1);
            exp.BorderBrush = Brushes.Gray;  
            exp.Header = feed;
            exp.Content = itemStack;
            sp.Children.Add(exp);            
           
            TextBox b = new TextBox();
            b.Width = l.Width;
            b.Foreground = Brushes.BlueViolet;
            b.HorizontalAlignment = HorizontalAlignment.Right;
            b.HorizontalContentAlignment = HorizontalAlignment.Right;
            b.MouseDoubleClick += handler;
            b.LostFocus += new RoutedEventHandler(b_LostFocus);
            b.Tag = feed;
            b.Text = feed.feedName;
            itemStack.Children.Add(b);   

            TextBox bURL = new TextBox();
            bURL.Width = l.Width;
            bURL.Foreground = Brushes.BlueViolet;
            bURL.HorizontalAlignment = HorizontalAlignment.Right;
            bURL.HorizontalContentAlignment = HorizontalAlignment.Right;
            bURL.LostFocus += new RoutedEventHandler(b_LostFocus);
            bURL.Tag = feed;
            bURL.Text = feed.url;
            itemStack.Children.Add(bURL);

            ComboBox cmb = new ComboBox();
            cmb.Items.Add(feed.catName);
            cmb.SelectedIndex = 0; 
            itemStack.Children.Add(cmb);
        }

        

   

        void b_LostFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).IsReadOnly = true;
        }

        void b_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
 
            ((TextBox)sender).IsReadOnly   = false;
            ((TextBox)sender).Focus(); 
        }
        



    }
    


    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

        
    public partial class Window1 : Window
    {  

        System.Windows.Forms.WebBrowser browser;
        FeedDB dataBase;
        pluginManager pManager;
        int simultaneousDownloads;
        Queue<String> waitQueue;
        List<String> downloadList;
        Boolean closeAllThreads = false;
        Object lockProgressBar = new Object(), lockLogFile = new Object();
        public delegate void NoArgDelegate();
        public delegate void OneArgDelegate(Object arg);
        private delegate void DelegShowhist();
        private DelegShowhist delegShowHist;

        MenuItem M, parent;
        Color color = new Color();
        protected string[] _styleList;
        protected int _styleIndex = 0;
        
        protected void DiscoverStyles()
        {
            _styleList = Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*_style.xaml");
        }

        protected void btnO_Click(object sender, RoutedEventArgs args)
        {
            // To detect new styles...
            this.DiscoverStyles();
            if (this._styleList == null || this._styleList.Length == 0)
            {
                //MessageBox.Show("No skins available");//eventual mesaj...daca nu exista alta cale(log sau ceva)
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

        void populateCategoryList()
        {
            treeView1.Background = Brushes.Transparent;
            treeView1.BorderThickness = new Thickness(0);  
            foreach (string s in dataBase.getCategories())
            {
                
                ListHeader  dp=new ListHeader(s,treeView1.Width );

                foreach (XmlFeed feed in dataBase.getFeeds(s))
                {
                    dp.Add(feed, new System.Windows.Input.MouseButtonEventHandler(b_MouseDown));
                    
                }
                treeView1.Items.Add(dp);
            }
        }

        void b_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            String feed = ((XmlFeed)((Expander)sender).Header).url;
            Thread t_nou = new Thread(new ParameterizedThreadStart(getHistoryAsync));
            t_nou.Start(feed);
        }


  



        private void Tmrcallback1(object o)
        {
            M.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new NoArgDelegate(transform));
        }

        private void transform()
        {
            if (color.R > 100)
                color.R -= 100;
            else
                color.R = 0;
            if (color.G > 100)
                color.G -= 100;
            else
                color.G = 0;
            M.Foreground = new SolidColorBrush(color);
        }

        private void menuitemback_MouseEnter(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem M = (MenuItem)sender;
            M.Background = new LinearGradientBrush(Colors.LightBlue, Colors.SlateBlue, 90);
        }

        private void menuitemback_MouseLeave(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem M = (MenuItem)sender;
            //M.Background = new SolidColorBrush(Colors.White);

        }

        private void menuitem1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            listBox1.Items.Clear();
            M = (MenuItem)sender;
            String feed = ((XmlFeed)(M.Header)).url;
            Thread t_nou = new Thread(new ParameterizedThreadStart(getHistoryAsync));
            t_nou.Start(feed);
        }




        private void ListRefresh()
        {
            treeView1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,new NoArgDelegate(ListRefresh2));
        }

        private void ListRefresh2()
        {
            treeView1.Items.Clear();
            populateCategoryList();

        }

        private void menuitem_MakeMenuRightClick(object sender, System.Windows.RoutedEventArgs e)
        {
            M = (MenuItem)sender;
            ContextMenu menuRightClick = new ContextMenu();
            MenuItem menuRightItem1 = new MenuItem();
            menuRightItem1.Header = "Remove";
            menuRightItem1.Click += menuitem_ContextMenuRemove;
            menuRightClick.Items.Add(menuRightItem1);
            MenuItem menuRightItem2 = new MenuItem();
            menuRightItem2.Header = "Rename";
            menuRightItem2.Click += menuitem_ContextMenuRename;
            menuRightClick.Items.Add(menuRightItem2);
            M.ContextMenu = menuRightClick;


        }

        private void menuitem_ContextMenuRemove(object sender, System.Windows.RoutedEventArgs e)
        {
            if (parent == null)
            {
                Thread t = new Thread(new ParameterizedThreadStart(new OneArgDelegate(dataBase.removeCategory)));
                t.Start(M.Header);
            }
            else
            {
                
                Thread t = new Thread(new ParameterizedThreadStart(new OneArgDelegate(dataBase.removeFeed)));
                t.Start(((XmlFeed)(M.Header)).url);
            }

        }
       
        private void menuitem_ContextMenuRename(Object sender, EventArgs args)
        {
            if (parent != null)
            {
                RenameWindow rw = new RenameWindow(((XmlFeed)(M.Header)).feedName, this._styleList[_styleIndex]);
                rw.ShowDialog();
                if (rw.DialogResult == true)
                {
                    String url = ((XmlFeed)(M.Header)).url;
                    String newName = rw.txtNewName.Text.Trim();
                    dataBase.renameFeed(url, newName);
                }
            }
            else
            {
                RenameWindow rw = new RenameWindow((String)(M.Header), this._styleList[_styleIndex]);
                rw.ShowDialog();
                if (rw.DialogResult == true)
                {
                    String oldName = (String)(M.Header);
                    String newName = rw.txtNewName.Text.Trim();
                    dataBase.renameCategory(oldName, newName);
                }
            }
        }

        public Window1()
        { 
            InitializeComponent();
            this.ContentRendered += Window1_ContentRendered;

            dataBase = new FeedDB();
            //dataBase.removeAllHistory();
            dataBase.getHistory("nu exista");
            tabs.TabStripPlacement = Dock.Bottom;
            browser = (System.Windows.Forms.WebBrowser)hh.Child;
            browser.Navigate("http://www.google.ro/firefox?client=firefox-a&rls=org.mozilla:en-US:official");
            this.Closing += window1_close;
            dataBase.delegCatFeedChanged += ListRefresh;
            button5.Click += button5_Click;
            button6.Click += button6_Click;
            button7.Click += button7_Click;
            button8.Click += button8_Click;
            btnO.Click += new RoutedEventHandler(btnO_Click);
            treeView1.MouseDoubleClick += treeView1_MouseDoubleClick;
            listBox1.SelectionChanged += listBox1_SelectionChanged;
            txtSearch.KeyDown += txtSearch_KeyDown;
            Button.Click += Button_Click;
            populateCategoryList();


        
        }

        void Window1_ContentRendered(Object sender, EventArgs e)
        {
            pManager = new pluginManager(ToolBar, dataBase, this);
            for (int i = 0; i < pManager.number; i++) pManager.plugins[i].addToToolbar(this.ToolBar);

            this.DiscoverStyles();
            if (this._styleList == null || this._styleList.Length == 0)
            {
                //MessageBox.Show("No skins available");//eventual mesaj...daca nu exista alta cale(log sau ceva)
                return;
            }

            this._styleIndex++;
            if (this._styleIndex > this._styleList.Length - 1)
                this._styleIndex = 0;

            this.ApplyStyle(this._styleIndex);

            simultaneousDownloads = 4;
            downloadList = new List<String>(simultaneousDownloads);
            waitQueue = new Queue<String>();

            XmlFeed[] allFeeds = dataBase.getAllFeeds();
            progressBar.Maximum = allFeeds.Length;
            foreach (XmlFeed feed in allFeeds)
                //ThreadPool.QueueUserWorkItem(new WaitCallback(downloadByDownList), feed.url);
                waitQueue.Enqueue(feed.url);
            Thread tWaitQueue = new Thread(new ThreadStart(checkWaitQueue));
            tWaitQueue.Start();
        }

        void txtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.CompareTo(System.Windows.Input.Key.Enter) == 0)
            {
                XmlHistory[] hist = dataBase.searchHistory(txtSearch.Text);
                putHistoryInList(hist);
            }
        }

        void Button_Click(Object sender, EventArgs e)
        {
            XmlHistory[] hist = dataBase.searchHistory(txtSearch.Text);
            putHistoryInList(hist);
        }
       

        //procesare feed
        private void startDld()
        {
            //nu se face asa... tre sa facem altfel...
            //findPluginFor(dldFeed(textBox1.Text));
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
                    String parsedHTML = pManager.plugins[i].parsedHTML();
                    setBrowserDocumentText(parsedHTML);
                }
                i++;
            }
            if (!gasit) setBrowserDocumentText("<html><body><h2><font color=\"red\">Nu exista plugin pentru acest tip de RSS.</font></h2></body></html>");
        }

        delegate void DelegSetDocumentText(Object parsedHTML_o);
        void setBrowserDocumentText(Object parsedHTML_o)
        {
            if (browser.InvokeRequired){
                DelegSetDocumentText delegForBrowser = setBrowserDocumentText;
                browser.Invoke(delegForBrowser, parsedHTML_o);
            }
            else
                browser.DocumentText = (String)parsedHTML_o;
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
                statusBar.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new OneArgDelegate(addStatusBarItem), url + "\n" + e.Message);
                WriteToLogFileAsync(DateTime.Now + "\r\n" + url + "\r\n" + e.Message + "\r\n\r\n");
                xDoc = null;
            }
        }

        private void addStatusBarItem(Object msg_o)
        {
            statusBar.Items.Clear();
            statusBar.Items.Add((String)msg_o);
        }
        
        private void WriteToLogFileAsync(String msg)
        {
            Thread t = new Thread(new ParameterizedThreadStart(WriteToLogFile));
            t.Start(msg);
        }

        private void WriteToLogFile(Object msg_o){
            lock (lockLogFile)
            {
                FileStream f = new FileStream("LogFile.txt", FileMode.Append);
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
        
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Thread t1 = new Thread(new ThreadStart(startDld));
            t1.Start();
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
            w.ShowDialog();
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            Window3 w = new Window3(this._styleList[_styleIndex]);
            w.ShowDialog();
        }

        private void treeView1_MouseDoubleClick(Object sender, EventArgs e)
        {
            System.Windows.Controls.TreeViewItem t = (System.Windows.Controls.TreeViewItem)treeView1.SelectedItem;
            if (t == null) return;
            if (t.Parent != treeView1)
            {
                String url = ((XmlFeed)(t.Header)).url;
                Thread thr = new Thread(new ParameterizedThreadStart(downloadFeed));
                thr.Start(url);

                listBox1.Items.Clear();
                Thread t_nou = new Thread(new ParameterizedThreadStart(getHistoryAsync));
                t_nou.Start(url);
            }
        }
        
        delegate void DelegPutHistoryInList(Object hist_o_vect);

        private void putHistoryInList(Object hist_o_vect)
        {
            listBox1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new OneArgDelegate(putHistoryInList2), hist_o_vect);
        }

        private void putHistoryInList2(Object hist_o_vect)
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
            lock (lockProgressBar)
            {
                progressBar.Value = progressBar.Value + 1;
                if (progressBar.Maximum == progressBar.Value)
                    progressBar.Opacity = 0;
            }
        }


        private void treeView1_ContextMenuRemove(Object sender, EventArgs args)
        {
            if (treeView1.SelectedItem != null)
                if (((TreeViewItem)treeView1.SelectedItem).Parent != treeView1)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(new OneArgDelegate(dataBase.removeFeed)));
                    t.Start(((XmlFeed)(((TreeViewItem)treeView1.SelectedItem).Header)).url);
                    ((TreeViewItem)((TreeViewItem)treeView1.SelectedItem).Parent).Items.Remove((TreeViewItem)treeView1.SelectedItem);
                }
                else
                {
                    Thread t = new Thread(new ParameterizedThreadStart(new OneArgDelegate(dataBase.removeCategory)));
                    t.Start((String)(((TreeViewItem)treeView1.SelectedItem).Header));
                    treeView1.Items.Remove((TreeViewItem)treeView1.SelectedItem);
                }
        }

        private void treeView1_ContextMenuRename(Object sender, EventArgs args)
        {
            if (treeView1.SelectedItem != null)
                if (((TreeViewItem)treeView1.SelectedItem).Parent != treeView1)
                {
                    RenameWindow rw = new RenameWindow(((XmlFeed)(((TreeViewItem)treeView1.SelectedItem).Header)).feedName, this._styleList[_styleIndex]);
                    rw.ShowDialog();
                    if (rw.DialogResult == true)
                    {
                        String url = ((XmlFeed)(((TreeViewItem)treeView1.SelectedItem).Header)).url;
                        String newName = rw.txtNewName.Text.Trim();
                        dataBase.renameFeed(url, newName);
                    }
                }
                else
                {
                    RenameWindow rw = new RenameWindow((String)(((TreeViewItem)treeView1.SelectedItem).Header), this._styleList[_styleIndex]);
                    rw.ShowDialog();
                    if (rw.DialogResult == true)
                    {
                        String oldName = (String)(((TreeViewItem)treeView1.SelectedItem).Header);
                        String newName = rw.txtNewName.Text.Trim();
                        dataBase.renameCategory(oldName, newName);
                    }
                }
        }


        private void window1_close(Object sender, EventArgs e)
        {
            closeAllThreads = true;
            dataBase.Optimize();
        }

    }
}

