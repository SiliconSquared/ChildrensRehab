using System;
using System.Globalization;
using System.Windows.Data;

namespace RehabKiosk.Converters
{
    public class CustomMessageBoxIconTypeToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            string character = null;
            var type = (PopupMessageBox.IconTypes)value;
            switch(type)
            {
                case PopupMessageBox.IconTypes.EXCLAMATION:
                    character = "\uE171";
                    break;

                case PopupMessageBox.IconTypes.QUESTION:
                    character = "\uE11B";
                    break;

                case PopupMessageBox.IconTypes.INFO:
                    character = "\uE134";
                    break;
            }
            return character;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
