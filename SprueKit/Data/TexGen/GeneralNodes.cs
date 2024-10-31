using System;
using System.ComponentModel;

using SprueKit.Data.Graph;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace SprueKit.Data.TexGen
{
    /// <summary>
    /// Base type for trivial channel in and out type of nodes like basic math
    /// </summary>
    public abstract class ChannelInOutNode : TexGenNode
    {
        public ChannelInOutNode()
        {
        }
        public override void Construct()
        {
            AddInput(new Data.Graph.GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Channel, IsInput = true });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true});
        }
    }

    /// <summary>
    /// Calls Cosine on the input
    /// </summary>
    [Description("Returns the calculation of Cosine on the input")]
    public partial class CosNode : ChannelInOutNode
    {
        public CosNode() {
        }
        public override void Construct() {
            base.Construct();
            Name = "Cos";
        }
        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = new Color(
                (float)Math.Cos(col.X), 
                (float)Math.Cos(col.Y), 
                (float)Math.Cos(col.Z), 
                1.0f);
        }
    }

    /// <summary>
    /// Calls ACosine on the input
    /// </summary>
    [Description("Returns the calculation of Arc-Cosine on the input")]
    public partial class ACosNode : ChannelInOutNode
    {
        public ACosNode() { }
        public override void Construct() { base.Construct(); Name = "ACos"; }
        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = new Color(
                (float)Math.Acos(col.X), 
                (float)Math.Acos(col.Y), 
                (float)Math.Acos(col.Z), 
                1.0f);
        }
    }

    /// <summary>
    /// Calls Sine on the input
    /// </summary>
    [Description("Returns the calculation of Sine on the input")]
    public partial class SinNode : ChannelInOutNode
    {
        public SinNode() { }
        public override void Construct() { base.Construct(); Name = "Sin"; }
        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = new Color(
                (float)Math.Sin(col.X), 
                (float)Math.Sin(col.Y), 
                (float)Math.Sin(col.Z), 
                1.0f);
        }
    }

    /// <summary>
    /// Calls asine on the input
    /// </summary>
    [Description("Returns the calculation of Arc-Sine on the input")]
    public partial class ASinNode : ChannelInOutNode
    {
        public ASinNode() { }
        public override void Construct() { base.Construct(); Name = "ASin"; }
        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = new Color(
                (float)Math.Asin(col.X),
                (float)Math.Asin(col.Y),
                (float)Math.Asin(col.Z),
                1.0f);
        }
    }

    /// <summary>
    /// Call tan on the input
    /// </summary>
    [Description("Returns the calculation of Tangent on the input")]
    public partial class TanNode : ChannelInOutNode
    {
        public TanNode() { }
        public override void Construct() { base.Construct(); Name = "Tan"; }
        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = new Color(
                (float)Math.Tan(col.X),
                (float)Math.Tan(col.Y),
                (float)Math.Tan(col.Z),
                1.0f);
        }
    }

    /// <summary>
    /// Call atan on the input
    /// </summary>
    [Description("Returns the calculation of Arc-Tangent on the input")]
    public partial class ATanNode : ChannelInOutNode
    {
        public ATanNode() { }
        public override void Construct() { base.Construct(); Name = "ATan"; }
        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = new Color(
                (float)Math.Atan(col.X),
                (float)Math.Atan(col.Y),
                (float)Math.Atan(col.Z),
                1.0f);
        }
    }

    /// <summary>
    /// Calls exp on the input
    /// </summary>
    [Description("Returns the exponential of the input, rebased to start at zero")]
    public partial class ExpNode : ChannelInOutNode
    {
        public ExpNode() { }
        public override void Construct() { base.Construct(); Name = "Exp"; }
        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = new Color(
                (float)Math.Exp(col.X) - 1.0f,
                (float)Math.Exp(col.Y) - 1.0f,
                (float)Math.Exp(col.Z) - 1.0f,
                1.0f);
        }
    }

    /// <summary>
    /// Calls pow on the input using the given parameters
    /// </summary>
    [Description("Raises an input by an exponent")]
    public partial class PowNode : ChannelInOutNode
    {
        public PowNode()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Pow";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Exponent", TypeID = SocketTypeID.Channel });
        }

        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor().ToVector4();
            var exp = InputSockets[1].GetColor().ToVector4();
            OutputSockets[0].Data = new Color(
                (float)Math.Pow(col.X, exp.X),
                (float)Math.Pow(col.Y, exp.Y),
                (float)Math.Pow(col.Z, exp.Z),
                1.0f);
        }
    }

    /// <summary>
    /// Calls sqrt on the input
    /// </summary>
    [Description("Outputs the square root of the input")]
    public partial class SqrtNode : ChannelInOutNode
    {
        public SqrtNode() { }
        public override void Construct() { base.Construct(); Name = "Sqrt"; }
        public override void Execute(object param)
        {

            var col = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = new Color(
                (float)Math.Sqrt(col.X),
                (float)Math.Sqrt(col.Y),
                (float)Math.Sqrt(col.Z),
                1.0f);
        }
    }

    /// <summary>
    /// Returns the RGB average of the input
    /// </summary>
    [Description("Outputs the average of the two input channels")]
    public partial class AverageRGBNode : ChannelInOutNode
    {
        public AverageRGBNode() { }
        public override void Construct() { base.Construct(); Name = "Average RGB"; }

        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor();
            int avg = (col.R + col.G + col.B) / 3;
            OutputSockets[0].Data = new Color(avg, avg, avg, 255);
        }
    }

    [Description("Outputs the minimum of two channels")]
    public partial class MinRGBNode : ChannelInOutNode
    {
        public MinRGBNode() { }
        public override void Construct() { base.Construct(); Name = "Min"; }

        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor();
            float val = Math.Min(col.R, Math.Min(col.G, col.B)) / 255.0f;
            OutputSockets[0].Data = val;
        }
    }

    [Description("Outputs the maximum of two channels")]
    public partial class MaxRGBNode : ChannelInOutNode
    {
        public MaxRGBNode() { }
        public override void Construct() { base.Construct(); Name = "Max"; }

        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor();
            float val = Math.Max(col.R, Math.Max(col.G, col.B)) / 255.0f;
            OutputSockets[0].Data = val;
        }
    }

    [Description("Clamps the input to between 0 and 1, only useful in fringe situations")]
    public partial class Clamp01Node : TexGenNode
    {
        public Clamp01Node() { }
        public override void Construct() {
            base.Construct(); Name = "Clamp 0-1";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Channel });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        byte Clamp01(int val) { return (byte)Math.Max(0, Math.Min(val, 255)); }

        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = new Vector4(Mathf.Clamp01(col.X), Mathf.Clamp01(col.Y), Mathf.Clamp01(col.Z), Mathf.Clamp01(col.W));
        }
    }

    [Description("Splits a channel into R, G, B, and A grayscale outputs")]
    public partial class SplitNode : TexGenNode
    {
        public SplitNode() {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Split";
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Color, Name = "In" });

            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Grayscale, Name = "R", IsInput = false, IsOutput = true });
            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Grayscale, Name = "G", IsInput = false, IsOutput = true });
            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Grayscale, Name = "B", IsInput = false, IsOutput = true });
            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Grayscale, Name = "A", IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = col.X;
            OutputSockets[1].Data = col.Y;
            OutputSockets[2].Data = col.Z;
            OutputSockets[3].Data = col.W;
        }
    }

    [Description("Combines 4 grayscale values into an RGBA output")]
    public partial class CombineNode : TexGenNode
    {
        public CombineNode() {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Combine";

            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Grayscale, IsInput = true, IsOutput=false, Name = "R" });
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Grayscale, IsInput = true, IsOutput=false, Name = "G" });
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Grayscale, IsInput = true, IsOutput=false, Name = "B" });
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Grayscale, IsInput = true, IsOutput=false, Name = "A" });

            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Color, Name = "Out", IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            OutputSockets[0].Data = Color.FromNonPremultiplied(new Vector4(
                InputSockets[0].GetFloatData(0.0f),
                InputSockets[1].GetFloatData(0.0f),
                InputSockets[2].GetFloatData(0.0f),
                InputSockets[3].GetFloatData(1.0f)));
        }
    }

    [Description("Returns the luminance of the input channel as grayscale")]
    public partial class BrightnessRGBNode : TexGenNode
    {
        public BrightnessRGBNode()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "RGB Brightness";
            AddInput(new GraphSocket(this) { IsInput = true, Name = "In", TypeID = SocketTypeID.Color });
            AddOutput(new GraphSocket(this) { IsInput = false, IsOutput = true, Name = "Out", TypeID = SocketTypeID.Grayscale });
        }

        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor().ToVector4();
            float val = (color.X * 0.2126f + color.Y * 0.7152f + color.Z * 0.0722f);
            OutputSockets[0].Data = val;
        }
    }

    [Description("Blends two inputs together")]
    public partial class BlendNode : TexGenNode
    {
        PSBlendMode blendMode_ = PSBlendMode.Normal;
        [PropertyData.AllowPermutations]
        [Description("Determines how blending will be performed")]
        public PSBlendMode BlendMode { get { return blendMode_; } set { blendMode_ = value; OnPropertyChanged(); } }
        PSAlphaMode alphaMode_ = PSAlphaMode.UseWeight;
        [PropertyData.AllowPermutations]
        [Description("Determines what value to use for controlling the blend, if source or destination alpha are used then weight is ignored and their alpha channels will be set to 1.0")]
        public PSAlphaMode BlendSource { get { return alphaMode_; } set { alphaMode_ = value;  OnPropertyChanged(); } }

        public BlendNode()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Blend";
            AddInput(new GraphSocket(this) { Name = "Dest", TypeID = SocketTypeID.Channel });
            AddInput(new GraphSocket(this) { Name = "Src", TypeID = SocketTypeID.Channel });
            AddInput(new GraphSocket(this) { Name = "Weight", TypeID = SocketTypeID.Channel });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            var dest = InputSockets[0].GetColor();
            var src = InputSockets[1].GetColor();
            float weight = InputSockets[2].GetFloatData(0.5f);

            var ret = PSBlend.Blend(src.ToVector4(), dest.ToVector4(), weight, blendMode_, alphaMode_);
            OutputSockets[0].Data = ret;
        }
    }

    [Description("Amplifies the brightness with a simple multiplication")]
    public partial class BrightnessNode : ChannelInOutNode
    {
        float power_ = 1.2f;
        [PropertyData.AllowPermutations]
        [Description("Factor to multiply the input by")]
        public float Power { get { return power_; } set { power_ = value; OnPropertyChanged(); } }

        public BrightnessNode() { }
        public override void Construct() { base.Construct(); Name = "Brightness"; }
        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor();
            color.R = (byte)(color.R * power_);
            color.G = (byte)(color.R * power_);
            color.B = (byte)(color.R * power_);
            OutputSockets[0].Data = color;
        }
    }

    [Description("Adjusts the contrast of the input channel")]
    public partial class ContrastNode : ChannelInOutNode
    {
        float power_ = 1.2f;
        [PropertyData.AllowPermutations]
        [Description("Intensity of the value partitioning")]
        public float Power { get { return power_; } set { power_ = value; OnPropertyChanged(); } }
        public ContrastNode() { }
        public override void Construct() { base.Construct(); Name = "Contrast"; }
        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor();
            color.R -= 128;
            color.G -= 128;
            color.B -= 128;
            color *= Power;
            color.R += 128;
            color.G += 128;
            color.B += 128;
            OutputSockets[0].Data = color;
        }
    }

    [Description("Converts the input into Gamma space")]
    public partial class ToGammaNode : ChannelInOutNode
    {
        public ToGammaNode() { }
        public override void Construct() { base.Construct(); Name = "To Gamma"; }
        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor().ToVector4();
            color.X = (float)Math.Pow(color.X, 1.0f / 2.2f);
            color.Y = (float)Math.Pow(color.Y, 1.0f / 2.2f);
            color.Z = (float)Math.Pow(color.Z, 1.0f / 2.2f);
            OutputSockets[0].Data = new Color(color.X, color.Y, color.Z, color.W);
        }
    }

    [Description("Converts the input out of Gamma space")]
    public partial class FromGammaNode : ChannelInOutNode
    {
        public FromGammaNode() { }
        public override void Construct() { base.Construct(); Name = "From Gamma"; }
        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor().ToVector4();
            color.X = (float)Math.Pow(color.X, 2.2f);
            color.Y = (float)Math.Pow(color.Y, 2.2f);
            color.Z = (float)Math.Pow(color.Z, 2.2f);
            OutputSockets[0].Data = new Color(color.X, color.Y, color.Z, color.W);
        }
    }

    [Description("Remaps values INTO a normalized range, ie. 0.5-1.0 into 0-1")]
    [PropertyData.HelpNoChannels]
    [PropertyData.NoPreviews]
    public partial class ToNormalizedRangeNode : ChannelInOutNode
    {
        Vector2 range_ = new Vector2(0, 1);
        [PropertyData.AllowPermutations]
        [Description("Lower and upper bound to remap as 0-1")]
        [PropertyData.ValidStep(Value = 0.1f)]
        public Vector2 Range { get { return range_; } set { range_ = value; OnPropertyChanged(); } }
        public ToNormalizedRangeNode() { }
        public override void Construct() { base.Construct(); Name = "To Normalized Range"; }
        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor().ToVector4();
            color.X = Mathf.Normalize(color.X, range_.X, range_.Y);
            color.Y = Mathf.Normalize(color.X, range_.X, range_.Y);
            color.Z = Mathf.Normalize(color.X, range_.X, range_.Y);
            OutputSockets[0].Data = new Color(color.X, color.Y, color.Z, color.W);
        }
    }

    [Description("Remaps values OUT of a normalized range, ie. 0.0-1.0 into 0.5-1.0")]
    [PropertyData.HelpNoChannels]
    [PropertyData.NoPreviews]
    public partial class FromNormalizedRangeNode : ChannelInOutNode
    {
        Vector2 range_ = new Vector2(0, 1);
        [PropertyData.AllowPermutations]
        [Description("Range to remap as")]
        [PropertyData.ValidStep(Value = 0.1f)]
        public Vector2 Range { get { return range_; }set { range_ = value;  OnPropertyChanged(); } }
        public FromNormalizedRangeNode() { }
        public override void Construct() { base.Construct(); Name = "From Normalized Range"; }
        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor().ToVector4();
            color.X = Mathf.Denormalize(color.X, range_.X, range_.Y);
            color.Y = Mathf.Denormalize(color.X, range_.X, range_.Y);
            color.Z = Mathf.Denormalize(color.X, range_.X, range_.Y);
            OutputSockets[0].Data = new Color(color.X, color.Y, color.Z, color.W);
        }
    }

    [Description("Clips the input value to fall within the specified range")]
    [PropertyData.HelpNoChannels]
    public partial class ClipNode : ChannelInOutNode
    {
        bool processAlpha_ = false;
        float minimum_ = 0.25f;
        float maximum_ = 0.75f;

        [PropertyData.AllowPermutations]
        [Description("Whether the alpha channel will also be clipped or not")]
        public bool ProcessAlpha { get { return processAlpha_; } set { processAlpha_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Sets the lower bound for clipping")]
        [PropertyData.ValidStep(Value = 0.1f)]
        public float Minimum { get { return minimum_; } set { minimum_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Sets the upper bound for clipping")]
        [PropertyData.ValidStep(Value = -0.1f)]
        public float Maximum { get { return maximum_; } set { maximum_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Clip";
            AddInput(new GraphSocket(this) { Name = "Min", TypeID = SocketTypeID.Channel });
            AddInput(new GraphSocket(this) { Name = "Max", TypeID = SocketTypeID.Channel });
        }
        bool hasMinIn = false;
        bool hasMaxIn = false;
        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            hasMinIn = InputSockets[1].HasConnections();
            hasMaxIn = InputSockets[2].HasConnections();
        }

        public override void Execute(object param)
        {
            var value = InputSockets[0].GetColor().ToVector4();
            if (hasMinIn)
            {
                var minVal = InputSockets[1].GetColor().ToVector4();
                var newVal = Vector4.Max(minVal, value);
                if (!processAlpha_)
                    newVal.W = value.W;
                value = newVal;
            }
            else
            {
                value.X = Math.Max(value.X, Minimum);
                value.Y = Math.Max(value.Y, Minimum);
                value.Z = Math.Max(value.Z, Minimum);
                if (processAlpha_)
                    value.W = Math.Max(value.W, Minimum);
            }

            if (hasMaxIn)
            {
                var maxVal = InputSockets[2].GetColor().ToVector4();
                var newVal = Vector4.Min(maxVal, value);
                if (!processAlpha_)
                    newVal.W = value.W;
                value = newVal;
            }
            else
            {
                value.X = Math.Min(value.X, Maximum);
                value.Y = Math.Min(value.Y, Maximum);
                value.Z = Math.Min(value.Z, Maximum);
                if (processAlpha_)
                    value.W = Math.Max(value.W, Maximum);
            }

            OutputSockets[0].Data = value;
        }
    }

    #region Trivial Math Nodes

    public class TwoChannelInOneOutNode : TexGenNode
    {
        public TwoChannelInOneOutNode()
        {
        }
        public override void Construct()
        {
            base.Construct();
            AddInput(new Data.Graph.GraphSocket(this) { Name = "LHS", TypeID = SocketTypeID.Channel, IsInput = true });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "RHS", TypeID = SocketTypeID.Channel, IsInput = true });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

    }

    [Description("Adds two input channels together")]
    public partial class AddNode : TwoChannelInOneOutNode
    {
        public AddNode() { }
        public override void Construct() { base.Construct(); Name = "Add"; }
        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor().ToVector4() + InputSockets[1].GetColor().ToVector4();
            color.W = 1.0f;
            OutputSockets[0].Data = color;
        }
    }
    [Description("Subtract 'RHS' from 'LHS'")]
    public partial class SubtractNode : TwoChannelInOneOutNode
    {
        public SubtractNode() { }
        public override void Construct() { base.Construct(); Name = "Subtract"; }
        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor().ToVector4() - InputSockets[1].GetColor().ToVector4();
            color.W = 1.0f;
            OutputSockets[0].Data = color;
        }
    }
    [Description("Multiplies two input channels together")]
    public partial class MultiplyNode : TwoChannelInOneOutNode
    {
        public MultiplyNode() { }
        public override void Construct() { base.Construct(); Name = "Multiply"; }
        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor().ToVector4() * InputSockets[1].GetColor().ToVector4();
            color.W = 1.0f;
            OutputSockets[0].Data = color;
        }
    }
    [Description("Divides 'LHS' by 'RHS'")]
    public partial class DivideNode : TwoChannelInOneOutNode
    {
        public DivideNode() { }
        public override void Construct() { base.Construct(); Name = "Divide"; }
        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor().ToVector4() / InputSockets[1].GetColor().ToVector4();
            color.W = 1.0f;
            OutputSockets[0].Data = color;
        }
    }

    #endregion
}
