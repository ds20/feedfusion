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
    private string[] cats;
    public delegate void deleteFeedDelegate(XmlFeed f);
    public event deleteFeedDelegate deleteClick;

    public delegate void feedRenameDelegate(XmlFeed f,string newName);
    public event feedRenameDelegate feedRename;

    public delegate void viewFeedDelegate(XmlFeed f);
    public event viewFeedDelegate viewFeed;

    public delegate void categoryClick(string cat);
    public event categoryClick categoryOptions;

    public delegate void categoryNameChange(string cat, string newCat);
    public event categoryNameChange categoryNameChanged;

    public FeedTree()
        : base()
    {
        this.HorizontalAlignment = HorizontalAlignment.Stretch;
        this.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(FeedTree_SelectedItemChanged); 

    }

    void FeedTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
    }

    public void MarkUnreadFeed(Object FeedWithNewItems)
    {
        foreach (TreeViewItem c in this.Items)
        {
            foreach (TreeViewItem f in c.Items)
            {
                if (((XmlFeed)f.Tag).url == ((string)FeedWithNewItems))
                {

                    ((FeedExpander)(f.Header)).myImage.Visibility = Visibility.Visible;    
                }
            }

        }

    }
    
    public void Populate(TreeViewItem t, string[] categories)
    {
        TreeViewItem catNode;
        cats=categories;
        while  (t.Items.Count>0 )
       {
            catNode = (TreeViewItem)t.Items[0];
            t.Items.RemoveAt(0); 
            ListHeader l = new ListHeader((string)(catNode.Tag),this.Width-60);
            l.myImage.MouseDown += new System.Windows.Input.MouseButtonEventHandler(myImage_MouseDown);
            l.editCatName.LostFocus += new RoutedEventHandler(editCatName_LostFocus);  
            catNode.Header = l;
            
            if (catNode.Items.Count>0) this.Items.Add(catNode);
            foreach (TreeViewItem feedNode in catNode.Items)
            {
                string s=((string)l.totalFeedsNo.Content) ;
                int i=int.Parse(s);
                i++;
                
                l.totalFeedsNo.Content =i.ToString() ;    
                feedNode.Width = this.Width;
                FeedExpander fn=new FeedExpander((XmlFeed)feedNode.Tag, categories,true);
                fn.Expanded += exp_Expanded; 
                feedNode.Header = fn;
                fn.feedbtn.Click += new RoutedEventHandler(feedbtn_Click);   

            }
            catNode.IsExpanded = true;
            
        }
    }

    void feedbtn_Click(object sender, RoutedEventArgs e)
    {
        FrameworkElement f = (FrameworkElement)sender;
        while (!(f is TreeViewItem)) f = (FrameworkElement)f.Parent;
        XmlFeed feed = (XmlFeed)(f.Tag);

        if (viewFeed != null) viewFeed(feed);
        ((FeedExpander)(((TreeViewItem)f).Header)).myImage.Visibility = Visibility.Collapsed;    
    }

    void editCatName_LostFocus(object sender, RoutedEventArgs e)
    {
        if (categoryNameChanged != null) categoryNameChanged(((TextBox)sender).Text, (string)((TextBox)sender).Tag);  
    }

    void myImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (categoryOptions != null) categoryOptions("GG"); 
    }

    void t_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ((TreeViewItem)sender).IsExpanded = !((TreeViewItem)sender).IsExpanded;
    }

    void exp_Expanded(object sender, RoutedEventArgs e)
    {
        if (((Expander)sender).IsExpanded) addExpandedOptions(null, (XmlFeed)((TreeViewItem)((Expander)sender).Parent).Tag, (Expander)sender);
        else
            ((Expander)sender).Content = null;
    }
    

    private void addExpandedOptions(string[] categories, XmlFeed feed, Expander exp)
    {
        Grid itemStack = new Grid();
        ColumnDefinition clm = new ColumnDefinition();
        //clm.Width = new System.Windows.GridLength(exp.Width * 1 / 2);
        itemStack.ColumnDefinitions.Add(clm);
        ColumnDefinition clm2 = new ColumnDefinition();
        //clm2.Width = new System.Windows.GridLength(exp.Width * 1 / 2);
        itemStack.ColumnDefinitions.Add(clm2);
        itemStack.RowDefinitions.Add(new RowDefinition());
        itemStack.RowDefinitions.Add(new RowDefinition());
        itemStack.RowDefinitions.Add(new RowDefinition());
        itemStack.RowDefinitions.Add(new RowDefinition());
        //exp.HorizontalContentAlignment = HorizontalAlignment.Right;
        //exp.HorizontalAlignment = HorizontalAlignment.Center;

        
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
        //btnFeedName.Width = exp.Width / 2 - 1;
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
        //cmb.Width = exp.Width / 2 - 1;
        if (categories!=null)
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
        //delete.Width = 30;
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




}
public class FeedExpander : Expander 
{
    public Button feedbtn;
    public Image myImage;

    public FeedExpander(XmlFeed feed,string[] categories,bool isRead)
    {
        this.BorderBrush = Brushes.Transparent;  
        this.Tag = feed;
        this.HorizontalAlignment = HorizontalAlignment.Stretch;
        feedbtn = new Button();
        //feedbtn.Background = Brushes.Transparent;
        feedbtn.BorderBrush = Brushes.Transparent;
        feedbtn.Width = 240;  
        feedbtn.HorizontalAlignment = HorizontalAlignment.Stretch;    
        feedbtn.FontSize = 16;
        feedbtn.Foreground = Brushes.SteelBlue;
        Label l = new Label();
        l.FontSize = 16;
        l.Background = Brushes.Transparent;
        l.Content = feed.feedName.Substring(0, Math.Min(22, feed.feedName.Length));
        feedbtn.HorizontalAlignment = HorizontalAlignment.Stretch;
        WrapPanel pp = new WrapPanel();
            myImage = new Image();
            myImage.Width = 20;
            myImage.Height = 20;
            // Create source
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\newItem.png");
            myBitmapImage.DecodePixelWidth = 20;
            myBitmapImage.EndInit();
            myImage.Source = myBitmapImage;
            pp.Children.Add(myImage);
            if (isRead) myImage.Visibility = Visibility.Collapsed;  
        pp.Children.Add(l);
        feedbtn.Content = pp;
        this.Header = feedbtn;
        this.BorderThickness = new Thickness(1);
        //this.BorderBrush = Brushes.Gray;
        //feedNode.Width = this.Width-20;
        this.Width = 270;
    }

}

public class ListHeader : StackPanel
{
    StackPanel sp;
    WrapPanel p;
    public TextBox editCatName = new TextBox();
    public Label unreadFeedsNo;
    public Label totalFeedsNo;
    Label catNameTextbox;
    public Image myImage;
    public ListHeader(string s, double w)
    {
        //this.HorizontalAlignment = HorizontalAlignment.Left;
        this.Background = Brushes.WhiteSmoke;
        catNameTextbox = new Label();
        //catNameTextbox.Width = w;
        catNameTextbox.HorizontalAlignment = HorizontalAlignment.Stretch;
        //catNameTextbox.ToolTip = t;
        p=new WrapPanel();
       
        Label lblChangeName = new Label();
        lblChangeName.Content = "Change name:";
        p.Children.Add(lblChangeName); 
        editCatName = new TextBox();
        editCatName.BorderBrush = Brushes.Transparent;
        editCatName.Background = Brushes.Transparent;    
        editCatName.Width = 200;  
        editCatName.Text = s;
        editCatName.Tag = s; 
        p.Children.Add(editCatName);
        
        Image deleteCatImage = new Image();
        deleteCatImage.Width = 20;
        BitmapImage myBitmapImage = new BitmapImage();
        myBitmapImage.BeginInit();
        myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\delete.png");
        myBitmapImage.DecodePixelWidth = 20;
        myBitmapImage.EndInit();
        deleteCatImage.Source = myBitmapImage;
        deleteCatImage.MouseEnter += myImage_MouseEnter;
        deleteCatImage.MouseLeave += myImage_MouseLeave;
        deleteCatImage.ToolTip = "Delete this whole category from the databse.";  
        p.Children.Add(deleteCatImage);


        //catNameTextbox.BorderBrush = Brushes.Transparent;   
        //catNameTextbox.Background =Brushes.Transparent;
        //catNameTextbox.IsEnabled = false;
        catNameTextbox.BitmapEffect = new System.Windows.Media.Effects.EmbossBitmapEffect();
        Grid headerPanel = new Grid();
        headerPanel.HorizontalAlignment = HorizontalAlignment.Stretch;    
        //headerPanel.BitmapEffect = new System.Windows.Media.Effects.DropShadowBitmapEffect();
        headerPanel.RowDefinitions.Add(new RowDefinition());
        ColumnDefinition clm = new ColumnDefinition();
        clm.Width = new GridLength(100);
        headerPanel.ColumnDefinitions.Add(clm);
        headerPanel.ColumnDefinitions.Add(new ColumnDefinition());
        catNameTextbox.Content = s;
        catNameTextbox.Foreground = new SolidColorBrush(Colors.Blue);
        catNameTextbox.FontSize = 22;
        catNameTextbox.FontStyle = FontStyles.Oblique;
        catNameTextbox.HorizontalAlignment = HorizontalAlignment.Stretch;
        WrapPanel pp = new WrapPanel();
        pp.Children.Add(catNameTextbox);
        unreadFeedsNo = new Label();
        p.Children.Add(unreadFeedsNo);
        totalFeedsNo = new Label();
        string ss = "0";
        totalFeedsNo.Content = ss;
        pp.Children.Add(totalFeedsNo); 
        headerPanel.Children.Add(pp);
        Grid.SetColumn(pp, 1);
        myImage = new Image();
        myImage.Width = 20;
        myImage.Height = 20;
        // Create source
        myBitmapImage = new BitmapImage();
        myBitmapImage.BeginInit();
        myBitmapImage.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\icons\AddressBook.ico");
        myBitmapImage.DecodePixelWidth = 20;
        myBitmapImage.EndInit();
        myImage.Source = myBitmapImage;
        myImage.MouseEnter += new System.Windows.Input.MouseEventHandler(myImage_MouseEnter);
        myImage.MouseLeave += new System.Windows.Input.MouseEventHandler(myImage_MouseLeave);
        myImage.MouseDown += new System.Windows.Input.MouseButtonEventHandler(myImage_MouseDown); 
        headerPanel.Children.Add(myImage);
        //myImage.HorizontalAlignment = HorizontalAlignment.Center;
        //l.Width = w * 16 / 17 - 100;
        //l.HorizontalAlignment = HorizontalAlignment.Left;
        //l.HorizontalContentAlignment = HorizontalAlignment.Left;
        this.Children.Add(headerPanel);
        sp = new StackPanel();
        sp.Visibility = Visibility.Collapsed;
        this.Children.Add(sp);
        this.MouseEnter += new System.Windows.Input.MouseEventHandler(ListHeader_MouseEnter);
        this.MouseUp += new System.Windows.Input.MouseButtonEventHandler(ListHeader_MouseUp);
        this.Visibility = Visibility.Visible;
        this.Children.Add(p);
        p.Visibility = Visibility.Collapsed;

    }

    void myImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (p.Visibility == Visibility.Visible) p.Visibility = Visibility.Collapsed;
        else p.Visibility = Visibility.Visible;  
        
    }






    void myImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        ((Image)sender).Opacity = 1; 
    }

    void myImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        ((Image)sender).Opacity = 0.5; 
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

