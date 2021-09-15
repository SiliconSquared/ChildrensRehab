using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RehabKiosk.Converters
{
    public sealed class DoubleToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            double dval = (double)value;
            double threshold = (double)System.Convert.ToDouble(parameter);
            return (double) dval >= threshold ? 1.0 : 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return new NotImplementedException();
        }
    }
}
