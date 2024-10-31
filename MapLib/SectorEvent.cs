using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapLib
{
    public abstract class SectorEvent
    {
        public abstract bool PropagateThrough(SideDef side);
        /// return true for the event to be swallowed
        public abstract bool DoEvent(Sector sector);
        /// popped is called whenever the event tree cycles back up
        public void Popped() { }
    }
}
