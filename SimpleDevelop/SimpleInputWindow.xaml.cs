using System.Windows;
using System.Windows.Input;

namespace SimpleDevelop
{
    /// <summary>
    /// Interaction logic for SimpleInputWindow.xaml
    /// </summary>
    public partial class SimpleInputWindow : Window
    {
        public static bool GetInput(string caption, string message, out string input)
        {
            var window = new SimpleInputWindow();
            window.Title = caption;
            window.Message = message;

            bool okClicked = window.ShowDialog() ?? false;

            input = okClicked ? window.Input : "";

            return okClicked;
        }

        private SimpleInputWindow()
        {
            InitializeComponent();
        }

        private string Message
        {
            get { return (string)_label.Content; }
            set { _label.Content = value ?? ""; }
        }

        private string Input
        {
            get { return _textBox.Text; }
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            Ok();
        }

        private void TextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                Ok();
            }
        }

        private void Ok()
        {
            DialogResult = true;
            Close();
        }
    }
}
