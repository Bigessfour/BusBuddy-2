using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BusBuddy.WPF.Converters
{
    /// <summary>
    /// Converts validation errors to visibility for showing error indicators
    /// </summary>
    public class ValidationErrorsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<ValidationError> errors && errors.Any())
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
    /// Converts validation errors to a string message for display
    /// </summary>
    public class ValidationErrorsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<ValidationError> errors && errors.Any())
            {
                // Get the first error message
                return errors.FirstOrDefault()?.ErrorContent.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean value to a Visibility value
    /// </summary>
    public class BooleanToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Formats dates according to the FormatUtils standards
    /// </summary>
    public class DateTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                string format = parameter as string ?? "MM/dd/yyyy";
                return dateTime.ToString(format, culture);
            }
            else if (value is DateTimeOffset dateTimeOffset)
            {
                string format = parameter as string ?? "MM/dd/yyyy";
                return dateTimeOffset.DateTime.ToString(format, culture);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                if (DateTime.TryParse(stringValue, culture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
