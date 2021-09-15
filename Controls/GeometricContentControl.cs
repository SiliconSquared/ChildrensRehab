using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RehabKiosk.Controls
{
    public class GeometricContentControl : ContentControl
    {
        static GeometricContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GeometricContentControl), new FrameworkPropertyMetadata(typeof(GeometricContentControl)));
        }



        public static readonly DependencyProperty ControlGeometryProperty = DependencyProperty.Register("ControlGeometry", typeof(Geometry), typeof(GeometricContentControl), new UIPropertyMetadata(null));
        public Geometry ControlGeometry
        {
            get { return (Geometry)GetValue(ControlGeometryProperty); }
            set { SetValue(ControlGeometryProperty, value); }
        }
    }
}

