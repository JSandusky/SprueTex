using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SprueKit.Data.Graph;
using Bounds = Microsoft.Xna.Framework.BoundingBox;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Color = Microsoft.Xna.Framework.Color;
using Matrix = Microsoft.Xna.Framework.Matrix;
using System.ComponentModel;

namespace SprueKit.Data.TexGen
{
    /// <summary>
    /// Relief is used to bake a scene
    /// </summary>
    public class ReliefBakeNode : Data.TexGen.TexGenNode
    {
        Matrix transform_ = Matrix.Identity;
        Matrix cameraTransform_ = Matrix.Identity;
        Matrix cameraView_ = Matrix.Identity;

        public Matrix ModelTransform { get { return transform_; } set { transform_ = value; OnPropertyChanged(); } }
        public Matrix CameraTransform { get { return cameraTransform_; } set { cameraTransform_ = value; OnPropertyChanged(); } }
        public Matrix ViewMatrix { get { return cameraView_; } set { cameraView_ = value; OnPropertyChanged(); } }

        [PropertyData.GUIMethod(Name = "Make Camera Perspective")]
        public void SetPerspective(float fov)
        {
            ViewMatrix = Matrix.CreatePerspectiveFieldOfView(fov, 1.0f, 0.0f, 500.0f);
        }

        [PropertyData.GUIMethod(Name = "Make Camera Orthographic")]
        public void SetOrthographic(float dim)
        {
            ViewMatrix = Matrix.CreateOrthographic(dim, dim, 0.0f, 500.0f);
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Relief";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Model", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Model });

            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Depth", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Normal", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Color });
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
        }

        public override void Execute(object param)
        {
            
        }
    }

    public enum BakeViewAngle
    {
        Front,
        Left,
        Right,
        Back,
        Top,
        Bottom,
        ThreeQuarter
    }

    [Description("Generates a depth and normal map from a render of a model")]
    public partial class SimpleReliefNode : Data.TexGen.TexGenNode
    {
        PluginLib.IntVector2 bakeSize_ = new PluginLib.IntVector2(256, 256);
        BakeViewAngle angle_ = BakeViewAngle.Left;

        public PluginLib.IntVector2 RenderSize { get { return bakeSize_; } set { bakeSize_ = value; OnPropertyChanged(); } }
        public BakeViewAngle Angle { get { return angle_; } set { angle_ = value; OnPropertyChanged(); } }

        Vector3 GetOffsetDir()
        {
            switch (Angle)
            {
            case BakeViewAngle.Front:
                return new Vector3(0, 0, 1.0f);
            case BakeViewAngle.Back:
                return new Vector3(0, 0, -1.0f);
            case BakeViewAngle.Left:
                return new Vector3(-1, 0, 0);
            case BakeViewAngle.Right:
                return new Vector3(1, 0, 0);
            case BakeViewAngle.Top:
                return new Vector3(0, -1, 0);
            case BakeViewAngle.Bottom:
                return new Vector3(0, 1, 0);
            }
            return Vector3.Normalize(new Vector3(1, 1, 0.75f));
        }

        Vector3 GetUpDir()
        {
            switch (Angle)
            {
            case BakeViewAngle.Top:
                return Vector3.UnitZ;
            case BakeViewAngle.Bottom:
                return Vector3.UnitZ;
            }
            return Vector3.UnitY;
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Simple Relief Map";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Model", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Model });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Normal", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Color });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Tangent", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Color });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Depth", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Reverse Depth", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
        }

        float[] dataCache_;
        System.Drawing.Bitmap cache_;

        ~SimpleReliefNode()
        {
            if (cache_ != null)
                cache_.Dispose();
        }

        public override bool WillForceExecute()
        {
            return true;
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);

            ForceExecuteSocketUpstream(null, InputSockets[0]);

            if (InputSockets[0].Data != null)
            {
                var meshes = InputSockets[0].Data as List<MeshData>;
                if (meshes != null && meshes.Count > 0)
                {
                    Bounds? bounds = null;
                    for (int i = 0; i < meshes.Count; ++i)
                    {
                        if (!bounds.HasValue)
                            bounds = meshes[i].Bounds;
                        else
                            bounds.Value.Extend(meshes[i].Bounds);
                    }

                    SprueKit.SoftwareRenderer rdr = new SoftwareRenderer(Math.Max(1, RenderSize.X), Math.Max(1, RenderSize.Y), 45.0f);
                    rdr.FocusOnBounds(bounds.Value, GetOffsetDir());
                    for (int i = 0; i < meshes.Count; ++i)
                        rdr.RasterizeTriangles(meshes[i].GetIndices(), meshes[i].GetVertices(), true);
                    cache_ = rdr.GetDepthImage();
                    dataCache_ = rdr.Data;

                    float minDepth = float.MaxValue;
                    float maxDepth = float.MinValue;
                    for (int i = 0; i < cache_.Width * cache_.Height; ++i)
                    {
                        if (dataCache_[i] != float.MaxValue)
                        {
                            minDepth = Math.Min(minDepth, dataCache_[i]);
                            maxDepth = Math.Max(maxDepth, dataCache_[i]);
                        }
                    }
                    for (int i = 0; i < cache_.Width * cache_.Height; ++i)
                        dataCache_[i] = Mathf.Normalize(Mathf.Clamp(dataCache_[i], minDepth, maxDepth), minDepth, maxDepth);
                }
            }
        }

        public override void Execute(object param)
        {
            var coord = (Vector4)param;
            if (dataCache_ != null && cache_ != null)
            {
                var data = cache_.GetPixelBilinear(coord.X, coord.Y).ToXNAColor();
                var asNorm = Vector3.Normalize(data.ToNormal());

                var upDir = GetUpDir();
                var rightDir = Vector3.Cross(GetOffsetDir(), upDir);
                float x = Vector3.Dot(asNorm, rightDir);
                float y = Vector3.Dot(asNorm, upDir);
                asNorm.X = x;
                asNorm.Y = y;
                asNorm.Z = 1.0f;
                asNorm.Normalize();

                float val = dataCache_.GetBilinear(coord.X, coord.Y, cache_.Width, cache_.Height);

                OutputSockets[0].Data = data;
                OutputSockets[1].Data = data != Color.TransparentBlack ? asNorm.ToColor() : new Color(128,128,255);
                OutputSockets[2].Data = 1.0f - val;
                OutputSockets[3].Data = val;
            }
        }
    }

    public partial class TexturedReliefNode : TexGenNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Textured Relief";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Model", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Model });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Texture", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Channel });

            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Color", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Color });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Depth", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Reverse Depth", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
        }

        public override void Execute(object param)
        {
        }
    }
}
