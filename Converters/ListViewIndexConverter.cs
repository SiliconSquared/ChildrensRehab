﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace RehabKiosk.Converters
{
    public class ListViewIndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            return (((int) value)+1).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
}
