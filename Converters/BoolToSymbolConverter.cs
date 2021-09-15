using System;
using System.Globalization;
using System.Windows.Data;

namespace RehabKiosk.Converters
{
    class BoolToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            var symbols = ((string)parameter).Split(',');
            return symbols[(bool)value ? 0 : 1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
