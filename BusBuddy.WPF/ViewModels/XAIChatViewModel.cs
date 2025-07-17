using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Serilog;
using BusBuddy.WPF.Models;
using BusBuddy.WPF.Services;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for XAI Chat interface
    /// Manages chat messages and AI interactions for transportation management assistance
    /// </summary>
    public class XAIChatViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = Log.ForContext<XAIChatViewModel>();

        private readonly IXAIChatService _chatService;
        private string _currentMessage = string.Empty;
        private bool _isTyping = false;
        private bool _showQuickActions = true;
        private bool _canSendMessage = true;

        public XAIChatViewModel(IXAIChatService chatService)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));

            Messages = new ObservableCollection<Models.ChatMessage>();

            // Initialize commands
            SendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => CanSendMessage && !string.IsNullOrWhiteSpace(CurrentMessage));
            ClearChatCommand = new RelayCommand(_ => ClearChat());
            QuickFleetStatusCommand = new RelayCommand(async _ => await QuickFleetStatusAsync());
            QuickFindBusCommand = new RelayCommand(async _ => await QuickFindBusAsync());
            QuickRouteInfoCommand = new RelayCommand(async _ => await QuickRouteInfoAsync());
            QuickEmergencyCommand = new RelayCommand(async _ => await QuickEmergencyAsync());

            // Initialize chat with welcome message
            InitializeChat();

            Logger.Information("XAI Chat ViewModel initialized");
        }

        #region Properties

        /// <summary>
        /// Collection of chat messages
        /// </summary>
        public ObservableCollection<Models.ChatMessage> Messages { get; }

        /// <summary>
        /// Current message being typed
        /// </summary>
        public string CurrentMessage
        {
            get => _currentMessage;
            set
            {
                _currentMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the AI is currently typing
        /// </summary>
        public bool IsTyping
        {
            get => _isTyping;
            set
            {
                _isTyping = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether to show quick actions panel
        /// </summary>
        public bool ShowQuickActions
        {
            get => _showQuickActions;
            set
            {
                _showQuickActions = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether messages can be sent
        /// </summary>
        public bool CanSendMessage
        {
            get => _canSendMessage;
            set
            {
                _canSendMessage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand SendMessageCommand { get; }
        public ICommand ClearChatCommand { get; }
        public ICommand QuickFleetStatusCommand { get; }
        public ICommand QuickFindBusCommand { get; }
        public ICommand QuickRouteInfoCommand { get; }
        public ICommand QuickEmergencyCommand { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event raised when a new message is added
        /// </summary>
        public event EventHandler? MessageAdded;

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the chat with welcome message
        /// </summary>
        private void InitializeChat()
        {
            AddMessage("Welcome to XAI Chat! I'm your AI assistant for transportation management. How can I help you today?", ChatMessageType.AI);
            AddMessage("Chat initialized", ChatMessageType.System);
        }

        /// <summary>
        /// Send the current message
        /// </summary>
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentMessage))
                return;

            var userMessage = CurrentMessage.Trim();
            CurrentMessage = string.Empty;

            // Add user message
            AddMessage(userMessage, ChatMessageType.User);

            // Show typing indicator
            IsTyping = true;
            CanSendMessage = false;

            try
            {
                // Get AI response
                var aiResponse = await _chatService.GetResponseAsync(userMessage);

                // Add AI response
                AddMessage(aiResponse, ChatMessageType.AI);

                Logger.Information("XAI Chat message processed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to process XAI Chat message");
                AddMessage("I apologize, but I encountered an error processing your request. Please try again.", ChatMessageType.AI);
            }
            finally
            {
                IsTyping = false;
                CanSendMessage = true;
            }
        }

        /// <summary>
        /// Clear all chat messages
        /// </summary>
        private void ClearChat()
        {
            Messages.Clear();
            InitializeChat();
            Logger.Information("XAI Chat cleared");
        }

        /// <summary>
        /// Quick action: Fleet status
        /// </summary>
        private async Task QuickFleetStatusAsync()
        {
            CurrentMessage = "What's the current fleet status?";
            await SendMessageAsync();
        }

        /// <summary>
        /// Quick action: Find bus
        /// </summary>
        private async Task QuickFindBusAsync()
        {
            CurrentMessage = "Help me find a specific bus";
            await SendMessageAsync();
        }

        /// <summary>
        /// Quick action: Route information
        /// </summary>
        private async Task QuickRouteInfoAsync()
        {
            CurrentMessage = "Show me route information";
            await SendMessageAsync();
        }

        /// <summary>
        /// Quick action: Emergency
        /// </summary>
        private async Task QuickEmergencyAsync()
        {
            CurrentMessage = "I need emergency assistance";
            await SendMessageAsync();
        }

        /// <summary>
        /// Add a message to the chat
        /// </summary>
        private void AddMessage(string message, ChatMessageType messageType)
        {
            var chatMessage = new Models.ChatMessage
            {
                Message = message,
                MessageType = messageType,
                Timestamp = DateTime.Now,
                IsDelivered = true,
                IsRead = messageType == ChatMessageType.User
            };

            Messages.Add(chatMessage);
            MessageAdded?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
