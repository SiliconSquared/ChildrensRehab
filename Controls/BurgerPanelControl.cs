using System;
using System.Windows.Controls;

namespace RehabKiosk.Controls
{
    public partial class BurgerPanelControl : UserControl
    {
        public EventHandler PanelCloseRequest = null;


        public virtual double PanelWidth
        {
            get
            {
                return 0;
            }
        }

    }
}   

