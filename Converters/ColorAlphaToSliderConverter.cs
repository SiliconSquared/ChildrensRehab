using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace RehabKiosk.Converters
{
    public sealed class ColorAlphaToSliderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            byte alpha = (byte)value;
            if (parameter == null)
                throw new Exception("ColorAlphaToSliderConverter exception Range parameter missing");
            double dparameter = System.Convert.ToDouble((string)parameter);
            return ((double)alpha / 255.0) * dparameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (parameter == null)
                throw new Exception("ColorAlphaToSliderConverter exception Range parameter missing");
            double dparameter = System.Convert.ToDouble((string)parameter);
            return (byte) (((double) value / (double) dparameter) * 255.0);
        }
    }
}
