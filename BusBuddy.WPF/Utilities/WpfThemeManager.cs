using Syncfusion.SfSkinManager;
using Syncfusion.Themes.Office2019Colorful.WPF;
using System.Windows;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// WPF Theme Manager - 100% Syncfusion SfSkinManager API Compliant
    /// Reference: https://help.syncfusion.com/cr/wpf/Syncfusion.SfSkinManager.SfSkinManager.html
    /// </summary>
    public static class WpfThemeManager
    {
        /// <summary>
        /// Apply Office2019Colorful theme to a specific element using official Syncfusion SetTheme method
        /// API Reference: https://help.syncfusion.com/cr/wpf/Syncfusion.SfSkinManager.SfSkinManager.html#Syncfusion_SfSkinManager_SfSkinManager_SetTheme_System_Windows_DependencyObject_Syncfusion_SfSkinManager_Theme_
        /// </summary>
        /// <param name="element">The DependencyObject to apply the theme to</param>
        public static void ApplyOffice2019ColorfulTheme(DependencyObject element)
        {
            // CRITICAL: Set ApplyThemeAsDefaultStyle before any theme application
            // API Reference: https://help.syncfusion.com/cr/wpf/Syncfusion.SfSkinManager.SfSkinManager.html#Syncfusion_SfSkinManager_SfSkinManager_ApplyThemeAsDefaultStyle
            SfSkinManager.ApplyThemeAsDefaultStyle = true;

            // Apply theme using official Syncfusion SetTheme method
            SfSkinManager.SetTheme(element, new Theme("Office2019Colorful"));
        }

        /// <summary>
        /// Apply global theme using ApplicationTheme property
        /// API Reference: https://help.syncfusion.com/cr/wpf/Syncfusion.SfSkinManager.SfSkinManager.html#Syncfusion_SfSkinManager_SfSkinManager_ApplicationTheme
        /// </summary>
        public static void ApplyGlobalTheme()
        {
            // CRITICAL: Must be set before InitializeComponent
            SfSkinManager.ApplyThemeAsDefaultStyle = true;

            // Apply theme globally using ApplicationTheme property
            SfSkinManager.ApplicationTheme = new Theme("Office2019Colorful");
        }

        /// <summary>
        /// Apply custom theme colors using official RegisterThemeSettings method
        /// API Reference: https://help.syncfusion.com/cr/wpf/Syncfusion.SfSkinManager.SfSkinManager.html#Syncfusion_SfSkinManager_SfSkinManager_RegisterThemeSettings_System_String_Syncfusion_SfSkinManager_IThemeSetting_
        /// </summary>
        /// <param name="element">The DependencyObject to apply the theme to</param>
        public static void ApplyCustomOffice2019Theme(DependencyObject element)
        {
            // Create custom theme settings using official Office2019ColorfulThemeSettings class
            var themeSettings = new Office2019ColorfulThemeSettings();

            // Customize colors as needed using documented properties
            // themeSettings.PrimaryBackground = new SolidColorBrush(Colors.CustomColor);
            // themeSettings.PrimaryForeground = new SolidColorBrush(Colors.CustomColor);
            // themeSettings.FontFamily = new FontFamily("Arial");
            // themeSettings.BodyFontSize = 12;

            // Register custom theme settings using official RegisterThemeSettings method
            SfSkinManager.RegisterThemeSettings("Office2019Colorful", themeSettings);

            // Apply theme using official SetTheme method
            SfSkinManager.SetTheme(element, new Theme("Office2019Colorful"));
        }

        /// <summary>
        /// Get current theme applied to an element using official GetTheme method
        /// API Reference: https://help.syncfusion.com/cr/wpf/Syncfusion.SfSkinManager.SfSkinManager.html#Syncfusion_SfSkinManager_SfSkinManager_GetTheme_System_Windows_DependencyObject_
        /// </summary>
        /// <param name="element">The DependencyObject to get the theme from</param>
        /// <returns>The current Theme applied to the element</returns>
        public static Theme GetCurrentTheme(DependencyObject element)
        {
            return SfSkinManager.GetTheme(element);
        }

        /// <summary>
        /// Apply theme to specific control with touch mode support
        /// API Reference: https://help.syncfusion.com/cr/wpf/Syncfusion.SfSkinManager.SfSkinManager.html#Syncfusion_SfSkinManager_SfSkinManager_SetSizeMode_System_Windows_DependencyObject_Syncfusion_SfSkinManager_SizeMode_
        /// </summary>
        /// <param name="element">The DependencyObject to apply the theme to</param>
        /// <param name="enableTouchMode">Whether to enable touch-friendly sizing</param>
        public static void ApplyThemeWithTouchSupport(DependencyObject element, bool enableTouchMode = true)
        {
            // Set ApplyThemeAsDefaultStyle before theme application
            SfSkinManager.ApplyThemeAsDefaultStyle = true;

            // Apply theme using official SetTheme method
            SfSkinManager.SetTheme(element, new Theme("Office2019Colorful"));

            // Set touch mode using official SetSizeMode method
            if (enableTouchMode)
            {
                SfSkinManager.SetSizeMode(element, SizeMode.Touch);
            }
        }

        /// <summary>
        /// Clean up SfSkinManager instance - NOTE: Dispose method is not available in current API
        /// This method is kept for compatibility but currently does nothing
        /// </summary>
        /// <param name="element">The element to dispose (currently not supported by API)</param>
        public static void Dispose(DependencyObject element)
        {
            // NOTE: Based on API documentation review, there is no Dispose method available
            // in the current SfSkinManager API. This method is kept for compatibility
            // but currently performs no operation.
            // 
            // If cleanup is needed, consider setting the Theme to null or default:
            // SfSkinManager.SetTheme(element, new Theme("Default"));
        }
    }
}
