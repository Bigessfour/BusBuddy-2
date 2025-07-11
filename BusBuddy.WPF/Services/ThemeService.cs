using Microsoft.Extensions.Logging;
using Syncfusion.SfSkinManager;
using System;
using System.Windows;

namespace BusBuddy.WPF.Services
{
    /// <summary>
    /// Theme management service for Dark/Light theme switching
    /// Handles Syncfusion theme application and persistence
    /// </summary>
    public interface IThemeService
    {
        string CurrentTheme { get; }
        bool IsDarkTheme { get; }
        void ApplyTheme(string themeName);
        void ToggleTheme();
        void InitializeTheme();
        event EventHandler<string> ThemeChanged;
    }

    public class ThemeService : IThemeService
    {
        private readonly ILogger<ThemeService> _logger;
        private string _currentTheme = "Office2019Colorful"; // Default light theme

        public event EventHandler<string>? ThemeChanged;

        public string CurrentTheme => _currentTheme;
        public bool IsDarkTheme => _currentTheme.Contains("Dark") || _currentTheme.Contains("Black");

        public ThemeService(ILogger<ThemeService> logger)
        {
            _logger = logger;
            _logger.LogDebug("[DEBUG] ThemeService initialized with default theme: {Theme}", _currentTheme);
        }

        public void ApplyTheme(string themeName)
        {
            try
            {
                _logger.LogDebug("[DEBUG] ThemeService.ApplyTheme: Switching to theme: {ThemeName}", themeName);

                // Set global theme for all Syncfusion controls
                SfSkinManager.ApplyThemeAsDefaultStyle = true;
                SfSkinManager.ApplicationTheme = new Theme(themeName);

                // Update current theme
                _currentTheme = themeName;

                // Apply theme to main window if it exists
                if (Application.Current.MainWindow != null)
                {
                    SfSkinManager.SetTheme(Application.Current.MainWindow, new Theme(themeName));
                    _logger.LogDebug("[DEBUG] ThemeService.ApplyTheme: Applied theme to MainWindow");
                }

                // Notify subscribers
                ThemeChanged?.Invoke(this, themeName);
                _logger.LogInformation("Theme successfully changed to: {ThemeName}", themeName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying theme {ThemeName}: {Error}", themeName, ex.Message);
                throw;
            }
        }

        public void ToggleTheme()
        {
            string newTheme = IsDarkTheme ? "Office2019Colorful" : "Office2019DarkGray";
            _logger.LogDebug("[DEBUG] ThemeService.ToggleTheme: Current={Current}, New={New}", _currentTheme, newTheme);
            ApplyTheme(newTheme);
        }

        /// <summary>
        /// Initialize theme system - call this during application startup
        /// </summary>
        public void InitializeTheme()
        {
            try
            {
                _logger.LogDebug("[DEBUG] ThemeService.InitializeTheme: Setting up global theme defaults");

                // Set global defaults for all Syncfusion controls
                SfSkinManager.ApplyThemeAsDefaultStyle = true;
                SfSkinManager.ApplicationTheme = new Theme(_currentTheme);

                _logger.LogInformation("Theme system initialized with theme: {Theme}", _currentTheme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing theme system: {Error}", ex.Message);
                throw;
            }
        }
    }
}
