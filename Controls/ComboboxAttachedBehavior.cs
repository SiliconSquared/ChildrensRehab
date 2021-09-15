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
    public static class ComboboxAttachedBehavior
    {
        public static readonly DependencyProperty ToggleButtonTooltipProperty = DependencyProperty.RegisterAttached("ToggleButtonTooltip", typeof(string), typeof(ComboboxAttachedBehavior), new PropertyMetadata(null));

        public static string GetToggleButtonTooltip(DependencyObject d)
        {
            return (string)d.GetValue(ToggleButtonTooltipProperty);
        }

        public static void SetToggleButtonTooltip(DependencyObject d, string value)
        {
            d.SetValue(ToggleButtonTooltipProperty, value);
        }



        public static readonly DependencyProperty ButtonGlyphContentProperty = DependencyProperty.RegisterAttached("ButtonGlyphContent", typeof(ControlTemplate), typeof(ComboboxAttachedBehavior), new PropertyMetadata(null));

        public static ControlTemplate GetButtonGlyphContent(DependencyObject d)
        {
            return (ControlTemplate)d.GetValue(ButtonGlyphContentProperty);
        }

        public static void SetButtonGlyphContent(DependencyObject d, string value)
        {
            d.SetValue(ButtonGlyphContentProperty, value);
        }


    }
}
