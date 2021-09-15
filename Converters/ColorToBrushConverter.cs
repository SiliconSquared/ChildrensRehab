using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace RehabKiosk.Converters
{
    public class ColorToBrushConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            System.Windows.Media.Color color = (System.Windows.Media.Color)value;

            if(color.A == 0)
                return Application.Current.FindResource("TransparencyBrush") as Brush;
            else
                return new SolidColorBrush((System.Windows.Media.Color)color);
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }

    }
}
