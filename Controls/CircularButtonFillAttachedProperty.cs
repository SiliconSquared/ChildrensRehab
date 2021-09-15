using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RehabKiosk.Controls
{
    public static class CircularButtonFillAttachedProperty
    {
        public static readonly DependencyProperty CircularButtonFillProperty = DependencyProperty.RegisterAttached("CircularButtonFill", typeof(Brush), typeof(CircularButtonFillAttachedProperty), new PropertyMetadata(null));

        public static SolidColorBrush GetCircularButtonFill(DependencyObject d)
        {
            return (SolidColorBrush)d.GetValue(CircularButtonFillProperty);
        }

        public static void SetCircularButtonFill(DependencyObject d, SolidColorBrush value)
        {
            d.SetValue(CircularButtonFillProperty, value);
        }

    }
}

