using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit
{
    /* Usage:
     *      RaycastRenderer render = new RaycastRenderer() { Width = 128, Height = 128 };
     *      render.SetToPerspective(45);
     *      render.FocusOnCloud(vertexPositions);
     *      render.DepthTrace(depthCallback);
     *      return render.GetDepthImage();      
     *      
     *      float depthCallback(Ray ray) {
     *          float retVal = 0.0f;
     *          if (myModelFile.DoMousePick(ray, out retVal))
     *              return retVal;
     *          return float.MaxValue; // maxvalue is our bogus depth
     *      }
     */
    public class SoftwareRenderer
    {
        Vector3 position_;
        Vector3 upDir_ = Vector3.UnitY;
        Vector3 direction_ = Vector3.UnitZ;
        Matrix viewMatrix_ = Matrix.Identity;

        public Vector3 Position { get { return position_; } set { position_ = value; UpdateMatrix(); } }
        public float[] Data;
        public Matrix ProjectionMatrix { get; set; }
        public Matrix ViewMatrix { get { return viewMatrix_; } set { viewMatrix_ = value; } }
        public float MinDepth { get; set; } = 0.1f;
        public float MaxDepth { get; set; } = 500.0f;

        public int Width { get; set; } = 128;
        public int Height { get; set; } = 128;

        public SoftwareRenderer(int width, int height, float fov)
        {
            Width = width;
            Height = height;
            SetToPerspective(fov);
        }

        public void SetToPerspective(float fov)
        {
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(fov),  // 45 degree angle
                (float)Width /
                (float)Height,
                0.1f, 500.0f) * (Matrix.Identity * Matrix.CreateScale(-1.0f, 1.0f, 1.0f));
        }

        public void LookAtDir(Vector3 dir)
        {
            direction_ = dir;
            direction_.Normalize();
            UpdateMatrix();
        }

        public void LookAtDir(Vector3 pos, Vector3 dir)
        {
            position_ = pos;
            LookAtDir(dir);
        }

        public void LookAtPoint(Vector3 tgt)
        {
            direction_ = (tgt - position_);
            direction_.Normalize();
            UpdateMatrix();
        }

        public void UpdateMatrix()
        {
            Vector3 dir = position_ + direction_;
            Matrix.CreateLookAt(ref position_, ref dir, ref upDir_, out viewMatrix_);
        }

        public void DepthTrace(Func<Ray, float> func)
        {
            UpdateMatrix();

            Data = new float[Width * Height];
            for (int i = 0; i < Data.Length; ++i)
                Data[i] = float.MaxValue;
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    Ray r = GetRay(x, y);
                    float result = func(r);
                    if (result != float.MaxValue)
                    {
                        Data[ToIndex(x, y)] = result;

                        Data[ToIndex(x+1, y)] = result;
                        Data[ToIndex(x-1, y)] = result;

                        Data[ToIndex(x, y+1)] = result;
                        Data[ToIndex(x, y-1)] = result;

                        Data[ToIndex(x+1, y+1)] = result;
                        Data[ToIndex(x-1, y-1)] = result;
                        Data[ToIndex(x+1, y-1)] = result;
                        Data[ToIndex(x-1, y+1)] = result;
                    }
                }
            }
        }

        System.Drawing.Bitmap raster;
        public System.Drawing.Bitmap GetDepthImage() { return raster; }

        int Clamp(int a, int l, int h)
        {
            return Math.Min(h, Math.Max(a, l));
        }

        int ToIndex(int x, int y)
        {
            x = Clamp(x, 0, Width - 1);
            y = Clamp(y, 0, Height - 1);
            return y * Width + x;
        }

        public Ray GetRay(int x, int y)
        {
            float xFactor = x / (Width - 1);
            float yFactor = y / (Height - 1);

            Vector3 nearPoint = new Vector3(x, y, 0);
            Vector3 farPoint = new Vector3(x, y, 1);
            nearPoint = Unproject(nearPoint, ProjectionMatrix, ViewMatrix, Matrix.Identity);
            farPoint = Unproject(farPoint, ProjectionMatrix, ViewMatrix, Matrix.Identity);
            Vector3 dir = (farPoint - nearPoint);
            dir.Normalize();
            return new Ray(nearPoint, dir);
        }

        Vector3 Unproject(Vector3 source, Matrix projection, Matrix view, Matrix world)
        {
            Matrix matrix = Matrix.Invert(Matrix.Multiply(Matrix.Multiply(world, view), projection));
            source.X = (((source.X) / ((float)this.Width)) * 2f) - 1f;
            source.Y = -((((source.Y) / ((float)this.Height)) * 2f) - 1f);
            source.Z = (source.Z - this.MinDepth) / (this.MaxDepth - this.MinDepth);
            Vector3 vector = Vector3.Transform(source, matrix);
            float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
            if (!WithinEpsilon(a, 1f))
            {
                vector.X = vector.X / a;
                vector.Y = vector.Y / a;
                vector.Z = vector.Z / a;
            }
            return vector;
        }

        Vector3 Project(Vector3 source, Matrix projection, Matrix view)
        {
            Matrix matrix = Matrix.Multiply(view, projection);
            Vector3 vector = Vector3.Transform(source, matrix);
            float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
            if (!WithinEpsilon(a, 1f))
            {
                vector.X = vector.X / a;
                vector.Y = vector.Y / a;
                vector.Z = vector.Z / a;
            }
            vector.X = (((vector.X + 1f) * 0.5f) * Width);
            vector.Y = (((-vector.Y + 1f) * 0.5f) * Height);
            vector.Z = (vector.Z * (MaxDepth - MinDepth)) + MinDepth;
            return vector;
        }

        static bool WithinEpsilon(float v, float target) { return Math.Abs(v - target) < eps; }

        static float eps = GetMachineEpsilonFloat();
        static float GetMachineEpsilonFloat()
        {
            float machineEpsilon = 1.0f;
            float comparison;
            do
            {
                machineEpsilon *= 0.5f;
                comparison = 1.0f + machineEpsilon;
            }
            while (comparison > 1.0f);
            return machineEpsilon;
        }

        public void FocusOnBounds(BoundingBox bounds, Vector3 offsetDir)
        {
            // Compute centroid and bounds (to approximate radius)
            Vector3 centroid = bounds.Min + (bounds.Max - bounds.Min) * 0.5f;
            Vector3 min = bounds.Min;
            Vector3 max = bounds.Max;

            float radius = (max - min).Length() / 2;
            double camDistance = (radius * 2.0) / Math.Tan((45 * DEGTORAD) / 2.0);
            float dist = (float)(camDistance) * 0.5f;

            // Set our position, then look at the centroid
            Position = centroid + offsetDir * dist; //TODO: check axis length to change to a more ideal view?
            if (Math.Abs(Vector3.Dot(offsetDir, Vector3.UnitY)) > 0.9f)
                upDir_ = Vector3.UnitZ;
            LookAtPoint(centroid);
        }

        public void FocusOnCloud(List<Vector3> ptCloud)
        {
            // Compute centroid and bounds (to approximate radius)
            Vector3 centroid = new Vector3();
            Vector3 min = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
            Vector3 max = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
            foreach (var pt in ptCloud)
            {
                centroid += pt;
                min = Vector3.Min(min, pt);
                max = Vector3.Max(max, pt);
            }
            // divide to get centroid
            centroid /= ptCloud.Count;

            float radius = (max - min).Length() / 2;
            double camDistance = (radius * 2.0) / Math.Tan((45 * DEGTORAD) / 2.0);
            float dist = (float)(camDistance) * 0.75f;

            // Set our position, then look at the centroid
            Vector3 offsetDir = Vector3.Normalize(new Vector3(1, 1, 0.75f));
            Position = centroid + offsetDir * dist; //TODO: check axis length to change to a more ideal view?
            LookAtPoint(centroid);
        }

        const float PI = 3.14159265358979323846264338327950288f;
        const float DEGTORAD = PI / 180.0f;

        public bool RasterizeTriangles(List<int> indices, List<PluginLib.VertexData> vertices, bool asNormal)
        {
            UpdateMatrix();

            if (Data == null)
            {
                Data = new float[Width * Height];
                raster = new System.Drawing.Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                for (int y = 0; y < raster.Height; ++y)
                    for (int x = 0; x < raster.Width; ++x)
                        raster.SetPixel(x, y, System.Drawing.Color.FromArgb(0, 0, 0, 0));

                // Fixes depth-test bug, was reclearing the z-buffer every draw
                for (int i = 0; i < Data.Length; ++i)
                    Data[i] = float.MaxValue;
            }

            bool wroteAnything = false;
            for (int i = 0; i < indices.Count; i += 3)
            {
                PluginLib.VertexData[] verts =
                {
                    vertices[indices[i]],
                    vertices[indices[i+1]],
                    vertices[indices[i+2]]
                };

                // get projected vertices in 0-1 domain
                Vector3 halfVec = new Vector3(0.5f, 0.5f, 1.0f);
                Vector3[] projectedVerts =
                {
                    (Project(verts[0].Position, ProjectionMatrix, ViewMatrix)),// + Vector3.One) * halfVec,
                    (Project(verts[1].Position, ProjectionMatrix, ViewMatrix)),// + Vector3.One) * halfVec,
                    (Project(verts[2].Position, ProjectionMatrix, ViewMatrix)),// + Vector3.One) * halfVec
                };

                Vector2[] coords =
                {
                    new Vector2(projectedVerts[0].X, projectedVerts[0].Y),
                    new Vector2(projectedVerts[1].X, projectedVerts[1].Y),
                    new Vector2(projectedVerts[2].X, projectedVerts[2].Y),
                };

                Vector3[] normals =
                {
                    verts[2].Normal,
                    verts[1].Normal,
                    verts[0].Normal,
                };

                float[] depths =
                {
                    projectedVerts[2].Z,
                    projectedVerts[1].Z,
                    projectedVerts[0].Z,
                };

                wroteAnything |= asNormal ? RasterizeNormalTriangle(coords, depths, normals) : RasterizeTriangle(coords, depths, normals);
            }
            return wroteAnything;
        }

        struct Rect
        {
            public float xMin;
            public float yMin;
            public float xMax;
            public float yMax;

            public float width { get { return xMax - xMin; } }
            public float height { get { return yMax - yMin; } }

            public Rect(float xmin, float ymin, float width, float height)
            {
                xMin = xmin;
                yMin = ymin;
                xMax = xMin + width;
                yMax = yMin + height;
            }
        }

        static Rect CalculateTriangleImageBounds(int width, int height, Vector2[] uvs)
        {
            int xMin = (int)Math.Min(uvs[0].X, Math.Min(uvs[1].X, uvs[2].X));
            int xMax = (int)Math.Max(uvs[0].X, Math.Max(uvs[1].X, uvs[2].X));
            
            int yMax = (int)Math.Max(uvs[0].Y, Math.Max(uvs[1].Y, uvs[2].Y));
            int yMin = (int)Math.Min(uvs[0].Y, Math.Min(uvs[1].Y, uvs[2].Y));
            Rect r = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            r.xMin = xMin - 1; r.xMax = xMax + 1;
            r.yMin = yMin - 1; r.yMax = yMax + 1;
            r.xMin = Math.Max(r.xMin, 0);
            r.yMin = Math.Max(r.yMin, 0);
            r.xMax = Math.Min(r.xMax, width);
            r.yMax = Math.Min(r.yMax, height);
            return r;
        }

        bool RasterizeTriangle(Vector2[] uvs, float[] inputs, Vector3[] normals)
        {
            bool wroteAnything = false;
            Rect triBounds = CalculateTriangleImageBounds(Width, Height, uvs);
            for (float y = triBounds.yMin; y <= triBounds.yMax && y < Height; y += 1.0f)
            {
                if (y < 0 || y >= Height)
                    continue;

                float yCoord = y / Height;
                for (float x = triBounds.xMin; x <= triBounds.xMax; x += 1.0f)
                {
                    if (x < 0 || x >= Width)
                        continue;

                    float xCoord = x / Width;
                    Vector2 pt;
                    pt.X = x; pt.Y = y;
                    if (IsPointContained(uvs, x, y))
                    {
                        float writeColor = TexelToWorldSpace(uvs, pt, inputs);
                        float curValue = Data[ToIndex((int)x, (int)y)];
                        if (writeColor < curValue)// && !WithinEpsilon(writeColor, curValue))
                        {
                            Data[ToIndex((int)x, (int)y)] = writeColor;
                            Vector3 interpNorm = TexelToWorldSpace(uvs, pt, normals);
                            interpNorm.Normalize();
                            // dot-product grayscale looks so much better, maybe a color tint at most
                            int colorvalue = (byte)(Math.Abs(Vector3.Dot(interpNorm, direction_)) * 255);
                            //interpNorm *= 0.5f;
                            //int red =   (byte)((interpNorm.X+0.5f) * 255);
                            //int green = (byte)((interpNorm.Y+0.5f) * 255);
                            //int blue =  (byte)((interpNorm.Z+0.5f) * 255);
                            raster.SetPixel((int)x, (int)y, System.Drawing.Color.FromArgb(colorvalue, colorvalue, colorvalue));
                            //raster.SetPixel((int)x, (int)y, System.Drawing.Color.FromArgb(red, green, blue));
                            wroteAnything |= true;
                        }
                    }
                }
            }
            return wroteAnything;
        }

        bool RasterizeNormalTriangle(Vector2[] uvs, float[] inputs, Vector3[] normals)
        {
            bool wroteAnything = false;
            Rect triBounds = CalculateTriangleImageBounds(Width, Height, uvs);
            for (float y = triBounds.yMin; y <= triBounds.yMax && y < Height; y += 1.0f)
            {
                if (y < 0 || y >= Height)
                    continue;

                float yCoord = y / Height;
                for (float x = triBounds.xMin; x <= triBounds.xMax; x += 1.0f)
                {
                    if (x < 0 || x >= Width)
                        continue;

                    float xCoord = x / Width;
                    Vector2 pt;
                    pt.X = x; pt.Y = y;
                    if (IsPointContained(uvs, x, y))
                    {
                        float writeColor = TexelToWorldSpace(uvs, pt, inputs);
                        float curValue = Data[ToIndex((int)x, (int)y)];
                        if (writeColor < curValue)// && !WithinEpsilon(writeColor, curValue))
                        {
                            Data[ToIndex((int)x, (int)y)] = writeColor;
                            Vector3 interpNorm = TexelToWorldSpace(uvs, pt, normals);
                            interpNorm.Normalize();
                            // dot-product grayscale looks so much better, maybe a color tint at most

                            interpNorm = interpNorm * 0.5f + new Vector3(0.5f);
                            raster.SetPixel((int)x, (int)y, System.Drawing.Color.FromArgb((byte)(interpNorm.X * 255.0f), (byte)(interpNorm.Y * 255.0f), (byte)(interpNorm.Z * 255.0f)));
                            wroteAnything |= true;
                        }
                    }
                }
            }
            return wroteAnything;
        }

        static Vector3 GetBarycentricFactors(Vector2[] uvs, Vector2 point)
        {
            Vector2 v0 = uvs[2] - uvs[0];
            Vector2 v1 = uvs[1] - uvs[0];
            Vector2 v2 = point - uvs[0];

            float dot00 = Vector2.Dot(v0, v0); //??
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);

            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            float w = 1.0f - v - u;

            return new Vector3(u, v, w);
        }

        static float TexelToWorldSpace(Vector2[] uvCoords, Vector2 pointCoords, float[] points)
        {
            Vector3 vec = GetBarycentricFactors(uvCoords, pointCoords);
            float x = points[0] * vec.X + points[1] * vec.Y + points[2] * vec.Z;
            float y = points[0] * vec.X + points[1] * vec.Y + points[2] * vec.Z;
            float z = points[0] * vec.X + points[1] * vec.Y + points[2] * vec.Z;
            return Math.Min(x, Math.Min(y, z));
        }

        static Vector3 TexelToWorldSpace(Vector2[] uvCoords, Vector2 pointCoords, Vector3[] points)
        {
            Vector3 vec = GetBarycentricFactors(uvCoords, pointCoords);
            float x = points[0].X * vec.X + points[1].X * vec.Y + points[2].X * vec.Z;
            float y = points[0].Y * vec.X + points[1].Y * vec.Y + points[2].Y * vec.Z;
            float z = points[0].Z * vec.X + points[1].Z * vec.Y + points[2].Z * vec.Z;
            return new Vector3(x, y, z);
        }

        static bool IsPointContained(Vector2[] uvs, float x, float y)
        {
            Vector2 pt = new Vector2(x, y);
            Vector2 v0 = uvs[2] - uvs[0];
            Vector2 v1 = uvs[1] - uvs[0];
            Vector2 v2 = pt - uvs[0];

            float dot00 = Vector2.Dot(v0, v0); //??
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);

            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            return (u >= 0) && (v >= 0) && (u + v < 1.0f);
        }
    }
}
