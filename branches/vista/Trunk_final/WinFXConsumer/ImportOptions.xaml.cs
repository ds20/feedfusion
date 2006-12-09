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

namespace WinFXConsumer
{
    /// <summary>
    /// Interaction logic for ImportWin.xaml
    /// </summary>

    public partial class ImportWin : System.Windows.Window
    {
        Indexer.FeedDB db;
        string defaultCat;
        bool add;
        public ImportWin(Indexer.FeedDB db):this() 
        {
            this.db = db;
        } 

        public ImportWin()
        {
            InitializeComponent();
            foreach (string cat in db.getCategories())
                comboBox1.Items.Add(cat);
            defaultCat = "";
            add = false;
            comboBox1.SelectionChanged += new SelectionChangedEventHandler(comboBox1_SelectionChanged);
            checkBox1.Checked += new RoutedEventHandler(checkBox1_Checked);
            checkBox1.Unchecked += new RoutedEventHandler(checkBox1_Unchecked);
        }

        void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            add = false;
        }

        void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            add = true;
        }

        void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            defaultCat = (string)comboBox1.SelectedValue;
        }

    }
}