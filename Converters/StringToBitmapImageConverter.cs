using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;



namespace RehabKiosk.Converters
{
    public class StringToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            BitmapImage bi = null;
            string filename = value as string;

            try
            {
                using (FileStream fileStream = new FileStream(filename, FileMode.Open))
                {
                    bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = fileStream;
                    bi.EndInit();
                }
            }
            catch
            {
                bi = null;
            }
            return bi;
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }

    }
}
