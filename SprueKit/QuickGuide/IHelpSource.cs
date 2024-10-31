using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.QuickGuide
{
    public interface IHelpSource
    {
        /// <summary>
        /// Return a Uri for the quick guide page
        /// </summary>
        /// <returns></returns>
        Uri GetQuickGuidePage();

        /// <summary>
        /// Return a Uri for the manual page.
        /// </summary>
        Uri GetManualPage();
    }
}
