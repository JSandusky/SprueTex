using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SprueKit.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StrokeVertex : IVertexType
    {
        public Vector3 Position;
        public Vector3 Start;
        public Vector3 End;
        public Vector2 StartSize;
        public Vector2 EndSize;
        public Color Color;

        void SetColor(Color col)
        {
            Color = col;
        }

        public VertexDeclaration VertexDeclaration
        {
            get { return decl; }
        }

        static VertexDeclaration decl;
        static StrokeVertex()
        {
            VertexElement[] elements = new VertexElement[] {
                new VertexElement(0,  VertexElementFormat.Vector3, VertexElementUsage.Position, 0),                  // 12 bytes
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),                 // 12 bytes
                new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Position, 2),                 // 12 bytes
                new VertexElement(36, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),        // 8 bytes
                new VertexElement(44, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),        // 8 bytes
                new VertexElement(52, VertexElementFormat.Color, VertexElementUsage.Color, 0),                      // 4 bytes
            };
            VertexDeclaration declaration = new VertexDeclaration(elements);
            decl = declaration;
        }
    }

    public struct StrokePoint
    {
        public Vector2 Position;
        public Color Color;
        public float Radius;
    }

    public class Stroke : List<StrokePoint>
    {
        public void SetColor(Color col)
        {
            for (int i = 0; i < Count; ++i)
            {
                var pt = this[i];
                pt.Color = col;
                this[i] = pt;
            }
        }
    }

    public class StrokeBatch : IDisposable
    {
        public const int MAX_VERTS = 12000;
        public const int MAX_INDICES = 4000;

        Paint.PaintStrokeEffect effect_;
        DynamicVertexBuffer vertexBuffer;
        DynamicIndexBuffer indexBuffer;
        ushort[] Indices = new ushort[MAX_INDICES];
        StrokeVertex[] Vertices = new StrokeVertex[MAX_VERTS];
        int IndexCount;
        int VertexCount;

        public Paint.PaintStrokeEffect Effect { get { return effect_; } }

        public StrokeBatch(GraphicsDevice device, ContentManager content)
        {
            effect_ = new Paint.PaintStrokeEffect(device, content);
            vertexBuffer = new DynamicVertexBuffer(device, typeof(StrokeVertex), MAX_VERTS, BufferUsage.WriteOnly);
            indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, MAX_INDICES, BufferUsage.WriteOnly);
        }

        public void Dispose()
        {
            if (vertexBuffer != null)
                vertexBuffer.Dispose();
            if (indexBuffer != null)
                indexBuffer.Dispose();
            vertexBuffer = null;
            indexBuffer = null;
        }

        public void Draw(Stroke stroke)
        {
            for (int i = 0; i < stroke.Count - 1; ++i)
            {
                StrokePoint curPt = stroke[i];
                StrokePoint nextPt = stroke[i + 1];
                Vector2 strokeVec = (nextPt.Position - curPt.Position);
                strokeVec.Normalize();
                var strokeOffset = strokeVec.Rotate(90);

                // if we have a previous then we need to mix it
                Vector2 a = curPt.Position + strokeOffset*curPt.Radius;
                Vector2 b = curPt.Position - strokeOffset*curPt.Radius;
                if (i > 0)
                {
                    StrokePoint prevPoint = stroke[i - 1];
                    var prevVec = curPt.Position - prevPoint.Position;
                    prevVec.Normalize();
                    prevVec = prevVec + strokeVec;
                    prevVec.Normalize();

                    prevVec = prevVec.Rotate(90);
                    a = curPt.Position + prevVec * curPt.Radius;
                    b = curPt.Position - prevVec * curPt.Radius;
                }
                else
                {
                    a = a - strokeVec * curPt.Radius;
                    b = b - strokeVec * curPt.Radius;
                }

                Vector2 c = Vector2.Zero;
                Vector2 d = Vector2.Zero;

                // continuation
                bool doRegular = false;
                bool regularIsCHoke = false;
                if (i < stroke.Count - 2)
                {
                    StrokePoint nextNextPt = stroke[i + 2];
                    var nextVec = nextNextPt.Position - nextPt.Position;
                    nextVec.Normalize();
                    if (Vector2.Dot(nextVec, strokeVec) > 0.3f)
                    {
                        nextVec = nextVec + strokeVec;
                        nextVec.Normalize();
                        var nextOfs = nextVec.Rotate(90);
                        c = nextPt.Position + nextOfs * nextPt.Radius;
                        d = nextPt.Position - nextOfs * nextPt.Radius;
                    }
                    else
                    {
                        doRegular = true;
                        regularIsCHoke = true;
                        Flush();
                    }
                }
                else
                    doRegular = true;
                
                if (doRegular)
                {
                    c = nextPt.Position + strokeOffset*nextPt.Radius;
                    d = nextPt.Position - strokeOffset*nextPt.Radius;
                    if (!regularIsCHoke)
                    {
                        c = c + strokeVec * nextPt.Radius;
                        d = d + strokeVec * nextPt.Radius;
                    }
                    else
                    {
                        a -= strokeVec * curPt.Radius;
                        b -= strokeVec * curPt.Radius;
                    }
                }

                AddTriangle(curPt, nextPt, a, b, c);
                AddTriangle(curPt, nextPt, d, c, b);
            }
            Flush();
        }

        void AddTriangle(StrokePoint a, StrokePoint b, Vector2 aa, Vector2 bb, Vector2 cc)
        {
            Reserve(3, 3);

            Indices[IndexCount++] = (ushort)VertexCount;
            Indices[IndexCount++] = (ushort)(VertexCount + 1);
            Indices[IndexCount++] = (ushort)(VertexCount + 2);

            StrokeVertex vA = new StrokeVertex
            {
                Position = new Vector3(aa.X, aa.Y, 0.0f),
                //Position = aa.XYY(),
                Start = a.Position.XYY(),
                End = b.Position.XYY(),
                Color = a.Color,
                StartSize = new Vector2(a.Radius, a.Radius),
                EndSize = new Vector2(b.Radius, b.Radius),
            };

            StrokeVertex vB = new StrokeVertex
            {
                Position = new Vector3(bb.X, bb.Y, 0.0f),
                //Position = bb.XYY(),
                Start = a.Position.XYY(),
                End = b.Position.XYY(),
                Color = a.Color,
                StartSize = new Vector2(a.Radius, a.Radius),
                EndSize = new Vector2(b.Radius, b.Radius),
            };

            StrokeVertex vC = new StrokeVertex
            {
                Position = new Vector3(cc.X, cc.Y, 0.0f),
                //Position = cc.XYY(),
                Start = a.Position.XYY(),
                End = b.Position.XYY(),
                Color = a.Color,
                StartSize = new Vector2(a.Radius, a.Radius),
                EndSize = new Vector2(b.Radius, b.Radius),
            };

            Vertices[VertexCount++] = vA;
            Vertices[VertexCount++] = vB;
            Vertices[VertexCount++] = vC;
        }

        private bool Reserve(int numVerts, int numIndices)
        {
            if (numVerts > MAX_VERTS || numIndices > MAX_INDICES)
            {
                // Whatever it is, we can't draw it
                return false;
            }
            if (VertexCount + numVerts > MAX_VERTS || IndexCount + numIndices >= MAX_INDICES)
            {
                // We can draw it, but we need to make room first
                Flush();
            }
            return true;
        }

        public void Begin()
        {

        }

        public void End()
        {
            Flush();
        }

        public static readonly BlendState StrokeBlend = new BlendState()
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Max
        };

        void Flush()
        {
            if (IndexCount > 0)
            {
                vertexBuffer.SetData(Vertices, 0, VertexCount, SetDataOptions.Discard);
                indexBuffer.SetData(Indices, 0, IndexCount, SetDataOptions.Discard);

                GraphicsDevice device = effect_.GraphicsDevice;
                var oldRas = device.RasterizerState;
                var oldDepth = device.DepthStencilState;
                var oldBlend = device.BlendState;
                device.SetVertexBuffer(vertexBuffer);
                device.Indices = indexBuffer;
                device.RasterizerState = RasterizerState.CullNone;
                device.DepthStencilState = DepthStencilState.None;
                device.BlendState = StrokeBlend;
                device.RasterizerState = new RasterizerState { FillMode = FillMode.Solid, CullMode = CullMode.None };
                foreach (EffectPass pass in effect_.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexCount / 3);
                }

                device.SetVertexBuffer(null);
                device.Indices = null;
                device.RasterizerState = oldRas;
                device.DepthStencilState = oldDepth;
                device.BlendState = oldBlend;
            }
            IndexCount = 0;
            VertexCount = 0;
        }
    }
}
