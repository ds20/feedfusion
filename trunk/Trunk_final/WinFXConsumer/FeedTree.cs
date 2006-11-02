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
public class FeedTree : System.Windows.Controls.TreeView
{
    public delegate void deleteFeedDelegate(XmlFeed f);
    public event deleteFeedDelegate deleteClick;

    public delegate void feedRenameDelegate(XmlFeed f,string newName);
    public event feedRenameDelegate feedRename;

    public delegate void viewFeedDelegate(XmlFeed f);
    public event viewFeedDelegate viewFeed;

    public FeedTree()
        : base()
    {

    }
    
    public void Populate(TreeViewItem t, string[] categories)
    {
        TreeViewItem catNode;
        while  (t.Items.Count>0 )
       {
            catNode = (TreeViewItem)t.Items[0];
            t.Items.RemoveAt(0); 
            ListHeader l = new ListHeader((string)(catNode.Tag), this.Width-60);
            catNode.Header = l;
            this.Items.Add(catNode);
            foreach (TreeViewItem feedNode in catNode.Items)
            {
                AddVisualTree(feedNode, categories);
                feedNode.IsExpanded = true; 
            }
            catNode.IsExpanded = true;
            
        }
    }

    void t_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ((TreeViewItem)sender).IsExpanded = !((TreeViewItem)sender).IsExpanded;
    }

    public void AddVisualTree(TreeViewItem feedNode, string[] categories)
    {
        XmlFeed feed = (XmlFeed)feedNode.Tag;
        Expander exp = new Expander();
        feedNode.Header=exp;
        feedNode.Width = this.Width-20;
        exp.Width = feedNode.Width - 80;

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
        //exp.HorizontalContentAlignment = HorizontalAlignment.Right;
        //exp.HorizontalAlignment = HorizontalAlignment.Center;
        exp.BorderThickness = new Thickness(1);
        exp.BorderBrush = Brushes.Gray;
        Button feedbtn = new Button();
        feedbtn.Click += b_MouseDown;
        feedbtn.Background = Brushes.Transparent;
        feedbtn.BorderBrush = Brushes.Transparent;
        feedbtn.FontSize = 16;
        feedbtn.Foreground = Brushes.SteelBlue;
        feedbtn.Content = feed;
        exp.Header = feedbtn;
        exp.Content = itemStack;
        /*
        //Change Expander apearence
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

        TextBox btnFeedName = new TextBox();
        btnFeedName.Background = Brushes.Transparent;
        btnFeedName.BorderThickness = new Thickness(0);
        btnFeedName.Width = exp.Width / 2 - 1;
        btnFeedName.Foreground = Brushes.BlueViolet;
        //b.HorizontalAlignment = HorizontalAlignment.Right;
        //b.HorizontalContentAlignment = HorizontalAlignment.Left;
        btnFeedName.Tag = feed;
        btnFeedName.Text = feed.feedName;
        btnFeedName.LostFocus += new RoutedEventHandler(b_LostFocus);
        itemStack.Children.Add(btnFeedName);

        Grid.SetColumn(btnFeedName, 1);
        Grid.SetRow(btnFeedName, 0);
        TextBox bURL = new TextBox();
        bURL.Background = Brushes.Transparent;
        bURL.BorderThickness = new Thickness(0);
        bURL.Width = exp.Width / 2 - 1;
        bURL.Foreground = Brushes.BlueViolet;
        //bURL.HorizontalAlignment = HorizontalAlignment.Right;
        //bURL.HorizontalContentAlignment = HorizontalAlignment.Left;
        bURL.Tag = feed;
        bURL.Text = feed.url;
        itemStack.Children.Add(bURL);
        Grid.SetColumn(bURL, 1);
        Grid.SetRow(bURL, 1);

        ComboBox cmb = new ComboBox();
        cmb.Background = Brushes.Transparent;
        cmb.BorderThickness = new Thickness(0);
        cmb.Width = exp.Width / 2 - 1;
        foreach (string f in categories)
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
        myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\delete.png");
        myBitmapImage.DecodePixelWidth = 20;
        myBitmapImage.EndInit();
        myImage.Source = myBitmapImage;
        delete.Content = myImage;
        delete.BorderBrush = new SolidColorBrush(Colors.Transparent);
        delete.Background = new SolidColorBrush(Colors.Transparent);
        delete.Width = 30;
        delete.Click += new RoutedEventHandler(delete_Click);
        //MatrixTransform m = new MatrixTransform(-1, 0, 0, 1, 180, 0);
        //MatrixTransform m1 = new MatrixTransform(-1, 0, 0, 1, 160, 0);
        //exp.RenderTransform = m;
        //feedbtn.RenderTransform = m1;
        //MatrixTransform m2 = new MatrixTransform(-1, 0, 0, 1, 190, 0);
        //itemStack.RenderTransform = m2;
    }

    void delete_Click(object sender, RoutedEventArgs e)
    {
        Grid g = (Grid)(((Button)sender).Parent);
        Expander ex = (Expander)g.Parent;
        TreeViewItem t = (TreeViewItem)ex.Parent;
        if (deleteClick!=null) deleteClick(((XmlFeed)(((TreeViewItem)ex.Parent).Tag)));
    }

    void b_LostFocus(object sender, RoutedEventArgs e)
    {
        Grid gr = (Grid)(((TextBox)sender).Parent);
        Expander s = (Expander)gr.Parent;
        XmlFeed f = (XmlFeed)((TreeViewItem)s.Parent).Tag;
        if (feedRename!=null) feedRename(f,((TextBox)sender).Text);  
    }


    void b_MouseDown(object sender, RoutedEventArgs e)
    {
        XmlFeed feed = (XmlFeed)((Button)sender).Content;
        if (viewFeed!=null) viewFeed(feed);
        ((Expander)((Button)sender).Parent).BorderThickness = new Thickness(1);
        ((Expander)((Button)sender).Parent).BorderBrush = Brushes.Gray;
    }

}


public class ListHeader : StackPanel
{
    StackPanel sp;
    Label l;
    public ListHeader(string s, double w)
    {
        //this.HorizontalAlignment = HorizontalAlignment.Left;
        this.Background = Brushes.WhiteSmoke;
        l = new Label();
        l.BitmapEffect = new System.Windows.Media.Effects.EmbossBitmapEffect();
        Grid headerPanel = new Grid();
        headerPanel.BitmapEffect = new System.Windows.Media.Effects.DropShadowBitmapEffect();
        headerPanel.RowDefinitions.Add(new RowDefinition());
        ColumnDefinition clm = new ColumnDefinition();
        clm.Width = new GridLength(100);
        headerPanel.ColumnDefinitions.Add(clm);
        headerPanel.ColumnDefinitions.Add(new ColumnDefinition());
        l.Content = s;
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
        myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\AddressBook.ico");
        myBitmapImage.DecodePixelWidth = 20;
        myBitmapImage.EndInit();
        myImage.Source = myBitmapImage;

        headerPanel.Children.Add(myImage);
        //myImage.HorizontalAlignment = HorizontalAlignment.Center;
        Grid.SetColumn(l, 1);
        l.Width = w * 16 / 17 - 100;
        //l.HorizontalAlignment = HorizontalAlignment.Left;
        //l.HorizontalContentAlignment = HorizontalAlignment.Left;
        this.Children.Add(headerPanel);
        sp = new StackPanel();
        sp.Visibility = Visibility.Collapsed;
        this.Children.Add(sp);
        this.MouseEnter += new System.Windows.Input.MouseEventHandler(ListHeader_MouseEnter);
        this.MouseUp += new System.Windows.Input.MouseButtonEventHandler(ListHeader_MouseUp);
        this.Visibility = Visibility.Visible;  

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

