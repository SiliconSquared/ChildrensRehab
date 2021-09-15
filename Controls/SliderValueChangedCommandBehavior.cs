using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RehabKiosk.Controls
{
    public static class SliderCommandBehavior
    {
        public static readonly DependencyProperty SliderValueChangedCommand = EventBehaviourFactory.CreateCommandExecutionEventBehaviour(Slider.ValueChangedEvent, "SliderValueChangedCommand", typeof(SliderCommandBehavior));

        public static void SetSliderValueChangedCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(SliderValueChangedCommand, value);
        }

        public static ICommand GetSliderValueChangedCommand(DependencyObject o)
        {
            return o.GetValue(SliderValueChangedCommand) as ICommand;
        }



        public static readonly DependencyProperty SliderLostFocusCommand = EventBehaviourFactory.CreateCommandExecutionEventBehaviour(Slider.ValueChangedEvent, "SliderLostFocusCommand", typeof(SliderCommandBehavior));

        public static void SetSliderLostFocusCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(SliderValueChangedCommand, value);
        }

        public static ICommand GetSliderLostFocusCommand(DependencyObject o)
        {
            return o.GetValue(SliderValueChangedCommand) as ICommand;
        }

    }
}
