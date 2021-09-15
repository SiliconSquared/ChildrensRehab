using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RehabKiosk.Controls
{
    public class DependencyEventer
    {
        public event EventHandler WiredEvent;

        public void TriggerEvent(EventArgs e)
        {
            if (WiredEvent != null)
                WiredEvent(this, e);
        }
    }
}
