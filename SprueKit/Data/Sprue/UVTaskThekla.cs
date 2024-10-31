using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SprueKit.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.Sprue
{
    public class UVTaskThekla : TaskItem
    {
        public override string TaskName { get { return "UV mapping"; } }

        internal MeshData resultMeshData;
        internal SprueModel targetModel;
        internal UVSettings settings;

        public UVTaskThekla(SprueModel targetModel, UVSettings settings) :
            base(null)
        {
            this.targetModel = targetModel;
            this.settings = settings;
        }

        int Canceled(float td)
        {
            return IsCanceled ? 1 : 0;
        }

        public override void TaskLaunch()
        {
            if (IsCanceled)
                return;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            ErrorHandler.inst().Debug("Starting UV Charting");

            // Do we have nothing to do?
            if (targetModel == null || targetModel.MeshData == null)
                return;

            var targetMeshData = targetModel.MeshData;
            this.settings.Width = targetModel.TextureSize.X;
            this.settings.Height = targetModel.TextureSize.Y;

            try
            {
                List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
                var meshVertices = targetMeshData.GetVertices();
                for (int i = 0; i < targetMeshData.VertexCount; ++i)
                    vertices.Add(new VertexPositionNormalTexture(meshVertices[i].Position, meshVertices[i].Normal, Vector2.Zero));

                var meshIndices = targetMeshData.GetIndices();
                Thekla.AtlasOutput mesh = Thekla.AtlasBuilder.ComputeTextureCoordinates(vertices, meshIndices);
                if (mesh != null)
                {
                    List<PluginLib.VertexData> outVerts = new List<PluginLib.VertexData>();
                    for (int i = 0; i < mesh.vertices.Length; ++i)
                        outVerts.Add(new PluginLib.VertexData(mesh.vertices[i].Position, mesh.vertices[i].Normal, mesh.vertices[i].TextureCoordinate));
                    resultMeshData = new MeshData(new List<int>(mesh.indices), outVerts);
                }
            }
            catch (Exception ex)
            {
                resultMeshData = null;
            }

            //SprueBindings.UVCallback cancelCallback = (float f) => { return this.IsCanceled ? unchecked((int)(0x80004004)) : (int)0x00000000; };
            //if (data.ComputeUVCoordinates(settings.Width, settings.Height, 1, settings.Stretch, settings.Gutter, cancelCallback))
            //{
            //    data.ReadFromAPI();
            //
            //    List<int> indices = new List<int>(data.Indices);
            //    List<PluginLib.VertexData> newVerts = new List<PluginLib.VertexData>();
            //    for (int i = 0; i < data.Positions.Length; ++i)
            //        newVerts.Add(new PluginLib.VertexData(data.Positions[i], data.Normals[i], data.UVCoordinates[i]));
            //
            //    resultMeshData = new MeshData(indices, newVerts);
            //}

            timer.Stop();
            if (resultMeshData != null)
                ErrorHandler.inst().Debug(string.Format("Generated UV coordinates in {0}", timer.Elapsed.ToString()));
            else if (!IsCanceled)
                ErrorHandler.inst().Error(string.Format("Failed to generate UV coordinates in {0}", timer.Elapsed.ToString()));
        }

        public override void TaskEnd()
        {
            base.TaskEnd();
            if (resultMeshData != null)
                targetModel.MeshData = resultMeshData;
        }
    }
}
