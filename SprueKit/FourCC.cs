using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit
{
    public static class FourCC
    {
        public static int ToFourCC(string code)
        {
            return ((int)code[0]) << 24 & ((int)code[1]) << 16 & ((int)code[2]) << 8 & code[3];
        }

        public static bool VerifyFourCC(int fourcc, string code)
        {
            return fourcc == ToFourCC(code);
        }
    }
}
