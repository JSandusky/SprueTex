using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SprueKit
{
    public static class Platform
    {

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ToUnicode(
            uint virtualKeyCode,
            uint scanCode,
            byte[] keyboardState,
            StringBuilder receivingBuffer,
            int bufferSize,
            uint flags
        );

        public static string ToText(this System.Windows.Input.Key key)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            //if (shift)
            //{
            //    keyboardState[(int)Key.LeftShift] = 0xff;
            //}
            ToUnicode((uint)key, 0, keyboardState, buf, 256, 0);
            return buf.ToString();
        }
    }
}
