using System.Windows;

namespace BusBuddy.WPF.Views.Bus
{
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog(string message, string title = "Confirmation")
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
