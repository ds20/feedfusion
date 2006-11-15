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
//using System.Windows.Shapes;
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
            //textBox1.Text = @"http://hosting.opml.org/dave/spec/subscriptionList.opml"; 
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
            System.Diagnostics.Process.Start(Path.GetTempPath() + "\\opml.htm");            
        }

        public void button1_click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text.Trim().Length > 5)
            {
                opml o = new opml();
                string fileName = Path.GetTempPath() + "\\opml.xml";
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
        }

        public void button2_click(object sender, RoutedEventArgs e)
        {
            opml o = new opml();

            TreeViewItem root = new TreeViewItem();
            root = o.getRootDataBase(database);

            string fileName = Path.GetTempPath() + "\\baza.opml";
            using (StreamWriter sw = File.CreateText(fileName))
            {
                sw.Write(o.makeOpml(database));
            }

            treeView1.Items.Clear();
            treeView1.Items.Add(root);
            MessageBox.Show("Opml File saved to: " + Path.GetTempPath()+"\\baza.opml .","Success",MessageBoxButton.OK,MessageBoxImage.Information); 
        }

        public void button3_click(object sender, RoutedEventArgs e)
        {
            //ThreadPool.QueueUserWorkItem(new WaitCallback(DoButton3Job), textBox1.Text.Trim());
            /*Thread t = new Thread(new ParameterizedThreadStart(DoButton3Job));
            t.TrySetApartmentState(ApartmentState.STA);
            t.Start(textBox1.Text.Trim());*/

            //DoButton3Job(textBox1.Text.Trim());
            if (textBox1.Text.Trim().Length > 5)
            {
                Window1.OneArgDelegate delegOpml = DoButton3Job;
                WaitWindow w = new WaitWindow(delegOpml, textBox1.Text.Trim(), "Adding OPML feeds to database");
                w.ShowDialog();
                //w.Visibility = Visibility.Hidden;   
                //progressBar.Visibility = Visibility.Visible;
                progressBar.IsIndeterminate = true;
               
                
            }
        }

        private void DoButton3Job(Object url_o)
        {
            opml o = new opml();
            o.import(url_o, database);
            MessageBox.Show("OPML import completed","FeedFusion Import"); 
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
        //            string Name =  System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\blablabla.txt";
        //            StreamWriter sw = File.CreateText(Name);
        //            sw.Write(i.Header);
        //            sw.Close();*/
        //        }
                
        //    }
        //}
    }
}