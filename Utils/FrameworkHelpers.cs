using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RehabKiosk.Utils
{
    public static class FrameworkHelpers
    {

        public static T FindVisualChildByName<T>(DependencyObject depObj, string name) where T : DependencyObject
        {
            if (depObj != null)
            {
                int count = VisualTreeHelper.GetChildrenCount(depObj);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    FrameworkElement ele = child as FrameworkElement;
                    if (ele != null && ele.Name.CompareTo(name) == 0)
                    {
                        return (T)child;
                    }

                    T childItem = FindVisualChildByName<T>(child, name);
                    if (childItem != null) return childItem;
                }
            }

            /*
            if (depObj is T)
                return depObj as T;
                */
            return null;
        }



        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }



    }
}
