using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace MapLib
{
    public class SideDefSorter : IComparer<SideDef>
    {
        public int Compare(SideDef lhs, SideDef rhs)
        {
            bool lhsOther = lhs.Opposite != null;
            bool rhsOther = rhs.Opposite != null;
            if (!lhsOther && !rhsOther)
                return 0;
            else if (lhsOther && !rhsOther)
                return -1;
            else if (rhsOther && !lhsOther)
                return 1;

            int lhsIndex = lhs.Opposite.Sector.Index;
            int rhsIndex = rhs.Opposite.Sector.Index;
            if (lhsIndex == rhsIndex)
                return 0;
            else if (lhsIndex < rhsIndex)
                return -1;
            else
                return 1;
        }
    }

    public static class Intersector
    {
        public static bool IntersectLines(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)
        {
            float x1 = p1.X, y1 = p1.Y, x2 = p2.X, y2 = p2.Y, x3 = p3.X, y3 = p3.Y, x4 = p4.X, y4 = p4.Y;

            float d = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            if (d == 0)
                return false;

            if (intersection != null)
            {
                float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / d;
                intersection = new Vector2(x1 + (x2 - x1) * ua, y1 + (y2 - y1) * ua);
            }
            return true;
        }

        public static float ManhattanDistance(Vector2 p1, Vector2 p2)
        {
            return Microsoft.Xna.Framework.MathHelper.Distance(p1.X, p2.X) + Microsoft.Xna.Framework.MathHelper.Distance(p1.Y, p2.Y);
        }
    }

    public static class General
    {
        public static int BitsForInt(int v)
        {
            int[] LOGTABLE = new[] {
              0, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3,
              4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
              5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
              5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
              6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
              6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
              6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
              6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7 };

            int r;  // r will be lg(v)
            int t, tt;

            if (Int2Bool(tt = v >> 16))
            {
                r = Int2Bool(t = tt >> 8) ? 24 + LOGTABLE[t] : 16 + LOGTABLE[tt];
            }
            else
            {
                r = Int2Bool(t = v >> 8) ? 8 + LOGTABLE[t] : LOGTABLE[v];
            }

            return r;
        }

        // Convert integer to bool
        internal static bool Int2Bool(int v)
        {
            return (v != 0);
        }
    }
}
