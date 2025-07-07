using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BusBuddy.WPF.Converters
{
    public class UserMsgBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isUser = value is bool b && b;
            return isUser ? new SolidColorBrush(Color.FromRgb(30, 144, 255)) : new SolidColorBrush(Color.FromRgb(44, 62, 80));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
