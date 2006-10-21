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

    public class ListHeader : StackPanel
    {
        StackPanel sp;
        Label l;
        public ListHeader(string s, double w)
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;   
            this.Background = Brushes.WhiteSmoke;   
            l=new Label();
            l.BitmapEffect = new System.Windows.Media.Effects.EmbossBitmapEffect();  
            Grid headerPanel = new Grid();
            headerPanel.BitmapEffect =new System.Windows.Media.Effects.DropShadowBitmapEffect();      
            headerPanel.RowDefinitions.Add(new RowDefinition() );
            ColumnDefinition clm = new ColumnDefinition();
            clm.Width = new GridLength(100);
            headerPanel.ColumnDefinitions.Add(clm);   
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition());   
            l.Content=s;
            l.Foreground = new SolidColorBrush(Colors.Blue);
            l.FontSize = 22;
            l.FontStyle = FontStyles.Oblique;
            headerPanel.Children.Add(l);
            Image myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory+@"\icons\AddressBook.ico"  );
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage; 
  
            headerPanel.Children.Add(myImage );
            myImage.HorizontalAlignment = HorizontalAlignment.Center ;   
            Grid.SetColumn(l, 1);
            l.Width = w * 16 / 17-100;
            l.HorizontalAlignment = HorizontalAlignment.Left;
            l.HorizontalContentAlignment = HorizontalAlignment.Left ;
            this.Children.Add(headerPanel); 
            sp=new StackPanel();
            sp.Visibility = Visibility.Collapsed;   
            this.Children.Add(sp);  
            this.MouseEnter += new System.Windows.Input.MouseEventHandler(ListHeader_MouseEnter);
            this.MouseUp += new System.Windows.Input.MouseButtonEventHandler(ListHeader_MouseUp); 
            
        }

        void ListHeader_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            sp.Visibility = Visibility.Collapsed;
        }

        void ListHeader_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            sp.Visibility = Visibility.Visible; 
        }



        
    }
    


    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

        
    public partial class Window1 : Window,PluginInterface.EventsClass  
    {
        string feedWithNewArticle;
        Random r = new Random();
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
        
        MenuItem M;
        Color color = new Color();
        protected string[] _styleList;
        protected int _styleIndex = 0;
        
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

        void populateCategoryList()
        {
            categoryList.Width = 290; 
            categoryList.Background = Brushes.Transparent;
            categoryList.BorderThickness = new Thickness(0);  
            foreach (string s in dataBase.getCategories())
            {
                TreeViewItem t=new TreeViewItem();
                t.MouseUp += new System.Windows.Input.MouseButtonEventHandler(t_MouseUp);
                
                ListHeader  dp=new ListHeader(s,categoryList.Width );
                t.Header=dp;
                t.Tag = dataBase; 
                foreach (XmlFeed feed in dataBase.getFeeds(s))
                {
                    Add(t,feed, new System.Windows.RoutedEventHandler(b_MouseDown));
                    
                }
                categoryList.Items.Add(t); 
            }
        }

        void t_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((TreeViewItem)sender).IsExpanded = !((TreeViewItem)sender).IsExpanded;
        }


        public void Add(TreeViewItem  t,XmlFeed feed, System.Windows.RoutedEventHandler handler)
        {

            
            Expander exp = new Expander();
               
            exp.Tag = feed; 
            t.Items.Add(exp);
            t.Width = categoryList.Width;
            exp.Width = t.Width -80;

            Grid itemStack = new Grid();
            ColumnDefinition clm = new ColumnDefinition();
            clm.Width = new System.Windows.GridLength(exp.Width * 1 / 2);
            itemStack.ColumnDefinitions.Add(clm);
            ColumnDefinition clm2 = new ColumnDefinition();
            clm2.Width = new System.Windows.GridLength(exp.Width * 1 / 2);
            itemStack.ColumnDefinitions.Add(clm2);
            itemStack.RowDefinitions.Add(new RowDefinition());
            itemStack.RowDefinitions.Add(new RowDefinition());
            itemStack.RowDefinitions.Add(new RowDefinition());
            itemStack.RowDefinitions.Add(new RowDefinition());
            exp.HorizontalContentAlignment = HorizontalAlignment.Right;
            exp.HorizontalAlignment = HorizontalAlignment.Center;
            exp.BorderThickness = new Thickness(1);
            exp.BorderBrush = Brushes.Gray;
            Button feedbtn = new Button();
            feedbtn.Click += handler;
            feedbtn.Background = Brushes.Transparent;
            feedbtn.BorderBrush = Brushes.Transparent;
            feedbtn.FontSize = 16;
            feedbtn.Foreground = Brushes.SteelBlue;
            feedbtn.Content = feed;
            exp.Header = feedbtn;
            exp.Content = itemStack;
            /*
            //Change Expander apparence
            try
            {
                string styleName = AppDomain.CurrentDomain.BaseDirectory + "\\Styles\\" + "Expander_nou.xaml";//Turqoiseskin_style.xaml;
                using (FileStream fs = new FileStream(styleName, FileMode.Open, FileAccess.Read))
                {
                    ResourceDictionary dictionary = (ResourceDictionary)XamlReader.Load(fs);
                    exp.Resources = dictionary;
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            */
            //sp.Children.Add(exp);

            TextBox b = new TextBox();
            b.Background = Brushes.Transparent;
            b.BorderThickness = new Thickness(0);
            b.Width = exp.Width / 2 - 1;
            b.Foreground = Brushes.BlueViolet;
            b.HorizontalAlignment = HorizontalAlignment.Right;
            b.HorizontalContentAlignment = HorizontalAlignment.Left;
            b.Tag = feed;
            b.Text = feed.feedName;
            b.LostFocus += new RoutedEventHandler(b_LostFocus); 
            itemStack.Children.Add(b);

            Grid.SetColumn(b, 1);
            Grid.SetRow(b, 0);
            TextBox bURL = new TextBox();
            bURL.Background = Brushes.Transparent;
            bURL.BorderThickness = new Thickness(0);
            bURL.Width = exp.Width / 2 - 1;
            bURL.Foreground = Brushes.BlueViolet;
            bURL.HorizontalAlignment = HorizontalAlignment.Right;
            bURL.HorizontalContentAlignment = HorizontalAlignment.Left;
            bURL.Tag = feed;
            bURL.Text = feed.url;
            itemStack.Children.Add(bURL);
            Grid.SetColumn(bURL, 1);
            Grid.SetRow(bURL, 1);

            ComboBox cmb = new ComboBox();
            cmb.Background = Brushes.Transparent;
            cmb.BorderThickness = new Thickness(0);
            cmb.Width = exp.Width / 2 - 1;
            foreach (string f in dataBase.getCategories())
            {
                cmb.Items.Add(f);
            }
            cmb.SelectedIndex = 0;
            itemStack.Children.Add(cmb);
            Grid.SetColumn(cmb, 1);
            Grid.SetRow(cmb, 2);
            Label lblName = new Label();
            lblName.Content = "Feed name:";
            itemStack.Children.Add(lblName);
            Grid.SetColumn(lblName, 0);
            Grid.SetRow(lblName, 0);

            Label lblAdress = new Label();
            lblAdress.Content = "Feed adress:";
            itemStack.Children.Add(lblAdress);
            Grid.SetColumn(lblAdress, 0);
            Grid.SetRow(lblAdress, 1);

            Label lblCat = new Label();
            lblCat.Content = "Category:";
            itemStack.Children.Add(lblCat);
            Grid.SetColumn(lblCat, 0);
            Grid.SetRow(lblCat, 2);

            Button delete = new Button(); 
            itemStack.Children.Add(delete);
            Grid.SetColumn(delete, 0);
            Grid.SetRow(delete, 3);

            Image myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\delete.png");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            delete.Content = myImage;
            delete.BorderBrush=new SolidColorBrush(Colors.Transparent);   
            delete.Background = new SolidColorBrush(Colors.Transparent);   
            delete.Width = 30;
            delete.Click += new RoutedEventHandler(delete_Click);
            MatrixTransform m=new MatrixTransform(-1,0,0,1,200,0);
            MatrixTransform m1 = new MatrixTransform(-1, 0, 0, 1, 180, 0);
            exp.RenderTransform = m;
            feedbtn.RenderTransform = m1;
            MatrixTransform m2 = new MatrixTransform(-1, 0, 0, 1, 210, 0);
            itemStack.RenderTransform = m2; 
        }

        void delete_Click(object sender, RoutedEventArgs e)
        {
            Grid g = (Grid)(((Button)sender).Parent);
            Expander ex = (Expander)g.Parent;
            TreeViewItem t = (TreeViewItem)ex.Parent;
            ((FeedDB)t.Tag).removeFeed(((XmlFeed)ex.Tag).url);
            if (dataBase.delegCatFeedChanged != null) dataBase.delegCatFeedChanged(); 
        }

        void b_LostFocus(object sender, RoutedEventArgs e)
        {
            Grid gr = (Grid)(((TextBox)sender).Parent);
            Expander s = (Expander)gr.Parent;
            XmlFeed f = (XmlFeed)s.Tag;
            TreeViewItem t = (TreeViewItem)s.Parent;
            ((FeedDB)(t.Tag)).renameFeed(f.url, ((TextBox)sender).Text);   
        }






        void b_MouseDown(object sender, RoutedEventArgs e)
        {
            String feed = ((XmlFeed)((Button)sender).Content ).url;
            Thread t_nou = new Thread(new ParameterizedThreadStart(getHistoryAsync));
            t_nou.Start(feed);
            listBox1.SelectedIndex = 0;
            ((Expander)((Button)sender).Parent).BorderThickness = new Thickness(1) ;
            ((Expander)((Button)sender).Parent).BorderBrush = Brushes.Gray;
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
            categoryList.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,new NoArgDelegate(ListRefresh2));
        }

        private void ListRefresh2()
        {
            categoryList.Items.Clear();
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
            if (true)
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
            if (true)
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

             
            Image myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\search.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage; 
            WrapPanel wp=new WrapPanel();
            Label l=new Label();
            l.Content ="Search";
            wp.Children.Add(myImage);
            wp.Children.Add(l);
            hist.Header = wp;

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\cat.ico");
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
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\latest.ico");
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
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\reload.png");
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
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\cat.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            button5.Icon = myImage;


            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\user.ico");
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
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\import.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            button7.Icon = myImage;

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\skin.ico");
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
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\bomb.png");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            button6.Icon = myImage;

            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\user.ico");
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
            myBitmapImage.UriSource = new Uri(Environment.CurrentDirectory + @"\icons\about.ico");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            button8.Icon = myImage;

            this.ContentRendered += Window1_ContentRendered;
            Button cc = new Button();
            dataBase = new FeedDB();
 
            //dataBase.removeAllHistory();
            dataBase.RegisterEventHandler(this); 
            dataBase.getHistory("threre isn't any");
            this.Closing += window1_close;
            dataBase.delegCatFeedChanged += ListRefresh;
            button5.Click += button5_Click;
            button6.Click += button6_Click;
            button7.Click += button7_Click;
            button8.Click += button8_Click;
            btnProgramOptions.Click += new RoutedEventHandler(btnProgramOptions_Click);  
            btnO.Click += new RoutedEventHandler(btnO_Click);
            categoryList.MouseDoubleClick += treeView1_MouseDoubleClick;
            listBox1.SelectionChanged += listBox1_SelectionChanged;
            txtSearch.KeyDown += txtSearch_KeyDown;
            Button.Click += Button_Click;
            populateCategoryList();
             
        }

        public void FeedDownloaded(string feed)
        {
            feedWithNewArticle = feed; 
            categoryList.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new NoArgDelegate(NewMethod));
        }

        private void NewMethod()
        {
            //MessageBox.Show("DOWN " + feedWithNewArticle );
            foreach (TreeViewItem c in categoryList.Items)
            {

                foreach (Expander f in c.Items)
                {

                    if (((XmlFeed)f.Tag).url == feedWithNewArticle)
                    {
                        f.BorderBrush=Brushes.Blue;
                        f.BorderThickness = new Thickness(2);  
                    }
                }
            }
        }

        public void CategoryAdded(string c)
        { }

        public void NewFeedAdded(string feed)
        { }  


        void btnProgramOptions_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("For now, feedFusion has no Options Dialog of its own, relying entierly on plugin options.","Info",MessageBoxButton.OK,MessageBoxImage.Information);  
        }

        void Window1_ContentRendered(Object sender, EventArgs e)
        {
            SetFrameBackground();
            pManager = new pluginManager(ToolBar, dataBase, this);
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
            Timer t = new Timer(new TimerCallback(refreshTimerCallback), null, 0, 60 * 15 * 1000); 
      
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
            progressBar.Maximum = allFeeds.Length;
            foreach (XmlFeed feed in allFeeds)
                //ThreadPool.QueueUserWorkItem(new WaitCallback(downloadByDownList), feed.url);
                waitQueue.Enqueue(feed.url);
            Thread tWaitQueue = new Thread(new ThreadStart(checkWaitQueue));
            tWaitQueue.Start();
        }

        private void SetFrameBackground()
        {
            ImageSource s = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\Icons\\bal.png"));
            ImageBrush ib = new ImageBrush(s);
            ib.Opacity = 0.5;
            ib.Stretch = Stretch.Fill;
            this.Background = ib;
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
            if (!gasit) setBrowserDocumentText("<html><body><h2><font color=\"red\">There is no plugin installed for this RSS type.</font></h2></body></html>");
        }

        delegate void DelegSetDocumentText(Object parsedHTML_o);
        void setBrowserDocumentText(Object parsedHTML_o)
        {

            double d = r.NextDouble();
            string htmlFile = Environment.CurrentDirectory + @"\temp\" + d.ToString() + ".htm";
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
                MessageBox.Show(e.Message, url);
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
            try { w.ShowDialog(); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            Window3 w = new Window3(this._styleList[_styleIndex]);
            w.ShowDialog();
        }

        private void treeView1_MouseDoubleClick(Object sender, EventArgs e)
        {
            System.Windows.Controls.TreeViewItem t = (System.Windows.Controls.TreeViewItem)categoryList.SelectedItem;
            if (t == null) return;
            if (t.Parent != categoryList)
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
            lock (lockProgressBar)
            {
                progressBar.Value = progressBar.Value + 1;
                if (progressBar.Maximum == progressBar.Value)
                    progressBar.Visibility  = Visibility.Hidden ;
            }
        }


        private void treeView1_ContextMenuRemove(Object sender, EventArgs args)
        {
            if (categoryList.SelectedItem != null)
                if (((TreeViewItem)categoryList.SelectedItem).Parent != categoryList)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(new OneArgDelegate(dataBase.removeFeed)));
                    t.Start(((XmlFeed)(((TreeViewItem)categoryList.SelectedItem).Header)).url);
                    ((TreeViewItem)((TreeViewItem)categoryList.SelectedItem).Parent).Items.Remove((TreeViewItem)categoryList.SelectedItem);
                }
                else
                {
                    Thread t = new Thread(new ParameterizedThreadStart(new OneArgDelegate(dataBase.removeCategory)));
                    t.Start((String)(((TreeViewItem)categoryList.SelectedItem).Header));
                    categoryList.Items.Remove((TreeViewItem)categoryList.SelectedItem);
                }
        }

        private void treeView1_ContextMenuRename(Object sender, EventArgs args)
        {
            if (categoryList.SelectedItem != null)
                if (((TreeViewItem)categoryList.SelectedItem).Parent != categoryList)
                {
                    RenameWindow rw = new RenameWindow(((XmlFeed)(((TreeViewItem)categoryList.SelectedItem).Header)).feedName, this._styleList[_styleIndex]);
                    rw.ShowDialog();
                    if (rw.DialogResult == true)
                    {
                        String url = ((XmlFeed)(((TreeViewItem)categoryList.SelectedItem).Header)).url;
                        String newName = rw.txtNewName.Text.Trim();
                        dataBase.renameFeed(url, newName);
                    }
                }
                else
                {
                    RenameWindow rw = new RenameWindow((String)(((TreeViewItem)categoryList.SelectedItem).Header), this._styleList[_styleIndex]);
                    rw.ShowDialog();
                    if (rw.DialogResult == true)
                    {
                        String oldName = (String)(((TreeViewItem)categoryList.SelectedItem).Header);
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

