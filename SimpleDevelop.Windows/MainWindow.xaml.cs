using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleDevelop.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Application app;

        public MainWindow()
        {
            InitializeComponent();

            this.app = new Application();
            this.app.Start();
            this.app.Stopped += HandleApplicationStopped;

            this.browser.Navigate("http://localhost:9999/index.html");
        }

        void HandleApplicationStopped(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(Close));
        }
    }
}
