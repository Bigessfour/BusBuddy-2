using System.Windows;

namespace BusBuddy.WPF.Views.Schedule
{
    public partial class CancellationReasonDialog : Window
    {
        public string CancellationReason { get; private set; } = string.Empty;

        public CancellationReasonDialog()
        {
            InitializeComponent();
            ReasonTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReasonTextBox.Text))
            {
                MessageBox.Show("Please provide a reason for cancellation.",
                    "Reason Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                ReasonTextBox.Focus();
                return;
            }

            CancellationReason = ReasonTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }
    }
}
