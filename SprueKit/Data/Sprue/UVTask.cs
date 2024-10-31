using System;
using System.Collections.Generic;
using SprueKit.Tasks;
using SprueKit.Settings;
using System.Diagnostics;

namespace SprueKit.Data.Sprue
{
    public class UVSettings
    {
        public int Width { get; set; } = 1024;
        public int Height { get; set; } = 1024;
        public UVQuality Quality { get; set; } = UVQuality.Automatic;
        public float Stretch { get; set; } = 0.5f;
        public float Gutter { get; set; } = 4.0f;
    }

    public class UVTask : TaskItem
    {
        public override string TaskName { get { return "UV mapping"; } }

        internal MeshData resultMeshData;
        internal SprueModel targetModel;
        internal UVSettings settings;

        public UVTask(SprueModel targetModel, UVSettings settings) :
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
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // Do we have nothing to do?
            if (targetModel == null || targetModel.MeshData == null)
                return;

            //try
            //{
            //    Thekla.AtlasOutput mesh = Thekla.AtlasBuilder.ComputeTextureCoordinates(targetMeshData.vertices, targetMeshData.indices);
            //    if (mesh != null)
            //        resultMeshData = new MeshData(new List<int>(mesh.indices), new List<VertexPositionNormalTexture>(mesh.vertices));
            //} catch (Exception ex)
            //{
            //    resultMeshData = null;
            //}

            var targetMeshData = targetModel.MeshData;
            // Run smoothing pass
            SprueBindings.MeshData data = BindingUtil.ToMeshData(targetMeshData);
            PushDisposable(data);

            ErrorHandler.inst().Debug("Starting UV Charting");
            data.WriteToAPI();
            
            SprueBindings.UVCallback cancelCallback = (float f) => { return this.IsCanceled ? unchecked((int)(0x80004004)) : (int)0x00000000; };
            if (data.ComputeUVCoordinates(settings.Width, settings.Height, 1, settings.Stretch, settings.Gutter, cancelCallback, ErrorHandler.inst()))
            {
                data.ReadFromAPI();
            
                List<int> indices = new List<int>(data.Indices);
                List<PluginLib.VertexData> newVerts = new List<PluginLib.VertexData>();
                for (int i = 0; i < data.Positions.Length; ++i)
                    newVerts.Add(new PluginLib.VertexData(data.Positions[i], data.Normals[i], data.UVCoordinates[i]));
            
                resultMeshData = new MeshData(indices, newVerts);
            }

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
