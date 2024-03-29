using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Indexer;
using System.Resources;
using System.IO;
using System.Windows.Markup;

namespace WinFXConsumer
{
    /// <summary>
    /// Interaction logic for Window3.xaml
    /// </summary>

    public partial class pluginConfigWindow : Window
    {

        FeedDB database;
        pluginManager plugins;
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


        public void ConfigurePlugin(object sender,RoutedEventArgs args)
        {
            if (listBox1.SelectedIndex != -1)
                plugins.plugins[listBox1.SelectedIndex].showConfiguration();
        }
        
        private void OnSelection(object sender, SelectionChangedEventArgs aArgs)
        {
            if (listBox1.SelectedIndex != -1)
                label1.Content = plugins.plugins[listBox1.SelectedIndex].description();
        }

        public pluginConfigWindow(FeedDB f, pluginManager pManager,string styleName)
        {
            InitializeComponent();
            database = f; plugins = pManager;
            listBox1.SelectionChanged += OnSelection;
            RefreshList();
            this.Closing += pluginConfig_Close;
            this.DiscoverStyles();
            if (this._styleList == null || this._styleList.Length == 0)
            {
                //MessageBox.Show("No skins available");//eventual mesaj...daca nu exista alta cale(log sau ceva)
                return;
            }
            this.ApplyStyle(styleName);
        }

        void RefreshList()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < plugins.number; i++) listBox1.Items.Add(plugins.plugins[i].name());
        }

        void MoveDown(object sender, RoutedEventArgs e)
        {
            int crt = listBox1.SelectedIndex;
            if (crt != -1 && crt != listBox1.Items.Count - 1)  //selection is not the last element
            {
                Object a, b;
                //swap plugins
                a = plugins.plugins[crt]; b = plugins.plugins[crt+1];
                plugins.plugins[crt] = (PluginInterface.rssInterface)b; plugins.plugins[crt + 1] = (PluginInterface.rssInterface)a;
                //swap plugins in FileNameList - to keep the order for next program run
                a = plugins.pluginCollection[crt]; b = plugins.pluginCollection[crt + 1];
                plugins.pluginCollection[crt] = (String)b; plugins.pluginCollection[crt + 1] = (String)a;
                RefreshList();
                listBox1.SelectedIndex = crt + 1;
            }
        }

        void MoveUp(object sender, RoutedEventArgs e)
        {
            int crt = listBox1.SelectedIndex;
            if (crt != -1 && crt != 0)  //selection is not the first element
            {
                Object a, b;
                //swap plugins
                a = plugins.plugins[crt]; b = plugins.plugins[crt - 1];
                plugins.plugins[crt] = (PluginInterface.rssInterface)b; plugins.plugins[crt - 1] = (PluginInterface.rssInterface)a;
                //swap plugins in FileNameList - to keep the order for next program run
                a = plugins.pluginCollection[crt]; b = plugins.pluginCollection[crt - 1];
                plugins.pluginCollection[crt] = (String)b; plugins.pluginCollection[crt - 1] = (String)a;
                RefreshList();
                listBox1.SelectedIndex = crt - 1;
            }
        }

        void pluginConfig_Close(Object sender, EventArgs e)
        {
            Properties.Settings.Default.PluginCollection = plugins.pluginCollection;
            Properties.Settings.Default.Save();
            //savePluginCollectionSetting();
        }

        void savePluginCollectionSetting()
        {
            
            /*ResourceWriter writer = new ResourceWriter(Environment.CurrentDirectory + "\\PluginPriority.resources");
            writer.AddResource("PluginCollection", Properties.Settings.Default.PluginCollection);
            writer.Close();*/
        }

    }
}