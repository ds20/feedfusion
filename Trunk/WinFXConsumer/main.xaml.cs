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

using System.Windows.Media.Media3D;
//using System.Drawing;
using System.Windows.Media;
using System.Windows.Markup;


namespace WinFXConsumer
{


    


    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

        
    public partial class Window1 : Window
    {  

        System.Windows.Forms.WebBrowser browser;
        long originalWidth;
        double a, b, c;
        double alfa;
        double beta;
        double r;

        FeedDB dataBase;
        pluginManager pManager;
        int simultaneousDownloads;
        Queue<String> waitQueue;
        List<String> downloadList;
        Boolean closeAllThreads = false;
        //Object useHistory = new Object();
        Object lockProgressBar = new Object(), lockLogFile = new Object();
        public delegate void NoArgDelegate();
        public delegate void OneArgDelegate(Object arg);

        MenuItem M, parent;
        Color color = new Color();
        
        ////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////
        
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



        ////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////
        

        private void start()
        {
            setLandscape(); 
            Model3DGroup cube = new Model3DGroup();
            Point3D p0 = new Point3D(5, 5, 5);
            Point3D p1 = new Point3D(8, 5, 5);
            Point3D p2 = new Point3D(8, 5, 8);
            Point3D p3 = new Point3D(5, 5, 8);
            Point3D p4 = new Point3D(5, 8, 5);
            Point3D p5 = new Point3D(8, 8, 5);
            Point3D p6 = new Point3D(8, 8, 8);
            Point3D p7 = new Point3D(5, 8, 8);
            //front side triangles
            cube.Children.Add(CreateTriangleModel(p3, p2, p6));
            cube.Children.Add(CreateTriangleModel(p3, p6, p7));
            //right side triangles
            cube.Children.Add(CreateTriangleModel(p2, p1, p5));
            cube.Children.Add(CreateTriangleModel(p2, p5, p6));
            //back side triangles
            cube.Children.Add(CreateTriangleModel(p1, p0, p4));
            cube.Children.Add(CreateTriangleModel(p1, p4, p5));
            //left side triangles
            cube.Children.Add(CreateTriangleModel(p0, p3, p7));
            cube.Children.Add(CreateTriangleModel(p0, p7, p4));
            //top side triangles
            cube.Children.Add(CreateTriangleModel(p7, p6, p5));
            cube.Children.Add(CreateTriangleModel(p7, p5, p4));
            //bottom side triangles
            cube.Children.Add(CreateTriangleModel(p2, p3, p0));
            cube.Children.Add(CreateTriangleModel(p2, p0, p1));

            ModelVisual3D model = new ModelVisual3D();
            model.Content = cube;
            this.mainViewport.Children.Add(model);


            //CUBE ANIMATION
            beta = 0;
            alfa = 0;
            r = 55;
            Timer tmrXXX = new Timer(new TimerCallback(moveCamera));
            tmrXXX.Change(0, 30);


        }

        private void moveCamera(object o)
        {
            double pr;
            //alfa = alfa + 0.002;
            beta = beta + 0.002;
            pr = r * System.Math.Cos(alfa);
            c = r * System.Math.Sin(alfa);
            a = pr * System.Math.Cos(beta);
            b = pr * System.Math.Sin(beta);
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,new NoArgDelegate(SetCamera));
        }

        private Model3DGroup CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            Vector3D normal = CalculateNormal(p0, p1, p2);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.AliceBlue));
            GeometryModel3D model = new GeometryModel3D(
                mesh, material);
            Model3DGroup group = new Model3DGroup();
            group.Children.Add(model);
            return group;
        }
        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(
                p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(
                p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }


        private void SetCamera()
        {
            PerspectiveCamera camera = (PerspectiveCamera)mainViewport.Camera;
            Point3D position = new Point3D(a,b,c);
      
            Vector3D lookDirection = new Vector3D(a,b,c);
         
            camera.Position = position;
            camera.LookDirection = -lookDirection;
        }



        private Point3D[] GetRandomTopographyPoints()
        {
            //create a 10x10 topography.
            Point3D[] points = new Point3D[100];
            Random r = new Random();
            double y;
            double denom = 1000;
            int count = 0;
            for (int z =0; z < 10; z++)
            {
                for (int x =0; x < 10; x++)
                {
                    System.Threading.Thread.Sleep(1);
                    y = Convert.ToDouble(r.Next(1, 999)) / denom;
                    points[count] = new Point3D(x, y, z);
                    count += 1;
                }
            }
            return points;
        }



        private void setLandscape()
        {
            //ClearViewport();
            //SetCamera();
            Model3DGroup topography = new Model3DGroup();
            Point3D[] points = GetRandomTopographyPoints();
            for (int z = 0; z <= 80; z = z + 10)
            {
                for (int x = 0; x < 9; x++)
                {
                    topography.Children.Add(
                        CreateTriangleModel(
                                points[x + z],
                                points[x + z + 10],
                                points[x + z + 1])
                    );
                    topography.Children.Add(
                        CreateTriangleModel(
                                points[x + z + 1],
                                points[x + z + 10],
                                points[x + z + 11])
                    );
                }
            }
            ModelVisual3D model = new ModelVisual3D();
            model.Content = topography;
            this.mainViewport.Children.Add(model);
        }

        ////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////

        void populateCategoryList()
        {
            foreach (string s in dataBase.getCategories())
            {
                TreeViewItem catNode = new TreeViewItem();

                catNode.Header = s;
                foreach (XmlFeed feed in dataBase.getFeeds(s))
                {
                    TreeViewItem rssItem = new TreeViewItem();
                    rssItem.Header = feed;
                    rssItem.ToolTip = feed.url;
                    catNode.Items.Add(rssItem);
                }
                treeView1.Items.Add(catNode);
            }
        }

        void populateAnimation()
        {
            StackPanel.Children.Clear();
            foreach (string s in dataBase.getCategories())
            {
                System.Windows.Controls.MenuItem menuitem = new System.Windows.Controls.MenuItem();
                menuitem.Header = s;
                menuitem.Click += new System.Windows.RoutedEventHandler(menuitem_Click);
                menuitem.MouseEnter += new System.Windows.Input.MouseEventHandler(menuitem_MouseEnter);
                menuitem.MouseLeave += new System.Windows.Input.MouseEventHandler(menuitem_MouseLeave);
                menuitem.Background = new LinearGradientBrush(Colors.Beige, Colors.Cornsilk, 90);
                menuitem.MouseRightButtonUp += menuitem_MakeMenuRightClick;
                //menuitem.MouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(menuitem_MouseRightButtonDown);
                parent = null;
                StackPanel.Children.Add(menuitem);

            }

            //Animation:
            Timer tmr = new Timer(new TimerCallback(Tmrcallback));
            tmr.Change(0, 50);
            StackPanel.Opacity = 0;
        }

        private void Tmrcallback(object o)
        {
            StackPanel.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new NoArgDelegate(fade));
        }

        private void fade()
        {
            StackPanel.Opacity += 0.02;
        }

        private void menuitem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StackPanel.Children.Clear();
            MenuItem M = (MenuItem)sender;
            parent = M;
            M.Background = new LinearGradientBrush(Colors.Beige, Colors.Gray, 90);
            //M.FontSize = M.FontSize * 1.05;
            M.Focusable = false;
            M.MouseEnter -= menuitem_MouseEnter;
            M.MouseLeave -= menuitem_MouseLeave;
            M.MouseRightButtonUp -= menuitem_MakeMenuRightClick;
            StackPanel.Children.Add(M);
            
            foreach (XmlFeed feed in dataBase.getFeeds(M.Header.ToString()))
            {
                MenuItem menuitem1 = new MenuItem();
                menuitem1.Header = feed;
                menuitem1.Click += new System.Windows.RoutedEventHandler(menuitem1_Click);//display feed
                menuitem1.MouseEnter += new System.Windows.Input.MouseEventHandler(menuitem_MouseEnter);
                menuitem1.MouseLeave += new System.Windows.Input.MouseEventHandler(menuitem_MouseLeave);
                menuitem1.Background = new LinearGradientBrush(Colors.Beige, Colors.Cornsilk, 90);
                menuitem1.MouseRightButtonUp += menuitem_MakeMenuRightClick;

                StackPanel.Children.Add(menuitem1);
            }
            System.Windows.Controls.MenuItem menuitemback = new System.Windows.Controls.MenuItem();
            menuitemback.Header = "BACK";
            menuitemback.Click += new System.Windows.RoutedEventHandler(back_Click);
            menuitemback.MouseLeave += new System.Windows.Input.MouseEventHandler(menuitemback_MouseLeave);
            menuitemback.MouseLeave += new System.Windows.Input.MouseEventHandler(menuitemback_MouseLeave);
            menuitemback.Background = new LinearGradientBrush(Colors.Beige, Colors.DarkGray, 90);
            menuitemback.Foreground = new SolidColorBrush(Colors.Blue);

            StackPanel.Children.Add(menuitemback);

            //Animation
            Timer tmr = new Timer(new TimerCallback(Tmrcallback));
            tmr.Change(0, 50);
            StackPanel.Opacity = 0;

        }

        private void menuitem_MouseEnter(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem M = (MenuItem)sender;
            M.Foreground = new SolidColorBrush(Colors.Cornsilk);
            M.FontSize = ((M.FontSize) * 1.5);
        }

        private void menuitem_MouseLeave(object sender, System.Windows.RoutedEventArgs e)
        {
            M = (MenuItem)sender;
            M.FontSize = ((M.FontSize) / 1.5);
            color = Color.FromRgb(255, 255, 0);
            Timer tmr = new Timer(new TimerCallback(Tmrcallback1));
            tmr.Change(0, 100);
            M.Foreground = new SolidColorBrush(Colors.Black);
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

        private void back_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            populateAnimation();
        }


        private void ListRefresh()
        {
            treeView1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,new NoArgDelegate(ListRefresh2));
        }

        private void ListRefresh2()
        {
            treeView1.Items.Clear();
            populateCategoryList();
            populateAnimation();
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
            //M.ContextMenu.AllowDrop = true;

        }

        private void menuitem_ContextMenuRemove(object sender, System.Windows.RoutedEventArgs e)
        {
            if (parent == null)
            {
                Thread t = new Thread(new ParameterizedThreadStart(new OneArgDelegate(dataBase.removeCategory)));
                t.Start(M.Header);
                populateAnimation();
            }
            else
            {
                StackPanel.Children.Remove(M);
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

        ////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////

        public Window1()
        { 
            InitializeComponent();
            this.ContentRendered += Window1_ContentRendered;

            dataBase = new FeedDB();
            //dataBase.removeAllHistory();
            dataBase.getHistory("nu exista");
            tabs.TabStripPlacement = Dock.Bottom;
            browser = (System.Windows.Forms.WebBrowser)hh.Child;
            //browser.Width = 600;
            //browser.Height = (int)hh.Height;
            //browser.MaximumSize = browser.Size;
            browser.Navigate("http://www.google.ro/firefox?client=firefox-a&rls=org.mozilla:en-US:official");
            this.Closing += window1_close;

            dataBase.delegCatFeedChanged += ListRefresh;
            //button2.Click += button2_Click;
            // button3.Click += button3_Click;
            // button4.Click += button4_Click;
            button5.Click += button5_Click;
            button6.Click += button6_Click;
            button7.Click += button7_Click;
            button8.Click += button8_Click;
            btnO.Click += new RoutedEventHandler(btnO_Click);
            treeView1.MouseDoubleClick += treeView1_MouseDoubleClick;
            treeView1.SelectedItemChanged += treeView1_SelectedItemChanged;
            listBox1.SelectionChanged += listBox1_SelectionChanged;
            txtSearch.KeyDown += txtSearch_KeyDown;
            Button.Click += Button_Click;
            populateCategoryList();
            start();
            //StartAnimation();
            populateAnimation();
        
        }



        /// ///////////////////////////////////////////////////////////////////////////////
        /// ///////////////////////////////////////////////////////////////////////////////
        /// ///////////////////////////////////////////////////////////////////////////////
        private void StartAnimation()
        {
            Timer tmr = new Timer(new TimerCallback(callbackTmr));
            tmr.Change(0, 75);
            listBox1.Opacity = 0;
            tabs.Opacity = 0;
            originalWidth = (long)tabs.Width;
            tabs.Width = 0;
            hh.Visibility = Visibility.Hidden;
            
        }

         private void callbackTmr(object o)
        {
            listBox1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new NoArgDelegate(moveLeft));
            tabs.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new NoArgDelegate(move2));  
         }

        private void moveLeft()
        {
            listBox1.Opacity += 0.02;
        }

       
        private void move2()
        {
            tabs.Opacity += 0.02;
            if (tabs.Width < originalWidth)
            {
                tabs.Width += 10;
            }
            else
            {
                hh.Visibility = Visibility.Visible;  
            }
        }

        /// ///////////////////////////////////////////////////////////////////////////////
        /// ///////////////////////////////////////////////////////////////////////////////
        /// ///////////////////////////////////////////////////////////////////////////////



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


        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //rss 1 
            findPluginFor(dldFeed("http://www.networkworld.com/rss/3com.xml"));//->feed cu ??
            //http://www.networkworld.com/rss/3com.xml"));
        }


        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //rss 2.0
            findPluginFor(dldFeed("http://lsy22.blogsome.com/feed/"));//http://msdn.microsoft.com/rss.xml"));//feed cu categorii
            //http://lsy22.blogsome.com/feed/" ->feed cu ?? si cu comentarii
           
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            //atom1.0
            findPluginFor(dldFeed("http://www.atomenabled.org/atom.xml"));
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
            //else
            //    MessageBox.Show("You double-clicked the category: " + t.Header);
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

        private void treeView1_SelectedItemChanged(Object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            System.Windows.Controls.TreeViewItem t = (System.Windows.Controls.TreeViewItem)treeView1.SelectedItem;
            if (t == null) return;
            if (t.Parent != treeView1)
            {
                String feed = ((XmlFeed)(t.Header)).url;
                Thread t_nou = new Thread(new ParameterizedThreadStart(getHistoryAsync));
                t_nou.Start(feed);
            }
        }

        private delegate void DelegShowhist();
        private DelegShowhist delegShowHist;

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

        private void treeView1_MouseUp(Object sender, EventArgs args)
        {
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

        /*private void DoContextMenuRemoveJobFeed(Object url_o)
        {
            dataBase.removeFeed(url_o);
            WaitWindow ww = new WaitWindow(new NoArgDelegate(ListRefresh),
                "Removing the feed and its history from the database");
            ww.ShowDialog();
        }

        private void DoContextMenuRemoveJobCategory(Object catName_o)
        {
            dataBase.removeCategory(catName_o);
            WaitWindow ww = new WaitWindow(new NoArgDelegate(ListRefresh),
                "Removing the category, its feeds and their history from the database");
            ww.ShowDialog();
        }*/

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

