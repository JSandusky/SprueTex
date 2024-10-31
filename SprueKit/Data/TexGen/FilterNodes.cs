using System;
using System.ComponentModel;

using SprueKit.Data.Graph;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Matrix = Microsoft.Xna.Framework.Matrix;
using IntVector2 = PluginLib.IntVector2;
using Mat3x3 = PluginLib.Mat3x3;

namespace SprueKit.Data.TexGen
{
    [Description("Outputs the inverse of the input")]
    public partial class InvertTextureModifier : ChannelInOutNode
    {
        public InvertTextureModifier() { }

        public override void Construct()
        {
            base.Construct();
            Name = "Invert";
        }

        public override void Execute(object param)
        {
            var col = InputSockets[0].GetColor();
            OutputSockets[0].Data = new Color(255 - col.R, 255 - col.G, 255 - col.B, 255);
        }
    }

    [Description("Applies a solarization effect to the input")]
    public partial class SolarizeTextureModifier : ChannelInOutNode
    {
        float threshold_ = 0.5f;
        bool invertLower_ = true;

        [PropertyData.AllowPermutations]
        [Description("Only values greater than the threshold will be solarized")]
        [PropertyData.ValidStep(Value = 0.1f)]
        public float Threshold { get { return threshold_; }set { threshold_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Whether solarization applies to values below or above the threshold")]
        public bool InvertLower { get { return invertLower_; } set { invertLower_ = value; OnPropertyChanged(); } }

        public SolarizeTextureModifier() { }
        public override void Construct() { base.Construct(); Name = "Solarize"; }

        public override void Execute(object param)
        {
            var color = InputSockets[0].GetColor().ToVector4();
            if (InvertLower)
            {
                color.X = color.X < Threshold ? 1.0f - color.X : color.X;
                color.Y = color.Y < Threshold ? 1.0f - color.Y : color.Y;
                color.Z = color.Z < Threshold ? 1.0f - color.Z : color.Z;
            }
            else
            {
                color.X = color.X > Threshold ? 1.0f - color.X : color.X;
                color.Y = color.Y > Threshold ? 1.0f - color.Y : color.Y;
                color.Z = color.Z > Threshold ? 1.0f - color.Z : color.Z;
            }
            OutputSockets[0].Data = Color.FromNonPremultiplied(color);
        }
    }

    [Description("Applies a kernel convolution filter to the input")]
    public partial class ConvolutionFilter : CachingNode
    {
        Mat3x3 kernel_ = new Mat3x3() { m = new float[3, 3] { { 0.0f, 0.0f, 0.0f }, { 0.0f, 1.0f, 0.0f }, { 0.0f, 0.0f, 0.0f } } };
        float stepSize_ = 1.0f;
        [Description("Kernel to apply")]
        public Mat3x3 Kernel { get { return kernel_; } set { kernel_ = value; OnPropertyChanged(); } }
        [Description("Fraction of image size to step when sampling the kernel")]
        public float StepSize { get { return stepSize_; } set { stepSize_ = value; OnPropertyChanged(); } }

        public ConvolutionFilter()
        {

        }

        [CommandAttribute("Balance")]
        [Description("Normalizes the kernel matrix to sum to 1.0")]
        public void Balance()
        {
            float sum = 0;
            for (int x = 0; x < 3; ++x)
            {
                for (int y = 0; y < 3; ++y)
                    sum += kernel_.m[x, y];
            }
            float invSum = 1.0f / sum;
            for (int x = 0; x < 3; ++x)
            {
                for (int y = 0; y < 3; ++y)
                    kernel_.m[x, y] = kernel_.m[x, y] * invSum;
            }
            OnPropertyChanged("Kernel");
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Convolution";
            //AddInput(new GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Channel });
            //AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            Vector4 pos = (Vector4)param;
            PrepareCache((int)pos.Z, (int)pos.W);
            float step = CalculateStepSize(StepSize, pos);
            Vector4 sum = Vector4.Zero;
            for (int y = -1; y <= 1; ++y)
            {
                for (int x = -1; x <= 1; ++x)
                {
                    sum += cache_.GetPixelBilinear(pos.X + x * step, pos.Y + y * step).ToVector4() * kernel_.m[1 + x, 1 + y];
                    //ForceExecuteUpstream(pos + new Vector4(x * step, y * step, 0, 0));
                    //sum += InputSockets[0].GetColor().ToVector4() * kernel_.m[1 + x, 1 + y];
                }
            }
            OutputSockets[0].Data = sum;
        }
    }

    [Description("Applies a sharpening kernel to the input")]
    public partial class SharpenFilter : ChannelInOutNode
    {
        float power_ = 1.2f;
        float stepSize_ = 1.0f;

        [PropertyData.AllowPermutations]
        [Description("Intensity of the sharpening effect")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public float Power { get { return power_; } set { power_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Fraction in image size to step when performing kernel sampling")]
        [PropertyData.ValidStep(Value = 0.5f)]
        public float StepSize { get { return stepSize_; }set { stepSize_ = value; OnPropertyChanged(); } }

        public SharpenFilter() {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Sharpen";
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            float center = 2.0f * Power;
            float side = -0.25f * Power;
            float[,] kernel = new float[,] {
                { 0,    side,   0 },
                { side, center, side },
                { 0,    side,   0 } };

            Vector4 pos = (Vector4)param;

            float stepping = CalculateStepSize(StepSize, pos);

            Vector4 sum = Vector4.Zero;
            for (int y = -1; y <= 1; ++y)
            {
                for (int x = -1; x <= 1; ++x)
                {
                    ForceExecuteUpstream(pos + new Vector4(x * stepping, y * stepping, 0, 0));
                    sum += InputSockets[0].GetColor().ToVector4() * kernel[1 + x, 1 + y];
                }
            }
            OutputSockets[0].Data = sum;
        }
    }

    [Description("Remaps values to a colored gradient ramp")]
    public partial class GradientRampTextureModifier : TexGenNode
    {
        ColorRamp ramp_ = new ColorRamp();
        [Description("Gradient to apply on 0-1, use CTRL+LEFT MOUSE to add/remove ticks")]
        public ColorRamp Ramp { get { return ramp_; } set { ramp_ = value; OnPropertyChanged(); } }

        public GradientRampTextureModifier()
        {

        }

        public override void Construct()
        {
            base.Construct();
            Name = "Gradient Ramp";
            AddInput(new GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Grayscale });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            Vector4 v = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = Ramp.Get(v.X);
        }
    }

    [Description("Remaps the input to user defined curves for each channel")]
    public partial class CurveTextureModifier : ChannelInOutNode
    {
        ColorCurves curves_ = new ColorCurves();
        [Description("Modifiers curves to remap the input, use CTRL+LEFT MOUSE to add/remove ticks")]
        public ColorCurves Curves { get { return curves_; }
            set {
                curves_ = value; OnPropertyChanged(); } }

        public CurveTextureModifier()
        {

        }

        public override void Construct()
        {
            base.Construct();
            Name = "Color Curves";
        }

        public override void Execute(object param)
        {
            Vector4 v = InputSockets[0].GetColor().ToVector4();
            v.X = Curves.R.GetValue(v.X);
            v.Y = Curves.G.GetValue(v.Y);
            v.Z = Curves.B.GetValue(v.Z);
            v.W = Curves.A.GetValue(v.W);
            OutputSockets[0].Data = v;
        }
    }

    [Description("Outputs the result of a Sobel edge detection filter")]
    public partial class SobelTextureModifier : CachingNode
    {
        float stepSize_ = 1.5f;
        [PropertyData.AllowPermutations]
        [Description("Fraction of image size to use for stepping during kernel sampling")]
        [PropertyData.ValidStep(Value = 1.5f)]
        public float StepSize { get { return stepSize_; } set { stepSize_ = value; OnPropertyChanged(); } }

        public SobelTextureModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Sobel Edge";
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Invert", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Channel });
        }

        public override bool WillForceExecute() { return true; }

        public static float SampleSobel(CachingNode nd, int inSocketIndex, int X, int Y, Vector4 pos, float stepping)
        {
            return nd.GetCache().GetPixelBilinear(pos.X + X * stepping, pos.Y + Y * stepping).ToVector4().X;
            //nd.ForceExecuteUpstream(pos + new Vector4(X * stepping, Y * stepping, 0, 0));
            //return nd.InputSockets[inSocketIndex].GetFloatData();
        }

        public override void Execute(object param)
        {
            Vector4 pos = (Vector4)param;
            PrepareCache((int)pos.Z, (int)pos.W);

            float stepping = CalculateStepSize(StepSize, pos);
            float l = SampleSobel(this, 0, -1, 0, pos, stepping);
            float r = SampleSobel(this, 0, 1, 0, pos, stepping);
            float t = SampleSobel(this, 0, 0, -1, pos, stepping);
            float b = SampleSobel(this, 0, 0, 1, pos, stepping);
            float tl = SampleSobel(this, 0, -1, -1, pos, stepping);
            float tr = SampleSobel(this, 0, 1, -1, pos, stepping);
            float bl = SampleSobel(this, 0, -1, 1, pos, stepping);
            float br = SampleSobel(this, 0, 1, 1, pos, stepping);

            //const float LowerFrac = 3 / 16.0f;
            //const float UpperFrac = 10 / 16.0f;
            //float dX = tl * LowerFrac + l * UpperFrac + bl * LowerFrac + tr * -LowerFrac + r * -UpperFrac + br * -LowerFrac;
            //float dY = tl * LowerFrac + t * UpperFrac + tr * LowerFrac + bl * -LowerFrac + b * -UpperFrac + br * -LowerFrac;
            //float dX = tr + 2 * r + br - tl - 2 * l - bl;
            //float dY = bl + 2 * b + br - tl - 2 * t - tr;
            float dX = tr + 2 * r + br - tl - 2 * l - bl;
            float dY = bl + 2 * b + br - tl - 2 * t - tr;

            float fVal = Mathf.Clamp01(dX * dY);
            OutputSockets[0].Data = fVal;
            OutputSockets[1].Data = 1.0f - fVal;
        }
    }

    [Description("Clips the input to be within the specified range")]
    [PropertyData.HelpNoChannels]
    public partial class ClipTextureModifier : TexGenNode
    {
        Vector2 range_ = new Vector2(0, 1);
        bool clipAlpha_ = false;

        [PropertyData.AllowPermutations]
        [Description("Whether the alpha channel will also be clipped or not")]
        public bool ClipAlpha { get { return clipAlpha_; } set { clipAlpha_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Range that the input must be contained within")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public Vector2 Range { get { return range_; } set { range_ = value; OnPropertyChanged(); } }

        public ClipTextureModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Clip";
            AddInput(new GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Channel });
            AddInput(new GraphSocket(this) { Name = "Min", TypeID = SocketTypeID.Channel });
            AddInput(new GraphSocket(this) { Name = "Max", TypeID = SocketTypeID.Channel });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        bool hasMin = false;
        bool hasMax = false;
        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            hasMin = InputSockets[1].HasConnections();
            hasMax = InputSockets[2].HasConnections();
        }

        public override void Execute(object param)
        {
            var value = InputSockets[0].GetColor().ToVector4();

            var lowerBound = hasMin ? InputSockets[1].GetColor(new Color(range_.X, range_.X, range_.X, range_.X)).ToVector4() : new Vector4(range_.X); 
            var upperBound = hasMax ? InputSockets[2].GetColor(new Color(range_.Y, range_.Y, range_.Y, range_.Y)).ToVector4() : new Vector4(range_.Y);

            var oldValue = value;
            value = Vector4.Max(value, lowerBound);
            value = Vector4.Min(value, upperBound);
            if (!clipAlpha_)
                value.W = oldValue.W;

            OutputSockets[0].Data = value;
        }
    }

    [Description("Performs basic tiling of the sampling coordinates")]
    public partial class TileModifier : ChannelInOutNode
    {
        Vector2 tiling_ = new Vector2(2, 2);
        [PropertyData.AllowPermutations]
        [Description("How many times to tiling the texture")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public Vector2 Tiling { get { return tiling_; } set { tiling_ = value; OnPropertyChanged(); } }

        public TileModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Tile";
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            Vector4 coord = (Vector4)param;
            coord.X = coord.X * Tiling.X;
            coord.Y = coord.Y * Tiling.Y;
            coord.X -= (float)Math.Floor(coord.X);
            coord.Y -= (float)Math.Floor(coord.Y);

            ForceExecuteUpstream(coord);

            OutputSockets[0].Data = InputSockets[0].Data;
        }
    }

    [Description("Perturbs the sampling coordinates of the texture using inputs for the X and Y axis")]
    public partial class WarpModifier : ChannelInOutNode
    {
        Vector2 intensity_ = new Vector2(0.01f, 0.01f);
        [PropertyData.AllowPermutations]
        [Description("Inputs will be multiplied by this value to arrive at a final warp intensity")]
        [PropertyData.ValidStep(Value = 0.02f)]
        public Vector2 Intensity { get { return intensity_; } set { intensity_ = value; OnPropertyChanged(); } }

        public WarpModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Warp";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Perturb X", TypeID = SocketTypeID.Channel });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Perturb Y", TypeID = SocketTypeID.Channel });
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            Vector4 vec = (Vector4)param;
            float x_coord = vec.X + InputSockets[1].GetFloatData() * Intensity.X;
            float x_coord_lo = (float)x_coord;
            float x_coord_hi = x_coord_lo + 1 * Intensity.X;
            float x_frac = x_coord - x_coord_lo;
            float y_coord = vec.Y + InputSockets[2].GetFloatData() * Intensity.Y;
            float y_coord_lo = (float)y_coord;
            float y_coord_hi = y_coord_lo + 1 * Intensity.Y;
            float y_frac = y_coord - y_coord_lo;

            ForceExecuteUpstream(new Vector4(x_coord_lo, y_coord_lo, vec.Z, vec.W));
            var tl = InputSockets[0].GetColor().ToVector4(); ;

            ForceExecuteUpstream(new Vector4(x_coord_hi, y_coord_lo, vec.Z, vec.W));
            var tr = InputSockets[0].GetColor().ToVector4(); ;

            ForceExecuteUpstream(new Vector4(x_coord_lo, y_coord_hi, vec.Z, vec.W));
            var bl = InputSockets[0].GetColor().ToVector4(); ;

            ForceExecuteUpstream(new Vector4(x_coord_hi, y_coord_hi, vec.Z, vec.W));
            var br = InputSockets[0].GetColor().ToVector4();

            var left = Vector4.Lerp(tl, tr, x_frac);
            var right = Vector4.Lerp(bl, br, x_frac);
            var result = Vector4.Lerp(left, right, y_frac);

            OutputSockets[0].Data = Color.FromNonPremultiplied(result);
        }
    }

    public partial class TransformModifier : ChannelInOutNode
    {
        PluginLib.Mat3x3 transform_ = new PluginLib.Mat3x3();

        public PluginLib.Mat3x3 Transform { get { return transform_; } set { transform_ = value; OnPropertyChanged(); } }

        public TransformModifier()
        {

        }

        public override void Construct()
        {
            Name = "Transform";
            AddInput(new GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Channel });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            Vector4 p = (Vector4)param;
            Vector2 coord = new Vector2(p.X, p.Y);

            float x = transform_.m[0, 0] * p.X + transform_.m[0, 1] * p.X + transform_.m[0, 2] * p.X;
            float y = transform_.m[1, 0] * p.Y + transform_.m[1, 1] * p.Y + transform_.m[1, 2] * p.Y;

            ForceExecuteUpstream(new Vector4(x, y, p.Z, p.W));
            OutputSockets[0].Data = InputSockets[0].Data;
        }
    }

    [Description("Manipulates the texture sampling coordinates to perform basic offsets and scaling")]
    public partial class SimpleTransformModifier : ChannelInOutNode
    {
        Vector2 offset_ = new Vector2();
        float rotation_ = 0.0f;
        Vector2 scale_ = new Vector2(1, 1);
        bool clipBounds_ = false;

        [PropertyData.AllowPermutations]
        [Description("Fraction of image size to shift in XY")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public Vector2 Offset { get { return offset_; } set { offset_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Multiplier by which to scale the sampling coordinates")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public Vector2 Scale { get { return scale_; } set { scale_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Angle in degrees to rotate the sampling coordinates")]
        [PropertyData.ValidStep(Value = 45.0f)]
        public float Rotation { get { return rotation_; } set { rotation_ = value; OnPropertyChanged(); } }

        [Description("Coordinates outside of the 0-1 range will be ignored")]
        public bool ClipBounds { get { return clipBounds_; }set { clipBounds_ = value; OnPropertyChanged(); } }

        public SimpleTransformModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Simple Transform";
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            Vector4 trueParam = (Vector4)param;
            Vector2 pt = new Vector2(trueParam.X, trueParam.Y);

            Matrix mat = Matrix.CreateScale(Scale.X, Scale.Y, 1.0f) * Matrix.CreateRotationZ(Rotation * Mathf.DEGTORAD) * Matrix.CreateTranslation(-Offset.X, -Offset.Y, 0.0f);
            pt = Vector2.Transform(pt, mat);

            if (clipBounds_)
            {
                if (pt.X < 0.0f || pt.X > 1.0f)
                {
                    OutputSockets[0].Data = null;
                    return;
                }
                else if (pt.Y < 0.0f || pt.Y > 1.0f)
                {
                    OutputSockets[0].Data = null;
                    return;
                }
            }
            ForceExecuteUpstream(new Vector4(pt.X, pt.Y, trueParam.Z, trueParam.W));
            OutputSockets[0].Data = InputSockets[0].Data;
        }
    }

    [Description("Converts the input into Polar coordinate")]
    public partial class CartesianToPolarModifier : ChannelInOutNode
    {
        public CartesianToPolarModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Cartesian To Polar";
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            Vector4 p = (Vector4)param;

            float R = Mathf.Sqrt(p.X * p.X + p.Y * p.Y);
            float Theta = Mathf.Atan2(p.Y, p.X);
            if (Theta < 0.0f)
                Theta += Mathf.PI * 2;
            p.X = R;
            p.Y = Theta;

            ForceExecuteUpstream(p);
            OutputSockets[0].Data = InputSockets[0].Data;
        }
    }

    [Description("Converts the input into cartesian coordinates, assuming it was in polar coordinates first")]
    public partial class PolarToCartesianModifier : ChannelInOutNode
    {
        public PolarToCartesianModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Polar To Cartesian";
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            Vector4 p = (Vector4)param;
            float X = p.X * Mathf.Cos(p.Y);
            float Y = p.X * Mathf.Sin(p.Y);
            p.X = X;
            p.Y = Y;
            ForceExecuteUpstream(p);
            OutputSockets[0].Data = InputSockets[0].Data;
        }
    }

    [Description("Combines two inputs into a simple left/right or top/bottom composition")]
    public partial class DivModifier : TexGenNode
    {
        float fraction_ = 0.5f;
        bool vertical_ = false;
        bool normalizeCoordinates_ = false;

        [PropertyData.AllowPermutations]
        [Description("Fraction of the image size at which to partition the division")]
        [PropertyData.ValidStep(Value = 0.15f)]
        public float Fraction { get { return fraction_; } set { fraction_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Division will be aligned vertically instead of horizontally")]
        public bool Vertical { get { return vertical_; }set { vertical_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Coordinates will be remapped for each side of the division, or left as is")]
        public bool NormalizeCoordinates { get { return normalizeCoordinates_; } set { normalizeCoordinates_ = value; OnPropertyChanged(); } }

        public DivModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Div Space";
            AddInput(new GraphSocket(this) { Name = "Left", TypeID = SocketTypeID.Channel });
            AddInput(new GraphSocket(this) { Name = "Right", TypeID = SocketTypeID.Channel });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsOutput = true, IsInput = false });
            AddOutput(new GraphSocket(this) { Name = "Left", TypeID = SocketTypeID.Grayscale, IsOutput = true, IsInput = false });
            AddOutput(new GraphSocket(this) { Name = "Right", TypeID = SocketTypeID.Grayscale, IsOutput = true, IsInput = false });
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            Vector4 value = (Vector4)param;
            if (Vertical)
            {
                if (value.X < Fraction)
                {
                    if (NormalizeCoordinates)
                        value.X = Mathf.Normalize(value.X, 0.0f, Fraction);
                    ForceExecuteUpstream(value);
                    OutputSockets[0].Data = InputSockets[0].Data;
                    OutputSockets[1].Data = 1.0f;
                    OutputSockets[2].Data = 0.0f;
                }
                else
                {
                    if (NormalizeCoordinates)
                        value.X = Mathf.Normalize(value.X, Fraction, 1.0f);
                    ForceExecuteUpstream(value);
                    OutputSockets[0].Data = InputSockets[1].Data;
                    OutputSockets[1].Data = 0.0f;
                    OutputSockets[2].Data = 1.0f;
                }
            }
            else // horizontal
            {
                if (value.Y < Fraction)
                {
                    if (NormalizeCoordinates)
                        value.Y = Mathf.Normalize(value.Y, 0.0f, Fraction);
                    ForceExecuteUpstream(value);
                    OutputSockets[0].Data = InputSockets[0].Data;
                    OutputSockets[1].Data = 1.0f;
                    OutputSockets[2].Data = 0.0f;
                }
                else
                {
                    if (NormalizeCoordinates)
                        value.Y = Mathf.Normalize(value.Y, Fraction, 1.0f);
                    ForceExecuteUpstream(value);
                    OutputSockets[0].Data = InputSockets[1].Data;
                    OutputSockets[1].Data = 0.0f;
                    OutputSockets[2].Data = 1.0f;
                }
            }
        }
    }

    [Description("Combines 3 inputs into a simple 'trim' composition")]
    public partial class TrimModifier : TexGenNode
    {
        float leftTrimSize_ = 0.2f;
        float rightTrimSize_ = 0.2f;
        bool vertical_ = false;
        bool normalizeCoordinates_ = false;

        [PropertyData.AllowPermutations]
        [Description("Height of the upper/left trim")]
        [PropertyData.ValidStep(Value = 0.1f)]
        public float LeftTrimSize { get { return leftTrimSize_; } set { leftTrimSize_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Height of the lower/right trim")]
        [PropertyData.ValidStep(Value = 0.1f)]
        public float RightTrimSize { get { return rightTrimSize_; } set { rightTrimSize_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Trim will be oriented vertically instead of horizontally")]
        public bool Vertical { get { return vertical_; } set { vertical_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Whether coordinates will be remapped for each cell or left as")]
        public bool NormalizeCoordinates { get { return normalizeCoordinates_; } set { normalizeCoordinates_ = value; OnPropertyChanged(); } }

        public TrimModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Trim";
            AddInput(new GraphSocket(this) { Name = "Left", TypeID = SocketTypeID.Channel });
            AddInput(new GraphSocket(this) { Name = "Center", TypeID = SocketTypeID.Channel });
            AddInput(new GraphSocket(this) { Name = "Right", TypeID = SocketTypeID.Channel });

            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsOutput = true, IsInput = false });
            AddOutput(new GraphSocket(this) { Name = "Left", TypeID = SocketTypeID.Grayscale, IsOutput = true, IsInput = false });
            AddOutput(new GraphSocket(this) { Name = "Center", TypeID = SocketTypeID.Grayscale, IsOutput = true, IsInput = false });
            AddOutput(new GraphSocket(this) { Name = "Right", TypeID = SocketTypeID.Grayscale, IsOutput = true, IsInput = false });
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            Vector4 coord = (Vector4)param;

            // do we need to ask for right edge?
            //const bool hasRightEdge = InputSockets[2].->HasConnections();

            OutputSockets[1].Data = 0.0f;
            OutputSockets[2].Data = 0.0f;
            OutputSockets[3].Data = 0.0f;

            // Vertically oriented trim
            if (Vertical)
            {
                // inside left trim?
                if (coord.X <= LeftTrimSize)
                {
                    if (NormalizeCoordinates)
                        coord.X = Mathf.Normalize(coord.X, 0.0f, LeftTrimSize);
                    ForceExecuteUpstream(coord);
                    OutputSockets[0].Data = InputSockets[0].Data;
                    OutputSockets[1].Data = 1.0f;
                }
                else if (coord.X > 1.0f - RightTrimSize) // inside right trim?
                {
                    if (NormalizeCoordinates)
                        coord.X = Mathf.Normalize(coord.X, 0.0f, RightTrimSize);
                    ForceExecuteUpstream(coord);
                    OutputSockets[0].Data = InputSockets[2].Data;
                    OutputSockets[3].Data = 1.0f;
                }
                else // inside center
                {
                    if (NormalizeCoordinates)
                        coord.X = Mathf.Normalize(coord.X, LeftTrimSize, 1.0f - RightTrimSize);
                    ForceExecuteUpstream(coord);
                    OutputSockets[0].Data = InputSockets[1].Data;
                    OutputSockets[2].Data = 1.0f;
                }
            }
            else // Horizontally oriented trim
            {
                // inside left trim?
                if (coord.Y <= LeftTrimSize)
                {
                    if (NormalizeCoordinates)
                        coord.Y = Mathf.Normalize(coord.Y, 0.0f, LeftTrimSize);
                    ForceExecuteUpstream(coord);
                    OutputSockets[0].Data = InputSockets[0].Data;
                    OutputSockets[1].Data = 1.0f;
                }
                else if (coord.Y > 1.0f - RightTrimSize) // inside right trim?
                {
                    if (NormalizeCoordinates)
                        coord.Y = Mathf.Normalize(coord.Y, 0.0f, RightTrimSize);
                    ForceExecuteUpstream(coord);
                    OutputSockets[0].Data = InputSockets[2].Data;
                    OutputSockets[3].Data = 1.0f;
                }
                else // inside center
                {
                    if (NormalizeCoordinates)
                        coord.Y = Mathf.Normalize(coord.Y, LeftTrimSize, 1.0f - RightTrimSize);
                    ForceExecuteUpstream(coord);
                    OutputSockets[0].Data = InputSockets[1].Data;
                    OutputSockets[2].Data = 1.0f;
                }
            }
        }
    }

    [Description("Applies a pseudo-relief effect to the input")]
    public partial class EmbossModifier : ChannelInOutNode
    {
        float angle_ = 0.0f;
        float stepSize_ = 1.0f;
        float bias_ = 0.5f;
        float power_ = 1.0f;

        [PropertyData.AllowPermutations]
        [Description("Angle of the incoming light source, in degrees")]
        [PropertyData.ValidStep(Value = 45.0f)]
        public float Angle { get { return angle_; } set { angle_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Fraction of the image size to step for the sampling kernel")]
        [PropertyData.ValidStep(Value = 0.5f)]
        public float StepSize { get { return stepSize_; } set { stepSize_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Constant factor added to the value")]
        [PropertyData.ValidStep(Value = 0.1f)]
        public float Bias { get { return bias_; } set { bias_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Inputs will be scaled by this value")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public float Power { get { return power_; }set { power_ = value; OnPropertyChanged(); } }

        public EmbossModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Emboss";
        }

        public override bool WillForceExecute() { return true; }

        Vector2 Rotate(Vector2 vec, float degrees)
        {
            float cosVal = Mathf.Cos(degrees * 0.0174533f);
            float sinVal = Mathf.Sin(degrees * 0.0174533f);
            float newX = vec.X * cosVal - vec.Y * sinVal;
            float newY = vec.X * sinVal + vec.Y * cosVal;
            return new Vector2(newX, newY);
        }

        public override void Execute(object param)
        {
            Vector4 pos = (Vector4)param;
            float stepping = CalculateStepSize(StepSize, pos);

            Vector2 angleVec = Vector2.Normalize(Rotate(Vector2.UnitY, Angle));
            Vector2 posXY = new Vector2(pos.X, pos.Y);
            Vector4 sum = new Vector4(Bias, Bias, Bias, 1.0f);
            for (int y = -1; y <= 1; ++y)
            {
                if (y == 0)
                    continue;
                for (int x = -1; x <= 1; ++x)
                {
                    if (x == 0)
                        continue;
                    Vector4 sampleVec = pos + new Vector4(x * stepping, y * stepping, 0, 0);
                    ForceExecuteUpstream(sampleVec);
                    Vector2 dirVec = Vector2.Normalize(new Vector2(sampleVec.X, sampleVec.Y) - posXY);
                    float dp = Vector2.Dot(dirVec, angleVec);
                    sum += InputSockets[0].GetColor().ToVector4() * dp * Power;
                }
            }

            OutputSockets[0].Data = sum;
        }
    }

    [Description("Partitions the input into distinct cut-out levels")]
    public partial class PosterizeModifier : ChannelInOutNode
    {
        int range_ = 4;
        [PropertyData.AllowPermutations]
        [Description("Number of cutouts to use")]
        [PropertyData.ValidStep(Value = 2.0f)]
        public int Range { get { return range_; } set { range_ = value; OnPropertyChanged(); } }

        public PosterizeModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Posterize";
        }

        float Posterize(float inData)
        {
            if (Range > 1)
                return ((int)(inData * Range) / (float)Range);
            else if (Range == 1)
                return 0.0f;
            else
                return 1.0f;
        }

        Vector4 Posterize(Vector4 inColor)
        {
            return new Vector4(Posterize(inColor.X), Posterize(inColor.Y), Posterize(inColor.Z), inColor.W);
        }

        public override void Execute(object param)
        {
            var inColor = InputSockets[0].GetColor().ToVector4();
            OutputSockets[0].Data = Posterize(inColor);
        }
    }

    [Description("Performs a simple Talus erosion")]
    [PropertyData.HelpNoChannels]
    public partial class ErosionModifier : TexGenNode
    {
        int iterations_ = 3;
        float intensity_ = 1.0f;
        float stepSize_ = 1.0f;
        float talus_ = 0.02f;

        [PropertyData.AllowPermutations]
        [Description("How many iterations to perform, has little impact beyond 3")]
        [PropertyData.ValidStep(Value = -1.0f)]
        public int Iterations { get { return iterations_; } set { iterations_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("How much 'material' the erosion process can move per iteration")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public float Intensity { get { return intensity_; }set { intensity_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Fraction of the image dimensions to step when sampling the neighborhood for calculating slope")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public float StepSize { get { return stepSize_; } set { stepSize_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Erosion will only occur when the slope is greater than the talus, small values are steep and large values are flat")]
        [PropertyData.ValidStep(Value = 0.01f)]
        public float Talus { get { return talus_; } set { talus_ = value; OnPropertyChanged(); } }

        public ErosionModifier()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Erosion";
            AddInput(new GraphSocket(this) { Name = "Height", TypeID = SocketTypeID.Grayscale });
            AddInput(new GraphSocket(this) { Name = "Intensity", TypeID = SocketTypeID.Grayscale });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Grayscale, IsInput = false, IsOutput = true });
            AddOutput(new GraphSocket(this) { Name = "Bedrock", TypeID = SocketTypeID.Grayscale, IsInput = false, IsOutput = true });
            AddOutput(new GraphSocket(this) { Name = "Sediment", TypeID = SocketTypeID.Grayscale, IsInput = false, IsOutput = true });
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            Vector4 pos = (Vector4)param;
            int iters = Math.Abs(Iterations);
            float stepping = CalculateStepSize(StepSize, pos);
            OutputSockets[0].Data = 0.0f;

            ForceExecuteUpstream(pos);
            float intensity = InputSockets[1].GetFloatData(1.0f) * Intensity;

            if (iters > 0)
            {
                float[,] neighbors = new float[,]{ 
                    { 0.0f, 0.0f, 0.0f }, 
                    { 0.0f, 0.0f, 0.0f }, 
                    { 0.0f, 0.0f, 0.0f }
                };
                float[,] originalNeighbors = new float[,]{
                    { 0.0f, 0.0f, 0.0f },
                    { 0.0f, 0.0f, 0.0f },
                    { 0.0f, 0.0f, 0.0f }
                };

                // Above
                ForceExecuteUpstream(pos + new Vector4(0, stepping * -1, 0, 0));
                originalNeighbors[1,0] = neighbors[1,0] = InputSockets[0].GetFloatData();
                // left    
                ForceExecuteUpstream(pos + new Vector4(stepping * -1, 0, 0, 0));
                originalNeighbors[0, 1] = neighbors[0,1] = InputSockets[0].GetFloatData();
                // right
                ForceExecuteUpstream(pos + new Vector4(stepping * 1, 0, 0, 0));
                originalNeighbors[2, 1] = neighbors[2,1] = InputSockets[0].GetFloatData();
                // Below
                ForceExecuteUpstream(pos + new Vector4(0, stepping * 1, 0, 0));
                originalNeighbors[1, 2] = neighbors[1, 2] = InputSockets[0].GetFloatData();
                // Center
                ForceExecuteUpstream(pos);
                originalNeighbors[1, 1] = neighbors[1,1] = InputSockets[0].GetFloatData();

                float accum = 0.0f;
                for (int i = 0; i < Iterations; ++i)
                {
                    float h1 = neighbors[1,0];
                    float h2 = neighbors[0,1];
                    float h3 = neighbors[2,1];
                    float h4 = neighbors[1,2];

                    float d1 = neighbors[1,1] - h1;
                    float d2 = neighbors[1,1] - h2;
                    float d3 = neighbors[1,1] - h3;
                    float d4 = neighbors[1,1] - h4;

                    float maxVal = 0.0f;
                    IntVector2 offset = new IntVector2();
                    if (d1 > maxVal)
                    {
                        maxVal = d1;
                        offset.Y = 1;
                    }
                    if (d2 > maxVal)
                    {
                        maxVal = d2;
                        offset.X = -1;
                    }
                    if (d3 > maxVal)
                    {
                        maxVal = d3;
                        offset.X = 1;
                    }
                    if (d4 > maxVal)
                    {
                        maxVal = d4;
                        offset.Y = -1;
                    }

                    if (maxVal < Talus)
                    {
                        accum += maxVal * 0.5f * intensity;
                        continue;
                    }

                    maxVal *= (0.5f * intensity);
                    neighbors[1 + offset.X, 1 + offset.Y] = neighbors[1 + offset.X, 1 + offset.Y] + maxVal;
                    neighbors[1,1] = neighbors[1,1] - maxVal;
                }

                OutputSockets[0].Data = neighbors[1,1];
                // where we went down
                OutputSockets[1].Data = originalNeighbors[1, 1] - neighbors[1, 1];
                // Where we went up
                OutputSockets[2].Data = accum;
            }
        }
    }

    [Description("Substitutes one color for another")]
    [PropertyData.HelpNoChannels]
    [PropertyData.NoPreviews]
    public partial class ReplaceColorModifier : TexGenNode
    {
        Color replace_ = Color.White;
        Color with_ = Color.Black;
        float tolerance_ = 0.1f;

        [PropertyData.AllowPermutations]
        [Description("Color to seek out for replacement")]
        public Color Replace { get { return replace_; } set { replace_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Color that will be substituted in place")]
        public Color With { get { return with_; } set { with_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Maximum deviation to allow replacement")]
        public float Tolerance { get { return tolerance_; } set { tolerance_ = value; } }

        public ReplaceColorModifier()
        {

        }

        public override void Construct()
        {
            base.Construct();
            Name = "Replace Color";
            AddInput(new GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Color });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Color, IsInput = false, IsOutput = true });
            AddOutput(new GraphSocket(this) { Name = "Mask", TypeID = SocketTypeID.Grayscale, IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            Vector4 withColor = With.ToVector4();
            Vector4 inColor = InputSockets[0].GetColor().ToVector4();
            Vector4 modified = Replace.ToVector4() - inColor;
            float length = Mathf.Sqrt(modified.X * modified.X + modified.Y * modified.Y + modified.Z * modified.Z);
            float multiplier = (inColor.X * 0.2126f + inColor.Y * 0.7152f + inColor.Z * 0.0722f);
            if (length < Tolerance)
            {
                OutputSockets[0].Data = new Vector4(withColor.X * multiplier, withColor.Y * multiplier, withColor.Z * multiplier, withColor.W);
                OutputSockets[1].Data = multiplier;
            }
            else
            {
                OutputSockets[0].Data = inColor;
                OutputSockets[1].Data = 0.0f;
            }
        }
    }

    [Description("Outputs a selected color as a grayscale mask")]
    [PropertyData.HelpNoChannels]
    [PropertyData.NoPreviews]
    public partial class SelectColorModifier : TexGenNode
    {
        Color select_ = Color.White;
        float tolerance_ = 0.1f;

        [PropertyData.AllowPermutations]
        [Description("Color to seek out")]
        public Color Select { get { return select_; } set { select_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Maximum deviation for color selection")]
        public float Tolerance { get { return tolerance_; } set { tolerance_ = value; } }

        public SelectColorModifier()
        {

        }

        public override void Construct()
        {
            Name = "Select Color";
            AddInput(new GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Color });
            AddOutput(new GraphSocket(this) { Name = "Soft", TypeID = SocketTypeID.Grayscale, IsInput = false, IsOutput = true });
            AddOutput(new GraphSocket(this) { Name = "Hard", TypeID = SocketTypeID.Grayscale, IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            Vector4 inColor = InputSockets[0].GetColor().ToVector4();
            Vector4 modified = Select.ToVector4() - inColor;
            float length = Mathf.Sqrt(modified.X * modified.X + modified.Y * modified.Y + modified.Z * modified.Z);
            float multiplier = (inColor.X * 0.2126f + inColor.Y * 0.7152f + inColor.Z * 0.0722f);

            if (length < Tolerance)
            {
                OutputSockets[0].Data = 1.0f - length;
                OutputSockets[1].Data = 1.0f;
            }
            else
            {
                OutputSockets[0].Data = 0.0f;
                OutputSockets[1].Data = 0.0f;
            }
        }
    }

    /// <summary>
    /// Uses an RGBA splat map to composite textures
    /// </summary>
    [Description("Composites up to 4 inputs blended together based on an RGBA 'Mix Map', commonly used in terrain texturing")]
    public partial class SplatMapNode : TexGenNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Splat Map";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "A", TypeID = SocketTypeID.Channel });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "B", TypeID = SocketTypeID.Channel });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "C", TypeID = SocketTypeID.Channel });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "D", TypeID = SocketTypeID.Channel });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Mix Map", TypeID = SocketTypeID.Color });

            AddOutput(new Data.Graph.GraphSocket(this) { Name = "D", TypeID = SocketTypeID.Channel, IsInput=false,IsOutput=true });
        }

        public override void Execute(object param)
        {
            Vector4 a = InputSockets[0].GetColor().ToVector4();
            Vector4 b = InputSockets[1].GetColor().ToVector4();
            Vector4 c = InputSockets[2].GetColor().ToVector4();
            Vector4 d = InputSockets[3].GetColor().ToVector4();
            Vector4 mix = InputSockets[4].GetColor().ToVector4();

            Vector4 outValue = a * mix.X + b * mix.Y + c * mix.Z + d * mix.W;
            OutputSockets[0].Data = outValue;
        }
    }

    [Description("Performs a 'pivoted' normalization to adjust the intensity of lights and darks in the output")]
    public partial class LevelsFilterNode : ChannelInOutNode
    {
        protected float lowerBound_ = 0.0f;
        protected float pivotFraction_ = 0.5f;
        protected float upperBound_ = 1.0f;

        [Description("The darkest the output can be")]
        [PropertyData.ValidStep(Value = 0.1f)]
        public float LowerBound { get { return lowerBound_; } set { lowerBound_ = value; OnPropertyChanged(); } }
        [Description("The brightest the output can be")]
        [PropertyData.ValidStep(Value = -0.1f)]
        public float UpperBound { get { return upperBound_; } set { upperBound_ = value; OnPropertyChanged(); } }
        [Description("Where the middle point is between the lower and upper bounds")]
        [PropertyData.ValidStep(Value = 0.1f)]
        public float PivotFraction { get { return pivotFraction_; } set { pivotFraction_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Levels";
        }

        public override void Execute(object param)
        {
            base.Execute(param);
            float range = upperBound_ - lowerBound_;
            // becomes zero
            float pivPos = lowerBound_ + (upperBound_ - lowerBound_) * pivotFraction_;

            var col = InputSockets[0].GetColor().ToDrawingColor();
            var brightness = col.GetBrightness();
            brightness = Data.FloatRamp.GetValue(brightness, lowerBound_, pivPos, upperBound_);
            var newCol = ColorFromHSL(col.GetHue() / 360.0f, col.GetSaturation(), brightness);
            newCol.A = col.A;
            var val = newCol;
            //var val = InputSockets[0].GetColor().ToVector4();
            //val.X = Data.FloatRamp.GetValue(val.X, lowerBound_, pivPos, upperBound_);
            //val.Y = Data.FloatRamp.GetValue(val.Y, lowerBound_, pivPos, upperBound_);
            //val.Z = Data.FloatRamp.GetValue(val.Z, lowerBound_, pivPos, upperBound_);
            //val.W = 1.0f;// Data.FloatRamp.GetValue(val.W, lowerBound_, pivotFraction_, upperBound_);
            //val.X = Mathf.Clamp(Mathf.Denormalize(Mathf.Clamp(val.X, lowerBound_, upperBound_), nLower, nUpper), 0, 1);
            //val.Y = Mathf.Clamp(Mathf.Denormalize(Mathf.Clamp(val.Y, lowerBound_, upperBound_), nLower, nUpper), 0, 1);
            //val.Z = Mathf.Clamp(Mathf.Denormalize(Mathf.Clamp(val.Z, lowerBound_, upperBound_), nLower, nUpper), 0, 1);
            //val.W = Mathf.Clamp(Mathf.Denormalize(Mathf.Clamp(val.W, lowerBound_, upperBound_), nLower, nUpper), 0, 1);
            OutputSockets[0].Data = val;
        }

        public static Color ColorFromHSL(float h, float s, float l)
        {
            float r = 0, g = 0, b = 0;
            if (l != 0)
            {
                if (s == 0)
                    r = g = b = l;
                else
                {
                    float temp2;
                    if (l < 0.5)
                        temp2 = l * (1.0f + s);
                    else
                        temp2 = l + s - (l * s);

                    float temp1 = 2.0f * l - temp2;

                    r = GetColorComponent(temp1, temp2, h + 1.0f / 3.0f);
                    g = GetColorComponent(temp1, temp2, h);
                    b = GetColorComponent(temp1, temp2, h - 1.0f / 3.0f);
                }
            }
            return new Color((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }

        private static float GetColorComponent(float temp1, float temp2, float temp3)
        {
            if (temp3 < 0.0f)
                temp3 += 1.0f;
            else if (temp3 > 1.0f)
                temp3 -= 1.0f;

            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0f * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0f / 3.0f)
                return temp1 + ((temp2 - temp1) * ((2.0f / 3.0f) - temp3) * 6.0f);
            else
                return temp1;
        }
    }

    [Description("Performs a 'pivoted' normalization to adjust the saturation of colors in the output")]
    public partial class SaturationFilterNode : LevelsFilterNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Saturation";
        }

        public override void Execute(object param)
        {
            base.Execute(param);
            float range = upperBound_ - lowerBound_;
            // becomes zero
            float pivPos = lowerBound_ + (upperBound_ - lowerBound_) * pivotFraction_;

            var col = InputSockets[0].GetColor().ToDrawingColor();
            var saturation = col.GetSaturation();
            saturation = Data.FloatRamp.GetValue(saturation, lowerBound_, pivPos, upperBound_);
            var newCol = ColorFromHSL(col.GetHue() / 360.0f, saturation, col.GetBrightness());
            newCol.A = col.A;
            var val = newCol;
            OutputSockets[0].Data = val;
        }
    }

    [Description("Outputs the input into a mask of exclusively 0.0 or 1.0")]
    public partial class BinarizeNode : TexGenNode
    {
        float cutoff_ = 0.5f;
        [Description("Value the defines the point at which the inputs are thresholded between black and white")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public float Cutoff { get { return cutoff_; } set { cutoff_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Binarize";
            AddInput(new GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Channel });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Grayscale, IsInput=false, IsOutput=true });
            AddOutput(new GraphSocket(this) { Name = "Inverted", TypeID = SocketTypeID.Grayscale, IsInput=false,IsOutput=true });
        }

        public override void Execute(object param)
        {
            base.Execute(param);
            float val = InputSockets[0].GetFloatData(0.5f);
            val = val > Cutoff ? 1.0f : 0.0f;
            OutputSockets[0].Data = val;
            OutputSockets[1].Data = 1.0f - val;
        }
    }

    [Description("Accumulates multiple passes of the input multiplied by gain and scaled by lacunarity")]
    public partial class OctaveSum : ChannelInOutNode
    {
        Vector2 lacunarity_ = new Vector2(2,2);
        [Description("Factor to multiply in the scaling of each octave pass")]
        public Vector2 Lacunarity { get { return lacunarity_; } set { lacunarity_ = value; OnPropertyChanged(); } }

        int octaves_ = 3;
        [Description("Number of passes to accumulate")]
        public int Octaves { get { return octaves_; } set { octaves_ = value; OnPropertyChanged(); } }

        float gain_ = 0.5f;
        [Description("Gain of each successive octave")]
        public float Gain { get { return gain_; } set { gain_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Octave Sum";
        }

        public override bool WillForceExecute()
        {
            return true;
        }

        public override void Execute(object param)
        {
            ForceExecuteUpstream(param);
            Vector4 currentValue = InputSockets[0].GetColor().ToVector4();

            float currentGain = Gain;
            float gainSum = 1.0f + currentGain;
            int octaves = Math.Max(1, Octaves);

            Vector4 coord = (Vector4)param;
            for (int i = 0; i < octaves; ++i)
            {
                coord.X *= Lacunarity.X;
                coord.Y *= Lacunarity.Y;
                ForceExecuteUpstream(coord);
                currentValue += InputSockets[0].GetColor().ToVector4() * currentGain;
                gainSum += currentGain;
                currentGain *= currentGain;
            }

            currentValue.X = Mathf.Normalize(currentValue.X, 0.0f, gainSum);
            currentValue.Y = Mathf.Normalize(currentValue.Y, 0.0f, gainSum);
            currentValue.Z = Mathf.Normalize(currentValue.Z, 0.0f, gainSum);
            currentValue.W = Mathf.Normalize(currentValue.W, 0.0f, gainSum);
            OutputSockets[0].Data = currentValue;
        }
    }

    [Description("Uses a lookup table to perform color adjustment. Based on the same method as color grading in realtime games.")]
    public partial class LookupTableRemapNode : ChannelInOutNode
    {
        int lutSize_ = 16;
        public override void Construct()
        {
            base.Construct();
            Name = "LUT Remap";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "LUT", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Channel });
        }

        public override void Execute(object param)
        {
            var coord = (Vector4)param;

            var input = InputSockets[0].GetColor();

            //float lutSize = 16.0f;
            //float scale = (lutSize - 1.0f) / lutSize;
            //float offset = 1.0f / (2.0f * lutSize);

            float cellDim = 1.0f / 16.0f;
            // position within an "X" cell
            float x = (input.R / 255.0f) / 16.0f;
            // position within a "Y" cell
            float y = input.G / 255.0f;
            int zTile = input.B / 16;
            float zTileFract = input.B / 16.0f - zTile;

            // if we rounded up then we need to flip this all around
            if (zTileFract < 0)
            {
                zTile -= 1;
                zTileFract = 1.0f - zTileFract;
            }

            //float one16 = 1 / 16.0f;
            //float x = input.R / 255.0f;
            //x *= one16; // map to the size of one cell
            //float y = input.G / 255.0f;
            //x += input.B * one16;

            ForceExecuteSocketUpstream(new Vector4(x + cellDim * Mathf.Clamp(zTile, 0, 15), y, coord.Z, coord.W), InputSockets[1]);
            var a = InputSockets[1].GetColor();
            ForceExecuteSocketUpstream(new Vector4(x + cellDim * Mathf.Clamp(zTile + 1, 0, 15), y, coord.Z, coord.W), InputSockets[1]);
            var b = InputSockets[1].GetColor();
            OutputSockets[0].Data = Color.Lerp(a, b, zTileFract);
        }
    }

    [Description("Remaps a grayscale value based on a lookup from an image or color source")]
    public partial class GradientLookupTable : TexGenNode
    {
        int index_ = -1;
        int lutHeight_ = 1;

        [Description("Specifies which row in the LUT to use. Use -1 to select a random row")]
        public int Index { get { return index_; } set { index_ = value; OnPropertyChanged(); } }
        [Description("Number of records in the lookup table. There should be one record for each pixel.")]
        public int LUTHeight { get { return lutHeight_; } set { lutHeight_ = value; OnPropertyChanged(); } }

        public override bool WillForceExecute() { return true; }

        public override void Construct()
        {
            base.Construct();
            Name = "Gradient LUT Remap";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Src", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Grayscale });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "LUT", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Channel });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Channel });
        }

        int randomIndex_ = 0;
        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            if (index_ == -1)
                randomIndex_ = new Random().Next(Math.Max(lutHeight_, 1));
        }

        public override void Execute(object param)
        {
            var coord = (Vector4)param;

            ForceExecuteSocketUpstream(param, InputSockets[0]);
            var lookupData = InputSockets[0].GetFloatData();

            int ht = Math.Max(lutHeight_, 1);
            float yCoord = 0.0f;
            if (index_ == -1)
                yCoord = randomIndex_ / (float)ht;
            else
                yCoord = index_ / (float)ht;

            ForceExecuteSocketUpstream(new Vector4(lookupData, yCoord, coord.Z, coord.W), InputSockets[1]);
            OutputSockets[0].Data = InputSockets[1].GetColor();
        }
    }

    [Description("Replaces unspecified (null) values with the given fill channel")]
    public partial class FillEmpty : ChannelInOutNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Fill Empty";
            AddInput(new GraphSocket(this) { Name = "Fill", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Channel });
        }

        public override void Execute(object param)
        {
            if (InputSockets[0].Data == null)
                OutputSockets[0].Data = InputSockets[1].Data;
            else
                OutputSockets[0].Data = InputSockets[0].Data;
        }
    }

    [Description("Pulls input values into the center using rotated offsets. Useful for hatching and pinching effects.")]
    public partial class HatchFilter : TexGenNode
    {
        public enum PinchMode
        {
            Cross,
            Diagonal,
            Octa
        }

        float power_ = 1.0f;
        PinchMode pinch_ = PinchMode.Diagonal;

        [Description("Intensity of the pinch")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public float Power
        {
            get { return power_; }
            set { power_ = value; OnPropertyChanged(); }
        }

        float stepSize_ = 0.2f;
        [Description("Length of the sample step")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public float StepSize
        {
            get { return stepSize_; }
            set { stepSize_ = value; OnPropertyChanged(); }
        }

        [Description("Layout of the pinch")]
        public PinchMode Pinch
        {
            get { return pinch_; }
            set { pinch_ = value;  OnPropertyChanged(); }
        }

        public override bool WillForceExecute()
        {
            return true;
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Hatch";
            AddInput(new GraphSocket(this) { Name = "Src", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Channel });
            AddInput(new GraphSocket(this) { Name = "Power", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Grayscale });
            AddInput(new GraphSocket(this) { Name = "Angle", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Grayscale });
            AddOutput(new GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Channel });
        }

        static Matrix[] CrossMat =
        {
            Matrix.CreateRotationZ(0 * Mathf.DEGTORAD),
            Matrix.CreateRotationZ(90 * Mathf.DEGTORAD),
            Matrix.CreateRotationZ(180 * Mathf.DEGTORAD),
            Matrix.CreateRotationZ((90+180) * Mathf.DEGTORAD)
        };

        static Matrix[] DiagMat =
        {
            Matrix.CreateRotationZ(45 * Mathf.DEGTORAD),
            Matrix.CreateRotationZ((45+90) * Mathf.DEGTORAD),
            Matrix.CreateRotationZ((45+180) * Mathf.DEGTORAD),
            Matrix.CreateRotationZ((45+90+180) * Mathf.DEGTORAD)
        };

        static Matrix[] OctaMat =
        {
            Matrix.CreateRotationZ(0 * Mathf.DEGTORAD),
            Matrix.CreateRotationZ(90 * Mathf.DEGTORAD),
            Matrix.CreateRotationZ(180 * Mathf.DEGTORAD),
            Matrix.CreateRotationZ((90+180) * Mathf.DEGTORAD),
            Matrix.CreateRotationZ(45 * Mathf.DEGTORAD),
            Matrix.CreateRotationZ((45+90) * Mathf.DEGTORAD),
            Matrix.CreateRotationZ((45+180) * Mathf.DEGTORAD),
            Matrix.CreateRotationZ((45+90+180) * Mathf.DEGTORAD)
        };

        public override void Execute(object param)
        {
            Vector4 pos = (Vector4)param;
            Vector2 scl = pos.ZW();
            Vector2 coord = pos.XY();

            float maxDim = Math.Max(scl.X, scl.Y);
            float invDim = 1.0f / maxDim;

            ForceExecuteUpstream(param);
            float angle = 0.0f;
            angle = InputSockets[2].GetFloatData(0.0f) * 360.0f;
            float power = InputSockets[1].GetFloatData(power_);


            Vector4 sum = Vector4.Zero;
            Matrix transMat = Matrix.CreateRotationZ(angle * Mathf.DEGTORAD);
            if (this.pinch_ == PinchMode.Cross)
            {
                for (int i = 0; i < 4; ++i)
                {
                    var m = CrossMat[i] * transMat;
                    var offs = Vector2.Transform(Vector2.UnitX, m);
                    var pt = pos.XY() + offs * stepSize_ * power;
                    ForceExecuteSocketUpstream(new Vector4(pt.X, pt.Y, scl.X, scl.Y), InputSockets[0]);
                    sum += InputSockets[0].GetVector4();
                }
                sum /= 4;
            }
            else if (this.pinch_ == PinchMode.Diagonal)
            {
                for (int i = 0; i < 4; ++i)
                {
                    var m = DiagMat[i] * transMat;
                    var offs = Vector2.Transform(Vector2.UnitX, m);
                    var pt = pos.XY() + offs * stepSize_ * power;
                    ForceExecuteSocketUpstream(new Vector4(pt.X, pt.Y, scl.X, scl.Y), InputSockets[0]);
                    sum += InputSockets[0].GetVector4();
                }
                sum /= 4;
            }
            else if (this.pinch_ == PinchMode.Octa)
            {
                for (int i = 0; i < 8; ++i)
                {
                    var m = OctaMat[i] * transMat;
                    var offs = Vector2.Transform(Vector2.UnitX, m);
                    var pt = pos.XY() + offs * stepSize_ * power;
                    ForceExecuteSocketUpstream(new Vector4(pt.X, pt.Y, scl.X, scl.Y), InputSockets[0]);
                    sum += InputSockets[0].GetVector4();
                }
                sum /= 8;
            }
            OutputSockets[0].Data = sum;
        }
    }

    [Description("Distorts the source data in a wave pattern")]
    public partial class WaveFilter : TexGenNode
    {
        float angle_ = 0.0f;
        float frequency_ = 2.0f;
        float amplitude_ = 0.5f;

        [Description("Angle of the wave")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public float Angle
        {
            get { return angle_; }
            set { angle_ = value; OnPropertyChanged(); }
        }

        [Description("Height of a wave crest from the lowest point")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public float Amplitude
        {
            get { return amplitude_; }
            set { amplitude_ = value; OnPropertyChanged(); }
        }

        [Description("Number of crests in the wave")]
        public float Frequency
        {
            get { return frequency_; }
            set { frequency_ = value; OnPropertyChanged(); }
        }

        public override bool WillForceExecute()
        {
            return true;
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Wave";
            AddInput(new GraphSocket(this) { Name = "Src", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Channel });
            AddInput(new GraphSocket(this) { Name = "Wave Pow", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Grayscale });
            AddInput(new GraphSocket(this) { Name = "Angle", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Grayscale });
            AddOutput(new GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Channel });
        }

        public override void Execute(object param)
        {
            Vector4 pos = (Vector4)param;
            Vector2 scl = pos.ZW();
            Vector2 coord = pos.XY();
            float height = Math.Max(scl.X, scl.Y);
            float amp = amplitude_;// * height;

            ForceExecuteUpstream(param);
            float wavePow = InputSockets[1].GetFloatData(1.0f);

            float angle = angle_;
            if (InputSockets[2].HasConnections())
                angle = InputSockets[2].GetFloatData(0.0f) * 360.0f;

            float dx = Mathf.Cos(wavePow * pos.X * frequency_) * amp;
            float dy = Mathf.Sin(wavePow * pos.X * frequency_) * amp;
            float x = pos.X + dx;
            float y = pos.Y + dy;
            Vector2 p = Vector2.Transform(new Vector2(x, y), Matrix.CreateRotationZ(angle * Mathf.DEGTORAD));

            ForceExecuteUpstream(new Vector4(p.X, p.Y, scl.X, scl.Y));
            OutputSockets[0].Data = InputSockets[0].Data;
        }
    }
}
