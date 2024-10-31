using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SprueKit
{
    public struct ColorF
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public ColorF(float R, float G, float B) { this.R = R; this.G = G; this.B = B; A = 1.0f; }
        public ColorF(float R, float G, float B, float A) { this.R = R; this.G = G; this.B = B; this.A = A; }
        public ColorF(byte R, byte G, byte B, byte A) { this.R = R/255.0f; this.G = G/255.0f; this.B = B / 255.0f; this.A = A / 255.0f; }

        public ColorF PreMultipliedAlpha()
        {
            return new ColorF { R = this.R * this.A, G = this.G * this.A, B = this.B * A, A = 1.0f };
        }

        public static ColorF operator*(ColorF lhs, float val)
        {
            return new ColorF { R = lhs.R * val, G = lhs.G * val, B = lhs.B * val, A = lhs.A * val };
        }

        public static ColorF operator*(ColorF lhs, ColorF rhs)
        {
            return new ColorF
            {
                R = lhs.R * rhs.R,
                G = lhs.G * rhs.G,
                B = lhs.B * rhs.B,
                A = lhs.A * rhs.A
            };
        }

        public static ColorF operator +(ColorF lhs, float rhs)
        {
            return new ColorF
            {
                R = lhs.R + rhs,
                G = lhs.G + rhs,
                B = lhs.B + rhs,
                A = lhs.A + rhs
            };
        }

        public static ColorF operator+(ColorF lhs, ColorF rhs)
        {
            return new ColorF
            {
                R = lhs.R + rhs.R,
                G = lhs.G + rhs.G,
                B = lhs.B + rhs.B,
                A = lhs.A + rhs.A
            };
        }


        public static ColorF operator -(ColorF lhs, ColorF rhs)
        {
            return new SprueKit.ColorF
            {
                R = lhs.R - rhs.R,
                G = lhs.G - rhs.G,
                B = lhs.B - rhs.B,
                A = lhs.A - rhs.A
            };
        }

        public Color ToMonoGame()
        {
            return new Color(R, G, B, A);
        }

        public static ColorF FromMonoGame(Color color)
        {
            return new SprueKit.ColorF
            {
                R = color.R / 255.0f,
                G = color.G / 255.0f,
                B = color.B / 255.0f,
                A = color.A / 255.0f,
            };
        }
    }

    public struct Matrix3
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public static readonly Matrix3 Identity = new Matrix3 { a = Vector3.UnitX, b = Vector3.UnitY, c = Vector3.UnitZ };

        public void Mul(Matrix3 rhs)
        {
            a *= rhs.a;
            b *= rhs.b;
            c *= rhs.c;
        }

        public void Transpose()
        {
            Matrix3 v = this;
            a.Y = v.b.X;
            a.Z = v.c.X;

            b.X = v.a.Y;
            b.Z = v.c.Y;

            c.X = v.a.Z;
            c.Y = v.b.Z;
        }

        public void MulTranspose(Matrix3 rhs)
        {
            Mul(rhs);
            Transpose();
        }

        public void TransposeMul(Matrix3 rhs)
        {
            Transpose();
            Mul(rhs);
        }

        public float Trace { get { return a.X + b.Y + c.Z; } }

        public bool CalcAngleAxis(out float angle, out Vector3 axis)
        {
            return CalcAngleAxis(Trace, out angle, out axis);
        }

        public bool CalcAngleAxis(float tr, out float angle, out Vector3 axis, float threshold = 1e-16f)
        {
            angle = 0;
            axis = Vector3.Zero;
            if (tr <= -1)
            {
                if (a.X >= b.Y && a.X >= c.Z)
                {
                    float r = 1 + a.X - b.Y - c.Z;
                    if (r <= threshold)
                        return false;
                    r = Mathf.Sqrt(r);
                    axis.X = 0.5f * r;
                    axis.Y = b.X / r;
                    axis.Z = c.X / r;
                }
                else if (b.Y >= c.Z)
                {
                    float r = 1 + b.Y - a.X - c.Z;
                    if (r <= threshold)
                        return false;
                    r = Mathf.Sqrt(r);
                    axis.Y = 0.5f * r;
                    axis.X = b.X / r;
                    axis.Z = c.Y / r;
                }
                else
                {
                    float r = 1 + b.Y - a.X - c.Z;
                    if (r <= threshold) return false;
                    r = Mathf.Sqrt(r);
                    axis.Z = 0.5f * r;
                    axis.X = c.X / r;
                    axis.Y = c.Y / r;
                }
                angle = Mathf.PI;
            }
            else if (tr >= 3)
            {
                axis = new Vector3(0, 0, 1);
                angle = 0;
            }
            else
            {
                axis = new Vector3(b.Z - c.Y, c.X - a.Z, a.Y - b.X);
                float r = axis.LengthSquared();
                if (r <= threshold)
                    return false;
                axis *= (1.0f / Mathf.Sqrt(r));
                angle = Mathf.Acos(0.5f * (tr - 1));
            }
            return true;
        }
    }

    public static class XNAExt
    {
        public static Matrix3 CreateBasis(Vector3 a, Vector3 b, Vector3 c)
        {
            return new Matrix3 { a = a, b = b, c = c };
        }

        public static Vector2 NegXY = new Vector2(-1, -1);
        public static Vector2 PosXY = new Vector2(1, 1);
        public static Vector3 NegXYZ = new Vector3(-1, -1, -1);
        public static Vector3 PosXYZ = new Vector3(1, 1, 1);
        public static Vector3 HalfNegXYZ = new Vector3(-0.5f, -0.5f, -0.5f);
        public static Vector3 HalfPosXYZ = new Vector3(0.5f, 0.5f, 0.5f);

        public static Plane PlaneFromPointNormal(Vector3 pt, Vector3 normal)
        {
            return new Plane(normal, Vector3.Dot(pt, normal));
        }

        public static float Distance(this Vector3 lhs, Vector3 rhs)
        {
            return (rhs - lhs).Length();
        }

        public static float DistanceSquared(this Vector3 lhs, Vector3 rhs)
        {
            return (rhs - lhs).LengthSquared();
        }

        public static Vector2 Normalize(Vector2 val, Vector2 min, Vector2 max)
        {
            return (val - min) / (max - min);
        }

        public static Vector3 Normalize(Vector3 val, Vector3 min, Vector3 max)
        {
            return (val - min) / (max - min);
        }

        public static Vector3 Project(this Plane plane, Vector3 pt)
        {
            return pt - (Vector3.Dot(plane.Normal, pt) - plane.D) * plane.Normal;
        }

        public static Color Lerp(Color start, Color end, float td)
        {
            return start.Mul(1.0f - td).Add(end.Mul(td));
        }

        public static Color Mul(this Color lhs, float val)
        {
            //return (ColorF.FromMonoGame(lhs) * val).ToMonoGame();
            return new Color(
                (byte)(lhs.R * val), 
                (byte)(lhs.G * val), 
                (byte)(lhs.B * val), 
                (byte)(lhs.A * val));
        }

        public static Color Mul(this Color lhs, Color rhs)
        {
            return new Color(lhs.R/255.0f * rhs.R / 255.0f, lhs.G / 255.0f * rhs.G / 255.0f, lhs.B / 255.0f * rhs.B / 255.0f, lhs.A / 255.0f * rhs.A / 255.0f);
        }

        public static Color Add(this Color lhs, Color rhs)
        {
            return new Color(lhs.R + rhs.R, lhs.G + rhs.G, lhs.B + rhs.B, lhs.A + rhs.A);
        }

        public static Color ToXNAColor(this System.Drawing.Color col)
        {
            return new Color(col.R, col.G, col.B, col.A);
        }

        public static System.Drawing.Color ToDrawingColor(this Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static System.Windows.Media.Color ToMediaColor(this Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Plane CreatePlane(Vector3 pos, Vector3 nor)
        {
            Vector3 n = Vector3.Normalize(nor);
            Vector3 absNormal = n.Abs();
            float d = -Vector3.Dot(n, pos);
            return new Plane(n, d);
        }

        public static BoundingBox Extend(this BoundingBox lhs, BoundingBox rhs)
        {
            return new BoundingBox(Vector3.Min(lhs.Min, rhs.Min), Vector3.Max(lhs.Max, rhs.Max));
        }

        public static Vector3 Centroid(this BoundingBox bb)
        {
            return bb.Min + (bb.Max - bb.Min) * 0.5f;
        }

        public static Vector3 Size(this BoundingBox lhs)
        {
            return lhs.Max - lhs.Min;
        }

        static int ToArrayIndex(int x, int y, int z, int width, int height, int depth)
        {
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);
            z = Mathf.Clamp(z, 0, depth - 1);
            return (z * width * height + y * width + x);
        }

        public static Vector3 NormalizedPosition(this BoundingBox bounds, Vector3 vec)
        {
            return (vec - bounds.Min) / bounds.Size();
        }

        public static Vector3 DenormalizedPosition(this BoundingBox bounds, Vector3 vec)
        {
            return vec * bounds.Size() + bounds.Min;
        }

        // Evenly subdivides a bounds
        public static BoundingBox[,,] Subdivide(this BoundingBox lhs, int xDiv, int yDiv, int zDiv)
        {
            Vector3 minPoint = lhs.Min;
            Vector3 cellSize = lhs.Size()  / new Vector3(xDiv, yDiv, zDiv);

            BoundingBox[,,] ret = new BoundingBox[xDiv, yDiv, zDiv];

            for (int x = 0; x < xDiv; ++x)
            {
                for (int y = 0; y < yDiv; ++y)
                {
                    for (int z = 0; z < zDiv; ++z)
                    {
                        //int idx = ToArrayIndex(x, y, 0, xDiv, yDiv, zDiv);
                        ret[x,y,z] = new BoundingBox(minPoint + new Vector3(cellSize.X * x, cellSize.Y * z, cellSize.Z * z), minPoint + cellSize);
                    }
                }
            }

            return ret;
        }

        public static BoundingBox[] Nineslice(this BoundingBox lhs, float edge, int axis)
        {
            BoundingBox[] ret = new BoundingBox[9];
            return ret;
        }

        public static System.Windows.Point ToWindowsPoint(this Vector2 v)
        {
            return new System.Windows.Point(v.X, v.Y);
        }

        public static Vector2 ToVector2(this System.Windows.Point pt)
        {
            return new Vector2((float)pt.X, (float)pt.Y);
        }

        public static System.Windows.Vector ToWindowsVector(this Vector2 v)
        {
            return new System.Windows.Vector(v.X, v.Y);
        }

        public static Vector3 ToNormal(this Color col)
        {
            var vec = col.ToVector4() * 2.0f - Vector4.One;
            return vec.XYZ();
        }

        /// <summary>
        /// Turns a 3d vector into normal color
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Color ToColor(this Vector3 vec)
        {
            var v = vec * 0.5f;
            return new Color(v.X + 0.5f, v.Y + 0.5f, v.Z + 0.5f, 1.0f);
        }

        public static Vector3 ToVec3(this Vector2 v)
        {
            return new Vector3(v.X, v.Y, 0);
        }

        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z));
        }

        public static float MaxElement(this Vector3 v)
        {
            return Math.Max(v.X, Math.Max(v.Y, v.Z));
        }

        public static int MaxElementIndex(this Vector3 v)
        {
            if (v.X > v.Y && v.X > v.Z)
                return 0;
            if (v.Y > v.X && v.Y > v.Z)
                return 1;
            if (v.Z > v.X && v.Z > v.Y)
                return 2;
            return 0;
        }

        public static float MaxElement(this Vector4 v)
        {
            return Math.Max(v.X, Math.Max(v.Y, Math.Max(v.Z, v.W)));
        }

        public static float MinElement(this Vector3 v)
        {
            return Math.Min(v.X, Math.Min(v.Y, v.Z));
        }

        public static Vector3 MaxVector(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(Math.Max(lhs.X, rhs.X), Math.Max(lhs.Y, rhs.Y), Math.Max(lhs.Z, rhs.Z));
        }

        public static bool Intersects(this Ray ray, BoundingBox bounds, Matrix transform)
        {
            Matrix inverseTrans = Matrix.Invert(transform);
            Ray newRay = new Ray(ray.Position + transform.Translation, Vector3.TransformNormal(ray.Direction, inverseTrans));
            return ray.Intersects(bounds).HasValue;
        }

        public static bool Intersects(this Ray ray, Vector3 tri0, Vector3 tri1, Vector3 tri2, ref float pickDistance, ref Vector2 barycentric)
        {
            barycentric = new Vector2();

            // Find vectors for two edges sharing vert0
            Vector3 edge1 = tri1 - tri0;
            Vector3 edge2 = tri2 - tri0;

            // Begin calculating determinant - also used to calculate barycentricU parameter
            Vector3 pvec = Vector3.Cross(ray.Direction, edge2);

            // If determinant is near zero, ray lies in plane of triangle
            float det = Vector3.Dot(edge1, pvec);
            if (det < 0.0001f)
                return false;

            // Calculate distance from vert0 to ray origin
            Vector3 tvec = ray.Position - tri0;

            // Calculate barycentricU parameter and test bounds
            barycentric.X = Vector3.Dot(tvec, pvec);
            if (barycentric.X < 0.0f || barycentric.X > det)
                return false;

            // Prepare to test barycentricV parameter
            Vector3 qvec = Vector3.Cross(tvec, edge1);

            // Calculate barycentricV parameter and test bounds
            barycentric.Y = Vector3.Dot(ray.Direction, qvec);
            if (barycentric.Y < 0.0f || barycentric.X + barycentric.Y > det)
                return false;

            // Calculate pickDistance, scale parameters, ray intersects triangle
            pickDistance = Vector3.Dot(edge2, qvec);
            float fInvDet = 1.0f / det;
            pickDistance *= fInvDet;
            barycentric.X *= fInvDet;
            barycentric.Y *= fInvDet;

            return true;
        }

        public static bool Intersects(this Ray ray, Vector3 tri0, Vector3 tri1, Vector3 tri2)
        {
            Vector2 barycentric = new Vector2();

            // Find vectors for two edges sharing vert0
            Vector3 edge1 = tri1 - tri0;
            Vector3 edge2 = tri2 - tri0;

            // Begin calculating determinant - also used to calculate barycentricU parameter
            Vector3 pvec = Vector3.Cross(ray.Direction, edge2);

            // If determinant is near zero, ray lies in plane of triangle
            float det = Vector3.Dot(edge1, pvec);
            if (det < 0.0001f)
                return false;

            // Calculate distance from vert0 to ray origin
            Vector3 tvec = ray.Position - tri0;

            // Calculate barycentricU parameter and test bounds
            barycentric.X = Vector3.Dot(tvec, pvec);
            if (barycentric.X < 0.0f || barycentric.X > det)
                return false;

            // Prepare to test barycentricV parameter
            Vector3 qvec = Vector3.Cross(tvec, edge1);

            // Calculate barycentricV parameter and test bounds
            barycentric.Y = Vector3.Dot(ray.Direction, qvec);
            if (barycentric.Y < 0.0f || barycentric.X + barycentric.Y > det)
                return false;

            return true;
        }

        public static float RayLineDistance(this Ray r, Vector3 p0, Vector3 p1)
        {
            return DistanceBetweenSegments(r.Position, r.Position + r.Direction * 500, p0, p1);
        }

        public static float DistanceBetweenSegments(Vector3 P0, Vector3 P1, Vector3 S0, Vector3 S1)
        {
            Vector3 u = P1 - P0;
            Vector3 v = S1 - S0;
            Vector3 w = P0 - S0;
            float a = Vector3.Dot(u, u);         // always >= 0
            float b = Vector3.Dot(u, v);
            float c = Vector3.Dot(v, v);         // always >= 0
            float d = Vector3.Dot(u, w);
            float e = Vector3.Dot(v, w);
            float D = a * c - b * b;        // always >= 0
            float sc, sN, sD = D;       // sc = sN / sD, default sD = D >= 0
            float tc, tN, tD = D;       // tc = tN / tD, default tD = D >= 0

            // compute the line parameters of the two closest points
            if (D < 0.00001f)
            { // the lines are almost parallel
                sN = 0.0f;         // force using point P0 on segment S1
                sD = 1.0f;         // to prevent possible division by 0.0 later
                tN = e;
                tD = c;
            }
            else
            {                 // get the closest points on the infinite lines
                sN = (b * e - c * d);
                tN = (a * e - b * d);
                if (sN < 0.0f)
                {        // sc < 0 => the s=0 edge is visible
                    sN = 0.0f;
                    tN = e;
                    tD = c;
                }
                else if (sN > sD)
                {  // sc > 1  => the s=1 edge is visible
                    sN = sD;
                    tN = e + b;
                    tD = c;
                }
            }

            if (tN < 0.0)
            {            // tc < 0 => the t=0 edge is visible
                tN = 0.0f;
                // recompute sc for this edge
                if (-d < 0.0)
                    sN = 0.0f;
                else if (-d > a)
                    sN = sD;
                else
                {
                    sN = -d;
                    sD = a;
                }
            }
            else if (tN > tD)
            {      // tc > 1  => the t=1 edge is visible
                tN = tD;
                // recompute sc for this edge
                if ((-d + b) < 0.0)
                    sN = 0;
                else if ((-d + b) > a)
                    sN = sD;
                else
                {
                    sN = (-d + b);
                    sD = a;
                }
            }
            // finally do the division to get sc and tc
            sc = (Mathf.Abs(sN) < 0.00001f ? 0.0f : sN / sD);
            tc = (Mathf.Abs(tN) < 0.00001f ? 0.0f : tN / tD);

            // get the difference of the two closest points
            Vector3 dP = w + (sc * u) - (tc * v);  // =  S1(sc) - S2(tc)

            return dP.Length();   // return the closest distance
        }

        public static Vector3? IntersectsSegment(this Ray r, Vector3 a, Vector3 b, float tolerance = 0.5f)
        {
            float lengthErrorThreshold = 1e-3f + tolerance;
            float coPlanerThreshold = 0.5f + tolerance;

            Vector3 da = (r.Position + r.Direction * 2000) - r.Position;  // Unnormalized direction of the ray
            Vector3 db = b - a;
            Vector3 dc = a - r.Position;

            if (Math.Abs(Vector3.Dot(dc, Vector3.Cross(da, db))) >= coPlanerThreshold) // Lines are not coplanar
                return null;

            float s = Vector3.Dot(Vector3.Cross(dc, db), Vector3.Cross(da, db)) / Vector3.Cross(da, db).LengthSquared();

            if (s >= 1.0f - tolerance && s <= 1.0f + tolerance)   // Means we have an intersection
            {
                Vector3 intersection = r.Position + r.Direction * s;
                return intersection;
                //// See if this lies on the segment
                //if ((intersection - a).LengthSquared() + (intersection - b).LengthSquared() <= (b-a).LengthSquared() + lengthErrorThreshold)
                //    return true;
            }

            return null;
        }

        public static Vector3 Intersection(this Ray r, Plane p)
        {
            float? hitDist = r.Intersects(p);
            if (hitDist.HasValue)
                return r.Position + r.Direction * hitDist.Value;
            return new Vector3(1000, 1000, 1000);
        }

        /// <summary>
        /// Mirrors the quaternion about the axes opposed to the given symmetric axis.
        /// </summary>
        /// <param name="q">Quaternion to mirror</param>
        /// <param name="axis">Symmetric axis to mirror about</param>
        /// <returns></returns>
        public static Quaternion Mirror(this Quaternion q, Data.SymmetricAxis axis)
        {
            Vector3 euler = q.ToEuler();
            if (axis == Data.SymmetricAxis.XAxis)
            {
                euler.Z = euler.Z * -1;
                euler.Y = euler.Y * -1;
            }
            else if (axis == Data.SymmetricAxis.YAxis)
            {
                euler.Z = euler.Z * -1;
                euler.X = euler.X * -1;
            }
            else
            {
                euler.X = euler.X * -1;
                euler.Y = euler.Y * -1;
            }
            return euler.QuaternionFromEuler();
        }

        public static void Write(this BinaryWriter writer, Vector2 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector3 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector4 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
            writer.Write(vector.W);
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Quaternion quat)
        {
            writer.Write(quat.X);
            writer.Write(quat.Y);
            writer.Write(quat.Z);
            writer.Write(quat.W);
        }

        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Color color)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
            writer.Write(color.A);
        }

        public static Color ReadColor(this BinaryReader reader)
        {
            return new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        public static void Write(this BinaryWriter writer, Matrix matrix)
        {
            for (int i = 0; i < 16; ++i)
                writer.Write(matrix[i]);
        }

        public static Matrix ReadMatrix(this BinaryReader reader)
        {
            return new Matrix(reader.ReadVector4(), reader.ReadVector4(), reader.ReadVector4(), reader.ReadVector4());
        }

        public static string ToTightString(this PluginLib.IntVector2 vec)
        {
            return string.Format("{0} {1}", vec.X, vec.Y);
        }

        public static string ToTightString(this Vector2 vec)
        {
            return string.Format("{0} {1}", vec.X, vec.Y);
        }

        public static string ToTightString(this Vector3 vec)
        {
            return string.Format("{0} {1} {2}", vec.X, vec.Y, vec.Z);
        }

        public static string ToTightString(this Vector4 vec)
        {
            return string.Format("{0} {1} {2} {3}", vec.X, vec.Y, vec.Z, vec.W);
        }

        public static string ToTightString(this Quaternion vec)
        {
            return string.Format("{0} {1} {2} {3}", vec.X, vec.Y, vec.Z, vec.W);
        }

        public static string ToTightString(this Color vec)
        {
            return string.Format("{0} {1} {2} {3}", vec.R, vec.G, vec.B, vec.A);
        }

        public static string ToTightString(this Matrix vec)
        {
            // gross, profiled as the fastest way on MSVC 4.5.2 framework
            return string.Format("{0} {1} {2} {3}",
                string.Format("{0} {1} {2} {3}", vec.M11, vec.M12, vec.M13, vec.M14),
                string.Format("{0} {1} {2} {3}", vec.M21, vec.M22, vec.M23, vec.M24),
                string.Format("{0} {1} {2} {3}", vec.M31, vec.M32, vec.M33, vec.M34),
                string.Format("{0} {1} {2} {3}", vec.M41, vec.M42, vec.M43, vec.M44));
        }

        public static PluginLib.IntVector2 ToIntVector2(this string str)
        {
            if (str == null)
                return new PluginLib.IntVector2 { X = 0, Y = 0 };
            var terms = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (terms == null || terms.Length != 2)
                return new PluginLib.IntVector2 { X = 0, Y = 0 };
            return new PluginLib.IntVector2(int.Parse(terms[0]), int.Parse(terms[1]));
        }

        public static Data.ResponseCurve ToResponseCurve(this string str)
        {
            return Data.ResponseCurve.FromString(str);
        }

        public static void Write(this BinaryWriter strm, Data.ResponseCurve curve)
        {
            strm.Write((int)curve.CurveShape);
            strm.Write(curve.XIntercept);
            strm.Write(curve.YIntercept);
            strm.Write(curve.SlopeIntercept);
            strm.Write(curve.Exponent);
            strm.Write(curve.FlipX);
            strm.Write(curve.FlipY);
        }

        public static Data.ResponseCurve ReadResponseCurve(this BinaryReader rdr)
        {
            Data.ResponseCurve ret = new Data.ResponseCurve();
            ret.CurveShape = (Data.CurveType)rdr.ReadInt32();
            ret.XIntercept = rdr.ReadSingle();
            ret.YIntercept = rdr.ReadSingle();
            ret.SlopeIntercept = rdr.ReadSingle();
            ret.Exponent = rdr.ReadSingle();
            ret.FlipX = rdr.ReadBoolean();
            ret.FlipY = rdr.ReadBoolean();
            return ret;
        }

        public static Vector2 ToVector2(this string str)
        {
            if (str == null) return Vector2.Zero;
            var terms = str.Split(' ');
            if (terms == null || terms.Length != 2)
                return Vector2.Zero;
            return new Vector2(float.Parse(terms[0]), float.Parse(terms[1]));
        }

        public static Vector3 ToVector3(this string str)
        {
            if (str == null) return Vector3.Zero;
            var terms = str.Split(' ');
            if (terms == null || terms.Length != 3)
                return Vector3.Zero;
            return new Vector3(float.Parse(terms[0]), float.Parse(terms[1]), float.Parse(terms[2]));
        }

        public static Vector4 ToVector4(this string str)
        {
            if (str == null) return Vector4.Zero;
            var terms = str.Split(' ');
            if (terms == null || terms.Length != 4)
                return Vector4.Zero;
            return new Vector4(float.Parse(terms[0]), float.Parse(terms[1]), float.Parse(terms[2]), float.Parse(terms[3]));
        }

        public static Quaternion ToQuaternion(this string str)
        {
            if (str == null) return Quaternion.Identity;
            var terms = str.Split(' ');
            if (terms == null || terms.Length != 4)
                return Quaternion.Identity;
            return new Quaternion(float.Parse(terms[0]), float.Parse(terms[1]), float.Parse(terms[2]), float.Parse(terms[3]));
        }

        static char[] MGSplitTerms = new char[] { ':', ' ' };
        public static Color ToColor(this string str)
        {
            if (str == null)
                return Color.White;
            if (str[0] == '{') // Monogame's {R: 128 G: 255 B: 197 A: 255} format
            {
                // remove { } wrapping it
                var termStr = str.Substring(1, str.Length - 2);
                string[] parts = termStr.Split(MGSplitTerms, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 8) // must have R # G # B # A #
                    return new Color(byte.Parse(parts[1]), byte.Parse(parts[3]), byte.Parse(parts[5]), byte.Parse(parts[7]));
                return Color.White;
            }
            var terms = str.Split(' ');
            if (terms == null || terms.Length != 4)
                return Color.White;
            return new Color(byte.Parse(terms[0]), byte.Parse(terms[1]), byte.Parse(terms[2]), byte.Parse(terms[3]));
        }

        public static Matrix ToMatrix(this string str)
        {
            if (str == null) return Matrix.Identity;
            var terms = str.Split(' ');
            if (terms == null || terms.Length != 4)
                return Matrix.Identity;

            return new Matrix(float.Parse(terms[0]), byte.Parse(terms[1]), byte.Parse(terms[2]), byte.Parse(terms[3]),
                float.Parse(terms[4]), byte.Parse(terms[5]), byte.Parse(terms[6]), byte.Parse(terms[7]),
                float.Parse(terms[8]), byte.Parse(terms[9]), byte.Parse(terms[10]), byte.Parse(terms[11]),
                float.Parse(terms[12]), byte.Parse(terms[13]), byte.Parse(terms[14]), byte.Parse(terms[15]));
        }

        public static string ToTightString(this Data.ColorRamp ramp)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}:", ramp.Colors.Count);
            foreach (var key in ramp.Colors)
                sb.AppendFormat("{0},{1}:", key.Key, key.Value.ToTightString());
            return sb.ToString();
        }

        public static Data.ColorRamp ToColorRamp(this string str)
        {
            Data.ColorRamp ret = new Data.ColorRamp();
            ret.Colors.Clear();
            string[] parts = str.Trim().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts == null || parts.Length == 0)
                return ret;
            int knotCt = int.Parse(parts[0]);
            for (int i = 1; i < parts.Length; ++i)
            {
                string[] subParts = parts[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                float t = float.Parse(subParts[0]);
                Color c = subParts[1].ToColor();
                ret.Colors.Add(new KeyValuePair<float, Color>(t, c));
            }
            return ret;
        }

        public static string ToTightString(this Data.ColorCurve curve)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", curve.Knots.Count);
            foreach (var knot in curve.Knots)
                sb.AppendFormat(" {0} {1}", knot.X, knot.Y);
            return sb.ToString();
        }

        public static Data.ColorCurve ToColorCurve(this string str)
        {
            Data.ColorCurve ret = new Data.ColorCurve();
            string[] terms = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (terms == null || terms.Length == 0)
                return ret;

            int ct = int.Parse(terms[0]);
            if (ct >= 2)
            {
                ret.Knots.Clear();
                for (int i = 0; i < ct; ++i)
                    ret.Knots.Add(new Vector2(float.Parse(terms[1 + i * 2]), float.Parse(terms[2 + i * 2])));
            }
            ret.CalculateDerivatives();
            return ret;
        }

        public static string ToTightString(this Data.ColorCurves curves)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}, {1}, {2}, {3}", curves.R.ToTightString(), curves.G.ToTightString(), curves.B.ToTightString(), curves.A.ToTightString());
            return sb.ToString();
        }

        public static Data.ColorCurves ToColorCurves(this string str)
        {
            Data.ColorCurves ret = new Data.ColorCurves();
            string[] parts = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts == null || parts.Length == 0)
                return ret;
            ret.R = parts[0].ToColorCurve();
            ret.G = parts[1].ToColorCurve();
            ret.B = parts[2].ToColorCurve();
            ret.A = parts[3].ToColorCurve();

            return ret;
        }

        public static void Write(this BinaryWriter strm, Data.ColorRamp ramp)
        {
            strm.Write(ramp.Colors.Count);
            for (int i = 0; i < ramp.Colors.Count; ++i)
            {
                strm.Write(ramp.Colors[i].Key);
                strm.Write(ramp.Colors[i].Value);
            }
        }

        public static Data.ColorRamp ReadColorRamp(this BinaryReader strm)
        {
            Data.ColorRamp ret = new Data.ColorRamp();
            ret.Colors.Clear();
            int ct = strm.ReadInt32();
            for (int i = 0; i < ct; ++i)
                ret.Colors.Add(new KeyValuePair<float, Color>(strm.ReadSingle(), strm.ReadColor()));
            return ret;
        }

        public static void Write(this BinaryWriter strm, Data.ColorCurve curve)
        {
            strm.Write(curve.Knots.Count);
            for (int i = 0; i < curve.Knots.Count; ++i)
            {
                strm.Write(curve.Knots[i].X);
                strm.Write(curve.Knots[i].Y);
            }
        }

        public static Data.ColorCurve ReadColorCurve(this BinaryReader strm)
        {
            Data.ColorCurve ret = new Data.ColorCurve();
            ret.Knots.Clear();
            int ct = strm.ReadInt32();
            for (int i = 0; i < ct; ++i)
                ret.Knots.Add(new Vector2(strm.ReadSingle(), strm.ReadSingle()));
            ret.CalculateDerivatives();
            return ret;
        }

        public static void Write(this BinaryWriter strm, Data.ColorCurves curves)
        {
            strm.Write(curves.R);
            strm.Write(curves.G);
            strm.Write(curves.B);
            strm.Write(curves.A);
        }

        public static Data.ColorCurves ReadColorCurves(this BinaryReader strm)
        {
            Data.ColorCurves ret = new Data.ColorCurves();
            ret.R = strm.ReadColorCurve();
            ret.G = strm.ReadColorCurve();
            ret.B = strm.ReadColorCurve();
            ret.A = strm.ReadColorCurve();
            return ret;
        }

        // Source: http://xboxforums.create.msdn.com/forums/t/3051.aspx
        public static Texture2D BitmapToTexture2D(this System.Drawing.Bitmap image, GraphicsDevice GraphicsDevice)
        {
            if (GraphicsDevice == null)
                return null;
            // Buffer size is size of color array multiplied by 4 because   
            // each pixel has four color bytes  
            int bufferSize = image.Height * image.Width * 4;  
         
            // Create new memory stream and save image to stream so   
            // we don't have to save and read file  
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bufferSize);
            image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);  
         
            // Creates a texture from IO.Stream - our memory stream  
            Texture2D texture = Texture2D.FromStream(GraphicsDevice, memoryStream);  
            return texture;  
        }

        public static System.Drawing.Bitmap Texture2DToBitmap(this Texture2D texture)
        {
            byte[] imageData = new byte[4 * texture.Width * texture.Height];
            texture.GetData<byte>(imageData);

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(texture.Width, texture.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Imaging.BitmapData bmData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, texture.Width, texture.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
            IntPtr pnative = bmData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(imageData, 0, pnative, imageData.Length);
            bitmap.UnlockBits(bmData);
            return bitmap;
    }

        public static Vector2 ClosestPoint(this Vector2 P, Vector2 A, Vector2 B)
        {
            Vector2 AP = P - A;       //Vector from A to P   
            Vector2 AB = B - A;       //Vector from A to B  

            float magnitudeAB = AB.LengthSquared();     //Magnitude of AB vector (it's length squared)     
            float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
            float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

            if (distance < 0)     //Check if P projection is over vectorAB     
                return A;
            else if (distance > 1)
                return B;
            else
                return A + AB * distance;
        }
    }

    public class VectorOscillator : PingPong
    {
        Vector3 a_, b_;
        public VectorOscillator(Vector3 a, Vector3 b) : base(0.0f, 1.0f)
        {
            a_ = a;
            b_ = b;
        }

        public Vector3 Get(float time)
        {
            float td = base.Add(time);
            return Vector3.LerpPrecise(a_, b_, td);
        }
    }

    public static class VectorExtensions
    {
        public static Vector2 Rotate(this Vector2 vec, float degrees)
        {
            float cosVal = (float)Math.Cos(MathHelper.ToRadians(degrees));
            float sinVal = (float)Math.Sin(MathHelper.ToRadians(degrees));
            float newX = vec.X * cosVal - vec.Y * sinVal;
            float newY = vec.X * sinVal + vec.Y * cosVal;
            return new Vector2(newX, newY);
        }

        // Vector2 Swizzles
        public static Vector2 XX(this Vector2 v) { return new Vector2(v.X, v.X); }
        public static Vector2 XY(this Vector2 v) { return new Vector2(v.X, v.Y); }
        public static Vector2 YX(this Vector2 v) { return new Vector2(v.Y, v.X); }
        public static Vector2 YY(this Vector2 v) { return new Vector2(v.Y, v.Y); }
        public static Vector3 XXX(this Vector2 v) { return new Vector3(v.X, v.X, v.X); }
        public static Vector3 XXY(this Vector2 v) { return new Vector3(v.X, v.X, v.Y); }
        public static Vector3 XYX(this Vector2 v) { return new Vector3(v.X, v.Y, v.X); }
        public static Vector3 XYY(this Vector2 v) { return new Vector3(v.X, v.Y, v.Y); }
        public static Vector3 YXX(this Vector2 v) { return new Vector3(v.Y, v.X, v.X); }
        public static Vector3 YXY(this Vector2 v) { return new Vector3(v.Y, v.X, v.Y); }
        public static Vector3 YYX(this Vector2 v) { return new Vector3(v.Y, v.Y, v.X); }
        public static Vector3 YYY(this Vector2 v) { return new Vector3(v.Y, v.Y, v.Y); }
        // Vector4 swizzles
        public static Vector4 XXXX(this Vector2 v) { return new Vector4(v.X, v.X, v.X, v.X); }
        public static Vector4 XXXY(this Vector2 v) { return new Vector4(v.X, v.X, v.X, v.Y); }
        public static Vector4 XXYX(this Vector2 v) { return new Vector4(v.X, v.X, v.Y, v.X); }
        public static Vector4 XXYY(this Vector2 v) { return new Vector4(v.X, v.X, v.Y, v.Y); }
        public static Vector4 XYXX(this Vector2 v) { return new Vector4(v.X, v.Y, v.X, v.X); }
        public static Vector4 XYXY(this Vector2 v) { return new Vector4(v.X, v.Y, v.X, v.Y); }
        public static Vector4 XYYX(this Vector2 v) { return new Vector4(v.X, v.Y, v.Y, v.X); }
        public static Vector4 XYYY(this Vector2 v) { return new Vector4(v.X, v.Y, v.Y, v.Y); }
        public static Vector4 YXXX(this Vector2 v) { return new Vector4(v.Y, v.X, v.X, v.X); }
        public static Vector4 YXXY(this Vector2 v) { return new Vector4(v.Y, v.X, v.X, v.Y); }
        public static Vector4 YXYX(this Vector2 v) { return new Vector4(v.Y, v.X, v.Y, v.X); }
        public static Vector4 YXYY(this Vector2 v) { return new Vector4(v.Y, v.X, v.Y, v.Y); }
        public static Vector4 YYXX(this Vector2 v) { return new Vector4(v.Y, v.Y, v.X, v.X); }
        public static Vector4 YYXY(this Vector2 v) { return new Vector4(v.Y, v.Y, v.X, v.Y); }
        public static Vector4 YYYX(this Vector2 v) { return new Vector4(v.Y, v.Y, v.Y, v.X); }
        public static Vector4 YYYY(this Vector2 v) { return new Vector4(v.Y, v.Y, v.Y, v.Y); }
        // Vector3 Swizzles
        public static Vector2 XX(this Vector3 v) { return new Vector2(v.X, v.X); }
        public static Vector2 XY(this Vector3 v) { return new Vector2(v.X, v.Y); }
        public static Vector2 XZ(this Vector3 v) { return new Vector2(v.X, v.Z); }
        public static Vector2 YX(this Vector3 v) { return new Vector2(v.Y, v.X); }
        public static Vector2 YY(this Vector3 v) { return new Vector2(v.Y, v.Y); }
        public static Vector2 YZ(this Vector3 v) { return new Vector2(v.Y, v.Z); }
        public static Vector2 ZX(this Vector3 v) { return new Vector2(v.Z, v.X); }
        public static Vector2 ZY(this Vector3 v) { return new Vector2(v.Z, v.Y); }
        public static Vector2 ZZ(this Vector3 v) { return new Vector2(v.Z, v.Z); }
        public static Vector3 XXX(this Vector3 v) { return new Vector3(v.X, v.X, v.X); }
        public static Vector3 XXY(this Vector3 v) { return new Vector3(v.X, v.X, v.Y); }
        public static Vector3 XXZ(this Vector3 v) { return new Vector3(v.X, v.X, v.Z); }
        public static Vector3 XYX(this Vector3 v) { return new Vector3(v.X, v.Y, v.X); }
        public static Vector3 XYY(this Vector3 v) { return new Vector3(v.X, v.Y, v.Y); }
        public static Vector3 XYZ(this Vector3 v) { return new Vector3(v.X, v.Y, v.Z); }
        public static Vector3 XZX(this Vector3 v) { return new Vector3(v.X, v.Z, v.X); }
        public static Vector3 XZY(this Vector3 v) { return new Vector3(v.X, v.Z, v.Y); }
        public static Vector3 XZZ(this Vector3 v) { return new Vector3(v.X, v.Z, v.Z); }
        public static Vector3 YXX(this Vector3 v) { return new Vector3(v.Y, v.X, v.X); }
        public static Vector3 YXY(this Vector3 v) { return new Vector3(v.Y, v.X, v.Y); }
        public static Vector3 YXZ(this Vector3 v) { return new Vector3(v.Y, v.X, v.Z); }
        public static Vector3 YYX(this Vector3 v) { return new Vector3(v.Y, v.Y, v.X); }
        public static Vector3 YYY(this Vector3 v) { return new Vector3(v.Y, v.Y, v.Y); }
        public static Vector3 YYZ(this Vector3 v) { return new Vector3(v.Y, v.Y, v.Z); }
        public static Vector3 YZX(this Vector3 v) { return new Vector3(v.Y, v.Z, v.X); }
        public static Vector3 YZY(this Vector3 v) { return new Vector3(v.Y, v.Z, v.Y); }
        public static Vector3 YZZ(this Vector3 v) { return new Vector3(v.Y, v.Z, v.Z); }
        public static Vector3 ZXX(this Vector3 v) { return new Vector3(v.Z, v.X, v.X); }
        public static Vector3 ZXY(this Vector3 v) { return new Vector3(v.Z, v.X, v.Y); }
        public static Vector3 ZXZ(this Vector3 v) { return new Vector3(v.Z, v.X, v.Z); }
        public static Vector3 ZYX(this Vector3 v) { return new Vector3(v.Z, v.Y, v.X); }
        public static Vector3 ZYY(this Vector3 v) { return new Vector3(v.Z, v.Y, v.Y); }
        public static Vector3 ZYZ(this Vector3 v) { return new Vector3(v.Z, v.Y, v.Z); }
        public static Vector3 ZZX(this Vector3 v) { return new Vector3(v.Z, v.Z, v.X); }
        public static Vector3 ZZY(this Vector3 v) { return new Vector3(v.Z, v.Z, v.Y); }
        public static Vector3 ZZZ(this Vector3 v) { return new Vector3(v.Z, v.Z, v.Z); }
        // Vector4 swizzles
        public static Vector4 XXXX(this Vector3 v) { return new Vector4(v.X, v.X, v.X, v.X); }
        public static Vector4 XXXY(this Vector3 v) { return new Vector4(v.X, v.X, v.X, v.Y); }
        public static Vector4 XXXZ(this Vector3 v) { return new Vector4(v.X, v.X, v.X, v.Z); }
        public static Vector4 XXYX(this Vector3 v) { return new Vector4(v.X, v.X, v.Y, v.X); }
        public static Vector4 XXYY(this Vector3 v) { return new Vector4(v.X, v.X, v.Y, v.Y); }
        public static Vector4 XXYZ(this Vector3 v) { return new Vector4(v.X, v.X, v.Y, v.Z); }
        public static Vector4 XXZX(this Vector3 v) { return new Vector4(v.X, v.X, v.Z, v.X); }
        public static Vector4 XXZY(this Vector3 v) { return new Vector4(v.X, v.X, v.Z, v.Y); }
        public static Vector4 XXZZ(this Vector3 v) { return new Vector4(v.X, v.X, v.Z, v.Z); }
        public static Vector4 XYXX(this Vector3 v) { return new Vector4(v.X, v.Y, v.X, v.X); }
        public static Vector4 XYXY(this Vector3 v) { return new Vector4(v.X, v.Y, v.X, v.Y); }
        public static Vector4 XYXZ(this Vector3 v) { return new Vector4(v.X, v.Y, v.X, v.Z); }
        public static Vector4 XYYX(this Vector3 v) { return new Vector4(v.X, v.Y, v.Y, v.X); }
        public static Vector4 XYYY(this Vector3 v) { return new Vector4(v.X, v.Y, v.Y, v.Y); }
        public static Vector4 XYYZ(this Vector3 v) { return new Vector4(v.X, v.Y, v.Y, v.Z); }
        public static Vector4 XYZX(this Vector3 v) { return new Vector4(v.X, v.Y, v.Z, v.X); }
        public static Vector4 XYZY(this Vector3 v) { return new Vector4(v.X, v.Y, v.Z, v.Y); }
        public static Vector4 XYZZ(this Vector3 v) { return new Vector4(v.X, v.Y, v.Z, v.Z); }
        public static Vector4 XZXX(this Vector3 v) { return new Vector4(v.X, v.Z, v.X, v.X); }
        public static Vector4 XZXY(this Vector3 v) { return new Vector4(v.X, v.Z, v.X, v.Y); }
        public static Vector4 XZXZ(this Vector3 v) { return new Vector4(v.X, v.Z, v.X, v.Z); }
        public static Vector4 XZYX(this Vector3 v) { return new Vector4(v.X, v.Z, v.Y, v.X); }
        public static Vector4 XZYY(this Vector3 v) { return new Vector4(v.X, v.Z, v.Y, v.Y); }
        public static Vector4 XZYZ(this Vector3 v) { return new Vector4(v.X, v.Z, v.Y, v.Z); }
        public static Vector4 XZZX(this Vector3 v) { return new Vector4(v.X, v.Z, v.Z, v.X); }
        public static Vector4 XZZY(this Vector3 v) { return new Vector4(v.X, v.Z, v.Z, v.Y); }
        public static Vector4 XZZZ(this Vector3 v) { return new Vector4(v.X, v.Z, v.Z, v.Z); }
        public static Vector4 YXXX(this Vector3 v) { return new Vector4(v.Y, v.X, v.X, v.X); }
        public static Vector4 YXXY(this Vector3 v) { return new Vector4(v.Y, v.X, v.X, v.Y); }
        public static Vector4 YXXZ(this Vector3 v) { return new Vector4(v.Y, v.X, v.X, v.Z); }
        public static Vector4 YXYX(this Vector3 v) { return new Vector4(v.Y, v.X, v.Y, v.X); }
        public static Vector4 YXYY(this Vector3 v) { return new Vector4(v.Y, v.X, v.Y, v.Y); }
        public static Vector4 YXYZ(this Vector3 v) { return new Vector4(v.Y, v.X, v.Y, v.Z); }
        public static Vector4 YXZX(this Vector3 v) { return new Vector4(v.Y, v.X, v.Z, v.X); }
        public static Vector4 YXZY(this Vector3 v) { return new Vector4(v.Y, v.X, v.Z, v.Y); }
        public static Vector4 YXZZ(this Vector3 v) { return new Vector4(v.Y, v.X, v.Z, v.Z); }
        public static Vector4 YYXX(this Vector3 v) { return new Vector4(v.Y, v.Y, v.X, v.X); }
        public static Vector4 YYXY(this Vector3 v) { return new Vector4(v.Y, v.Y, v.X, v.Y); }
        public static Vector4 YYXZ(this Vector3 v) { return new Vector4(v.Y, v.Y, v.X, v.Z); }
        public static Vector4 YYYX(this Vector3 v) { return new Vector4(v.Y, v.Y, v.Y, v.X); }
        public static Vector4 YYYY(this Vector3 v) { return new Vector4(v.Y, v.Y, v.Y, v.Y); }
        public static Vector4 YYYZ(this Vector3 v) { return new Vector4(v.Y, v.Y, v.Y, v.Z); }
        public static Vector4 YYZX(this Vector3 v) { return new Vector4(v.Y, v.Y, v.Z, v.X); }
        public static Vector4 YYZY(this Vector3 v) { return new Vector4(v.Y, v.Y, v.Z, v.Y); }
        public static Vector4 YYZZ(this Vector3 v) { return new Vector4(v.Y, v.Y, v.Z, v.Z); }
        public static Vector4 YZXX(this Vector3 v) { return new Vector4(v.Y, v.Z, v.X, v.X); }
        public static Vector4 YZXY(this Vector3 v) { return new Vector4(v.Y, v.Z, v.X, v.Y); }
        public static Vector4 YZXZ(this Vector3 v) { return new Vector4(v.Y, v.Z, v.X, v.Z); }
        public static Vector4 YZYX(this Vector3 v) { return new Vector4(v.Y, v.Z, v.Y, v.X); }
        public static Vector4 YZYY(this Vector3 v) { return new Vector4(v.Y, v.Z, v.Y, v.Y); }
        public static Vector4 YZYZ(this Vector3 v) { return new Vector4(v.Y, v.Z, v.Y, v.Z); }
        public static Vector4 YZZX(this Vector3 v) { return new Vector4(v.Y, v.Z, v.Z, v.X); }
        public static Vector4 YZZY(this Vector3 v) { return new Vector4(v.Y, v.Z, v.Z, v.Y); }
        public static Vector4 YZZZ(this Vector3 v) { return new Vector4(v.Y, v.Z, v.Z, v.Z); }
        public static Vector4 ZXXX(this Vector3 v) { return new Vector4(v.Z, v.X, v.X, v.X); }
        public static Vector4 ZXXY(this Vector3 v) { return new Vector4(v.Z, v.X, v.X, v.Y); }
        public static Vector4 ZXXZ(this Vector3 v) { return new Vector4(v.Z, v.X, v.X, v.Z); }
        public static Vector4 ZXYX(this Vector3 v) { return new Vector4(v.Z, v.X, v.Y, v.X); }
        public static Vector4 ZXYY(this Vector3 v) { return new Vector4(v.Z, v.X, v.Y, v.Y); }
        public static Vector4 ZXYZ(this Vector3 v) { return new Vector4(v.Z, v.X, v.Y, v.Z); }
        public static Vector4 ZXZX(this Vector3 v) { return new Vector4(v.Z, v.X, v.Z, v.X); }
        public static Vector4 ZXZY(this Vector3 v) { return new Vector4(v.Z, v.X, v.Z, v.Y); }
        public static Vector4 ZXZZ(this Vector3 v) { return new Vector4(v.Z, v.X, v.Z, v.Z); }
        public static Vector4 ZYXX(this Vector3 v) { return new Vector4(v.Z, v.Y, v.X, v.X); }
        public static Vector4 ZYXY(this Vector3 v) { return new Vector4(v.Z, v.Y, v.X, v.Y); }
        public static Vector4 ZYXZ(this Vector3 v) { return new Vector4(v.Z, v.Y, v.X, v.Z); }
        public static Vector4 ZYYX(this Vector3 v) { return new Vector4(v.Z, v.Y, v.Y, v.X); }
        public static Vector4 ZYYY(this Vector3 v) { return new Vector4(v.Z, v.Y, v.Y, v.Y); }
        public static Vector4 ZYYZ(this Vector3 v) { return new Vector4(v.Z, v.Y, v.Y, v.Z); }
        public static Vector4 ZYZX(this Vector3 v) { return new Vector4(v.Z, v.Y, v.Z, v.X); }
        public static Vector4 ZYZY(this Vector3 v) { return new Vector4(v.Z, v.Y, v.Z, v.Y); }
        public static Vector4 ZYZZ(this Vector3 v) { return new Vector4(v.Z, v.Y, v.Z, v.Z); }
        public static Vector4 ZZXX(this Vector3 v) { return new Vector4(v.Z, v.Z, v.X, v.X); }
        public static Vector4 ZZXY(this Vector3 v) { return new Vector4(v.Z, v.Z, v.X, v.Y); }
        public static Vector4 ZZXZ(this Vector3 v) { return new Vector4(v.Z, v.Z, v.X, v.Z); }
        public static Vector4 ZZYX(this Vector3 v) { return new Vector4(v.Z, v.Z, v.Y, v.X); }
        public static Vector4 ZZYY(this Vector3 v) { return new Vector4(v.Z, v.Z, v.Y, v.Y); }
        public static Vector4 ZZYZ(this Vector3 v) { return new Vector4(v.Z, v.Z, v.Y, v.Z); }
        public static Vector4 ZZZX(this Vector3 v) { return new Vector4(v.Z, v.Z, v.Z, v.X); }
        public static Vector4 ZZZY(this Vector3 v) { return new Vector4(v.Z, v.Z, v.Z, v.Y); }
        public static Vector4 ZZZZ(this Vector3 v) { return new Vector4(v.Z, v.Z, v.Z, v.Z); }
        // Vector4 Swizzles
        public static Vector2 XX(this Vector4 v) { return new Vector2(v.X, v.X); }
        public static Vector2 XY(this Vector4 v) { return new Vector2(v.X, v.Y); }
        public static Vector2 XZ(this Vector4 v) { return new Vector2(v.X, v.Z); }
        public static Vector2 XW(this Vector4 v) { return new Vector2(v.X, v.W); }
        public static Vector2 YX(this Vector4 v) { return new Vector2(v.Y, v.X); }
        public static Vector2 YY(this Vector4 v) { return new Vector2(v.Y, v.Y); }
        public static Vector2 YZ(this Vector4 v) { return new Vector2(v.Y, v.Z); }
        public static Vector2 YW(this Vector4 v) { return new Vector2(v.Y, v.W); }
        public static Vector2 ZX(this Vector4 v) { return new Vector2(v.Z, v.X); }
        public static Vector2 ZY(this Vector4 v) { return new Vector2(v.Z, v.Y); }
        public static Vector2 ZZ(this Vector4 v) { return new Vector2(v.Z, v.Z); }
        public static Vector2 ZW(this Vector4 v) { return new Vector2(v.Z, v.W); }
        public static Vector2 WX(this Vector4 v) { return new Vector2(v.W, v.X); }
        public static Vector2 WY(this Vector4 v) { return new Vector2(v.W, v.Y); }
        public static Vector2 WZ(this Vector4 v) { return new Vector2(v.W, v.Z); }
        public static Vector2 WW(this Vector4 v) { return new Vector2(v.W, v.W); }
        public static Vector3 XXX(this Vector4 v) { return new Vector3(v.X, v.X, v.X); }
        public static Vector3 XXY(this Vector4 v) { return new Vector3(v.X, v.X, v.Y); }
        public static Vector3 XXZ(this Vector4 v) { return new Vector3(v.X, v.X, v.Z); }
        public static Vector3 XXW(this Vector4 v) { return new Vector3(v.X, v.X, v.W); }
        public static Vector3 XYX(this Vector4 v) { return new Vector3(v.X, v.Y, v.X); }
        public static Vector3 XYY(this Vector4 v) { return new Vector3(v.X, v.Y, v.Y); }
        public static Vector3 XYZ(this Vector4 v) { return new Vector3(v.X, v.Y, v.Z); }
        public static Vector3 XYW(this Vector4 v) { return new Vector3(v.X, v.Y, v.W); }
        public static Vector3 XZX(this Vector4 v) { return new Vector3(v.X, v.Z, v.X); }
        public static Vector3 XZY(this Vector4 v) { return new Vector3(v.X, v.Z, v.Y); }
        public static Vector3 XZZ(this Vector4 v) { return new Vector3(v.X, v.Z, v.Z); }
        public static Vector3 XZW(this Vector4 v) { return new Vector3(v.X, v.Z, v.W); }
        public static Vector3 XWX(this Vector4 v) { return new Vector3(v.X, v.W, v.X); }
        public static Vector3 XWY(this Vector4 v) { return new Vector3(v.X, v.W, v.Y); }
        public static Vector3 XWZ(this Vector4 v) { return new Vector3(v.X, v.W, v.Z); }
        public static Vector3 XWW(this Vector4 v) { return new Vector3(v.X, v.W, v.W); }
        public static Vector3 YXX(this Vector4 v) { return new Vector3(v.Y, v.X, v.X); }
        public static Vector3 YXY(this Vector4 v) { return new Vector3(v.Y, v.X, v.Y); }
        public static Vector3 YXZ(this Vector4 v) { return new Vector3(v.Y, v.X, v.Z); }
        public static Vector3 YXW(this Vector4 v) { return new Vector3(v.Y, v.X, v.W); }
        public static Vector3 YYX(this Vector4 v) { return new Vector3(v.Y, v.Y, v.X); }
        public static Vector3 YYY(this Vector4 v) { return new Vector3(v.Y, v.Y, v.Y); }
        public static Vector3 YYZ(this Vector4 v) { return new Vector3(v.Y, v.Y, v.Z); }
        public static Vector3 YYW(this Vector4 v) { return new Vector3(v.Y, v.Y, v.W); }
        public static Vector3 YZX(this Vector4 v) { return new Vector3(v.Y, v.Z, v.X); }
        public static Vector3 YZY(this Vector4 v) { return new Vector3(v.Y, v.Z, v.Y); }
        public static Vector3 YZZ(this Vector4 v) { return new Vector3(v.Y, v.Z, v.Z); }
        public static Vector3 YZW(this Vector4 v) { return new Vector3(v.Y, v.Z, v.W); }
        public static Vector3 YWX(this Vector4 v) { return new Vector3(v.Y, v.W, v.X); }
        public static Vector3 YWY(this Vector4 v) { return new Vector3(v.Y, v.W, v.Y); }
        public static Vector3 YWZ(this Vector4 v) { return new Vector3(v.Y, v.W, v.Z); }
        public static Vector3 YWW(this Vector4 v) { return new Vector3(v.Y, v.W, v.W); }
        public static Vector3 ZXX(this Vector4 v) { return new Vector3(v.Z, v.X, v.X); }
        public static Vector3 ZXY(this Vector4 v) { return new Vector3(v.Z, v.X, v.Y); }
        public static Vector3 ZXZ(this Vector4 v) { return new Vector3(v.Z, v.X, v.Z); }
        public static Vector3 ZXW(this Vector4 v) { return new Vector3(v.Z, v.X, v.W); }
        public static Vector3 ZYX(this Vector4 v) { return new Vector3(v.Z, v.Y, v.X); }
        public static Vector3 ZYY(this Vector4 v) { return new Vector3(v.Z, v.Y, v.Y); }
        public static Vector3 ZYZ(this Vector4 v) { return new Vector3(v.Z, v.Y, v.Z); }
        public static Vector3 ZYW(this Vector4 v) { return new Vector3(v.Z, v.Y, v.W); }
        public static Vector3 ZZX(this Vector4 v) { return new Vector3(v.Z, v.Z, v.X); }
        public static Vector3 ZZY(this Vector4 v) { return new Vector3(v.Z, v.Z, v.Y); }
        public static Vector3 ZZZ(this Vector4 v) { return new Vector3(v.Z, v.Z, v.Z); }
        public static Vector3 ZZW(this Vector4 v) { return new Vector3(v.Z, v.Z, v.W); }
        public static Vector3 ZWX(this Vector4 v) { return new Vector3(v.Z, v.W, v.X); }
        public static Vector3 ZWY(this Vector4 v) { return new Vector3(v.Z, v.W, v.Y); }
        public static Vector3 ZWZ(this Vector4 v) { return new Vector3(v.Z, v.W, v.Z); }
        public static Vector3 ZWW(this Vector4 v) { return new Vector3(v.Z, v.W, v.W); }
        public static Vector3 WXX(this Vector4 v) { return new Vector3(v.W, v.X, v.X); }
        public static Vector3 WXY(this Vector4 v) { return new Vector3(v.W, v.X, v.Y); }
        public static Vector3 WXZ(this Vector4 v) { return new Vector3(v.W, v.X, v.Z); }
        public static Vector3 WXW(this Vector4 v) { return new Vector3(v.W, v.X, v.W); }
        public static Vector3 WYX(this Vector4 v) { return new Vector3(v.W, v.Y, v.X); }
        public static Vector3 WYY(this Vector4 v) { return new Vector3(v.W, v.Y, v.Y); }
        public static Vector3 WYZ(this Vector4 v) { return new Vector3(v.W, v.Y, v.Z); }
        public static Vector3 WYW(this Vector4 v) { return new Vector3(v.W, v.Y, v.W); }
        public static Vector3 WZX(this Vector4 v) { return new Vector3(v.W, v.Z, v.X); }
        public static Vector3 WZY(this Vector4 v) { return new Vector3(v.W, v.Z, v.Y); }
        public static Vector3 WZZ(this Vector4 v) { return new Vector3(v.W, v.Z, v.Z); }
        public static Vector3 WZW(this Vector4 v) { return new Vector3(v.W, v.Z, v.W); }
        public static Vector3 WWX(this Vector4 v) { return new Vector3(v.W, v.W, v.X); }
        public static Vector3 WWY(this Vector4 v) { return new Vector3(v.W, v.W, v.Y); }
        public static Vector3 WWZ(this Vector4 v) { return new Vector3(v.W, v.W, v.Z); }
        public static Vector3 WWW(this Vector4 v) { return new Vector3(v.W, v.W, v.W); }
        // Vector4 swizzles
        public static Vector4 XXXX(this Vector4 v) { return new Vector4(v.X, v.X, v.X, v.X); }
        public static Vector4 XXXY(this Vector4 v) { return new Vector4(v.X, v.X, v.X, v.Y); }
        public static Vector4 XXXZ(this Vector4 v) { return new Vector4(v.X, v.X, v.X, v.Z); }
        public static Vector4 XXXW(this Vector4 v) { return new Vector4(v.X, v.X, v.X, v.W); }
        public static Vector4 XXYX(this Vector4 v) { return new Vector4(v.X, v.X, v.Y, v.X); }
        public static Vector4 XXYY(this Vector4 v) { return new Vector4(v.X, v.X, v.Y, v.Y); }
        public static Vector4 XXYZ(this Vector4 v) { return new Vector4(v.X, v.X, v.Y, v.Z); }
        public static Vector4 XXYW(this Vector4 v) { return new Vector4(v.X, v.X, v.Y, v.W); }
        public static Vector4 XXZX(this Vector4 v) { return new Vector4(v.X, v.X, v.Z, v.X); }
        public static Vector4 XXZY(this Vector4 v) { return new Vector4(v.X, v.X, v.Z, v.Y); }
        public static Vector4 XXZZ(this Vector4 v) { return new Vector4(v.X, v.X, v.Z, v.Z); }
        public static Vector4 XXZW(this Vector4 v) { return new Vector4(v.X, v.X, v.Z, v.W); }
        public static Vector4 XXWX(this Vector4 v) { return new Vector4(v.X, v.X, v.W, v.X); }
        public static Vector4 XXWY(this Vector4 v) { return new Vector4(v.X, v.X, v.W, v.Y); }
        public static Vector4 XXWZ(this Vector4 v) { return new Vector4(v.X, v.X, v.W, v.Z); }
        public static Vector4 XXWW(this Vector4 v) { return new Vector4(v.X, v.X, v.W, v.W); }
        public static Vector4 XYXX(this Vector4 v) { return new Vector4(v.X, v.Y, v.X, v.X); }
        public static Vector4 XYXY(this Vector4 v) { return new Vector4(v.X, v.Y, v.X, v.Y); }
        public static Vector4 XYXZ(this Vector4 v) { return new Vector4(v.X, v.Y, v.X, v.Z); }
        public static Vector4 XYXW(this Vector4 v) { return new Vector4(v.X, v.Y, v.X, v.W); }
        public static Vector4 XYYX(this Vector4 v) { return new Vector4(v.X, v.Y, v.Y, v.X); }
        public static Vector4 XYYY(this Vector4 v) { return new Vector4(v.X, v.Y, v.Y, v.Y); }
        public static Vector4 XYYZ(this Vector4 v) { return new Vector4(v.X, v.Y, v.Y, v.Z); }
        public static Vector4 XYYW(this Vector4 v) { return new Vector4(v.X, v.Y, v.Y, v.W); }
        public static Vector4 XYZX(this Vector4 v) { return new Vector4(v.X, v.Y, v.Z, v.X); }
        public static Vector4 XYZY(this Vector4 v) { return new Vector4(v.X, v.Y, v.Z, v.Y); }
        public static Vector4 XYZZ(this Vector4 v) { return new Vector4(v.X, v.Y, v.Z, v.Z); }
        public static Vector4 XYZW(this Vector4 v) { return new Vector4(v.X, v.Y, v.Z, v.W); }
        public static Vector4 XYWX(this Vector4 v) { return new Vector4(v.X, v.Y, v.W, v.X); }
        public static Vector4 XYWY(this Vector4 v) { return new Vector4(v.X, v.Y, v.W, v.Y); }
        public static Vector4 XYWZ(this Vector4 v) { return new Vector4(v.X, v.Y, v.W, v.Z); }
        public static Vector4 XYWW(this Vector4 v) { return new Vector4(v.X, v.Y, v.W, v.W); }
        public static Vector4 XZXX(this Vector4 v) { return new Vector4(v.X, v.Z, v.X, v.X); }
        public static Vector4 XZXY(this Vector4 v) { return new Vector4(v.X, v.Z, v.X, v.Y); }
        public static Vector4 XZXZ(this Vector4 v) { return new Vector4(v.X, v.Z, v.X, v.Z); }
        public static Vector4 XZXW(this Vector4 v) { return new Vector4(v.X, v.Z, v.X, v.W); }
        public static Vector4 XZYX(this Vector4 v) { return new Vector4(v.X, v.Z, v.Y, v.X); }
        public static Vector4 XZYY(this Vector4 v) { return new Vector4(v.X, v.Z, v.Y, v.Y); }
        public static Vector4 XZYZ(this Vector4 v) { return new Vector4(v.X, v.Z, v.Y, v.Z); }
        public static Vector4 XZYW(this Vector4 v) { return new Vector4(v.X, v.Z, v.Y, v.W); }
        public static Vector4 XZZX(this Vector4 v) { return new Vector4(v.X, v.Z, v.Z, v.X); }
        public static Vector4 XZZY(this Vector4 v) { return new Vector4(v.X, v.Z, v.Z, v.Y); }
        public static Vector4 XZZZ(this Vector4 v) { return new Vector4(v.X, v.Z, v.Z, v.Z); }
        public static Vector4 XZZW(this Vector4 v) { return new Vector4(v.X, v.Z, v.Z, v.W); }
        public static Vector4 XZWX(this Vector4 v) { return new Vector4(v.X, v.Z, v.W, v.X); }
        public static Vector4 XZWY(this Vector4 v) { return new Vector4(v.X, v.Z, v.W, v.Y); }
        public static Vector4 XZWZ(this Vector4 v) { return new Vector4(v.X, v.Z, v.W, v.Z); }
        public static Vector4 XZWW(this Vector4 v) { return new Vector4(v.X, v.Z, v.W, v.W); }
        public static Vector4 XWXX(this Vector4 v) { return new Vector4(v.X, v.W, v.X, v.X); }
        public static Vector4 XWXY(this Vector4 v) { return new Vector4(v.X, v.W, v.X, v.Y); }
        public static Vector4 XWXZ(this Vector4 v) { return new Vector4(v.X, v.W, v.X, v.Z); }
        public static Vector4 XWXW(this Vector4 v) { return new Vector4(v.X, v.W, v.X, v.W); }
        public static Vector4 XWYX(this Vector4 v) { return new Vector4(v.X, v.W, v.Y, v.X); }
        public static Vector4 XWYY(this Vector4 v) { return new Vector4(v.X, v.W, v.Y, v.Y); }
        public static Vector4 XWYZ(this Vector4 v) { return new Vector4(v.X, v.W, v.Y, v.Z); }
        public static Vector4 XWYW(this Vector4 v) { return new Vector4(v.X, v.W, v.Y, v.W); }
        public static Vector4 XWZX(this Vector4 v) { return new Vector4(v.X, v.W, v.Z, v.X); }
        public static Vector4 XWZY(this Vector4 v) { return new Vector4(v.X, v.W, v.Z, v.Y); }
        public static Vector4 XWZZ(this Vector4 v) { return new Vector4(v.X, v.W, v.Z, v.Z); }
        public static Vector4 XWZW(this Vector4 v) { return new Vector4(v.X, v.W, v.Z, v.W); }
        public static Vector4 XWWX(this Vector4 v) { return new Vector4(v.X, v.W, v.W, v.X); }
        public static Vector4 XWWY(this Vector4 v) { return new Vector4(v.X, v.W, v.W, v.Y); }
        public static Vector4 XWWZ(this Vector4 v) { return new Vector4(v.X, v.W, v.W, v.Z); }
        public static Vector4 XWWW(this Vector4 v) { return new Vector4(v.X, v.W, v.W, v.W); }
        public static Vector4 YXXX(this Vector4 v) { return new Vector4(v.Y, v.X, v.X, v.X); }
        public static Vector4 YXXY(this Vector4 v) { return new Vector4(v.Y, v.X, v.X, v.Y); }
        public static Vector4 YXXZ(this Vector4 v) { return new Vector4(v.Y, v.X, v.X, v.Z); }
        public static Vector4 YXXW(this Vector4 v) { return new Vector4(v.Y, v.X, v.X, v.W); }
        public static Vector4 YXYX(this Vector4 v) { return new Vector4(v.Y, v.X, v.Y, v.X); }
        public static Vector4 YXYY(this Vector4 v) { return new Vector4(v.Y, v.X, v.Y, v.Y); }
        public static Vector4 YXYZ(this Vector4 v) { return new Vector4(v.Y, v.X, v.Y, v.Z); }
        public static Vector4 YXYW(this Vector4 v) { return new Vector4(v.Y, v.X, v.Y, v.W); }
        public static Vector4 YXZX(this Vector4 v) { return new Vector4(v.Y, v.X, v.Z, v.X); }
        public static Vector4 YXZY(this Vector4 v) { return new Vector4(v.Y, v.X, v.Z, v.Y); }
        public static Vector4 YXZZ(this Vector4 v) { return new Vector4(v.Y, v.X, v.Z, v.Z); }
        public static Vector4 YXZW(this Vector4 v) { return new Vector4(v.Y, v.X, v.Z, v.W); }
        public static Vector4 YXWX(this Vector4 v) { return new Vector4(v.Y, v.X, v.W, v.X); }
        public static Vector4 YXWY(this Vector4 v) { return new Vector4(v.Y, v.X, v.W, v.Y); }
        public static Vector4 YXWZ(this Vector4 v) { return new Vector4(v.Y, v.X, v.W, v.Z); }
        public static Vector4 YXWW(this Vector4 v) { return new Vector4(v.Y, v.X, v.W, v.W); }
        public static Vector4 YYXX(this Vector4 v) { return new Vector4(v.Y, v.Y, v.X, v.X); }
        public static Vector4 YYXY(this Vector4 v) { return new Vector4(v.Y, v.Y, v.X, v.Y); }
        public static Vector4 YYXZ(this Vector4 v) { return new Vector4(v.Y, v.Y, v.X, v.Z); }
        public static Vector4 YYXW(this Vector4 v) { return new Vector4(v.Y, v.Y, v.X, v.W); }
        public static Vector4 YYYX(this Vector4 v) { return new Vector4(v.Y, v.Y, v.Y, v.X); }
        public static Vector4 YYYY(this Vector4 v) { return new Vector4(v.Y, v.Y, v.Y, v.Y); }
        public static Vector4 YYYZ(this Vector4 v) { return new Vector4(v.Y, v.Y, v.Y, v.Z); }
        public static Vector4 YYYW(this Vector4 v) { return new Vector4(v.Y, v.Y, v.Y, v.W); }
        public static Vector4 YYZX(this Vector4 v) { return new Vector4(v.Y, v.Y, v.Z, v.X); }
        public static Vector4 YYZY(this Vector4 v) { return new Vector4(v.Y, v.Y, v.Z, v.Y); }
        public static Vector4 YYZZ(this Vector4 v) { return new Vector4(v.Y, v.Y, v.Z, v.Z); }
        public static Vector4 YYZW(this Vector4 v) { return new Vector4(v.Y, v.Y, v.Z, v.W); }
        public static Vector4 YYWX(this Vector4 v) { return new Vector4(v.Y, v.Y, v.W, v.X); }
        public static Vector4 YYWY(this Vector4 v) { return new Vector4(v.Y, v.Y, v.W, v.Y); }
        public static Vector4 YYWZ(this Vector4 v) { return new Vector4(v.Y, v.Y, v.W, v.Z); }
        public static Vector4 YYWW(this Vector4 v) { return new Vector4(v.Y, v.Y, v.W, v.W); }
        public static Vector4 YZXX(this Vector4 v) { return new Vector4(v.Y, v.Z, v.X, v.X); }
        public static Vector4 YZXY(this Vector4 v) { return new Vector4(v.Y, v.Z, v.X, v.Y); }
        public static Vector4 YZXZ(this Vector4 v) { return new Vector4(v.Y, v.Z, v.X, v.Z); }
        public static Vector4 YZXW(this Vector4 v) { return new Vector4(v.Y, v.Z, v.X, v.W); }
        public static Vector4 YZYX(this Vector4 v) { return new Vector4(v.Y, v.Z, v.Y, v.X); }
        public static Vector4 YZYY(this Vector4 v) { return new Vector4(v.Y, v.Z, v.Y, v.Y); }
        public static Vector4 YZYZ(this Vector4 v) { return new Vector4(v.Y, v.Z, v.Y, v.Z); }
        public static Vector4 YZYW(this Vector4 v) { return new Vector4(v.Y, v.Z, v.Y, v.W); }
        public static Vector4 YZZX(this Vector4 v) { return new Vector4(v.Y, v.Z, v.Z, v.X); }
        public static Vector4 YZZY(this Vector4 v) { return new Vector4(v.Y, v.Z, v.Z, v.Y); }
        public static Vector4 YZZZ(this Vector4 v) { return new Vector4(v.Y, v.Z, v.Z, v.Z); }
        public static Vector4 YZZW(this Vector4 v) { return new Vector4(v.Y, v.Z, v.Z, v.W); }
        public static Vector4 YZWX(this Vector4 v) { return new Vector4(v.Y, v.Z, v.W, v.X); }
        public static Vector4 YZWY(this Vector4 v) { return new Vector4(v.Y, v.Z, v.W, v.Y); }
        public static Vector4 YZWZ(this Vector4 v) { return new Vector4(v.Y, v.Z, v.W, v.Z); }
        public static Vector4 YZWW(this Vector4 v) { return new Vector4(v.Y, v.Z, v.W, v.W); }
        public static Vector4 YWXX(this Vector4 v) { return new Vector4(v.Y, v.W, v.X, v.X); }
        public static Vector4 YWXY(this Vector4 v) { return new Vector4(v.Y, v.W, v.X, v.Y); }
        public static Vector4 YWXZ(this Vector4 v) { return new Vector4(v.Y, v.W, v.X, v.Z); }
        public static Vector4 YWXW(this Vector4 v) { return new Vector4(v.Y, v.W, v.X, v.W); }
        public static Vector4 YWYX(this Vector4 v) { return new Vector4(v.Y, v.W, v.Y, v.X); }
        public static Vector4 YWYY(this Vector4 v) { return new Vector4(v.Y, v.W, v.Y, v.Y); }
        public static Vector4 YWYZ(this Vector4 v) { return new Vector4(v.Y, v.W, v.Y, v.Z); }
        public static Vector4 YWYW(this Vector4 v) { return new Vector4(v.Y, v.W, v.Y, v.W); }
        public static Vector4 YWZX(this Vector4 v) { return new Vector4(v.Y, v.W, v.Z, v.X); }
        public static Vector4 YWZY(this Vector4 v) { return new Vector4(v.Y, v.W, v.Z, v.Y); }
        public static Vector4 YWZZ(this Vector4 v) { return new Vector4(v.Y, v.W, v.Z, v.Z); }
        public static Vector4 YWZW(this Vector4 v) { return new Vector4(v.Y, v.W, v.Z, v.W); }
        public static Vector4 YWWX(this Vector4 v) { return new Vector4(v.Y, v.W, v.W, v.X); }
        public static Vector4 YWWY(this Vector4 v) { return new Vector4(v.Y, v.W, v.W, v.Y); }
        public static Vector4 YWWZ(this Vector4 v) { return new Vector4(v.Y, v.W, v.W, v.Z); }
        public static Vector4 YWWW(this Vector4 v) { return new Vector4(v.Y, v.W, v.W, v.W); }
        public static Vector4 ZXXX(this Vector4 v) { return new Vector4(v.Z, v.X, v.X, v.X); }
        public static Vector4 ZXXY(this Vector4 v) { return new Vector4(v.Z, v.X, v.X, v.Y); }
        public static Vector4 ZXXZ(this Vector4 v) { return new Vector4(v.Z, v.X, v.X, v.Z); }
        public static Vector4 ZXXW(this Vector4 v) { return new Vector4(v.Z, v.X, v.X, v.W); }
        public static Vector4 ZXYX(this Vector4 v) { return new Vector4(v.Z, v.X, v.Y, v.X); }
        public static Vector4 ZXYY(this Vector4 v) { return new Vector4(v.Z, v.X, v.Y, v.Y); }
        public static Vector4 ZXYZ(this Vector4 v) { return new Vector4(v.Z, v.X, v.Y, v.Z); }
        public static Vector4 ZXYW(this Vector4 v) { return new Vector4(v.Z, v.X, v.Y, v.W); }
        public static Vector4 ZXZX(this Vector4 v) { return new Vector4(v.Z, v.X, v.Z, v.X); }
        public static Vector4 ZXZY(this Vector4 v) { return new Vector4(v.Z, v.X, v.Z, v.Y); }
        public static Vector4 ZXZZ(this Vector4 v) { return new Vector4(v.Z, v.X, v.Z, v.Z); }
        public static Vector4 ZXZW(this Vector4 v) { return new Vector4(v.Z, v.X, v.Z, v.W); }
        public static Vector4 ZXWX(this Vector4 v) { return new Vector4(v.Z, v.X, v.W, v.X); }
        public static Vector4 ZXWY(this Vector4 v) { return new Vector4(v.Z, v.X, v.W, v.Y); }
        public static Vector4 ZXWZ(this Vector4 v) { return new Vector4(v.Z, v.X, v.W, v.Z); }
        public static Vector4 ZXWW(this Vector4 v) { return new Vector4(v.Z, v.X, v.W, v.W); }
        public static Vector4 ZYXX(this Vector4 v) { return new Vector4(v.Z, v.Y, v.X, v.X); }
        public static Vector4 ZYXY(this Vector4 v) { return new Vector4(v.Z, v.Y, v.X, v.Y); }
        public static Vector4 ZYXZ(this Vector4 v) { return new Vector4(v.Z, v.Y, v.X, v.Z); }
        public static Vector4 ZYXW(this Vector4 v) { return new Vector4(v.Z, v.Y, v.X, v.W); }
        public static Vector4 ZYYX(this Vector4 v) { return new Vector4(v.Z, v.Y, v.Y, v.X); }
        public static Vector4 ZYYY(this Vector4 v) { return new Vector4(v.Z, v.Y, v.Y, v.Y); }
        public static Vector4 ZYYZ(this Vector4 v) { return new Vector4(v.Z, v.Y, v.Y, v.Z); }
        public static Vector4 ZYYW(this Vector4 v) { return new Vector4(v.Z, v.Y, v.Y, v.W); }
        public static Vector4 ZYZX(this Vector4 v) { return new Vector4(v.Z, v.Y, v.Z, v.X); }
        public static Vector4 ZYZY(this Vector4 v) { return new Vector4(v.Z, v.Y, v.Z, v.Y); }
        public static Vector4 ZYZZ(this Vector4 v) { return new Vector4(v.Z, v.Y, v.Z, v.Z); }
        public static Vector4 ZYZW(this Vector4 v) { return new Vector4(v.Z, v.Y, v.Z, v.W); }
        public static Vector4 ZYWX(this Vector4 v) { return new Vector4(v.Z, v.Y, v.W, v.X); }
        public static Vector4 ZYWY(this Vector4 v) { return new Vector4(v.Z, v.Y, v.W, v.Y); }
        public static Vector4 ZYWZ(this Vector4 v) { return new Vector4(v.Z, v.Y, v.W, v.Z); }
        public static Vector4 ZYWW(this Vector4 v) { return new Vector4(v.Z, v.Y, v.W, v.W); }
        public static Vector4 ZZXX(this Vector4 v) { return new Vector4(v.Z, v.Z, v.X, v.X); }
        public static Vector4 ZZXY(this Vector4 v) { return new Vector4(v.Z, v.Z, v.X, v.Y); }
        public static Vector4 ZZXZ(this Vector4 v) { return new Vector4(v.Z, v.Z, v.X, v.Z); }
        public static Vector4 ZZXW(this Vector4 v) { return new Vector4(v.Z, v.Z, v.X, v.W); }
        public static Vector4 ZZYX(this Vector4 v) { return new Vector4(v.Z, v.Z, v.Y, v.X); }
        public static Vector4 ZZYY(this Vector4 v) { return new Vector4(v.Z, v.Z, v.Y, v.Y); }
        public static Vector4 ZZYZ(this Vector4 v) { return new Vector4(v.Z, v.Z, v.Y, v.Z); }
        public static Vector4 ZZYW(this Vector4 v) { return new Vector4(v.Z, v.Z, v.Y, v.W); }
        public static Vector4 ZZZX(this Vector4 v) { return new Vector4(v.Z, v.Z, v.Z, v.X); }
        public static Vector4 ZZZY(this Vector4 v) { return new Vector4(v.Z, v.Z, v.Z, v.Y); }
        public static Vector4 ZZZZ(this Vector4 v) { return new Vector4(v.Z, v.Z, v.Z, v.Z); }
        public static Vector4 ZZZW(this Vector4 v) { return new Vector4(v.Z, v.Z, v.Z, v.W); }
        public static Vector4 ZZWX(this Vector4 v) { return new Vector4(v.Z, v.Z, v.W, v.X); }
        public static Vector4 ZZWY(this Vector4 v) { return new Vector4(v.Z, v.Z, v.W, v.Y); }
        public static Vector4 ZZWZ(this Vector4 v) { return new Vector4(v.Z, v.Z, v.W, v.Z); }
        public static Vector4 ZZWW(this Vector4 v) { return new Vector4(v.Z, v.Z, v.W, v.W); }
        public static Vector4 ZWXX(this Vector4 v) { return new Vector4(v.Z, v.W, v.X, v.X); }
        public static Vector4 ZWXY(this Vector4 v) { return new Vector4(v.Z, v.W, v.X, v.Y); }
        public static Vector4 ZWXZ(this Vector4 v) { return new Vector4(v.Z, v.W, v.X, v.Z); }
        public static Vector4 ZWXW(this Vector4 v) { return new Vector4(v.Z, v.W, v.X, v.W); }
        public static Vector4 ZWYX(this Vector4 v) { return new Vector4(v.Z, v.W, v.Y, v.X); }
        public static Vector4 ZWYY(this Vector4 v) { return new Vector4(v.Z, v.W, v.Y, v.Y); }
        public static Vector4 ZWYZ(this Vector4 v) { return new Vector4(v.Z, v.W, v.Y, v.Z); }
        public static Vector4 ZWYW(this Vector4 v) { return new Vector4(v.Z, v.W, v.Y, v.W); }
        public static Vector4 ZWZX(this Vector4 v) { return new Vector4(v.Z, v.W, v.Z, v.X); }
        public static Vector4 ZWZY(this Vector4 v) { return new Vector4(v.Z, v.W, v.Z, v.Y); }
        public static Vector4 ZWZZ(this Vector4 v) { return new Vector4(v.Z, v.W, v.Z, v.Z); }
        public static Vector4 ZWZW(this Vector4 v) { return new Vector4(v.Z, v.W, v.Z, v.W); }
        public static Vector4 ZWWX(this Vector4 v) { return new Vector4(v.Z, v.W, v.W, v.X); }
        public static Vector4 ZWWY(this Vector4 v) { return new Vector4(v.Z, v.W, v.W, v.Y); }
        public static Vector4 ZWWZ(this Vector4 v) { return new Vector4(v.Z, v.W, v.W, v.Z); }
        public static Vector4 ZWWW(this Vector4 v) { return new Vector4(v.Z, v.W, v.W, v.W); }
        public static Vector4 WXXX(this Vector4 v) { return new Vector4(v.W, v.X, v.X, v.X); }
        public static Vector4 WXXY(this Vector4 v) { return new Vector4(v.W, v.X, v.X, v.Y); }
        public static Vector4 WXXZ(this Vector4 v) { return new Vector4(v.W, v.X, v.X, v.Z); }
        public static Vector4 WXXW(this Vector4 v) { return new Vector4(v.W, v.X, v.X, v.W); }
        public static Vector4 WXYX(this Vector4 v) { return new Vector4(v.W, v.X, v.Y, v.X); }
        public static Vector4 WXYY(this Vector4 v) { return new Vector4(v.W, v.X, v.Y, v.Y); }
        public static Vector4 WXYZ(this Vector4 v) { return new Vector4(v.W, v.X, v.Y, v.Z); }
        public static Vector4 WXYW(this Vector4 v) { return new Vector4(v.W, v.X, v.Y, v.W); }
        public static Vector4 WXZX(this Vector4 v) { return new Vector4(v.W, v.X, v.Z, v.X); }
        public static Vector4 WXZY(this Vector4 v) { return new Vector4(v.W, v.X, v.Z, v.Y); }
        public static Vector4 WXZZ(this Vector4 v) { return new Vector4(v.W, v.X, v.Z, v.Z); }
        public static Vector4 WXZW(this Vector4 v) { return new Vector4(v.W, v.X, v.Z, v.W); }
        public static Vector4 WXWX(this Vector4 v) { return new Vector4(v.W, v.X, v.W, v.X); }
        public static Vector4 WXWY(this Vector4 v) { return new Vector4(v.W, v.X, v.W, v.Y); }
        public static Vector4 WXWZ(this Vector4 v) { return new Vector4(v.W, v.X, v.W, v.Z); }
        public static Vector4 WXWW(this Vector4 v) { return new Vector4(v.W, v.X, v.W, v.W); }
        public static Vector4 WYXX(this Vector4 v) { return new Vector4(v.W, v.Y, v.X, v.X); }
        public static Vector4 WYXY(this Vector4 v) { return new Vector4(v.W, v.Y, v.X, v.Y); }
        public static Vector4 WYXZ(this Vector4 v) { return new Vector4(v.W, v.Y, v.X, v.Z); }
        public static Vector4 WYXW(this Vector4 v) { return new Vector4(v.W, v.Y, v.X, v.W); }
        public static Vector4 WYYX(this Vector4 v) { return new Vector4(v.W, v.Y, v.Y, v.X); }
        public static Vector4 WYYY(this Vector4 v) { return new Vector4(v.W, v.Y, v.Y, v.Y); }
        public static Vector4 WYYZ(this Vector4 v) { return new Vector4(v.W, v.Y, v.Y, v.Z); }
        public static Vector4 WYYW(this Vector4 v) { return new Vector4(v.W, v.Y, v.Y, v.W); }
        public static Vector4 WYZX(this Vector4 v) { return new Vector4(v.W, v.Y, v.Z, v.X); }
        public static Vector4 WYZY(this Vector4 v) { return new Vector4(v.W, v.Y, v.Z, v.Y); }
        public static Vector4 WYZZ(this Vector4 v) { return new Vector4(v.W, v.Y, v.Z, v.Z); }
        public static Vector4 WYZW(this Vector4 v) { return new Vector4(v.W, v.Y, v.Z, v.W); }
        public static Vector4 WYWX(this Vector4 v) { return new Vector4(v.W, v.Y, v.W, v.X); }
        public static Vector4 WYWY(this Vector4 v) { return new Vector4(v.W, v.Y, v.W, v.Y); }
        public static Vector4 WYWZ(this Vector4 v) { return new Vector4(v.W, v.Y, v.W, v.Z); }
        public static Vector4 WYWW(this Vector4 v) { return new Vector4(v.W, v.Y, v.W, v.W); }
        public static Vector4 WZXX(this Vector4 v) { return new Vector4(v.W, v.Z, v.X, v.X); }
        public static Vector4 WZXY(this Vector4 v) { return new Vector4(v.W, v.Z, v.X, v.Y); }
        public static Vector4 WZXZ(this Vector4 v) { return new Vector4(v.W, v.Z, v.X, v.Z); }
        public static Vector4 WZXW(this Vector4 v) { return new Vector4(v.W, v.Z, v.X, v.W); }
        public static Vector4 WZYX(this Vector4 v) { return new Vector4(v.W, v.Z, v.Y, v.X); }
        public static Vector4 WZYY(this Vector4 v) { return new Vector4(v.W, v.Z, v.Y, v.Y); }
        public static Vector4 WZYZ(this Vector4 v) { return new Vector4(v.W, v.Z, v.Y, v.Z); }
        public static Vector4 WZYW(this Vector4 v) { return new Vector4(v.W, v.Z, v.Y, v.W); }
        public static Vector4 WZZX(this Vector4 v) { return new Vector4(v.W, v.Z, v.Z, v.X); }
        public static Vector4 WZZY(this Vector4 v) { return new Vector4(v.W, v.Z, v.Z, v.Y); }
        public static Vector4 WZZZ(this Vector4 v) { return new Vector4(v.W, v.Z, v.Z, v.Z); }
        public static Vector4 WZZW(this Vector4 v) { return new Vector4(v.W, v.Z, v.Z, v.W); }
        public static Vector4 WZWX(this Vector4 v) { return new Vector4(v.W, v.Z, v.W, v.X); }
        public static Vector4 WZWY(this Vector4 v) { return new Vector4(v.W, v.Z, v.W, v.Y); }
        public static Vector4 WZWZ(this Vector4 v) { return new Vector4(v.W, v.Z, v.W, v.Z); }
        public static Vector4 WZWW(this Vector4 v) { return new Vector4(v.W, v.Z, v.W, v.W); }
        public static Vector4 WWXX(this Vector4 v) { return new Vector4(v.W, v.W, v.X, v.X); }
        public static Vector4 WWXY(this Vector4 v) { return new Vector4(v.W, v.W, v.X, v.Y); }
        public static Vector4 WWXZ(this Vector4 v) { return new Vector4(v.W, v.W, v.X, v.Z); }
        public static Vector4 WWXW(this Vector4 v) { return new Vector4(v.W, v.W, v.X, v.W); }
        public static Vector4 WWYX(this Vector4 v) { return new Vector4(v.W, v.W, v.Y, v.X); }
        public static Vector4 WWYY(this Vector4 v) { return new Vector4(v.W, v.W, v.Y, v.Y); }
        public static Vector4 WWYZ(this Vector4 v) { return new Vector4(v.W, v.W, v.Y, v.Z); }
        public static Vector4 WWYW(this Vector4 v) { return new Vector4(v.W, v.W, v.Y, v.W); }
        public static Vector4 WWZX(this Vector4 v) { return new Vector4(v.W, v.W, v.Z, v.X); }
        public static Vector4 WWZY(this Vector4 v) { return new Vector4(v.W, v.W, v.Z, v.Y); }
        public static Vector4 WWZZ(this Vector4 v) { return new Vector4(v.W, v.W, v.Z, v.Z); }
        public static Vector4 WWZW(this Vector4 v) { return new Vector4(v.W, v.W, v.Z, v.W); }
        public static Vector4 WWWX(this Vector4 v) { return new Vector4(v.W, v.W, v.W, v.X); }
        public static Vector4 WWWY(this Vector4 v) { return new Vector4(v.W, v.W, v.W, v.Y); }
        public static Vector4 WWWZ(this Vector4 v) { return new Vector4(v.W, v.W, v.W, v.Z); }
        public static Vector4 WWWW(this Vector4 v) { return new Vector4(v.W, v.W, v.W, v.W); }


    }

}
