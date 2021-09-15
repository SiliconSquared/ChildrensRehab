using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RehabKiosk.Controls
{
    /// <summary>
    /// Interaction logic for WaitingSpinner.xaml
    /// </summary>
    public partial class WaitingSpinner : UserControl
    {
        private Storyboard _spinnerStoryboard;



        public static readonly DependencyProperty ActiveProperty = DependencyProperty.Register("Active", typeof(bool), typeof(WaitingSpinner), new PropertyMetadata(false, OnActivePropertyChanged));
        public bool Active
        {
            get { return (bool)GetValue(ActiveProperty); }
            set { SetValue(ActiveProperty, value); }
        }



        private static void OnActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue != (bool)e.OldValue)
            {
                if ((bool)e.NewValue)
                    ((WaitingSpinner)d)._spinnerStoryboard.Begin();
                else
                    ((WaitingSpinner)d)._spinnerStoryboard.Stop();
            }
        }



        public WaitingSpinner()
        {
            InitializeComponent();

            _spinnerStoryboard = FindResource("spinner") as Storyboard;
        }
    }
}
