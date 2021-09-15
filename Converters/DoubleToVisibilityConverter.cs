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
    public class DoubleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            double dVal  = (double) value;

            bool inverse = (parameter != null && String.Compare((string)parameter, "True", StringComparison.OrdinalIgnoreCase) == 0);
            return (dVal == 0) ? (inverse ? Visibility.Collapsed : Visibility.Visible) : (inverse ? Visibility.Visible : Visibility.Collapsed);
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
