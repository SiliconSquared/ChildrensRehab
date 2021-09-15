using System;
using System.Globalization;
using System.Windows.Data;

namespace RehabKiosk.Converters
{
    public class EnumStateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            var checkValue = value.ToString();
            var comparisonValue = (string)parameter;
            return String.Compare(checkValue, comparisonValue) == 0;
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
