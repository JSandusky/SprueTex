using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Plane = Microsoft.Xna.Framework.Plane;

namespace MapLib
{
    public static class Slopes
    {
        public static LineDef FindSlopeLine(Sector sector, bool forCeiling)
        {
            int argIndex = forCeiling ? 1 : 0;
            for (int i = 0; i < sector.Sides.Length; ++i)
            {
                SideDef side = sector.Sides[i];
                if (side.Line.ActionCode == ActionCodes.PLANE_ALIGN && side.Line.ActionArgs != null && side.Line.ActionArgs.Length > 1)
                {
                    int arg = side.Line.ActionArgs[argIndex];
                    if (side.IsFront && arg == 1)
                    { //Plane_Align front side
                        return side.Line;
                    }
                    else if (!side.IsFront && arg == 2)
                    { //Plane_Align back side
                        return side.Line;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the farthest vertex from the given line in order to form a plane based on that pt and the two points of the given line
        /// The height of the sector on the opposite side of the point containing the line is chosen for the lower height
        /// </summary>
        /// <param name="sector"></param>
        /// <param name="line"></param>
        /// <param name="ceiling"></param>
        /// <returns></returns>
        public static Plane ConstructPlane(Sector sector, LineDef line, bool ceiling)
        {
            float sectorHeight = ceiling ? sector.CurrentCeilingHeight : sector.CurrentFloorHeight;
            Sector otherSector = line.Front.Sector != sector ? line.Front.Sector : line.Back.Sector;
            if (otherSector == null)
                return new Plane();

            float otherSectorHeight = ceiling ? otherSector.CurrentCeilingHeight : otherSector.CurrentFloorHeight;

            Vector2 midPoint = line.MidPoint;
            Vector2 farthestPoint = midPoint;
            float farthestDist = 0f;
            for (int i = 0; i < sector.Sides.Length; ++i)
            {
                SideDef side = sector.Sides[i];
                if (side.Line != line)
                {
                    float dst = DistanceLinePoint(line.A.Position, line.B.Position, side.Line.A.Position);
                    if (dst > farthestDist)
                    {
                        farthestDist = dst;
                        farthestPoint = side.Line.A.Position;
                    }
                    dst = DistanceLinePoint(line.A.Position, line.B.Position, side.Line.B.Position);
                    if (dst > farthestDist)
                    {
                        farthestDist = dst;
                        farthestPoint = side.Line.B.Position;
                    }
                }
            }

            Vector3 mid3d = new Vector3(line.A.Position.X, otherSectorHeight, line.A.Position.Y);
            Vector3 rightEdge = new Vector3(line.B.Position.X, otherSectorHeight, line.B.Position.Y);
            Vector3 far3d = new Vector3(farthestPoint.X, sectorHeight, farthestPoint.Y);

            return new Plane(mid3d, far3d, rightEdge);
        }

        static float DistanceLinePoint(Vector2 a, Vector2 b, Vector2 t)
        {
            return DistanceLinePoint(a.X, a.Y, b.X, b.Y, t.X, t.Y);
        }

        static float DistanceLinePoint(float startX, float startY, float endX, float endY, float pointX, float pointY)
        {
            float normalLength = (float)Math.Sqrt((endX - startX) * (endX - startX) + (endY - startY) * (endY - startY));
            return Math.Abs((pointX - startX) * (endY - startY) - (pointY - startY) * (endX - startX)) / normalLength;
        }

    }
}
