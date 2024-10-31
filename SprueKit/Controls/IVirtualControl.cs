using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SprueKit.Controls
{
    public interface IVirtualControl
    {
        /// <summary>
        /// Area required by this control.
        /// </summary>
        Size RequiredArea();

        /// <summary>
        /// Sets the offset inside of this control.
        /// </summary>
        void SetScrollOffset(Size size);

        VirtualScrollArea Area { get; set; }
    }
}
