using System.Windows;
using System.Windows.Media;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Unified configuration for Syncfusion controls to ensure consistency
    /// and proper property usage across the application
    /// </summary>
    public static class SyncfusionControlConfig
    {
        #region SfLinearProgressBar Standard Properties

        /// <summary>
        /// Standard properties for SfLinearProgressBar in Syncfusion WPF 30.1.39
        /// </summary>
        public static class ProgressBar
        {
            // Core Progress Properties
            public const string ValueProperty = "Value";
            public const string MinimumProperty = "Minimum";
            public const string MaximumProperty = "Maximum";
            public const string IsIndeterminateProperty = "IsIndeterminate";

            // Visual Properties (Standard WPF properties)
            public const string ForegroundProperty = "Foreground";  // Progress color
            public const string BackgroundProperty = "Background";  // Track color
            public const string WidthProperty = "Width";
            public const string HeightProperty = "Height";

            // ❌ INVALID PROPERTIES (DO NOT USE):
            // - ProgressBackground  (Use Foreground instead)
            // - TrackBackground     (Use Background instead)
            // - CornerRadius        (Not available)
            // - ShowProgressValue   (Not available)

            // Standard Configurations
            public static readonly SolidColorBrush DefaultTrackColor = new SolidColorBrush(Color.FromRgb(224, 224, 224));
            public static readonly SolidColorBrush DefaultProgressColor = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            public static readonly SolidColorBrush SuccessColor = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            public static readonly SolidColorBrush WarningColor = new SolidColorBrush(Color.FromRgb(255, 152, 0));
            public static readonly SolidColorBrush ErrorColor = new SolidColorBrush(Color.FromRgb(244, 67, 54));

            // Dark Theme Colors
            public static readonly SolidColorBrush DarkTrackColor = new SolidColorBrush(Color.FromRgb(72, 72, 72));
            public static readonly SolidColorBrush DarkProgressColor = new SolidColorBrush(Color.FromRgb(0, 120, 212));
        }

        #endregion

        #region SfDataGrid Standard Properties

        /// <summary>
        /// Standard properties for SfDataGrid in Syncfusion WPF 30.1.39
        /// </summary>
        public static class DataGrid
        {
            // Selection Properties
            public const string SelectionModeProperty = "SelectionMode";
            public const string SelectionUnitProperty = "SelectionUnit";
            public const string ShowRowHeaderProperty = "ShowRowHeader";

            // ❌ INVALID PROPERTIES (DO NOT USE):
            // - ShowCheckBox  (Use GridCheckBoxColumn instead)

            // Grid Features
            public const string AllowGroupingProperty = "AllowGrouping";
            public const string ShowGroupDropAreaProperty = "ShowGroupDropArea";
            public const string AllowFilteringProperty = "AllowFiltering";
            public const string AllowSortingProperty = "AllowSorting";

            // Performance
            public const string EnableDataVirtualizationProperty = "EnableDataVirtualization";
            public const string ScrollModeProperty = "ScrollMode";
        }

        #endregion

        #region Common Styling Guidelines

        /// <summary>
        /// Common styling guidelines for consistency
        /// </summary>
        public static class Styling
        {
            // Standard Heights
            public const double StandardButtonHeight = 48;
            public const double StandardInputHeight = 48;
            public const double StandardRowHeight = 40;
            public const double StandardHeaderHeight = 48;

            // Progress Bar Heights
            public const double SmallProgressHeight = 6;
            public const double StandardProgressHeight = 8;
            public const double LargeProgressHeight = 12;

            // Corner Radius (for standard WPF controls)
            public const double StandardCornerRadius = 8;
            public const double SmallCornerRadius = 4;

            // Fluent Dark Theme Colors
            public static readonly Color FluentDarkSurface = Color.FromRgb(45, 45, 48);
            public static readonly Color FluentDarkOnSurface = Color.FromRgb(255, 255, 255);
            public static readonly Color FluentDarkPrimary = Color.FromRgb(0, 120, 212);
            public static readonly Color FluentDarkSecondary = Color.FromRgb(138, 136, 134);
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates if a property is supported by SfLinearProgressBar
        /// </summary>
        public static bool IsValidProgressBarProperty(string propertyName)
        {
            var validProperties = new[]
            {
                ProgressBar.ValueProperty,
                ProgressBar.MinimumProperty,
                ProgressBar.MaximumProperty,
                ProgressBar.IsIndeterminateProperty,
                ProgressBar.ForegroundProperty,
                ProgressBar.BackgroundProperty,
                ProgressBar.WidthProperty,
                ProgressBar.HeightProperty,
                "Margin", "HorizontalAlignment", "VerticalAlignment"
            };

            return System.Array.Exists(validProperties, prop =>
                string.Equals(prop, propertyName, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the correct property name for progress bar styling
        /// </summary>
        public static string GetCorrectProgressBarProperty(string attemptedProperty)
        {
            return attemptedProperty.ToLowerInvariant() switch
            {
                "progressbackground" => ProgressBar.ForegroundProperty,
                "trackbackground" => ProgressBar.BackgroundProperty,
                "progresscolor" => ProgressBar.ForegroundProperty,
                "trackcolor" => ProgressBar.BackgroundProperty,
                _ => attemptedProperty
            };
        }

        #endregion
    }
}
