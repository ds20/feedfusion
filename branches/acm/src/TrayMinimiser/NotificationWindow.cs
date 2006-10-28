using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Timers; 
using System.Windows.Media.Imaging;   
namespace TrayMinimiser
{
    class NotificationWindow:System.Windows.Window  
    {
        int i = 0;
        Label l;
        public NotificationWindow(string s)
        {
            InitComponent(s);
            this.ShowInTaskbar = false;
        }
        public void setText(string s)
        {
            l.Content = s; 
        }
        public void ca(object sender)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(this.Hide));     
        }

        public void Show1()
        {
            base.Show();
            i = 0;
            System.Threading.Timer t = new System.Threading.Timer(new System.Threading.TimerCallback(ca), null, 2000, 3000);
        }
        private void InitComponent(string s)
        {
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            this.Visibility = System.Windows.Visibility.Hidden;
            this.Opacity = 1;
            this.Width = 500;
            this.Height = 100;

            this.Title = "FeedFusion";
            WrapPanel p = new WrapPanel();
            this.Content = p;

            ImageSource v = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\icons\\feather.png"));
            ImageBrush ib = new ImageBrush(v);
            ib.Opacity = 0.5;
            ib.Stretch = Stretch.Fill;
            Image i = new Image();
            i.Source = v;
            p.Children.Add(i);

            l = new Label();
            l.Content = s;
            p.Children.Add(l);		
            this.Left = System.Windows.SystemParameters.FullPrimaryScreenWidth - this.Width;
            this.Top = System.Windows.SystemParameters.FullPrimaryScreenHeight - this.Height;    
        }
        public delegate void NoArgDelegate();

    }
}
