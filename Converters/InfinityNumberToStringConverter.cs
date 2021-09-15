using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace RehabKiosk.Converters
{
    public class InfinityNumberToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            double val = (double)value;
            if(val == 0)
                return Char.ConvertFromUtf32('\u221E');
            return (string) val.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return new NotImplementedException();
        }

    }
}
