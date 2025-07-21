using System;
using System.Linq;
using System.Windows;
using Serilog;
using Syncfusion.SfSkinManager;

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
        string[] AvailableThemes { get; }
        string PrimaryTheme { get; }
        string FallbackTheme { get; }
        void ApplyTheme(string themeName);
        void ToggleTheme();
        void InitializeTheme();
        bool IsThemeSupported(string themeName);
        event EventHandler<string> ThemeChanged;
    }

    public class ThemeService : IThemeService
    {
        private static readonly ILogger Logger = Log.ForContext<ThemeService>();
        private string _currentTheme = "FluentDark"; // 🎨 FLUENT DARK as primary theme
        private const string PRIMARY_THEME = "FluentDark";
        private const string FALLBACK_THEME = "FluentLight";

        public event EventHandler<string>? ThemeChanged;

        public string CurrentTheme => _currentTheme;
        public bool IsDarkTheme => _currentTheme.Contains("Dark") || _currentTheme.Contains("Black");
        public string[] AvailableThemes => new[] { PRIMARY_THEME, FALLBACK_THEME };
        public string PrimaryTheme => PRIMARY_THEME;
        public string FallbackTheme => FALLBACK_THEME;

        public ThemeService()
        {
            Logger.Debug("[DEBUG] 🎨 ThemeService initialized with FluentDark as primary theme: {Theme}", _currentTheme);
            Logger.Debug("[DEBUG] 🎨 Fallback theme available: {FallbackTheme}", FALLBACK_THEME);
        }

        public void ApplyTheme(string themeName)
        {
            try
            {
                Logger.Debug("[DEBUG] ThemeService.ApplyTheme: Switching to theme: {ThemeName}", themeName);

                // Validate theme name and fall back to supported themes if necessary
                string validatedTheme = ValidateTheme(themeName);
                if (validatedTheme != themeName)
                {
                    Logger.Warning("[THEME] Theme '{RequestedTheme}' not supported, using '{ValidatedTheme}' instead",
                        themeName, validatedTheme);
                    themeName = validatedTheme;
                }

                // CRITICAL: Set global theme FIRST before applying to individual windows
                SfSkinManager.ApplyThemeAsDefaultStyle = true;
                using var globalTheme = new Theme(themeName);
                SfSkinManager.ApplicationTheme = globalTheme;

                // Update current theme
                _currentTheme = themeName;

                // Apply theme to ALL windows in the application
                if (Application.Current != null)
                {
                    // Create theme once and reuse to avoid multiple disposals
                    using var theme = new Theme(themeName);

                    // Apply to main window EXPLICITLY
                    if (Application.Current.MainWindow != null)
                    {
                        SfSkinManager.SetTheme(Application.Current.MainWindow, theme);
                        Logger.Debug("[DEBUG] ThemeService.ApplyTheme: Applied theme to MainWindow");
                    }

                    // Apply to ALL other windows (including popups, dialogs, etc.)
                    foreach (Window window in Application.Current.Windows)
                    {
                        SfSkinManager.SetTheme(window, theme);
                        Logger.Debug("[DEBUG] ThemeService.ApplyTheme: Applied theme to window: {WindowType}", window.GetType().Name);
                    }

                    // ENHANCED: Force immediate visual refresh
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        foreach (Window window in Application.Current.Windows)
                        {
                            // Force complete visual refresh
                            window.InvalidateVisual();
                            window.UpdateLayout();

                            // Refresh all child elements recursively
                            RefreshVisualTree(window);
                        }
                    }, System.Windows.Threading.DispatcherPriority.Render);
                }

                // Notify subscribers
                ThemeChanged?.Invoke(this, themeName);
                Logger.Information("[THEME] Theme successfully changed to: {ThemeName} and applied universally", themeName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[THEME] Error applying theme {ThemeName}: {Error}", themeName, ex.Message);

                // Fallback to primary theme if current theme fails
                if (themeName != PRIMARY_THEME)
                {
                    Logger.Warning("[THEME] Falling back to primary theme: {PrimaryTheme}", PRIMARY_THEME);
                    try
                    {
                        ApplyTheme(PRIMARY_THEME);
                    }
                    catch (Exception fallbackEx)
                    {
                        Logger.Error(fallbackEx, "[THEME] Fallback to primary theme also failed");
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Recursively refresh the visual tree to ensure theme is applied to all controls
        /// </summary>
        private void RefreshVisualTree(DependencyObject parent)
        {
            if (parent == null)
            {
                return;
            }

            // Refresh the current element
            if (parent is FrameworkElement element)
            {
                element.InvalidateVisual();
                element.UpdateLayout();
            }

            // Recursively refresh all children
            int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                RefreshVisualTree(child);
            }
        }

        public void ToggleTheme()
        {
            // 🎨 Enhanced theme toggle: Switch between FluentDark and FluentLight
            string newTheme = _currentTheme == PRIMARY_THEME ? FALLBACK_THEME : PRIMARY_THEME;
            Logger.Debug("[DEBUG] 🎨 ThemeService.ToggleTheme: Current={Current}, New={New}", _currentTheme, newTheme);
            ApplyTheme(newTheme);
        }

        /// <summary>
        /// Validates the theme name and returns a supported theme
        /// </summary>
        /// <param name="themeName">The requested theme name</param>
        /// <returns>A valid theme name</returns>
        private string ValidateTheme(string themeName)
        {
            // List of supported themes in order of preference
            var supportedThemes = new[] { PRIMARY_THEME, FALLBACK_THEME };

            if (supportedThemes.Contains(themeName))
            {
                return themeName;
            }

            // If requested theme is not supported, return primary theme
            Logger.Warning("[THEME] Theme '{RequestedTheme}' not in supported list: [{SupportedThemes}]",
                themeName, string.Join(", ", supportedThemes));
            return PRIMARY_THEME;
        }

        /// <summary>
        /// Checks if a theme is supported by the application
        /// </summary>
        /// <param name="themeName">The theme name to check</param>
        /// <returns>True if the theme is supported, false otherwise</returns>
        public bool IsThemeSupported(string themeName)
        {
            return AvailableThemes.Contains(themeName);
        }

        /// <summary>
        /// Initialize theme system - call this during application startup
        /// </summary>
        public void InitializeTheme()
        {
            try
            {
                Logger.Debug("[DEBUG] ThemeService.InitializeTheme: Setting up global theme defaults");

                // Set global defaults for all Syncfusion controls
                SfSkinManager.ApplyThemeAsDefaultStyle = true;
                using var initTheme = new Theme(_currentTheme);
                SfSkinManager.ApplicationTheme = initTheme;

                // Ensure both primary and fallback themes are registered
                try
                {
                    SfSkinManager.RegisterThemeSettings(PRIMARY_THEME, new Syncfusion.Themes.FluentDark.WPF.FluentDarkThemeSettings());
                    Logger.Debug("[DEBUG] ThemeService.InitializeTheme: FluentDark theme settings registered");
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "[THEME] Failed to register FluentDark theme settings");
                }

                try
                {
                    SfSkinManager.RegisterThemeSettings(FALLBACK_THEME, new Syncfusion.Themes.FluentLight.WPF.FluentLightThemeSettings());
                    Logger.Debug("[DEBUG] ThemeService.InitializeTheme: FluentLight theme settings registered");
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "[THEME] Failed to register FluentLight theme settings");
                }

                Logger.Information("Theme system initialized with primary theme: {PrimaryTheme}, fallback available: {FallbackTheme}",
                    _currentTheme, FALLBACK_THEME);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error initializing theme system: {Error}", ex.Message);
                throw;
            }
        }
    }
}
