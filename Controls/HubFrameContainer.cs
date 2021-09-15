using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RehabKiosk.Controls
{
    public class HubFrameContainer : Grid
    {
        private Dictionary<string, FrameworkElement> _pages = new Dictionary<string, FrameworkElement>();
        private string _activePageKey = null;


        public void AddPage(object keyObject, FrameworkElement page)
        {
            _pages.Add(keyObject.ToString(), page);
            page.Visibility = Visibility.Collapsed;
            page.Opacity = 0;
            this.Children.Add(page);
        }



        public void Display(object keyObject)
        {
            string key = keyObject.ToString();
            if(String.Compare(_activePageKey, key) != 0)
            {
                if (_activePageKey != null)
                {
                    Storyboard hubFrameContainerSwitchHide = _pages[_activePageKey].FindResource("HubFrameContainerSwitchHide") as Storyboard;
                    hubFrameContainerSwitchHide.Begin();
                }

                Storyboard hubFrameContainerSwitchShow = _pages[key].FindResource("HubFrameContainerSwitchShow") as Storyboard;
                hubFrameContainerSwitchShow.Begin();
                _activePageKey = key;
            }
        }


    }
}
