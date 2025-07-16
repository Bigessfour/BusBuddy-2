using System;
using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentDark.WPF;
using Syncfusion.UI.Xaml.NavigationDrawer;

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
                // Register FluentDark theme settings with reveal effects enabled
                var fluentDarkSettings = new FluentDarkThemeSettings();
                SfSkinManager.RegisterThemeSettings("FluentDark", fluentDarkSettings);

                // Apply theme using modern Theme property with reveal effects
                var fluentTheme = new Theme() { ThemeName = "FluentDark" };
                SfSkinManager.SetTheme(this, fluentTheme);

                Log.Information("FluentDark theme applied successfully to MainWindow with reveal effects enabled");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying FluentDark theme to MainWindow");
            }
        }

        /// <summary>
        /// Toggle navigation drawer visibility
        /// </summary>
        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Toggle the SfNavigationDrawer
                if (MainNavigationDrawer != null)
                {
                    MainNavigationDrawer.IsOpen = !MainNavigationDrawer.IsOpen;

                    // Update button content based on state
                    if (sender is Syncfusion.Windows.Tools.Controls.ButtonAdv button)
                    {
                        button.Content = MainNavigationDrawer.IsOpen ? "☰" : "☰";
                    }

                    Log.Debug("Navigation drawer toggled to: {IsOpen}", MainNavigationDrawer.IsOpen);
                }
                else
                {
                    Log.Warning("MainNavigationDrawer not found in MainWindow");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error toggling navigation drawer");
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
