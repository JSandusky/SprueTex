using FirstFloor.ModernUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit
{
    class ContentLoader : DefaultContentLoader
    {
        /// <summary>
        /// Overriden implementation prevents multiplicitous construction of main UI pages
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected override object LoadContent(Uri uri)
        {
            //moveable to an interface? How to get at the instance?
            if (uri.OriginalString.EndsWith("DesignScreen.xaml"))
            {
                if (Pages.DesignScreen.inst() != null)
                    return Pages.DesignScreen.inst();
            }
            return base.LoadContent(uri);
        }
    }
}
