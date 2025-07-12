using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Converters
{
    /// <summary>
    /// Converter to show enhanced header background for Dashboard view
    /// </summary>
    public class ViewModelToHeaderBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DashboardViewModel)
            {
                return new LinearGradientBrush(
                    new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(0x1E, 0x88, 0xE5), 0.0),
                        new GradientStop(Color.FromRgb(0x42, 0xA5, 0xF5), 1.0)
                    },
                    new Point(0, 0),
                    new Point(1, 1));
            }
            return new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter to adjust header height for Dashboard view
    /// </summary>
    public class ViewModelToHeaderHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DashboardViewModel)
            {
                return 120.0; // Enhanced height for dashboard
            }
            return 80.0; // Standard height for other views
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter to show/hide enhanced dashboard header elements
    /// </summary>
    public class ViewModelToHeaderVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DashboardViewModel)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter to determine if current view is Dashboard
    /// </summary>
    public class IsDashboardViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is DashboardViewModel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter to get view-specific title
    /// </summary>
    public class ViewModelToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                DashboardViewModel => "Enhanced Dashboard",
                BusManagementViewModel => "Bus Management",
                DriverManagementViewModel => "Driver Management",
                RouteManagementViewModel => "Route Management",
                StudentManagementViewModel => "Student Management",
                MaintenanceTrackingViewModel => "Maintenance Tracking",
                FuelManagementViewModel => "Fuel Management",
                ActivityLogViewModel => "Activity Log",
                SettingsViewModel => "Settings",
                _ => "Bus Buddy"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter to get view-specific subtitle for Dashboard
    /// </summary>
    public class ViewModelToSubtitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DashboardViewModel)
            {
                return "Real-time Fleet Overview & Analytics";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
