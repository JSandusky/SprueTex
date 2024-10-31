using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SprueKit.Data;

namespace SprueKit.Controls.SculptPaint
{
    public class ModelTree : GenericTreeControl
    {
        Data.Sculpt.SculptDocument SculptDocument;

        public ModelTree(Document document) : base(document)
        {
            SculptDocument = document as Data.Sculpt.SculptDocument;
            Loaded += ModelTree_Loaded;
        }

        private void ModelTree_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ContextMenu = new System.Windows.Controls.ContextMenu();

            MenuItem addMeshItem = new MenuItem();
            addMeshItem.Header = "Add mesh";
            addMeshItem.Click += AddMeshItem_Click;

            ContextMenu.Items.Add(addMeshItem);
        }

        private void AddMeshItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = Data.FileData.ModelFileMask;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (System.IO.File.Exists(dlg.FileName))
                {
                    var modelData = SprueBindings.ModelData.LoadModel(dlg.FileName, ErrorHandler.inst());
                    if (modelData == null)
                    {
                        ErrorHandler.inst().Error(string.Format("Unable to load model: {0}", dlg.FileName));
                        return;
                    }
                    GenericTreeObject modelRoot = new GenericTreeObject() { DataObject = modelData };
                    foreach (var mesh in modelData.Meshes)
                        modelRoot.Children.Add(new GenericTreeObject() { DataObject = mesh });
                    SculptDocument.MeshesTree.Children.Add(modelRoot);
                }
            }
        }
    }
}
