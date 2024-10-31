using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SprueKit.Commands
{
    public class CommandInfo
    {
        /// <summary>
        /// Name for display. Likely just a title case version of the tooltip unless the tooltip is more descriptive.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Access key text
        /// </summary>
        public string ShortCut { get; set; }

        /// <summary>
        /// Bitmap icon to use in UI as needed.
        /// </summary>
        public BitmapImage Icon { get; set; }

        /// <summary>
        /// Tooltip to use in UI as needed.
        /// </summary>
        public string ToolTip { get; set; }

        /// <summary>
        /// Action to execute when the command is invoked. Parameter object is determined elsewhere.
        /// </summary>
        public Action<Document, object> Action { get; set; }
    }
}
