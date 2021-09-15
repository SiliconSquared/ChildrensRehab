using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RehabKiosk.Converters
{
    public class ByteArrayToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            BitmapImage image = null;
            byte[] imageBytes = value as byte[];

            if (imageBytes != null)
            {
                try
                {
                    MemoryStream ms = new MemoryStream((byte[])value);
                    image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = ms;
                    image.EndInit();
                }
                catch
                {
                    image = null;
                }
            }
            return image;
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
