using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprueKit.Tasks;
using Microsoft.Xna.Framework;

namespace SprueKit.Data.Sprue
{
    public class CSGTask : TaskItem
    {
        public override string TaskName { get { return "c. solid geometry"; } }

        SprueModel target_;
        MeshData resultMesh_;

        public CSGTask(SprueModel target) : base(null)
        {
            target_ = target;
        }

        public override void TaskLaunch()
        {
            if (IsCanceled)
                return;

            if (target_ == null || target_.MeshData == null)
                return;

            List<SprueBindings.CSGOperand> csgOperands = new List<SprueBindings.CSGOperand>();
            csgOperands.Add(new SprueBindings.CSGOperand { Geometry = BindingUtil.ToMeshData(target_.MeshData), Task = SprueBindings.CSGTask.CSG_Add, Transform = Matrix.Identity });

            target_.VisitAll((SpruePiece piece) =>
            {
                if (piece is ModelPiece)
                {
                    ModelPiece mdl = piece as ModelPiece;
                    var meshes = mdl.GetMeshes();
                    if (meshes == null || meshes.Count == 0)
                        return;
                    switch (mdl.Combine)
                    {
                        case SprueBindings.CSGTask.Merge:
                        case SprueBindings.CSGTask.MergeSmoothNormals:
                        case SprueBindings.CSGTask.ClipIndependent:
                        case SprueBindings.CSGTask.CSG_Add:
                        case SprueBindings.CSGTask.CSG_Subtract:
                        case SprueBindings.CSGTask.CSG_Clip:
                            foreach (var mesh in meshes)
                                csgOperands.Add(new SprueBindings.CSGOperand { Geometry = BindingUtil.ToMeshData(mesh), Task = mdl.Combine, Transform = mdl.Transform });
                            break;
                    }
                }
            });

            if (csgOperands.Count <= 1)
                return;

            ErrorHandler.inst().Info("Processing CSG operands");
            SprueBindings.CSGProcessor csgProc = new SprueBindings.CSGProcessor();
            var resultMesh = csgProc.ProcessUnions(csgOperands, ErrorHandler.inst());
            if (resultMesh != null)
                resultMesh_ = BindingUtil.ToMesh(resultMesh);
        }

        public override void TaskEnd()
        {
            if (target_ != null && resultMesh_ != null)
                target_.MeshData = resultMesh_;
        }
    }
}
