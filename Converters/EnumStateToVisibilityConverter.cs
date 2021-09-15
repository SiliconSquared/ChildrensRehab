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
    public class EnumStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            bool inverse = false;
            string enumVal = value.ToString();

            var pair = ((string)parameter).Split(',');
            if (pair.Length > 1)
                inverse = String.Compare(pair[1], "True", StringComparison.OrdinalIgnoreCase) == 0;

            if(inverse)
                return String.Compare(enumVal, pair[0], true) == 0 ? Visibility.Collapsed : Visibility.Visible;
            else
                return String.Compare(enumVal, pair[0], true) == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
