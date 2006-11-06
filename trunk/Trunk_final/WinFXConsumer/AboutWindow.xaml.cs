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
using System.IO;
using System.Windows.Markup;
using System.Diagnostics; 
namespace WinFXConsumer
{
    /// <summary>
    /// Interaction logic for Window3.xaml
    /// </summary>

    public partial class Window3 : Window
    {
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

        public Window3(string styleName)
        {
            InitializeComponent();
            this.DiscoverStyles();
            if (this._styleList == null || this._styleList.Length == 0)
            {
                return;
            }
            this.ApplyStyle(styleName);
            tidav.Click += new RoutedEventHandler(tidav_Click);
            homepage.Click += new RoutedEventHandler(homepage_Click);
            group.Click += new RoutedEventHandler(group_Click);
        }

        void group_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://groups.google.com/group/feedfusion-discuss"); 
        }

        void homepage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://feedfusion.googlepages.com/home");  
        }

        void tidav_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://tidav.deviantart.com/");  
        }


    }
}