using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views.Settings
{
    /// <summary>
    /// Interaction logic for XaiChatView.xaml
    /// </summary>
    public partial class XaiChatView : UserControl
    {
        public XaiChatView()
        {
            InitializeComponent();
            DataContext = new XaiChatViewModel();
        }

        private void OnMessagesChanged()
        {
            // Auto-scroll to bottom when new messages are added
            if (MessagesScrollViewer != null)
            {
                MessagesScrollViewer.ScrollToBottom();
            }
        }
    }
}
