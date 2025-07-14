// Obsolete: All theme resource loading is now handled by SfSkinManager/NuGet/XAML theme references only.

using Syncfusion.SfSkinManager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Helper class for ensuring theme resources are properly loaded and applied
    /// </summary>
    public static class ThemeAwaitHelper
    {
        private static readonly HashSet<string> _loadedThemeResources = new HashSet<string>();
        private static readonly object _lockObject = new object();
        private static bool _resourcesInitialized = false;

        /// <summary>
        /// Ensures that theme resources are loaded before attempting to use them
        /// </summary>
        public static void EnsureThemeResourcesLoaded()
        {
            if (_resourcesInitialized)
                return;

            lock (_lockObject)
            {
                if (_resourcesInitialized)
                    return;

                try
                {
                    // Ensure Syncfusion's FluentDark theme resources are loaded
                    var fluentDarkTheme = new Syncfusion.Themes.FluentDark.WPF.FluentDarkThemeSettings();
                    SfSkinManager.RegisterThemeSettings("FluentDark", fluentDarkTheme);

                    // Also register FluentLight theme
                    var fluentLightTheme = new Syncfusion.Themes.FluentLight.WPF.FluentLightThemeSettings();
                    SfSkinManager.RegisterThemeSettings("FluentLight", fluentLightTheme);

                    // Set application-wide theme settings
                    SfSkinManager.ApplyStylesOnApplication = true;
                    SfSkinManager.ApplyThemeAsDefaultStyle = true;

                    // Mark as initialized
                    _resourcesInitialized = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error initializing theme resources: {ex.Message}");
                    // Continue execution - we'll attempt to handle missing resources gracefully
                }
            }
        }

        /// <summary>
        /// Applies the current theme recursively to all elements in the visual tree
        /// </summary>
        /// <param name="element">The root element to start from</param>
        public static void ApplyThemeRecursively(DependencyObject element)
        {
            if (element == null)
                return;

            try
            {
                // Apply theme to current element if it's a FrameworkElement
                if (element is FrameworkElement frameworkElement)
                {
                    // Use Theme object for setting theme
                    SfSkinManager.SetTheme(frameworkElement, new Theme { ThemeName = "FluentDark" });
                }

                // Apply to all children recursively
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(element, i);
                    ApplyThemeRecursively(child);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme recursively: {ex.Message}");
                // Continue to process other elements
            }
        }

        /// <summary>
        /// Waits asynchronously for theme resources to be fully loaded
        /// </summary>
        public static async Task WaitForThemeResourcesAsync()
        {
            // First ensure resources are initialized
            EnsureThemeResourcesLoaded();

            // Then wait for any async resource loading to complete
            await Task.Yield(); // Allow UI thread to process

            // Additional waiting logic if needed for complex theme loading
            // This is a placeholder for more complex resource loading scenarios
        }

        /// <summary>
        /// Resets the theme cache to force reloading of resources
        /// </summary>
        public static void ResetThemeCache()
        {
            lock (_lockObject)
            {
                _loadedThemeResources.Clear();
                _resourcesInitialized = false;
            }
        }
    }
}
