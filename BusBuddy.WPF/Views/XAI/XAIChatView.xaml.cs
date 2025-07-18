using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Serilog;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.Models;

namespace BusBuddy.WPF.Views.XAI
{
    /// <summary>
    /// Interaction logic for XAIChatView.xaml
    /// XAI Chat interface for intelligent transportation management assistance
    /// </summary>
    public partial class XAIChatView : UserControl
    {
        private static readonly ILogger Logger = Log.ForContext<XAIChatView>();

        public XAIChatView()
        {
            InitializeComponent();
            Unloaded += UserControl_Unloaded; // Subscribe to cleanup event
            Logger.Information("XAI Chat view initialized");
        }

        /// <summary>
        /// Handles key down events in the message input — sends message on Enter
        /// </summary>
        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Send message on Enter (without Shift)
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    e.Handled = true;

                    if (DataContext is XAIChatViewModel viewModel && viewModel.SendMessageCommand.CanExecute(null))
                    {
                        viewModel.SendMessageCommand.Execute(null);
                    }
                }
            }
        }

        /// <summary>
        /// Scrolls to the bottom of the chat when new messages are added
        /// </summary>
        private void ScrollToBottom()
        {
            try
            {
                ChatScrollViewer.ScrollToBottom();
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Failed to scroll chat to bottom");
            }
        }

        /// <summary>
        /// Handles data context changes to wire up message collection events
        /// FIXED: Properly unsubscribe from old view model events to prevent memory leaks
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe from old view model if it exists
            if (e.OldValue is XAIChatViewModel oldViewModel)
            {
                oldViewModel.MessageAdded -= OnMessageAdded;
                Logger.Debug("XAI Chat old view model disconnected");
            }

            // Subscribe to new view model if it exists
            if (e.NewValue is XAIChatViewModel newViewModel)
            {
                // Wire up events for auto-scrolling
                newViewModel.MessageAdded += OnMessageAdded;
                Logger.Debug("XAI Chat view model connected");
            }
        }

        /// <summary>
        /// Handle message added event for auto-scrolling
        /// </summary>
        private void OnMessageAdded(object? sender, EventArgs args)
        {
            Dispatcher.BeginInvoke(new Action(ScrollToBottom));
        }

        /// <summary>
        /// Override to handle data context changes
        /// </summary>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            DataContextChanged += OnDataContextChanged;
        }

        /// <summary>
        /// Clean up event subscriptions when control is unloaded
        /// </summary>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is XAIChatViewModel viewModel)
                {
                    viewModel.MessageAdded -= OnMessageAdded;
                }
                DataContextChanged -= OnDataContextChanged;
                Logger.Debug("XAI Chat view cleaned up successfully");
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error during XAI Chat view cleanup");
            }
        }
    }

    /// <summary>
    /// Template selector for chat messages based on message type
    /// </summary>
    public class ChatMessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? UserMessageTemplate { get; set; }
        public DataTemplate? AIMessageTemplate { get; set; }
        public DataTemplate? SystemMessageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Models.ChatMessage message)
            {
                return message.MessageType switch
                {
                    ChatMessageType.User => UserMessageTemplate ?? base.SelectTemplate(item, container),
                    ChatMessageType.AI => AIMessageTemplate ?? base.SelectTemplate(item, container),
                    ChatMessageType.System => SystemMessageTemplate ?? base.SelectTemplate(item, container),
                    _ => base.SelectTemplate(item, container)
                };
            }

            // STREAMLINED: Return base template for null or unknown items
            return base.SelectTemplate(item, container);
        }
    }
}
