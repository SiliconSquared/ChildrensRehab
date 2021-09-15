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
    public class ModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            var checkValue = value.ToString();
            var comparisonValue = (string)parameter;
            return String.Compare(checkValue, comparisonValue) == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            var useValue = (bool)value;
            var comparisonValue = parameter.ToString();

            if (useValue)
            {
                return Enum.Parse(targetType, comparisonValue);
            }

            return null;
        }
    }

}
