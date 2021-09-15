using System;
using System.Globalization;
using System.Windows.Data;

namespace RehabKiosk.Converters
{
    public class SelectableArcControlHeightAdjuster : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return (double)value + System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}

