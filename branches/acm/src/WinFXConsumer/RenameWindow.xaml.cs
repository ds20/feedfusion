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

namespace WinFXConsumer
{
    /// <summary>
    /// Interaction logic for RenameWindow.xaml
    /// </summary>

    public partial class RenameWindow : Window
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


        public RenameWindow(String oldName,string styleName)
        {
            InitializeComponent();
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            txtNewName.Text = oldName;
            txtNewName.Focus();
            this.DiscoverStyles();
            if (this._styleList == null || this._styleList.Length == 0)
            {
                //MessageBox.Show("No skins available");//eventual mesaj...daca nu exista alta cale(log sau ceva)
                return;
            }
            this.ApplyStyle(styleName);
        }

        private void button1_Click(Object sender, EventArgs e)
        {
            this.DialogResult = true;
        }

        private void button2_Click(Object sender, EventArgs e)
        {
            this.DialogResult = false;
        }

    }
}