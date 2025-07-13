using System;
using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentDark.WPF;

namespace BusBuddy.WPF
{
    /// <summary>
    /// Main Window for Bus Buddy Application with FluentDark Theme
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow>? _logger;

        public MainWindow()
        {
            try
            {
                // Get logger first for error tracking
                if (Application.Current is App app && app.Services != null)
                {
                    _logger = app.Services.GetService<ILogger<MainWindow>>();
                }

                InitializeComponent();

                // Apply FluentDark theme consistently
                ApplyFluentDarkTheme();

                // Set DataContext to MainViewModel
                if (Application.Current is App appInstance && appInstance.Services != null)
                {
                    DataContext = appInstance.Services.GetService<MainViewModel>();
                    _logger?.LogInformation("MainWindow initialized with MainViewModel and FluentDark theme");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing MainWindow");
                throw;
            }
        }

        /// <summary>
        /// Apply FluentDark theme consistently across the application
        /// </summary>
        private void ApplyFluentDarkTheme()
        {
            try
            {
                // Register FluentDark theme
                SfSkinManager.RegisterThemeSettings("FluentDark", new FluentDarkThemeSettings());

                // Apply theme to current window
                SfSkinManager.SetVisualStyle(this, VisualStyles.FluentDark);

                _logger?.LogInformation("FluentDark theme applied successfully to MainWindow");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error applying FluentDark theme to MainWindow");
            }
        }

        /// <summary>
        /// Toggle navigation panel visibility
        /// </summary>
        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationPanel != null)
                {
                    NavigationPanel.Visibility = NavigationPanel.Visibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;

                    _logger?.LogDebug("Navigation panel toggled to: {Visibility}", NavigationPanel.Visibility);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error toggling navigation panel");
            }
        }

        /// <summary>
        /// Handle window loaded event
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure theme is applied after window loads
                ApplyFluentDarkTheme();

                _logger?.LogInformation("MainWindow loaded successfully with FluentDark theme");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in Window_Loaded event");
            }
        }
    }
}
