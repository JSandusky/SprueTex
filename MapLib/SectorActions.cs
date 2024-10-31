using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapLib
{
    // Used for doing changes to a sector such as dropping the floor into a pit, crushing it into the ceiling, turning it into lava,
    // or just making it slippery
    public abstract class SectorAction
    {
        /// Return false if the action is done, in which case it will be removed
        public abstract bool Update(float td);

        /// Apply any necessary updates to things in this sector, such as physics forces/wind
        public virtual void UpdateThing(MapThing thing, float td)
        {

        }
    }

    public class PitTrapAction : SectorAction
    {
        public override bool Update(float td)
        {
            throw new NotImplementedException();
        }
    }

    public class FloorCrusherAction : SectorAction
    {
        public override bool Update(float td)
        {
            throw new NotImplementedException();
        }
    }
}
