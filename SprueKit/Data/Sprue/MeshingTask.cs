using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SprueKit.Data.Sprue
{
    public class MeshingTask : SprueKit.Tasks.TaskItem
    {
        public override string TaskName { get { return "CPU mesh voxelization"; } }

        static CancellationTokenSource source;

        MeshData meshedData;
        SprueModel target;

        public MeshingTask(Data.SprueModelDocument document, Data.SprueModel targetModel, Data.IHaveDensity meshable) :
            base(null)
        {
            target = targetModel;
            if (source != null)
                source.Cancel();
            source = new CancellationTokenSource();
            cancelationToken = source.Token;

            Consequences.Add(new UVTask(target, new Sprue.UVSettings()) { cancelationToken = this.cancelationToken });
            Consequences.Add(new SkeletonBuilderTask(target) { cancelationToken = this.cancelationToken });
            Consequences.Add(new BoneWeightTask(target) { cancelationToken = this.cancelationToken });
            Consequences.Add(new FinalizationTask(document) { cancelationToken = this.cancelationToken });
        }

        public override void TaskLaunch()
        {
            base.TaskLaunch();
            if (IsCanceled)
                return;

            // Main meshing
            var meshResult = (new SprueKit.Data.Processing.NaiveSurfaceNets(target, new Microsoft.Xna.Framework.Vector3(-32, -8, -32))).GetMeshData(null);

            if (IsCanceled)
            {
                meshedData = meshResult;
                return;
            }

            // Run smoothing pass
            SprueBindings.MeshData data = new SprueBindings.MeshData
            {
                Positions = new Vector3[meshResult.VertexCount],
                Normals = new Vector3[meshResult.VertexCount],
                Indices = new int[meshResult.IndexCount]
            };
            PushDisposable(data);

            var vertices = meshResult.GetVertices();
            var indices = meshResult.GetIndices();
            for (int i = 0; i < data.Positions.Length; ++i)
                data.Positions[i] = vertices[i].Position;
            for (int i = 0; i < data.Normals.Length; ++i)
                data.Normals[i] = vertices[i].Normal;
            for (int i = 0; i < data.Indices.Length; ++i)
                data.Indices[i] = indices[i];

            data.WriteToAPI();
            //data.Subdivide(true);
            data.Smooth(1.0f);
            data.ReadFromAPI();
            
            List<PluginLib.VertexData> newVerts = new List<PluginLib.VertexData>();
            for (int i = 0; i < data.Positions.Length; ++i)
                newVerts.Add(new PluginLib.VertexData(data.Positions[i], data.Normals[i], new Vector2()));

            meshResult.SetData(data.Indices.ToList(), newVerts);
            meshedData = meshResult;
        }

        public override void CanceledEnd()
        {
            if (meshedData != null)
                target.MeshData = meshedData;
            else
                target.MeshData = null;
        }

        public override void TaskEnd()
        {
            base.TaskEnd();
            target.MeshData = meshedData;
        }
    }
}
