using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SprueKit.Controls.Code.Intellisense
{
    public interface IntellisenseSource
    {
        Globals GetGlobals();
        void HookEditor(TextEditor editor, object item);
        void DocumentChanged(TextEditor editor, object item);
        void EditorKeyUp(TextEditor editor, DepthScanner depthScanner, KeyEventArgs e);
        void EditorMouseHover(TextEditor editor, DepthScanner depthScanner, MouseEventArgs e);
    }
}
