using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace MapLib
{
    public class Polygon
    {
        Vector4 bounds_;
        Vector2[] verts_;
        Vector2 origin_ = Vector2.Zero;
        Vector2 offset_ = Vector2.Zero;
        Vector2 scale_ = Vector2.One;
        Vector2 centroid_ = Vector2.Zero;
        float rotation_ = 0;

        public Vector2[] WorldVertices { get; private set; }
        public Vector2[] Vertices { get { return verts_; } set { verts_ = value; Dirty = true; } }
        public Vector2 Origin { get { return origin_; } set { origin_ = value; Dirty = true; } }
        public Vector2 Offset { get { return offset_; } set { offset_ = value; Dirty = true; } }
        public Vector2 Scale { get { return scale_; } set { scale_ = value; Dirty = true; } }
        public float Rotation { get { return rotation_; } set { rotation_ = value; Dirty = true; } }
        bool Dirty { get; set; }

        public Polygon(Vector2[] verts)
        {
            Vertices = verts;
        }

        public void Translate(Vector2 by)
        {
            Offset = Offset + by;
        }

        public void Rotate(float degrees)
        {
            Rotation += degrees;
        }

        public Vector2[] GetTransformedVertices()
        {
            if (!Dirty)
                return WorldVertices;
            Dirty = false;

            Vector2[] localVertices = Vertices;
            if (WorldVertices == null || WorldVertices.Length != Vertices.Length)
                WorldVertices = new Vector2[localVertices.Length];

            Vector2[] worldVertices = WorldVertices;
            float positionX = Offset.X;
            float positionY = Offset.Y;
            float originX = Origin.X;
            float originY = Origin.Y;
            float scaleX = Scale.X;
            float scaleY = Scale.Y;
            bool scale = Scale.X != 1 || Scale.Y != 1;
            float rotation = Rotation;
            float cos = (float)Math.Cos(rotation * 0.0174533); // degress -> radians
            float sin = (float)Math.Sin(rotation * 0.0174533);

            for (int i = 0, n = localVertices.Length; i < n; ++i)
            {
                float x = localVertices[i].X - originX;
                float y = localVertices[i].Y - originY;

                // scale if needed
                if (scale)
                {
                    x *= scaleX;
                    y *= scaleY;
                }

                // rotate if needed
                if (rotation != 0)
                {
                    float oldX = x;
                    x = cos * x - sin * y;
                    y = sin * oldX + cos * y;
                }

                worldVertices[i] = new Vector2(positionX + x + originX, positionY + y + originY);
            }
            return worldVertices;
        }

        public bool Contains(float x, float y)
        {
            Vector2[] vertices = GetTransformedVertices();
            int intersects = 0;

            for (int i = 0; i < vertices.Length; ++i)
            {
                float x1 = vertices[i].X;
                float y1 = vertices[i].Y;
                float x2 = vertices[(i + 2) % vertices.Length].X;
                float y2 = vertices[(i + 3) % vertices.Length].Y;
                if (((y1 <= y && y < y2) || (y2 <= y && y < y1)) && x < ((x2 - x1) / (y2 - y1) * (y - y1) + x1))
                    intersects++;
            }
            return (intersects & 1) == 1;
        }

        public bool Contains(Vector2 v)
        {
            return Contains(v.X, v.Y);
        }

        public Vector4 GetBoundingRectangle()
        {
            if (!Dirty)
                return bounds_;

            Vector2[] vertices = GetTransformedVertices();

            float minX = vertices[0].X;
            float minY = vertices[0].Y;
            float maxX = vertices[0].X;
            float maxY = vertices[0].Y;

            for (int i = 0; i < vertices.Length; ++i)
            {
                minX = minX > vertices[i].X ? vertices[i].X : minX;
                minY = minY > vertices[i].Y ? vertices[i].Y : minY;
                maxX = maxX < vertices[i].X ? vertices[i].X : maxX;
                maxY = maxY < vertices[i].Y ? vertices[i].Y : maxY;
            }

            bounds_ = new Vector4(minX, minY, maxX - minX, maxY - minY);
            return bounds_;
        }

        public bool IsClockwise()
        {
            if (Vertices == null || Vertices.Length == 0)
                return false;

            float area = 0, p1x, p1y, p2x, p2y;
            int n = 0 + Vertices.Length - 1;
            for (int i = 0; i < n; ++i)
            {
                p1x = Vertices[i].X;
                p1y = Vertices[i].Y;
                p2x = Vertices[i + 1].X;
                p2y = Vertices[i + 1].Y;
                area += p1x * p2y - p2x * p1y;
            }
            p1x = Vertices[Vertices.Length - 1].X;
            p1y = Vertices[Vertices.Length - 1].Y;
            p2x = Vertices[0].X;
            p2y = Vertices[0].Y;
            return area + p1x * p2y - p2x * p1y < 0;
        }

        public Vector2 Centroid()
        {
            if (!Dirty)
                return centroid_;
            if (Vertices.Length < 3)
                return Vector2.Zero;
            float x = 0, y = 0;

            float signedArea = 0;
            int i = 0;
            int offset = 0;
            int count = Vertices.Length;
            for (int n = offset + count - 1; i < n; i += 1)
            {
                float x00 = Vertices[i].X;
                float y00 = Vertices[i].Y;
                float x11 = Vertices[i + 1].X;
                float y11 = Vertices[i + 1].Y;
                float aa = x00 * y11 - x11 * y00;
                signedArea += aa;
                x += (x00 + x11) * aa;
                y += (y00 + y11) * aa;
            }

            float x0 = Vertices[i].X;
            float y0 = Vertices[i].Y;
            float x1 = Vertices[0].X;
            float y1 = Vertices[0].Y;
            float a = x0 * y1 - x1 * y0;
            signedArea += a;
            x += (x0 + x1) * a;
            y += (y0 + y1) * a;

            if (signedArea == 0)
            {
                centroid_.X = 0;
                centroid_.Y = 0;
            }
            else
            {
                signedArea *= 0.5f;
                centroid_.X = x / (6 * signedArea);
                centroid_.Y = y / (6 * signedArea);
            }
            return centroid_;
        }

        public void EnsureCCW()
        {
            if (!IsClockwise())
                return;

            Dirty = true;
            int offset = 0;
            int count = Vertices.Length;
            int lastX = offset + count - 1;
            for (int i = offset, n = offset + count / 2; i < n; i += 2)
            {
                int other = lastX - i;
                float x = Vertices[i].X;
                float y = Vertices[i].Y;
                Vertices[i] = Vertices[other];
                Vertices[other] = new Vector2(x, y);
            }
        }
    }
}
