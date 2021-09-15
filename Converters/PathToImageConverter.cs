using RehabKiosk;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RehabKiosk.Converters
{
    public class PathToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string path = value as string;
                if (path != null)
                {
                    BitmapImage image = new BitmapImage();
                    string filename = App.ViewModel.AssetManager.WorkingFolder + path;
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        image.BeginInit();
                        image.StreamSource = stream;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit(); // load the image from the stream
                    } // close the stream
                    return image;
                }
            }
            catch { }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
