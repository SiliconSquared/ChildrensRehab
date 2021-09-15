using RehabKiosk.Model;
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
    /// Interaction logic for AssetPanel.xaml
    /// </summary>
    public partial class AssetPanel : UserControl
    {
        public AssetPanel()
        {
            InitializeComponent();
        }



        public string FullAssetImageFilename
        {
            get
            {
                RehabAsset asset = DataContext as RehabAsset;

                return App.ViewModel.AssetManager.WorkingFolder + asset.AssetImageFilename;
            }
        }




    }
}
