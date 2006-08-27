using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using Xml.Opml;
using Indexer;
using System.IO;
using System.Threading;
using System.Windows.Markup;

namespace WinFXConsumer
{
    /// <summary>
    /// Interaction logic for Window3.xaml
    /// </summary>

    public partial class OpmlWindow : Window
    {
        FeedDB database;
        protected string[] _styleList;

        protected void DiscoverStyles()
        {
            _styleList = Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*_style.xaml");
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
            }

            catch (Exception ex)
            {
                //ErrorLog.LogHandledException(ex);
            }

        }


        public OpmlWindow(FeedDB f,string styleName)
        {
            InitializeComponent();
            database = f;
            button1.Click += button1_click;
            button2.Click += button2_click;
            button3.Click += button3_click;
            button4.Click += button4_click;
            this.DiscoverStyles();
            if (this._styleList == null || this._styleList.Length == 0)
            {
                //MessageBox.Show("No skins available");//eventual mesaj...daca nu exista alta cale(log sau ceva)
                return;
            }
            this.ApplyStyle(styleName);
        }

        public void button4_click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Environment.CurrentDirectory + "\\opml.htm");            
        }

        public void button1_click(object sender, RoutedEventArgs e)
        {
            opml o = new opml();
            string fileName = Environment.CurrentDirectory + "\\opml.xml";
            XmlTextWriter w = new XmlTextWriter(fileName, Encoding.UTF8);
            XmlDocument doc = Window1.dldFeed(textBox1.Text.Trim());
            //http://hosting.opml.org/dave/spec/states.opml    
            //http://hosting.opml.org/dave/spec/subscriptionList.opml
            doc.Save(w);
            w.Flush();
            w.Close();
            button4.Visibility = Visibility.Visible;

            treeView1.Items.Clear();
            treeView1.Items.Add(o.Parse(fileName));
        }

        public void button2_click(object sender, RoutedEventArgs e)
        {
            opml o = new opml();

            TreeViewItem root = new TreeViewItem();
            root = o.getRootDataBase(database);
            
            string fileName = Environment.CurrentDirectory + "\\baza.opml";
            using (StreamWriter sw = File.CreateText(fileName))
            {
                sw.Write(o.makeOpml(database));
            }

            treeView1.Items.Clear();
            treeView1.Items.Add(root);        
        }

        public void button3_click(object sender, RoutedEventArgs e)
        {
            //ThreadPool.QueueUserWorkItem(new WaitCallback(DoButton3Job), textBox1.Text.Trim());
            /*Thread t = new Thread(new ParameterizedThreadStart(DoButton3Job));
            t.TrySetApartmentState(ApartmentState.STA);
            t.Start(textBox1.Text.Trim());*/

            //DoButton3Job(textBox1.Text.Trim());

            Window1.OneArgDelegate delegOpml = DoButton3Job;
            WaitWindow w = new WaitWindow(delegOpml, textBox1.Text.Trim(), "Adding OPML feeds to database");
            w.ShowDialog();
        }

        private void DoButton3Job(Object url_o)
        {
            TreeViewItem root = new TreeViewItem();
            String enc;

            opml o = new opml();
            string fileName = Environment.CurrentDirectory + "\\opml.xml";
            
            XmlDocument doc = new XmlDocument();
            try { doc.Load((String)url_o); }
            catch (Exception e) { MessageBox.Show(e.Message, (String)url_o); }

            XmlNode xn = doc.FirstChild;
            while (xn!=null && xn.NodeType != XmlNodeType.XmlDeclaration)
                xn = xn.NextSibling;
            if (xn == null || xn.NodeType != XmlNodeType.XmlDeclaration) enc = "UTF-8";     //default encoding
            else
            {
                enc = ((XmlDeclaration)xn).Encoding;
                if (enc == null || enc == "") enc = "UTF-8";   //default encoding
            }

            XmlTextWriter w = new XmlTextWriter(fileName, Encoding.GetEncoding(enc));
            doc.Save(w);
            w.Flush();
            w.Close();

            int nrFeeds = 0;
            root = o.Parse(fileName,ref nrFeeds);

           /* string Name = Environment.CurrentDirectory + "\\blablabla.txt";
            StreamWriter sw = File.CreateText(Name);
            */
            //add(root, root, sw);
            XmlFeed[] feeds = new XmlFeed[nrFeeds];
            int i=0;
            TreeToVector(root,root, feeds,ref i);
            //MessageBox.Show(nrFeeds.ToString());
            database.addFeeds(feeds);
            //MessageBox.Show("gata add...");
            //sw.Close();          
        }

        public void TreeToVector(TreeViewItem node,TreeViewItem parent, XmlFeed[] feeds,ref int i)
        {
                Feed f = new Feed();
                f=(Feed)node.Tag;                
                if (f.IsLeaf==true)
                {
                    if (f.XmlUrl == "")
                        MessageBox.Show("Nu am ce adresa sa adaug");
                    else
                    {
                        feeds[i] = new XmlFeed();
                        feeds[i].catName =(string) parent.Header;
                        feeds[i].feedName = f.ToString();
                        feeds[i].url = f.XmlUrl;
                        i++;
                    }
                }
                else
                {
                    foreach (TreeViewItem it in node.Items)
                    {
                        TreeToVector(it, node, feeds, ref i);
                        /*
                        string Name = Environment.CurrentDirectory + "\\blablabla.txt";
                        StreamWriter sw = File.CreateText(Name);
                        sw.Write(i.Header);
                        sw.Close();*/
                    }

                }

        }
        
        //public void add(TreeViewItem node, TreeViewItem parent,StreamWriter sw)
        //{
        //    Feed f=new Feed();
        //    f=(Feed)node.Tag;
        //    string cat = (string)parent.Header;
        //    if (f.IsLeaf==true)
        //    {
        //        if (!database.categoryExists(cat))
        //        {
        //            MessageBoxResult accept = MessageBox.Show("Nu exista categoria dorita de dumneavoastra.Creem?", "?", MessageBoxButton.YesNo);
        //            if (accept == MessageBoxResult.Yes) database.addCategory(cat);
        //            else
        //            {
        //                //IESI DIN FUNCTIE!!!! 
        //                return;
        //            }
        //            //MessageBox.Show("nu exista cat!!!");
        //        }
        //        if(f.XmlUrl=="")
        //            MessageBox.Show("Nu am ce adresa sa adaug");
        //        else
        //           //sw.WriteLine("adaug feedul html: {0}", f.XmlUrl);
        //             database.addFeed(cat, f.XmlUrl);
        //        //MessageBox.Show("adaug!!!");
        //    }
        //    else
        //    {
        //        //MessageBox.Show((string)node.Header);
        //        foreach (TreeViewItem i in node.Items)
        //        {
        //            add(i,node,sw);
        //            /*
        //            string Name = Environment.CurrentDirectory + "\\blablabla.txt";
        //            StreamWriter sw = File.CreateText(Name);
        //            sw.Write(i.Header);
        //            sw.Close();*/
        //        }
                
        //    }
        //}
    }
}