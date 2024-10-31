using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.Sculpt
{
    public enum SculptBrushTypes
    {
        Sculpt,
        Smooth,
        Flatten,
        Pinch,
        Noise,
        Drag,
        Inflate,
        Twist,
        Mask,
        Paint,
    }

    public class SculptBrush : BaseClass
    {
        SculptBrushTypes brushType_ = SculptBrushTypes.Sculpt;
        public SculptBrushTypes Brush { get { return brushType_; } set { brushType_ = value; OnPropertyChanged(); } }

        float radius_ = 1.0f;
        public float Radius { get { return radius_; } set { radius_ = value;  OnPropertyChanged(); } }

        ResponseCurve falloffCurve_ = new ResponseCurve();
        public ResponseCurve FallOff { get { return falloffCurve_; } set { falloffCurve_ = value; OnPropertyChanged(); } }
    }

    public class MeshMask
    {
        public HashSet<int> Indices { get; set; }
    }

    public class SculptDocument : Document
    {
        public GenericTreeObject Root { get; private set; } = new GenericTreeObject();
        public GenericTreeObject MeshesTree { get; private set; }
        public GenericTreeObject BonesTree { get; private set; }

        public SculptDocument()
        {
            CommonConstruct();
        }

        void CommonConstruct()
        {
            DocumentTypeName = "Sculpt";
            var sceneView = new Graphics.BaseScene();

            Root.Children.Add(MeshesTree = new GenericTreeObject() { DataObject = "Meshes" });
            Root.Children.Add(BonesTree = new GenericTreeObject() { DataObject = "Bones" });

            Graphics.Sculpt.SculptingView sculptView = new Graphics.Sculpt.SculptingView(sceneView);
            sceneView.ActiveViewport = sculptView;

            SprueKit.Controls.SculptPaint.ModelTree modelTree = new SprueKit.Controls.SculptPaint.ModelTree(this);
            modelTree.DataContext = Root;

            Controls.LeftPanelControl = modelTree;
            Controls.ContentControl = sceneView;
        }

        public override bool WriteFile(Uri path)
        {
            throw new NotImplementedException();
        }
    }
}
