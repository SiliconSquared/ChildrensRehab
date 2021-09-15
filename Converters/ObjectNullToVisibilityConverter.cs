using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RehabKiosk.Converters
{
    public class ObjectNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            bool inverse = (parameter != null && String.Compare((string)parameter, "True", StringComparison.OrdinalIgnoreCase) == 0);
            return (value != null) ? (inverse ? Visibility.Collapsed : Visibility.Visible) : (inverse ? Visibility.Visible : Visibility.Collapsed);
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
