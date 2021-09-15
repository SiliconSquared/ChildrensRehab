using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RehabKiosk.Controls
{
    public class ColorDropperBehavior : Behavior<Image>
    {
        public class DropperEventArgs
        {
            public Color SampledColor { get; set; }
            public bool RightClick { get; set; }
        }

        private RenderTargetBitmap _renderTargetBitmap = null;

        public event EventHandler<DropperEventArgs> OnDroppedColorChanged;


        public static readonly DependencyProperty HoverOverColorCursorProperty = DependencyProperty.Register("HoverOverColorCursor", typeof(Cursor), typeof(ColorDropperBehavior), new UIPropertyMetadata(Cursors.Arrow));

        public Cursor HoverOverColorCursor
        {
            get { return (Cursor)GetValue(HoverOverColorCursorProperty); }
            set { SetValue(HoverOverColorCursorProperty, value); }
        }



        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorDropperBehavior), new UIPropertyMetadata(Colors.White));

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }



        public static readonly DependencyProperty HostProperty = DependencyProperty.Register("Host", typeof(FrameworkElement), typeof(ColorDropperBehavior), new UIPropertyMetadata(null));
        public FrameworkElement Host
        {
            get { return (FrameworkElement)GetValue(HostProperty); }
            set { SetValue(HostProperty, value); }
        }



        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        }



        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        }



        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
                SamplePixelForColor(e.RightButton == MouseButtonState.Pressed);
        }



        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            var pixels = ExtractPixelUnderPoint();
            if (pixels == null || (pixels[2] == 0 && pixels[1] == 0 && pixels[0] == 0))
                Host.Cursor = Cursors.Arrow;
            else
                Host.Cursor = HoverOverColorCursor;

            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
                SamplePixelForColor(e.RightButton == MouseButtonState.Pressed);
        }



        private byte [] ExtractPixelUnderPoint()
        {
            byte[] pixels = null;

            // Retrieve the coordinate of the mouse position in relation to the supplied image.
            var point = Mouse.GetPosition(AssociatedObject);

            // if (_renderTargetBitmap == null)
            {
                // Use RenderTargetBitmap to get the visual, in case the image has been transformed.
                _renderTargetBitmap = new RenderTargetBitmap((int)AssociatedObject.ActualWidth,
                                                                (int)AssociatedObject.ActualHeight,
                                                                96, 96, PixelFormats.Default);
                _renderTargetBitmap.Render(AssociatedObject);

                /*
                using (FileStream stream = File.Create("c:\\temp\\Render.bmp"))
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(_renderTargetBitmap));
                    encoder.Save(stream);
                }*/                
            }

            // Make sure that the point is within the dimensions of the image.
            if (_renderTargetBitmap != null && point.X >= 0 && (point.X < _renderTargetBitmap.PixelWidth) && point.Y >= 0 && (point.Y < _renderTargetBitmap.PixelHeight))
            {
                // Create a cropped image at the supplied point coordinates.
                var croppedBitmap = new CroppedBitmap(_renderTargetBitmap, new Int32Rect((int)point.X, (int)point.Y, 1, 1));

                // Copy the sampled pixel to a byte array.
                pixels = new byte[4];
                croppedBitmap.CopyPixels(pixels, 4, 0);
            }
            return pixels;
        }



        private void SamplePixelForColor(bool rightButton)
        {
            var pixels = ExtractPixelUnderPoint();
            if (pixels[2] == 0 && pixels[1] == 0 && pixels[0] == 0)
                SelectedColor = Colors.Transparent;
            else
            {
                // Assign the sampled color to a SolidColorBrush and return as conversion.
                SelectedColor = Color.FromArgb(255, pixels[2], pixels[1], pixels[0]);
                //Debug.WriteLine("X, Y" + point.X + ", " + point.Y + " - " + pixels[2] + pixels[1] + pixels[0]);
            }
            OnDroppedColorChanged?.Invoke(this, new DropperEventArgs() { SampledColor = SelectedColor, RightClick = rightButton });
        }
    }
}
