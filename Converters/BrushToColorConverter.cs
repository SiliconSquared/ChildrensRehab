using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RehabKiosk.Converters
{
    class BrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            var brush = value as SolidColorBrush;
            return brush.Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
