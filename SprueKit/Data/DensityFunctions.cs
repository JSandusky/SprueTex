using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data
{
    public static class DensityFunctions
    {

        public static float SphereDensity(Vector3 pos, Vector4 param)
        {
            //float val = (param.X * param.X) - pos.LengthSquared();
            //return -val;//-(val * val);
            return pos.Length() - param.X;
        }

        public static float CubeDensity(Vector3 pos, Vector4 param)
        {
            Vector3 d = pos.Abs() - (param.XYZ() / 2.0f);
            return Math.Min(d.MaxElement(), 0.0f) + XNAExt.MaxVector(d, Vector3.Zero).Length();
        }

        public static float RoundedBoxDensity(Vector3 p, Vector4 param)
        {
            Vector3 d = (p.Abs() - param.XYZ()) * 0.5f;
            return Math.Min(d.MaxElement(), 0.0f) + XNAExt.MaxVector(d, Vector3.Zero).Length() - param.W;
        }

        public static float CapsuleDensity(Vector3 pos, Vector4 param)
        {
            Vector3 a = new Vector3(0.0f, 0.0f, param.Y/2.0f);
            Vector3 b = new Vector3(0.0f, 0.0f, -a.Z);
            
            Vector3 nearest = Trig.ClosestPoint(a, b, pos);
            return (pos - nearest).Length() - param.X;
        }

        public static float CylinderDensity(Vector3 p, Vector4 param)
        {
            float d = p.XZ().Length() - param.X;
            return Math.Max(d, Mathf.Abs(p.Y) - param.Y/2.0f);
        }

        public static float ConeDensity(Vector3 p, Vector4 param)
        {
            float c = param.Y;// / 2.0f;
            float r = param.X;
            float d = p.XZ().Length() - r * (1.0f - (c + p.Y) / (c + c));
            d = Math.Max(d, -p.Y - c);
            d = Math.Max(d, p.Y - c);
            return d;
        }

        public static float CappedConeDensity(Vector3 p, Vector4 param)
        {
            //Vec3 c(&params.r);
            //Vec2 q = Vec2(Vec2(p.x, p.z).Length(), p.y);
            //Vec2 v = Vec2(c.z * c.y / c.x, -c.z);
            //Vec2 w = v - q;
            //Vec2 vv = Vec2(v.Dot(v), v.x * v.x);
            //Vec2 qv = Vec2(v.Dot(w), v.x * w.x);
            //Vec2 d = SprueMax(qv, Vec2(0.0f, 0.0f)) * qv / vv;
            //return sqrtf(w.Dot(w) - SprueMax(d.x, d.y)) * sgn(SprueMax(q.y * v.x - q.x * v.y, w.y));
            return 0.0f;
        }

        public static float PlaneDistance(Vector3 p, Vector4 param)
        {
            // n must be normalized
            return Vector3.Dot(p, Vector3.UnitY);// + params.r;
        }

        public static float EllipsoidDistance(Vector3 p, Vector4 param)
        {
            // At 1.0f we're on the surface of the ellipsoid, if < than inside it, if > then outside it
            Vector3 r = param.XYZ();
            return ((p / r).Length() - 1.0f) * Math.Min(param.X, Math.Min(param.Y, param.Z));
        }

        public static float TorusDensity(Vector3 p, Vector4 param)
        {
            Vector2 q = new Vector2(p.XZ().Length() - param.X, p.Y);
            return q.Length() - param.Y;
        }

        public static float CombineDensity(float currentDensity, float newDensity, CSGOperation operand)
        {
            switch (operand)
            {
                case CSGOperation.Add:
                    return Math.Min(currentDensity, newDensity);
                case CSGOperation.Intersect:
                    return Math.Max(currentDensity, newDensity);
                case CSGOperation.Subtract:
                    return Math.Max(currentDensity, -newDensity);
                //case SM_Displace:
                //    return currentDensity + newDensity;
                case CSGOperation.Blend:
                    return Mathf.SmoothMin(currentDensity, newDensity);
                    //return powSmoothMin(currentDensity, newDensity);
                //default:
                //    return SprueMax(currentDensity, newDensity);
            }
            return Math.Max(currentDensity, newDensity);
        }
    }
}
