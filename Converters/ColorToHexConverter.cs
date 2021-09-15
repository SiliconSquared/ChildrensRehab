using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace RehabKiosk.Converters
{
    public class ColorToHexConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            Color val = (Color)value;
            return "#" + String.Format("{0:X2}", val.A) + String.Format("{0:X2}", val.R) + String.Format("{0:X2}", val.G) + String.Format("{0:X2}", val.B);
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            byte alpha;
            byte pos = 0;
            if (value == null)
                return Colors.Transparent;

            string strColor = value.ToString();

            if (strColor.Length < 6)
                return Colors.Transparent;
            if (strColor[0] != '#')
                return Colors.Red;
            string hex = value.ToString().Replace("#", "");

            if (hex.Length == 8)
            {
                alpha = System.Convert.ToByte(hex.Substring(pos, 2), 16);
                pos = 2;
            }
            else
            {
                alpha = System.Convert.ToByte("ff", 16);
            }

            byte red = System.Convert.ToByte(hex.Substring(pos, 2), 16);

            pos += 2;
            byte green = System.Convert.ToByte(hex.Substring(pos, 2), 16);

            pos += 2;
            byte blue = System.Convert.ToByte(hex.Substring(pos, 2), 16);

            return Color.FromArgb(alpha, red, green, blue);
        }
    }

}
