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
    /// Interaction logic for WaitWindow.xaml
    /// </summary>

    public partial class WaitWindow : Window
    {
        Window1.OneArgDelegate delegOneArg;
        Window1.NoArgDelegate delegNoArg;
        Object param;

        public WaitWindow(Window1.OneArgDelegate delegOneArg, Object param, String messageToDisplay)
        {
            InitializeComponent();
            this.delegOneArg = delegOneArg;
            this.param = param;
            this.ContentRendered += WaitWindow_OnLoad_oneArgument;
            label1.Content = messageToDisplay;
        }

        public WaitWindow(Window1.NoArgDelegate delegNoArg, String messageToDisplay)
        {
            InitializeComponent();
            this.delegNoArg = delegNoArg;
            this.ContentRendered += WaitWindow_OnLoad_noArgument;
            label1.Content = messageToDisplay;
        }

        void WaitWindow_OnLoad_oneArgument(Object sender, EventArgs e)
        {
            delegOneArg(param);
            this.DialogResult = true;
        }

        void WaitWindow_OnLoad_noArgument(Object sender, EventArgs e)
        {
            delegNoArg();
            this.DialogResult = true;
        }

    }
}