using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapLib
{
    public static class MapProcessing
    {
        // Removes a sector and all of its contents
        public static void RemoveSector(this Map fromMap, Sector sector)
        {
            fromMap.Sectors.Remove(sector);
            foreach (var sideDef in sector.Sides)
            {
                if (sideDef.Opposite != null)
                    sideDef.Line.RemoveSide(sideDef);
                fromMap.Sides.Remove(sideDef);
            }

            foreach (var lineDef in sector.Lines)
            {
                if (lineDef.IsNullLine())
                    fromMap.Lines.Remove(lineDef);
            }

            for (int i = 0; i < fromMap.Things.Count; ++i)
            {
                if (fromMap.Things[i].CurrentSector == sector)
                {
                    fromMap.Things.Remove(fromMap.Things[i]);
                    --i;
                }
            }
        }
    }
}
