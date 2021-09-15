using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RehabKiosk.Converters
{
    public sealed class DoubleInflatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            double val = (double)value;
            double inflation = (double)System.Convert.ToDouble(parameter);
            return Math.Max(0, val + inflation);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
