using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace MapLib
{
    public abstract class MapObject
    {
        public abstract Vector2 Centroid { get; }
    }
}
