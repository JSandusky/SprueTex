using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexData : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Tangent; //W contains handedness
        public Vector2 TextureCoordinate;
        public Vector4 BoneWeights;
        public Vector4 BoneIndices;

        public static readonly VertexDeclaration VertexDeclaration;

        public void SetPosition(Vector3 pos)
        {
            Position = pos;
        }

        public VertexData(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
        {
            this.Position = position;
            this.Normal = normal;
            this.TextureCoordinate = textureCoordinate;
            this.Tangent = new Vector4();

            this.BoneWeights = new Vector4();
            this.BoneIndices = new Vector4(-1, -1, -1, -1);
        }

        public VertexData(Vector3 position, Vector3 normal, Vector4 tangent, Vector2 textureCoordinate) : this(position, normal, textureCoordinate)
        {
            this.Tangent = tangent;
        }

        public VertexData(Vector3 position, Vector3 normal, Vector4 tangent, Vector2 textureCoordinate, Vector4 boneWeights, Vector4 boneIndices) : this(position, normal, tangent, textureCoordinate)
        {
            this.BoneWeights = boneWeights;
            this.BoneIndices = boneIndices;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }
        public override int GetHashCode()
        {
            // TODO: FIc gethashcode
            return 0;
        }

        public override string ToString()
        {
            return string.Format("{{Position:{0} Normal:{1} TextureCoordinate:{2}}}", new object[] { this.Position, this.Normal, this.TextureCoordinate });
        }

        public static bool operator ==(VertexData left, VertexData right)
        {
            return (((left.Position == right.Position) && (left.Normal == right.Normal)) && (left.TextureCoordinate == right.TextureCoordinate));
        }

        public static bool operator !=(VertexData left, VertexData right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != base.GetType())
                return false;
            return (this == ((VertexData)obj));
        }

        static VertexData()
        {
            VertexElement[] elements = new VertexElement[] {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),                  // 12 bytes
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),                   // 12 bytes
                new VertexElement(24, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0),                  // 16 bytes
                new VertexElement(40, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),        // 8 bytes
                new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),              // 16 bytes
                new VertexElement(64, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0)               // 16 bytes
            };
            VertexDeclaration declaration = new VertexDeclaration(elements);
            VertexDeclaration = declaration;
        }
    }
}
