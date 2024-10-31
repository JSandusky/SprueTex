using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using SprueKit.Data.Graph;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Matrix = Microsoft.Xna.Framework.Matrix;
using IntVector2 = PluginLib.IntVector2;
using Mat3x3 = PluginLib.Mat3x3;
using Quaternion = Microsoft.Xna.Framework.Quaternion;

namespace SprueKit.Data.TexGen
{
    [Description("Adjusts the outward power of an input normal map")]
    public partial class NormalPower : TexGenNode
    {
        float power_ = 0.75f;
        [Description("Intensity to modulate the normal")]
        public float Power { get { return power_; } set { power_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Normal Power";
            AddInput(new GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Channel });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel });
        }

        public override void Execute(object param)
        {
            Vector3 inColor = InputSockets[0].GetColor().ToVector3();
            Vector3 vec = (inColor - new Vector3(0.5f)) * 2;
            vec.Z *= (1.0f / Power);
            inColor.Normalize();
            OutputSockets[0].Data = new Vector4(vec.X * 0.5f + 0.5f, vec.Y * 0.5f + 0.5f, vec.Z * 0.5f + 0.5f, 1.0f);
        }
    }

    [Description("Rotates the values of a normal map")]
    public partial class RotateNormals : TexGenNode
    {

        Vector3 rotation_;
        [Description("Rotation about each axis to apply, this is relative to the existing tangent space direction")]
        public Vector3 Rotation { get { return rotation_; } set { rotation_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Rotate Normals";
            AddInput(new GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Channel });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel });
        }

        public override void Execute(object param)
        {
            Vector3 inColor = InputSockets[0].GetColor().ToVector3();
            Vector3 vec = (inColor - new Vector3(0.5f)) * 2;
            Quaternion rotQuat = Quaternion.CreateFromAxisAngle(Vector3.UnitX, Rotation.X) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, Rotation.Y) * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, Rotation.Z);
            vec = Vector3.Transform(vec, rotQuat);
            vec.Normalize();
            OutputSockets[0].Data = new Vector4(vec.X * 0.5f + 0.5f, vec.Y * 0.5f + 0.5f, vec.Z * 0.5f + 0.5f, 1.0f);
        }
    }

    [Description("Outputs the dot-product of the normal map and the outward vector")]
    [PropertyData.NoPreviews]
    public partial class NormalMapDeviation : TexGenNode
    {
        public NormalMapDeviation() { }
        public override void Construct()
        {
            base.Construct();
            Name = "Normal Map Dev.";
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Color, Name = "In" });
            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Grayscale, Name = "Out", IsInput = false, IsOutput = true });
        }
        public override void Execute(object param)
        {
            Vector3 inColor = InputSockets[0].GetColor().ToVector3();
            Vector3 vec = (inColor - new Vector3(0.5f)) * 2;
            float deviation = Mathf.Abs(Vector3.Dot(vec, new Vector3(0, 0, 1)));
            OutputSockets[0].Data = deviation;
        }
    }

    [Description("Treats inputs as an RGB vector and outputs the normalized RGB vector")]
    [PropertyData.NoPreviews]
    public partial class NormalMapNormalize : TexGenNode
    {
        public NormalMapNormalize() { }
        public override void Construct()
        {
            base.Construct();
            Name = "Normal Map Norm.";
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Color, Name = "In" });
            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Color, Name = "Out", IsInput = false, IsOutput = true });
        }
        public override void Execute(object param)
        {
            Vector3 inColor = InputSockets[0].GetColor().ToVector3();
            inColor = (inColor - new Vector3(0.5f)) * 2;
            inColor.Normalize();
            OutputSockets[0].Data = new Vector4(inColor.X * 0.5f + 0.5f, inColor.Y * 0.5f + 0.5f, inColor.Z * 0.5f + 0.5f, 1.0f);
        }
    }

    [Description("Converts a grayscale heightfield into a normal map")]
    public partial class ToNormalMap : TexGenNode
    {
        float stepSize_ = 0.2f;
        float power_ = 1.0f;

        [PropertyData.AllowPermutations]
        [Description("Fraction of the image dimensions to step when sampling the height field")]
        [PropertyData.ValidStep(Value = 0.5f)]
        public float StepSize { get { return stepSize_; } set { stepSize_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("How intensely the outward vector should be biased, low values increase angular sharpness and high values smooth out the normal map")]
        [PropertyData.ValidStep(Value = -0.2f)]
        public float Power { get { return power_; } set { power_ = value; OnPropertyChanged(); } }

        public override bool WillForceExecute() { return true; }

        public ToNormalMap() { }
        public override void Construct()
        {
            base.Construct();
            Name = "To Normal Map";
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Grayscale, Name = "In" });
            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Color, Name = "Out", IsInput = false, IsOutput = true });
        }
        public override void Execute(object param)
        {
            Vector4 pos = (Vector4)param;
            float stepScale = CalculateStepSize(StepSize, pos);

            float l = SampleHeightMap(pos, -1, 0, stepScale);
            float r = SampleHeightMap(pos, 1, 0, stepScale);
            float t = SampleHeightMap(pos, 0, -1, stepScale);
            float b = SampleHeightMap(pos, 0, 1, stepScale);
            float tl= SampleHeightMap(pos,  -1, -1, stepScale);
            float tr= SampleHeightMap(pos,  1, -1, stepScale);
            float bl= SampleHeightMap(pos,  -1, 1, stepScale);
            float br= SampleHeightMap(pos,  1, 1, stepScale);

            float dX = tr + 2 * r + br - tl - 2 * l - bl;
            float dY = bl + 2 * b + br - tl - 2 * t - tr;
            Vector3 normal = new Vector3(-dX, -dY, Power);
            normal.Normalize();
            normal = (normal * 0.5f) + new Vector3(0.5f);

            OutputSockets[0].Data = new Vector4(normal.X, normal.Y, normal.Z, 1.0f);
        }

        float SampleHeightMap(Vector4 pos, float X, float Y, float stepScale)
        {
            ForceExecuteUpstream(pos + new Vector4(X * stepScale, Y * stepScale, 0, 0));
            return InputSockets[0].GetFloatData();
        }
    }
}
