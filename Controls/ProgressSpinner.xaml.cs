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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RehabKiosk.Controls
{
    /// <summary>
    /// Interaction logic for PraogressSpinner.xaml
    /// </summary>
    public partial class ProgressSpinner : UserControl
    {
        public ProgressSpinner()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty CompletionProperty = DependencyProperty.Register("Completion", typeof(double), typeof(ProgressSpinner), new PropertyMetadata(0.0));
        public double Completion
        {
            get { return (double)GetValue(CompletionProperty); }
            set { SetValue(CompletionProperty, value); }
        }
    }
}
