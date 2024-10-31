using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace SprueKit.Data
{
    public static class Trig
    {
        public static Vector3 ClosestPoint(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 dir = b - a;
            return a + Mathf.Clamp01(Vector3.Dot((point - a), dir) / dir.LengthSquared()) * dir;
        }

        public static Vector2 ClosestPoint(Vector2 a, Vector2 b, Vector2 point)
        {
            Vector2 dir = b - a;
            return a + Mathf.Clamp01(Vector2.Dot((point - a), dir) / dir.LengthSquared()) * dir;
        }
    }
}
