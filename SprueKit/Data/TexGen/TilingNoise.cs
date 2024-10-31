using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.TexGen
{
    public class TilingNoise
    {
        // Hashing
        private const int X_PRIME = 1619;
        private const int Y_PRIME = 31337;
        private const int Z_PRIME = 6971;
        private const int W_PRIME = 1013;

        [MethodImplAttribute(256)]
        private static int Hash4D(int seed, int x, int y, int z, int w)
        {
            int hash = seed;
            hash ^= X_PRIME * x;
            hash ^= Y_PRIME * y;
            hash ^= Z_PRIME * z;
            hash ^= W_PRIME * w;
            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;
            return hash;
        }
    }
}
