using System;
using System.Globalization;
using System.Windows.Data;
using BusBuddy.WPF.ViewModels.Student;

namespace BusBuddy.WPF.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string parameterString = parameter.ToString() ?? string.Empty;
            if (value is FilterStatus status)
            {
                return status.ToString() == parameterString;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null || !(bool)value)
                return Binding.DoNothing;

            string parameterString = parameter.ToString() ?? string.Empty;
            if (Enum.TryParse(typeof(FilterStatus), parameterString, out var result))
            {
                return result;
            }

            return Binding.DoNothing;
        }
    }
}
