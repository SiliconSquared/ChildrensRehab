using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace RehabKiosk.Converters
{
    public class ActiveColorPatchToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            try
            {
                var pair = ((string)parameter).Split(',');
                if (String.Compare(pair[0], (string)value.ToString(), true) == 0)
                    return (SolidColorBrush)new BrushConverter().ConvertFromString(pair[1]);
                else
                    return (SolidColorBrush)new BrushConverter().ConvertFromString(pair[2]);
            }
            catch { }
            return new SolidColorBrush(Colors.Transparent);
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
