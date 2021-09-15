using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RehabKiosk.Converters
{
    public class CustomMessageBoxButtonTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            var visibility = Visibility.Collapsed;
            var button = (string)parameter;
            var type = (PopupMessageBox.ButtonTypes)value;

            if (String.Compare(button, "OK") == 0 && type == PopupMessageBox.ButtonTypes.OK)
                visibility = Visibility.Visible;
            else if (String.Compare(button, "YES") == 0 && type == PopupMessageBox.ButtonTypes.YESNO)
                visibility = Visibility.Visible;
            else if (String.Compare(button, "NO") == 0 && type == PopupMessageBox.ButtonTypes.YESNO)
                visibility = Visibility.Visible;

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
