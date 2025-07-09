using BusBuddy.Core.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BusBuddy.WPF.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IConfigurationService _configService;
        private string _theme = string.Empty;
        private string _notificationPreference = string.Empty;

        public string[] ThemeOptions { get; } = new[] { "Office2019Colorful", "Office2019DarkGray", "Office2019Black", "Office2019White" };
        public string[] NotificationOptions { get; } = new[] { "All", "Important Only", "None" };

        public System.Windows.Input.ICommand SaveSettingsCommand { get; }

        public string Theme
        {
            get => _theme;
            set { _theme = value; OnPropertyChanged(); }
        }

        public string NotificationPreference
        {
            get => _notificationPreference;
            set { _notificationPreference = value; OnPropertyChanged(); }
        }


        public SettingsViewModel(IConfigurationService configService)
        {
            _configService = configService;
            Theme = _configService.GetValue<string>("Theme") ?? "Office2019Colorful";
            NotificationPreference = _configService.GetValue<string>("NotificationPreference") ?? "All";
            SaveSettingsCommand = new global::BusBuddy.WPF.RelayCommand(_ => SaveSettingsAsync().GetAwaiter().GetResult());
        }

        public async Task SaveSettingsAsync()
        {
            // Save to config or database (stub, replace with real logic)
            // _configService.SetValue("Theme", Theme);
            // _configService.SetValue("NotificationPreference", NotificationPreference);
            await Task.CompletedTask;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
