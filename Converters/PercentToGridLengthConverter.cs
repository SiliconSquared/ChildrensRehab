using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RehabKiosk.Converters
{
    public class PercentToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            int pct = (int)value;
            bool invert = (parameter != null && String.Compare((string)parameter, "True", StringComparison.OrdinalIgnoreCase) == 0);
            return new GridLength(invert ? 100 - pct : pct, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return new NotImplementedException();
        }
    }
}

