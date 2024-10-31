using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Graphics.Materials
{
    public enum UVRenderMode
    {
        Wireframe,
        Normals,
        Textured
       //?? Positions
    }

    public interface IUVEffect
    {
        Vector4 OffsetScale { get; set; }
        Matrix WorldView { get; set; }
        Matrix Transform { get; set; }

        void Begin(GraphicsDevice device);
        void End(GraphicsDevice device);
    }

    /// <summary>
    /// Renders the normals in UV space
    /// </summary>
    public class UVNormalsEffect : Effect, IUVEffect
    {
        public UVNormalsEffect(GraphicsDevice graphicsDevice, ContentManager content) : base(content.Load<Effect>("Effects/UV_NormalShader"))
        {
            CurrentTechnique = Techniques[0];
        }        

        public Vector4 OffsetScale { get; set; }
        public Matrix WorldView { get; set; }
        public Matrix Transform { get; set; } = Matrix.Identity;

        protected override void OnApply()
        {
            Parameters["OffsetScale"].SetValue(OffsetScale);
            //Parameters["Transform"].SetValue(Transform);
            Parameters["WorldViewProjection"].SetValue(WorldView);
        }

        RasterizerState oldState_;
        RasterizerState state_;
        public void Begin(GraphicsDevice device)
        {
            oldState_ = device.RasterizerState;
            if (state_ == null)
                state_ = new RasterizerState { CullMode = CullMode.None };
            device.RasterizerState = state_;
        }

        public void End(GraphicsDevice device)
        {
            device.RasterizerState = oldState_;
        }

        public void Draw(Data.MeshData mesh)
        {
            if (mesh.IndexCount > 0 && mesh.VertexCount > 0)
            {
                if (mesh.VertexBuffer == null)
                    mesh.Initialize(GraphicsDevice);

                GraphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
                GraphicsDevice.Indices = mesh.IndexBuffer;
                foreach (EffectPass pass in CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mesh.IndexBuffer.IndexCount / 3);
                }

                GraphicsDevice.SetVertexBuffer(null);
                GraphicsDevice.Indices = null;
            }
        }
    }

    public class UVWireframeEffect : Effect, IUVEffect
    {
        public UVWireframeEffect(GraphicsDevice graphicsDevice, ContentManager content) : base(content.Load<Effect>("Effects/UV_WireframeShader"))
        {
            CurrentTechnique = Techniques[0];
        }

        public Vector4 OffsetScale { get; set; } = new Vector4(0, 0, 1, 1);
        public Matrix WorldView { get; set; }
        public Matrix Transform { get; set; } = Matrix.Identity;

        protected override void OnApply()
        {
            Parameters["OffsetScale"].SetValue(OffsetScale);
            Parameters["WorldViewProjection"].SetValue(WorldView);
            Parameters["Transform"].SetValue(Transform);
        }

        RasterizerState oldState_;
        RasterizerState state_;
        public void Begin(GraphicsDevice device)
        {
            oldState_ = device.RasterizerState;
            if (state_ == null)
                state_ = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
            device.RasterizerState = state_;
        }

        public void End(GraphicsDevice device)
        {
            device.RasterizerState = oldState_;
        }

        public void Draw(Data.MeshData mesh)
        {
            if (mesh.IndexCount > 0 && mesh.VertexCount > 0)
            {
                if (mesh.VertexBuffer == null)
                    mesh.Initialize(GraphicsDevice);

                GraphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
                GraphicsDevice.Indices = mesh.IndexBuffer;
                foreach (EffectPass pass in CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mesh.IndexBuffer.IndexCount / 3);
                }

                GraphicsDevice.SetVertexBuffer(null);
                GraphicsDevice.Indices = null;
            }
        }
    }
}
