using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstFloor.ModernUI.Windows.Navigation
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFrameLocator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ModernFrame TargetFrame();
    }
}
