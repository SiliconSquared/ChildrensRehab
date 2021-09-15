using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RehabKiosk.Controls
{
    //
    // Summary:
    //     Notifies clients that a property value has changed.
    public interface IInfoPopupChild
    {
        //
        // Summary:
        //     Occurs when a property value changes.
        event EventHandler CloseRequestEvent;
    }



}

