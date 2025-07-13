using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Chat;
using BusBuddy.Core.Services;

namespace BusBuddy.WPF.ViewModels
{
    public class XaiChatViewModel : INotifyPropertyChanged
    {
        private readonly XAIService _xaiService;
        private Author _currentUser;
        private Author _botUser;
        private string _inputText = string.Empty;
        private bool _isTyping = false;

        public XaiChatViewModel(XAIService xaiService)
        {
            _xaiService = xaiService ?? throw new ArgumentNullException(nameof(xaiService));

            // Initialize users
            _currentUser = new Author { Name = "You" };
            _botUser = new Author { Name = "Bus Buddy AI" };

            // Initialize collections
            Messages = new ObservableCollection<object>();
            Suggestions = new ObservableCollection<ChatSuggestion>();

            // Initialize commands
            SendCommand = new RelayCommand(async (param) => await SendMessageAsync(), (param) => !string.IsNullOrWhiteSpace(InputText) && !IsTyping);
            SuggestionCommand = new RelayCommand(async (param) => await HandleSuggestionAsync(param as ChatSuggestion), (param) => param is ChatSuggestion);

            // Initialize with welcome message and suggestions
            InitializeChat();
        }

        /// <summary>
        /// Collection of chat messages for the AI AssistView
        /// </summary>
        public ObservableCollection<object> Messages { get; set; }

        /// <summary>
        /// Collection of AI suggestions for quick responses
        /// </summary>
        public ObservableCollection<ChatSuggestion> Suggestions { get; set; }

        /// <summary>
        /// Current user for the chat
        /// </summary>
        public Author CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Input text from the user
        /// </summary>
        public string InputText
        {
            get => _inputText;
            set { _inputText = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Indicates if AI is currently typing/processing
        /// </summary>
        public bool IsTyping
        {
            get => _isTyping;
            set { _isTyping = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Command to send messages
        /// </summary>
        public ICommand SendCommand { get; set; }

        /// <summary>
        /// Command to handle suggestion clicks
        /// </summary>
        public ICommand SuggestionCommand { get; set; }

        /// <summary>
        /// Initialize chat with welcome message and suggestions
        /// </summary>
        private void InitializeChat()
        {
            // Add welcome message
            Messages.Add(new TextMessage
            {
                Author = _botUser,
                Text = "🚌 Welcome to Bus Buddy AI Assistant! I can help you with:\n\n• Route optimization and planning\n• Maintenance predictions and scheduling\n• Safety analysis and recommendations\n• Student assignment optimization\n• General transportation insights\n\nHow can I assist you today?"
            });

            // Add initial suggestions
            UpdateSuggestions(new[]
            {
                "Analyze route efficiency",
                "Predict maintenance needs",
                "Safety risk assessment",
                "Optimize student assignments",
                "Transportation insights"
            });
        }

        /// <summary>
        /// Send message to AI and handle response
        /// </summary>
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(InputText) || IsTyping)
                return;

            var userMessage = InputText;
            InputText = string.Empty;

            // Add user message
            Messages.Add(new TextMessage
            {
                Author = CurrentUser,
                Text = userMessage
            });

            // Show typing indicator
            IsTyping = true;

            try
            {
                // Get AI response using the XAI service
                var response = await _xaiService.SendChatMessageAsync(userMessage);

                // Add AI response
                Messages.Add(new TextMessage
                {
                    Author = _botUser,
                    Text = response
                });

                // Update suggestions based on the conversation context
                UpdateContextualSuggestions(userMessage);
            }
            catch (Exception ex)
            {
                Messages.Add(new TextMessage
                {
                    Author = _botUser,
                    Text = $"⚠️ Sorry, I encountered an error: {ex.Message}\n\nPlease try again or rephrase your question."
                });
            }
            finally
            {
                IsTyping = false;
            }
        }

        /// <summary>
        /// Handle suggestion click
        /// </summary>
        private async Task HandleSuggestionAsync(ChatSuggestion? suggestion)
        {
            if (suggestion == null || IsTyping)
                return;

            // Add suggestion text to input and send it automatically
            InputText = suggestion.Text;

            // Automatically send the suggestion as a message
            await SendMessageAsync();
        }

        /// <summary>
        /// Update suggestions based on conversation context
        /// </summary>
        private void UpdateContextualSuggestions(string userMessage)
        {
            var suggestions = new List<string>();

            // Contextual suggestions based on user input
            if (userMessage.ToLower().Contains("route") || userMessage.ToLower().Contains("optimization"))
            {
                suggestions.AddRange(new[]
                {
                    "Show route efficiency metrics",
                    "Analyze traffic patterns",
                    "Suggest alternative routes",
                    "Calculate fuel savings"
                });
            }
            else if (userMessage.ToLower().Contains("maintenance") || userMessage.ToLower().Contains("repair"))
            {
                suggestions.AddRange(new[]
                {
                    "Schedule preventive maintenance",
                    "Analyze wear patterns",
                    "Cost optimization tips",
                    "Emergency maintenance protocol"
                });
            }
            else if (userMessage.ToLower().Contains("safety") || userMessage.ToLower().Contains("risk"))
            {
                suggestions.AddRange(new[]
                {
                    "Weather impact analysis",
                    "Driver safety recommendations",
                    "Route safety assessment",
                    "Emergency procedures"
                });
            }
            else if (userMessage.ToLower().Contains("student") || userMessage.ToLower().Contains("assignment"))
            {
                suggestions.AddRange(new[]
                {
                    "Optimize pickup times",
                    "Balance bus capacity",
                    "Minimize travel distance",
                    "Special needs accommodation"
                });
            }
            else
            {
                // Default suggestions
                suggestions.AddRange(new[]
                {
                    "Tell me more about this",
                    "Show me examples",
                    "How can I implement this?",
                    "What are the benefits?"
                });
            }

            UpdateSuggestions(suggestions);
        }

        /// <summary>
        /// Update the suggestions collection
        /// </summary>
        private void UpdateSuggestions(IEnumerable<string> suggestionTexts)
        {
            Suggestions.Clear();
            foreach (var text in suggestionTexts)
            {
                Suggestions.Add(new ChatSuggestion { Text = text });
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Simple implementation for chat suggestions
    /// </summary>
    public class ChatSuggestion
    {
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// Legacy chat message class for backward compatibility
    /// </summary>
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsUser { get; set; }
    }
}
