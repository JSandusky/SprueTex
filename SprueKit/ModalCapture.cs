using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit
{
    /// <summary>
    /// A Modal Capture prevents other UI interactions outside of a set of white-listed controls
    /// </summary>
    public interface ModalCapture
    {
        string Title { get; }
        List<object> Whitelist { get; }
        void Finish();
        void Cancel();
    }
}
