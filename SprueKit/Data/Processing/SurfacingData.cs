using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.Processing
{
    class SurfacingData
    {
        public float[,,] Densities; // +1 of the dual, corners
        public bool[,,] Visited;    // to the dual vert

    }
}
