using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.WPF.Views.Settings
{
    /// <summary>
    /// Interaction logic for XaiChatView.xaml - Modern AI AssistView implementation
    /// </summary>
    public partial class XaiChatView : UserControl
    {
        private XaiChatViewModel _viewModel;

        public XaiChatView()
        {
            InitializeComponent();

            // Get ViewModel from DI container
            var app = System.Windows.Application.Current as App;
            if (app?.Services != null)
            {
                _viewModel = app.Services.GetRequiredService<XaiChatViewModel>();
                DataContext = _viewModel;

                // Subscribe to collection changes for auto-scrolling
                _viewModel.Messages.CollectionChanged += Messages_CollectionChanged;
            }
            else
            {
                // Fallback for design time - create with null service (will use mock responses)
                throw new InvalidOperationException("DI container not available. XaiChatView requires dependency injection.");
            }
        }

        /// <summary>
        /// Auto-scroll to bottom when new messages are added
        /// </summary>
        private void Messages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                // The AI AssistView will handle auto-scrolling automatically
                // This is here for future enhancements if needed
            }
        }

        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        private void OnMessagesChanged()
        {
            // This method is kept for backward compatibility
            // The new implementation uses the Messages_CollectionChanged event
        }
    }
}
