using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprueKit.Tasks;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace SprueKit.Data.Sprue
{
    public class BoneWeightTask : TaskItem
    {
        SprueModel targetModel_;
        SprueBindings.MeshData resultMesh_;

        public override string TaskName { get { return "bone weights"; } }

        public BoneWeightTask(SprueModel targetModel) :
            base(null)
        {
            targetModel_ = targetModel;
        }

        public override void TaskLaunch()
        {
            if (targetModel_ != null && targetModel_.MeshData != null && targetModel_.MeshData.Skeleton != null)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();

                // Run smoothing pass
                SprueBindings.MeshData data = BindingUtil.ToMeshData(targetModel_.MeshData);
                PushDisposable(data);

                SprueBindings.RCurve meshCurve = new SprueBindings.RCurve
                {
                    CurveShape = (int)targetModel_.BoneWeightsCurve.CurveShape,
                    x = targetModel_.BoneWeightsCurve.XIntercept,
                    y = targetModel_.BoneWeightsCurve.YIntercept,
                    slope = targetModel_.BoneWeightsCurve.SlopeIntercept,
                    exp = targetModel_.BoneWeightsCurve.Exponent,
                    flipX = targetModel_.BoneWeightsCurve.FlipX,
                    flipY = targetModel_.BoneWeightsCurve.FlipY,
                };
                SprueBindings.BoneWeightCancel cancelFunc = () => { return !IsCanceled; };
                if (SprueBindings.LaplaceProcessing.CalculateBoneWeights(data, meshCurve, cancelFunc))
                {
                    data.ReadFromAPI();
                    resultMesh_ = data;
                }

                timer.Stop();
                ErrorHandler.inst().Debug(string.Format("Calculated bone weights in: {0}", timer.Elapsed.ToString()));
            }
        }

        public override void TaskEnd()
        {
            if (targetModel_ != null && targetModel_.MeshData != null && resultMesh_ != null)
            {
                var old = targetModel_.MeshData;
                List<int> indices = new List<int>();
                List<PluginLib.VertexData> verts = new List<PluginLib.VertexData>();
                Data.BindingUtil.ToMesh(resultMesh_, ref indices, ref verts);
                targetModel_.MeshData = new MeshData(indices, verts);
                targetModel_.MeshData.Skeleton = old.Skeleton;
                old.Dispose();
            }
        }
    }
}
