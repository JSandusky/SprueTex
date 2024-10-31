using SprueKit.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.Sprue
{
    public class FinalizationTask : TaskItem
    {
        public override string TaskName { get { return "model finalization"; } }

        SprueModelDocument document_;

        public FinalizationTask(SprueModelDocument document) : base(null)
        {
            document_ = document;
        }

        public override void TaskLaunch()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                // Collect all meshes
                GenericTreeObject meshesTree = new GenericTreeObject { DataObject = "Meshes" };
                document_.DataRoot.VisitChildren<SprueModel>((mdl) =>
                {
                    if (mdl.MeshData != null && mdl.MeshData.VertexCount > 0)
                        meshesTree.Children.Add(mdl);
                });

                document_.DataRoot.VisitAll<ModelPiece>((mdl) =>
                {
                    List<MeshData> meshes = mdl.GetMeshes();
                    if (meshes != null && meshes.Count > 0 && (mdl.Combine == SprueBindings.CSGTask.Independent || mdl.Combine == SprueBindings.CSGTask.ClipIndependent))
                    {
                        meshesTree.Children.Add(mdl);
                        if (mdl.Symmetric != SymmetricAxis.None)
                            meshesTree.Children.Add(mdl);
                    }
                });

                document_.ResultRoot.Children.Clear();
                document_.ResultRoot.Children.Add(meshesTree);
                if (document_.DataRoot.MeshData != null && document_.DataRoot.MeshData.Skeleton != null)
                    document_.ResultRoot.Children.Add(document_.DataRoot.MeshData.Skeleton);
            }));
            // Collect all skeletons

            ErrorHandler.inst().Info("Mesh finalized");
        }
    }
}
