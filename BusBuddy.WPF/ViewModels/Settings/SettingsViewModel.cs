using BusBuddy.Core.Services;
using BusBuddy.WPF.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IConfigurationService _configService;
        private readonly IThemeService _themeService;
        private readonly IUserSettingsService _userSettingsService;
        private readonly ILogger<SettingsViewModel>? _logger;
        private string _theme = string.Empty;
        private string _notificationPreference = string.Empty;
        private bool _isDarkTheme = false;

        public string[] ThemeOptions { get; } = new[] { "Office2019Colorful", "Office2019DarkGray", "Office2019Black", "Office2019White" };
        public string[] NotificationOptions { get; } = new[] { "All", "Important Only", "None" };

        public ICommand SaveSettingsCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand ResetSettingsCommand { get; }

        public string Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    OnPropertyChanged();
                    _themeService.ApplyTheme(value);
                    UpdateDarkThemeStatus();
                }
            }
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (_isDarkTheme != value)
                {
                    _isDarkTheme = value;
                    OnPropertyChanged();

                    // Apply appropriate theme based on toggle
                    string newTheme = value ? "Office2019DarkGray" : "Office2019Colorful";
                    if (_theme != newTheme)
                    {
                        Theme = newTheme;
                    }
                }
            }
        }

        public string NotificationPreference
        {
            get => _notificationPreference;
            set { _notificationPreference = value; OnPropertyChanged(); }
        }

        public SettingsViewModel(IConfigurationService configService, IThemeService themeService,
            IUserSettingsService userSettingsService, ILogger<SettingsViewModel>? logger = null)
        {
            _configService = configService;
            _themeService = themeService;
            _userSettingsService = userSettingsService;
            _logger = logger;

            SaveSettingsCommand = new global::BusBuddy.WPF.RelayCommand(_ => SaveSettingsAsync().GetAwaiter().GetResult());
            ToggleThemeCommand = new global::BusBuddy.WPF.RelayCommand(_ => ToggleTheme());
            ResetSettingsCommand = new global::BusBuddy.WPF.RelayCommand(_ => ResetSettingsAsync().GetAwaiter().GetResult());

            // Load settings asynchronously
            _ = InitializeSettingsAsync();
        }

        private async Task InitializeSettingsAsync()
        {
            try
            {
                // Load user settings first
                await _userSettingsService.LoadSettingsAsync();

                // Initialize with saved settings or defaults
                Theme = await _userSettingsService.GetSettingAsync("Theme", "Office2019Colorful");
                NotificationPreference = await _userSettingsService.GetSettingAsync("NotificationPreference", "All");

                // Apply the saved theme and update status
                _themeService.ApplyTheme(Theme);
                UpdateDarkThemeStatus();

                // Subscribe to theme changes
                _themeService.ThemeChanged += OnThemeChanged;

                _logger?.LogInformation("Settings initialized successfully. Theme: {Theme}, Notifications: {Notifications}",
                    Theme, NotificationPreference);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing settings, using defaults");

                // Fall back to defaults
                Theme = "Office2019Colorful";
                NotificationPreference = "All";
                _themeService.ApplyTheme(Theme);
                UpdateDarkThemeStatus();
                _themeService.ThemeChanged += OnThemeChanged;
            }
        }

        private void OnThemeChanged(object? sender, string newTheme)
        {
            if (_theme != newTheme)
            {
                _theme = newTheme;
                OnPropertyChanged(nameof(Theme));
                UpdateDarkThemeStatus();
            }
        }

        private void UpdateDarkThemeStatus()
        {
            bool isDark = _theme.Contains("Dark") || _theme.Contains("Black");
            if (_isDarkTheme != isDark)
            {
                _isDarkTheme = isDark;
                OnPropertyChanged(nameof(IsDarkTheme));
            }
        }

        private void ToggleTheme()
        {
            _themeService.ToggleTheme();
        }

        public async Task SaveSettingsAsync()
        {
            try
            {
                // Save theme setting to user settings service
                await _userSettingsService.SetSettingAsync("Theme", Theme);

                // Save notification preference
                await _userSettingsService.SetSettingAsync("NotificationPreference", NotificationPreference);

                // Persist the settings to file
                bool saveSuccess = await _userSettingsService.SaveSettingsAsync();

                if (saveSuccess)
                {
                    _logger?.LogInformation("Settings saved successfully. Theme: {Theme}, Notifications: {Notifications}",
                        Theme, NotificationPreference);

                    // Optional: Show success message to user
                    System.Windows.MessageBox.Show(
                        "Settings saved successfully!",
                        "Settings",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    throw new InvalidOperationException("Failed to save settings to persistent storage.");
                }
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error saving settings");

                // Handle save error appropriately
                System.Windows.MessageBox.Show(
                    $"Error saving settings: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                throw;
            }
        }

        public async Task ResetSettingsAsync()
        {
            try
            {
                var result = System.Windows.MessageBox.Show(
                    "Are you sure you want to reset all settings to their default values?",
                    "Reset Settings",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    bool resetSuccess = await _userSettingsService.ResetSettingsAsync();

                    if (resetSuccess)
                    {
                        // Reset to defaults
                        Theme = "Office2019Colorful";
                        NotificationPreference = "All";

                        _logger?.LogInformation("Settings reset to defaults successfully");

                        System.Windows.MessageBox.Show(
                            "Settings have been reset to default values.",
                            "Settings Reset",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information);
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to reset settings.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error resetting settings");

                System.Windows.MessageBox.Show(
                    $"Error resetting settings: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
