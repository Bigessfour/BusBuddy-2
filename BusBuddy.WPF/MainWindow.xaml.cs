using System;
using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentDark.WPF;

namespace BusBuddy.WPF
{
    /// <summary>
    /// Main Window for Bus Buddy Application with FluentDark Theme
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                // CRITICAL: Set ApplyThemeAsDefaultStyle before InitializeComponent
                // This ensures FluentDark theme resources are available globally
                SfSkinManager.ApplyThemeAsDefaultStyle = true;

                InitializeComponent();

                // Apply FluentDark theme consistently
                ApplyFluentDarkTheme();

                // Set DataContext to MainViewModel
                if (Application.Current is App appInstance && appInstance.Services != null)
                {
                    DataContext = appInstance.Services.GetService<MainViewModel>();
                    Log.Information("MainWindow initialized with MainViewModel and FluentDark theme");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing MainWindow");
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
                // Register FluentDark theme settings
                SfSkinManager.RegisterThemeSettings("FluentDark", new FluentDarkThemeSettings());

                // Apply theme using modern Theme property instead of deprecated VisualStyle
                SfSkinManager.SetTheme(this, new Theme() { ThemeName = "FluentDark" });

                Log.Information("FluentDark theme applied successfully to MainWindow using modern Theme API");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying FluentDark theme to MainWindow");
            }
        }

        /// <summary>
        /// Toggle navigation panel visibility
        /// </summary>
        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Find the NavigationPanel by name
                var navigationPanel = FindName("NavigationPanel") as FrameworkElement;
                if (navigationPanel != null)
                {
                    navigationPanel.Visibility = navigationPanel.Visibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;

                    Log.Debug("Navigation panel toggled to: {Visibility}", navigationPanel.Visibility);
                }
                else
                {
                    Log.Warning("NavigationPanel not found in MainWindow");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error toggling navigation panel");
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

                Log.Information("MainWindow loaded successfully with FluentDark theme");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in Window_Loaded event");
            }
        }
    }
}
