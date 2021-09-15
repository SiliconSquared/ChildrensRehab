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
    public class EnumToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            try
            {
                Dictionary<string, string> setDictionary = new Dictionary<string, string>();
                var sets = ((string)parameter).Split(';');
                foreach (string set in sets)
                {
                    var pair = set.Split(',');
                    setDictionary[pair[0]] = pair[1];
                }
                var val = setDictionary[value.ToString()];
                return (SolidColorBrush)new BrushConverter().ConvertFromString(val);
            }
            catch { }
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
