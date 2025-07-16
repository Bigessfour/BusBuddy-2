using BusBuddy.Core.Services;
using BusBuddy.WPF.Services;
using Serilog;
using Serilog.Context;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IConfigurationService _configService;
        private readonly IThemeService _themeService;
        private readonly IUserSettingsService _userSettingsService;
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
            IUserSettingsService userSettingsService)
        {
            _configService = configService;
            _themeService = themeService;
            _userSettingsService = userSettingsService;

            using (LogContext.PushProperty("ViewModelType", nameof(SettingsViewModel)))
            using (LogContext.PushProperty("OperationType", "Construction"))
            {
                Logger.Information("SettingsViewModel constructor started");

                SaveSettingsCommand = new global::BusBuddy.WPF.RelayCommand(_ => SaveSettingsAsync().GetAwaiter().GetResult());
                ToggleThemeCommand = new global::BusBuddy.WPF.RelayCommand(_ => ToggleTheme());
                ResetSettingsCommand = new global::BusBuddy.WPF.RelayCommand(_ => ResetSettingsAsync().GetAwaiter().GetResult());

                // Load settings asynchronously
                _ = InitializeSettingsAsync();

                Logger.Information("SettingsViewModel constructor completed");
            }
        }

        private async Task InitializeSettingsAsync()
        {
            await LoadDataAsync(async () =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];

                using (LogContext.PushProperty("CorrelationId", correlationId))
                using (LogContext.PushProperty("ViewModelType", nameof(SettingsViewModel)))
                using (LogContext.PushProperty("OperationType", "InitializeSettings"))
                {
                    Logger.Information("Initializing settings");

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

                    Logger.Information("Settings initialized successfully. Theme: {Theme}, Notifications: {Notifications}",
                        Theme, NotificationPreference);
                }
            });
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
            await ExecuteCommandAsync(async () =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];

                using (LogContext.PushProperty("CorrelationId", correlationId))
                using (LogContext.PushProperty("ViewModelType", nameof(SettingsViewModel)))
                using (LogContext.PushProperty("OperationType", "SaveSettings"))
                {
                    Logger.Information("Saving settings");

                    // Save theme setting to user settings service
                    await _userSettingsService.SetSettingAsync("Theme", Theme);

                    // Save notification preference
                    await _userSettingsService.SetSettingAsync("NotificationPreference", NotificationPreference);

                    // Persist the settings to file
                    bool saveSuccess = await _userSettingsService.SaveSettingsAsync();

                    if (saveSuccess)
                    {
                        Logger.Information("Settings saved successfully. Theme: {Theme}, Notifications: {Notifications}",
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
            });
        }

        public async Task ResetSettingsAsync()
        {
            await ExecuteCommandAsync(async () =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];

                using (LogContext.PushProperty("CorrelationId", correlationId))
                using (LogContext.PushProperty("ViewModelType", nameof(SettingsViewModel)))
                using (LogContext.PushProperty("OperationType", "ResetSettings"))
                {
                    Logger.Information("Resetting settings");

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

                            Logger.Information("Settings reset to defaults successfully");

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
                    else
                    {
                        Logger.Information("Settings reset cancelled by user");
                    }
                }
            });
        }
    }
}
