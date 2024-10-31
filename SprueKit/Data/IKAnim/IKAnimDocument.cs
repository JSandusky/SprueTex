using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.IKAnim
{
    public class IKAnimDocumentRecord : DocumentRecord
    {
        public override string DocumentName { get { return "IKAnim"; } }

        public override string OpenFileMask { get { return "IK Anim (*.ikan)|*.ikan"; } }

        public override Document CreateFromTemplate(Uri templateUri)
        {
            return new IKAnimDocument(templateUri, true);
        }

        public override Document CreateNewDocument()
        {
            return new IKAnimDocument();
        }

        public override void DoReport(string value) { }

        public override KeyValuePair<string, string>[] GetSignificantReports() { return null; }

        public override Document OpenDocument(Uri templateUri)
        {
            return new IKAnimDocument(templateUri, false);
        }
    }

    public class IKAnimDocument : Document
    {
        public IKScene ikScene_;
        public IKScene IKScene { get { return ikScene_; } private set { ikScene_ = value; } }

        public IKRig Rig { get; set; }
        MeshData meshData_;
        SprueBindings.ModelData model_;
        public MeshData MeshData { get { return meshData_; } }
        public SprueBindings.ModelData Model { get { return model_; }
            set
            {
                if (model_ == value)
                    return;
                if (model_ != null)
                    model_.Dispose();
                if (meshData_ != null)
                    meshData_.Dispose();
                model_ = value;
                if (model_ != null)
                {
                    meshData_ = BindingUtil.ToMesh(model_);
                    meshData_.Initialize(ikScene_.GraphicsDevice);
                }
            }
        }

        public IKAnimDocument()
        {
            ErrorHandler.inst().Info("Created new IK Anim");

            CommonConstruct(null);
        }

        public IKAnimDocument(Uri filePath, bool isTemplate = false)
        {
            string path = filePath.AbsolutePath;
            if (!isTemplate)
                FileURI = filePath;
            if (string.IsNullOrEmpty(path))
                throw new Exception(string.Format("Cannot open file {0}", filePath));

            CommonConstruct(null);
        }

        void CommonConstruct(Dictionary<string, object> parameters)
        {
            DocumentTypeName = "IK Rig";
            var sceneView = new Graphics.BaseScene();
            IKScene = new IKScene(sceneView, Rig, this);

            sceneView.ActiveViewport = IKScene;

            Controls.ContentControl = sceneView;
            Controls.Disposables.Add(sceneView);

            DocumentCommands = new Commands.CommandInfo[]
            {
                new Commands.CommandInfo { Name = "Open Model", Action = (doc, obj) => {
                    this.OpenFBX();
                }, ToolTip = "Open a model for rig setup", Icon = WPFExt.GetEmbeddedImage("Images/godot/icon_folder.png") },
                new Commands.CommandInfo { Name = "Save", Action = (doc, obj) => {
                    this.Save();
                }, ToolTip = "Save the current IK rig", Icon = WPFExt.GetEmbeddedImage("Images/save_white.png") },
            };
        }

        void OpenFBX()
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "FBX Model (*.fbx)|*.fbx";
            dlg.FilterIndex = 0;
            dlg.AddExtension = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = dlg.FileName;
                if (Model != null)
                    Model.Dispose();
                Model = SprueBindings.ModelData.LoadFBX(path, ErrorHandler.inst());
            }
        }
    }
}
