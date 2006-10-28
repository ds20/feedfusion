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
using System.Threading;
using Indexer;
using System.IO;
using System.Windows.Markup;

namespace WinFXConsumer
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>

    public partial class Window2 : Window
    {
        FeedDB dataBase;
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


        public Window2(FeedDB dataBase,string styleName)
        {
            InitializeComponent();
            this.dataBase = dataBase;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            LoadData();
            this.DiscoverStyles();
            if (this._styleList == null || this._styleList.Length == 0)
            {
                //MessageBox.Show("No skins available");//eventual mesaj...daca nu exista alta cale(log sau ceva)
                return;
            }
            this.ApplyStyle(styleName);
        }
        
        public Window2(FeedDB dataBase, string styleName, string feedURL):this(dataBase,styleName)
        {
            textBox1.Text = feedURL;  
        }

        public void button1_Click(object sender, RoutedEventArgs e)
        { 
            string cat=comboBox1.Text.Trim();
            string rss=textBox1.Text.Trim();
            String customName = txtCustomName.Text.Trim();
            if (rss == "" || cat == "") return;

            
            if (!dataBase.categoryExists(cat) )
            {
                MessageBoxResult accept = MessageBox.Show("The category you desire is not available. Do you wish to create it?", "Option", MessageBoxButton.YesNo,MessageBoxImage.Question );
                if (accept==MessageBoxResult.Yes) dataBase.addCategory(cat);
                else
                {
                    //IESI DIN FUNCTIE!!!! 
                    return;
                }
            }
            Thread t = new Thread(new ParameterizedThreadStart(DoButton1Job));
            t.ApartmentState = ApartmentState.STA;  
            Object[] obj = new Object[] { cat, rss, customName };
            t.Start(obj);
            this.DialogResult = true;
        }
        [System.STAThreadAttribute] 
        public void DoButton1Job(Object vect_o)
        {
            String cat = (String)(((Object[])vect_o)[0]), rss = (String)(((Object[])vect_o)[1]), customName = (String)(((Object[])vect_o)[2]);
            dataBase.addFeed(cat, rss, customName);
        }

        public void button2_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public void LoadData()
        {
            String[] categorii = dataBase.getCategories();
            comboBox1.Items.Clear();
            for (int i = 0; i < categorii.Length; i++)
                comboBox1.Items.Add(categorii[i]);
        }


    }
}