using System.Windows;
using System.Windows.Media;

namespace BusBuddy.WPF.Views.Bus
{
    public partial class NotificationWindow : Window
    {
        public enum NotificationType
        {
            Success,
            Error,
            Warning,
            Information
        }

        public NotificationWindow(string message, string title = "Notification", NotificationType type = NotificationType.Information)
        {
            InitializeComponent();

            TitleText.Text = title;
            MessageText.Text = message;

            // Set colors based on notification type
            switch (type)
            {
                case NotificationType.Success:
                    MainBorder.BorderBrush = new SolidColorBrush(Colors.Green);
                    TitleText.Foreground = new SolidColorBrush(Colors.Green);
                    break;
                case NotificationType.Error:
                    MainBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                    TitleText.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case NotificationType.Warning:
                    MainBorder.BorderBrush = new SolidColorBrush(Colors.Orange);
                    TitleText.Foreground = new SolidColorBrush(Colors.Orange);
                    break;
                case NotificationType.Information:
                    MainBorder.BorderBrush = new SolidColorBrush(Colors.Blue);
                    TitleText.Foreground = new SolidColorBrush(Colors.Blue);
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
