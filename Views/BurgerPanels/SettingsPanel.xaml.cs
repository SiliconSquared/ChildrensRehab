using RehabKiosk.Controls;
using RehabKiosk.Views;
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

namespace RehabKiosk
{
    /// <summary>
    /// Interaction logic for SettingsPanel.xaml
    /// </summary>
    public partial class SettingsPanel : BurgerPanelControl
    {
        public event EventHandler OnCloseRequest = null;


        public SettingsPanel()
        {
            InitializeComponent();
        }


        public override double PanelWidth
        {
            get
            {
                return 200;
            }
        }



        private void Hyperlink_Navigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }



        private void WelcomeVideoButton_Click(object sender, RoutedEventArgs e)
        {

        }



        private void ContentTermsHyperlink_Navigate(object sender, RequestNavigateEventArgs e)
        {
        }


    }
}
