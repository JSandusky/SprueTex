using System;

using SprueKit.Data.Graph;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Matrix = Microsoft.Xna.Framework.Matrix;
using IntVector2 = PluginLib.IntVector2;
using System.ComponentModel;

namespace SprueKit.Data.TexGen
{

    public static class NoiseHelpers
    {
        public static Vector4 Make4D(Vector2 coord, Vector2 tiling)
        {
            float x1 = -1, y1 = -1, x2 = 1, y2 = 1;// tiling.X, y2 = tiling.Y;
            float s = coord.X;
            float t = coord.Y;
            float dx = x2 - x1;
            float dy = y2 - y1;


            float nx = (float)(x1 + Math.Cos(s * 2 * 3.141596f) * dx / (2 * 3.141596f));
            float ny = (float)(y1 + Math.Cos(t * 2 * 3.141596f) * dy / (2 * 3.141596f));
            float nz = (float)(x1 + Math.Sin(s * 2 * 3.141596f) * dx / (2 * 3.141596f));
            float nw = (float)(y1 + Math.Sin(t * 2 * 3.141596f) * dy / (2 * 3.141596f));

            return new Vector4(nx * tiling.X, ny * tiling.Y, nz * tiling.X, nw * tiling.Y);
        }
    }

    [Description("Emits tubular rows with optional perturbation")]
    public partial class RowsGenerator : TexGenNode
    {
        int rowCount_ = 6;
        float perturbPower_ = 1.0f;
        bool vertical_ = true;
        bool alternateDeadColumns_ = false;

        [PropertyData.AllowPermutations]
        [Description("How many tubes to pack into the width/heigh dimension")]
        [PropertyData.ValidStep(Value = 2.0f)]
        public int RowCount { get { return rowCount_; } set { rowCount_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Adjusts the intensity of the perturbation inputs")]
        public float PerturbationPower { get { return perturbPower_; } set { perturbPower_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Determines whether tubes will be oriented vertically or horizontally")]
        public bool Vertical { get { return vertical_; } set { vertical_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Rows will not be output in alteration")]
        public bool AlternateDeadColumns { get { return alternateDeadColumns_; } set { alternateDeadColumns_ = value; OnPropertyChanged(); } }

        public RowsGenerator()
        {
        }
        public override void Construct()
        {
            Name = "Rows";
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Channel, Name = "Perturb" });
            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Color, Name = "Out", IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            float perturb = InputSockets[0].GetFloatData() * perturbPower_;

            Vector4 v = (Vector4)param;
            Vector2 pos = new Vector2(v.X, v.Y);

            float samplingX = vertical_ ? pos.X : pos.Y;

            float sineValue = (float)Math.Sin((samplingX + perturb) * 3.141596f * rowCount_);
            if (alternateDeadColumns_)
                sineValue = Mathf.Clamp(sineValue, 0.0f, 1.0f);
            else
                sineValue = Mathf.Clamp(Math.Abs(sineValue), 0.0f, 1.0f);

            OutputSockets[0].Data = sineValue;
        }
    }

    [Description("Outputs a simple colored checker pattern with optional perturbation")]
    public partial class CheckerGenerator : TexGenNode
    {
        Color colorA_ = Color.White;
        Color colorB_ = Color.Black;
        PluginLib.IntVector2 tileCount_ = new PluginLib.IntVector2(4, 4);

        [PropertyData.AllowPermutations]
        [Description("Color of one set of tiles")]
        public Color ColorA { get { return colorA_; } set { colorA_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Color of the other set of tiles")]
        public Color ColorB { get { return colorB_; } set { colorB_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Number of checker tiles to output per side")]
        public PluginLib.IntVector2 TileCount { get { return tileCount_; } set { tileCount_ = value; OnPropertyChanged(); } }

        public CheckerGenerator()
        {
        }
        public override void Construct()
        {
            Name = "Checker";
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Channel, Name = "Perturb X" });
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Channel, Name = "Perturb Y" });
            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Color, Name = "Out", IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            float dX = InputSockets[0].GetFloatData();
            float dY = InputSockets[1].GetFloatData();

            Vector4 coord = (Vector4)param;
            Vector2 samplePos = new Vector2(coord.X + dX, coord.Y + dY);

            int horizontalIndex = ((int)samplePos.X) / 1;
            int verticalIndex = ((int)samplePos.Y) / 1;

            bool useAlternateColor = (((int)((samplePos.X * tileCount_.X)) + ((int)((samplePos.Y * tileCount_.Y)))) & 1) == 0;
            if (useAlternateColor)
                OutputSockets[0].Data = colorB_;
            else
                OutputSockets[0].Data = colorA_;
        }
    }

    [Description("Generates a basic brick and mortar pattern with optional perturbation")]
    public partial class BricksGenerator : TexGenNode
    {
        Vector2 tileSize_ = new Vector2(0.5f, 0.5f);
        float rowOffset_ = 0.5f;
        Vector2 baseOffset_ = new Vector2(0.0f, 0.0f);
        Vector2 gutter_ = new Vector2(0.05f, 0.05f);
        Vector2 perturbPower_ = new Vector2(1, 1);
        Color blockColor_ = Color.White;
        Color groutColor_ = Color.Black;
        bool randomizeBlocks_ = false;

        [PropertyData.AllowPermutations]
        [Description("Adjusts the starting offset of the entire set of bricks. Useful when compositing the brick with something else.")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public Vector2 BaseOffset { get { return baseOffset_; } set { baseOffset_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Adjusts the size of each individual brick's space, gutter is taken out of this")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public Vector2 TileSize { get { return tileSize_; } set { tileSize_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Adjusts the offset of each row of bricks")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public float RowOffset { get { return rowOffset_; } set { rowOffset_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Controls how much gutter space exists between the bricks")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public Vector2 Gutter { get { return gutter_; } set { gutter_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Adjusts the intensity with which perturbation is applied")]
        public Vector2 PerturbPower { get { return perturbPower_; } set { perturbPower_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Color used for the bricks")]
        public Color BlockColor { get { return blockColor_; } set { blockColor_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Color to use for the mortar between blocks")]
        public Color GroutColor { get { return groutColor_; } set { groutColor_ = value; OnPropertyChanged(); } }
        [Description("Block colors will use a randomized value adjustment")]
        public bool RandomizeBlocks { get { return randomizeBlocks_; } set { randomizeBlocks_ = value; OnPropertyChanged(); } }

        public BricksGenerator()
        {
        }
        public override void PostDeserialize()
        {
            base.PostDeserialize();
            if (Version <= 1)
            {
                AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Channel, Name = "Shift" });
                Version = 2;
            }
        }
        public override void Construct()
        {
            Version = 2;
            Name = "Bricks";
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Channel, Name = "Perturb X" });
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Channel, Name = "Perturb Y" });
            AddInput(new GraphSocket(this) { TypeID = SocketTypeID.Channel, Name = "Shift" });
            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Color, Name = "Out", IsInput = false, IsOutput = true });
            AddOutput(new GraphSocket(this) { TypeID = SocketTypeID.Grayscale, Name = "Mask", IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            float dX = InputSockets[0].GetFloatData() * perturbPower_.X;
            float dY = InputSockets[1].GetFloatData() * perturbPower_.Y;
            Vector4 coord = (Vector4)param;
            Vector2 pos = new Vector2(coord.X + dX + BaseOffset.X, coord.Y + dY + BaseOffset.Y);

            float xtile = pos.X / tileSize_.X;
            float ytile = pos.Y / tileSize_.Y;
            int row = (int)Math.Floor(ytile);

            ForceExecuteSocketUpstream(new Vector4(0.0f, row * tileSize_.Y, coord.Z, coord.W), InputSockets[2]);

            xtile += (rowOffset_ * row * InputSockets[2].GetFloatData(1.0f)) % 1.0f;
            int column = (int)Math.Floor(xtile);

            xtile -= column;
            ytile -= row;

            float xGutter = gutter_.X;
            float yGutter = gutter_.Y;

            float value = xtile > (0.0f + gutter_.X / 2.0f) && xtile < (1.0f - gutter_.X / 2.0f) && ytile > (0.0f + gutter_.Y / 2.0f) && ytile < (1.0f - gutter_.Y / 2.0f) ? 1.0f : 0.0f;

            // Get the maximum value, then clip
            OutputSockets[0].Data = value > 0.5f ? blockColor_ : groutColor_;
            if (randomizeBlocks_ && value > 0.5f)
            {
                var blockCol = blockColor_.ToVector4();
                column += 3257;
                row += 6573;
                float randVal = Mathf.Clamp((float)(new Random(column * row).NextDouble()) + 0.2f, 0.0f, 1.0f);
                OutputSockets[0].Data = new Color(blockCol.X * randVal, blockCol.Y * randVal, blockCol.Z * randVal, blockCol.W);
            }
            OutputSockets[1].Data = value > 0.5f ? 1.0f : 0.0f;
        }
    }

    public partial class NoiseGenerator : TexGenNode
    {
        protected FastNoise noise_ = new FastNoise();
        protected bool inverted_ = false;
        protected Vector2 period_ = new Vector2(8, 8);

        [PropertyData.AllowPermutations]
        [Description("Controls the seed value for the RNG")]
        [PropertyData.ValidStep(Value = 5.0f)]
        public int Seed { get { return noise_.GetSeed(); } set { noise_.SetSeed(value); OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Output will be inverted")]
        public bool Inverted { get { return inverted_; } set { inverted_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Density of the noise in a single tiling iteration")]
        [PropertyData.ValidStep(Value = 2.0f)]
        public Vector2 Period { get { return period_; } set { period_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Method of interpolating the noise values")]
        public FastNoise.Interp Interpolation { get { return noise_.m_interp; } set { noise_.SetInterp(value); OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("How densely packed the noise is")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public float Frequency { get { return noise_.m_frequency; } set { noise_.SetFrequency(value); OnPropertyChanged(); } }

        public NoiseGenerator()
        {
        }
        public override void Construct()
        {
            noise_.SetFrequency(1);
            OutputSockets.Add(new Data.Graph.GraphSocket(this) { TypeID = SocketTypeID.Color, IsInput = false, IsOutput = true });
            OutputSockets.Add(new Data.Graph.GraphSocket(this) { TypeID = SocketTypeID.Grayscale, IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            noise_.m_period = new FastNoise.Float3(period_.X, period_.Y, 0);
            Vector4 coord = (Vector4)param;
            var newCoord = NoiseHelpers.Make4D(new Vector2(coord.X, coord.Y), new Vector2(period_.X, period_.Y));
            //float val = noise_.GetNoise(newCoord.X, newCoord.Y, newCoord.Z, newCoord.W);
            float val = noise_.GetNoise(coord.X, coord.Y);
            if (noise_.m_noiseType != FastNoise.NoiseType.Cellular)
                val = Mathf.Normalize(val, -1, 1);
            if (inverted_)
                val = 1.0f - val;
            OutputSockets[0].Data = Color.FromNonPremultiplied(new Vector4(val, val, val, 1));
            OutputSockets[1].Data = val;
        }
    }

    [Description("Generates random white noise")]
    [PropertyData.PropertyIgnore("Period")]
    public partial class WhiteNoiseGenerator : NoiseGenerator
    {
        public WhiteNoiseGenerator()
        {
            noise_.SetNoiseType(FastNoise.NoiseType.WhiteNoise);
        }
        public override void Construct()
        {
            base.Construct();
            Name = "White Noise";
            noise_.SetNoiseType(FastNoise.NoiseType.WhiteNoise);
            noise_.SetFrequency(256);
        }
    }

    [Description("Emits the classic perlin noise")]
    public partial class PerlinNoiseGenerator : NoiseGenerator
    {
        public PerlinNoiseGenerator()
        {
            noise_.SetNoiseType(FastNoise.NoiseType.Perlin);
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Perlin Noise";
            noise_.SetNoiseType(FastNoise.NoiseType.Perlin);
            noise_.SetFrequency(8);
        }
    }

    [Description("Outputs fractal noise, often seen in terrain height maps")]
    public partial class FBMNoiseGenerator : NoiseGenerator
    {
        [Description("Summation method used for combining the octaves of the fractal noise")]
        public FastNoise.FractalType Fractal { get { return noise_.m_fractalType; } set { noise_.SetFractalType(value); OnPropertyChanged(); } }
        [Description("Intensity of each combination of octaves")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public float Gain { get { return noise_.m_gain; } set { noise_.SetFractalGain(value); OnPropertyChanged(); } }
        [Description("Multiplier used to scale each successive octave")]
        [PropertyData.ValidStep(Value = 0.5f)]
        public float Lacunarity { get { return noise_.m_lacunarity; } set { noise_.SetFractalLacunarity(value); OnPropertyChanged(); } }
        [Description("Number of octaves to use")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public int Octaves { get { return noise_.m_octaves; } set { noise_.SetFractalOctaves(value); OnPropertyChanged(); } }

        public FBMNoiseGenerator()
        {
            noise_.SetNoiseType(FastNoise.NoiseType.ValueFractal);
        }
        public override void Construct()
        {
            base.Construct();
            Name = "FBM Noise";
            noise_.SetNoiseType(FastNoise.NoiseType.ValueFractal);
            noise_.SetFrequency(8);
        }
    }

    [Description("Produces several different kinds of cellular noise ranging from cells to plasma")]
    [PropertyData.PropertyIgnore("Interpolation")]
    public partial class VoronoiGenerator : NoiseGenerator
    {
        bool knockOutMode_ = false;
        [Description("How the distance between points is calculated")]
        public FastNoise.CellularDistanceFunction Function { get { return noise_.m_cellularDistanceFunction; } set { noise_.SetCellularDistanceFunction(value); OnPropertyChanged(); } }
        [Description("The output style of the cellular function")]
        public FastNoise.CellularReturnType CellType { get { return noise_.m_cellularReturnType; } set { noise_.SetCellularReturnType(value); OnPropertyChanged(); } }
        [Description("Cells will be randomly dropped from the output to increase the sparsity")]
        public bool KnockOutMode { get { return knockOutMode_; } set { knockOutMode_ = value; OnPropertyChanged(); } }

        public VoronoiGenerator()
        {
            noise_.SetNoiseType(FastNoise.NoiseType.Cellular);
            noise_.SetCellularNoiseLookup(new FastNoise(7865));
        }
        public override void Construct()
        {
            base.Construct();
            Name = "Voronoi";
            Frequency = 4.0f;
            noise_.SetNoiseType(FastNoise.NoiseType.Cellular);
            noise_.SetCellularNoiseLookup(new FastNoise(7865));
        }

        public override void Execute(object param)
        {
            base.Execute(param);
            if (knockOutMode_)
            {
                float val = OutputSockets[1].GetFloatData();
                val = Mathf.Denormalize(val, -1.0f, 1.0f);
                val = Math.Max(0, val);
                OutputSockets[1].Data = val;
                OutputSockets[0].Data = new Vector4(val, val, val, 1);
            }
        }
    }

    [Description("Generates a random scratch like pattern")]
    public partial class ScratchesGenerator : TexGenNode
    {
        FastNoise noise_ = new FastNoise();
        int density_ = 18;
        float length_ = 0.1f;
        bool inverted_ = false;
        bool fadeOff_ = false;

        [Description("Random number seed for the RNG")]
        [PropertyData.ValidStep(Value = 5.0f)]
        public int Seed { get { return noise_.m_seed; } set { noise_.SetSeed(value); OnPropertyChanged(); } }
        [Description("How many scratches should be emitted")]
        [PropertyData.ValidStep(Value = 5.0f)]
        public int Density { get { return density_; } set { density_ = value; OnPropertyChanged(); } }
        [Description("The length of each scratch")]
        [PropertyData.ValidStep(Value = 0.02f)]
        public float Length { get { return length_; } set { length_ = value; OnPropertyChanged(); } }
        [Description("Output will be inverted")]
        public bool Inverted { get { return inverted_; } set { inverted_ = value; OnPropertyChanged(); } }
        [Description("Wether scratches will fade out along their length or not")]
        public bool FadeOff { get { return fadeOff_; } set { fadeOff_ = value; OnPropertyChanged(); } }

        public ScratchesGenerator()
        {
        }
        public override void Construct()
        {
            Name = "Scratches";
            OutputSockets.Add(new GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
        }

        float GetRandVal(ref Vector3 offset) {
            float ret = noise_.GetWhiteNoise(offset.X, offset.Y * 67);
            offset *= 1.2f;
            ret *= 2.0f;
            ret += 1.0f;
            return ret;
        }

        Vector2 ClosestPoint(Vector2 a, Vector2 b, Vector2 to)
        {
            Vector2 dir = b - a;
            return a + Mathf.Clamp01(Vector2.Dot((to - a), dir) / dir.LengthSquared()) * dir;
        }

        public override void Execute(object param)
        {
            Vector4 p = (Vector4)param;
            Vector2 pos = new Vector2(p.X, p.Y);
            float output = 0.0f;

            Vector3 offset = new Vector3(5, 7, 11);
            float len2 = Length * Length;

            for (int np = 0; np < Density; ++np)
            {

                float px = GetRandVal(ref offset);
                float py = GetRandVal(ref offset);
                float ox = GetRandVal(ref offset);
                float oy = GetRandVal(ref offset);

                Vector2 start = new Vector2(ox, oy);
                Vector2 end = new Vector2(px, py);
                Vector2 closest = ClosestPoint(start, end, pos);
                float distance = (closest - pos).LengthSquared();
                if (distance < len2)
                {
                    float mult = 1.0f;
                    if (FadeOff)
                    {
                        mult = (closest - start).LengthSquared() / (start - end).LengthSquared();
                    }
                    distance = len2 - distance;
                    distance = Mathf.Normalize(distance, 0, len2);
                    output += Mathf.Clamp01(distance) * mult;
                }
            }

            output = Mathf.Clamp01(output);
            if (Inverted)
                output = 1.0f - output;
            OutputSockets[0].Data = output;
        }
    }

    [Description("Outputs the combination of different mathematical functions in different ways, a basic building block")]
    public partial class TextureFunction2D : TexGenNode
    {
        public enum TextureFunctionFunction
        {
            None,
            Sin,
            Cos,
            Saw,
            Square,
            Triangle
        };

        public enum TextureFunctionMix
        {
            Add,
            Subtract,
            Mul,
            Divide,
            Max,
            Min,
            Pow,
            Average,
            Offset
        };

        TextureFunctionFunction xFunc_ = TextureFunctionFunction.Cos;
        TextureFunctionFunction yFunc_ = TextureFunctionFunction.Cos;
        TextureFunctionFunction diagFunc_ = TextureFunctionFunction.None;
        TextureFunctionMix mix_ = TextureFunctionMix.Add;
        TextureFunctionMix diagMix_ = TextureFunctionMix.Add;
        Vector3 pertrub_ = new Vector3(0.1f, 0.1f, 0.1f);
        Vector2 period_ = new Vector2(32, 32);
        Vector2 offset_ = new Vector2();

        [Description("Offset to shift the output by")]
        public Vector2 Offset { get { return offset_; } set { offset_ = value; OnPropertyChanged(); } }
        [Description("Intensity to to apply to perturbation inputs")]
        public Vector3 Perturb { get { return pertrub_; } set { pertrub_ = value; OnPropertyChanged(); } }
        [Description("Frequency at which the output repeats")]
        public Vector2 Period { get { return period_; } set { period_ = value; OnPropertyChanged(); } }
        [Description("Function to use on the X coordinate")]
        public TextureFunctionFunction XFunction { get { return xFunc_; } set { xFunc_ = value; OnPropertyChanged(); } }
        [Description("Function to use on the Y coordinate")]
        public TextureFunctionFunction YFunction { get { return yFunc_; } set { yFunc_ = value; OnPropertyChanged(); } }
        [Description("Function to use on the XY diagonal")]
        public TextureFunctionFunction DiagonalFunction { get { return diagFunc_; } set { diagFunc_ = value; OnPropertyChanged(); } }
        [Description("Method of mixing the functions together")]
        public TextureFunctionMix Mix { get { return mix_; } set { mix_ = value; OnPropertyChanged(); } }
        [Description("Method of mixing the diagonal function with the result of the other two")]
        public TextureFunctionMix DiagonalMix { get { return diagMix_; } set { diagMix_ = value; OnPropertyChanged(); } }

        public TextureFunction2D()
        {
        }
        public override void Construct()
        {
            Name = "2D Function";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Pertrub X", TypeID = SocketTypeID.Channel });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Pertrub Y", TypeID = SocketTypeID.Channel });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Pertrub Diag", TypeID = SocketTypeID.Channel });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Grayscale, IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            Vector4 p = (Vector4)param;
            Vector2 coord = new Vector2(p.X, p.Y);

            coord.X += InputSockets[0].GetFloatData() * pertrub_.X;
            coord.Y += InputSockets[1].GetFloatData() * pertrub_.Y;

            coord += Offset;
            coord *= Period;

            float diagVal = DoFunction(coord.Length() * Vector2.Dot(Vector2.Normalize(p.XY()), Vector2.Normalize(Vector2.One)) + InputSockets[2].GetFloatData() * pertrub_.Z, DiagonalFunction);
            float sX = 0;
            float sD = 0;
            float yVal = DoFunction(coord.Y, YFunction);
            if (Mix == TextureFunctionMix.Offset)
                sX += yVal;
            if (DiagonalMix == TextureFunctionMix.Offset)
                sD += diagVal;
            float xVal = DoFunction(coord.X + sX + sD, XFunction);

            float finalVal = Mix != TextureFunctionMix.Offset ? DoMix(xVal, yVal, Mix) : xVal;
            if (DiagonalFunction != TextureFunctionFunction.None && DiagonalMix != TextureFunctionMix.Offset)
                finalVal = DoMix(finalVal, diagVal, DiagonalMix);


            finalVal = Mathf.Clamp01(finalVal);
            OutputSockets[0].Data = finalVal;
        }

        float DoMix(float xVal, float yVal, TextureFunctionMix mix)
        {
            switch (mix)
            {
                case TextureFunctionMix.Add:
                    return xVal + yVal;
                case TextureFunctionMix.Subtract:
                    return xVal - yVal;
                case TextureFunctionMix.Mul:
                    return xVal * yVal;
                case TextureFunctionMix.Divide:
                    return xVal / (yVal != 0.0f ? yVal : 1.0f);
                case TextureFunctionMix.Max:
                    return Math.Max(xVal, yVal);
                case TextureFunctionMix.Min:
                    return Math.Min(xVal, yVal);
                case TextureFunctionMix.Pow:
                    return Mathf.Pow(xVal, yVal);
                case TextureFunctionMix.Average:
                    return (xVal + yVal) * 0.5f;
            }
            return xVal + yVal * 0.5f;
        }

        float DoFunction(float val, TextureFunctionFunction func)
        {
            if (!val.IsFinite())
                val = 0.0f;
            switch (func)
            {
                case TextureFunctionFunction.Sin:
                    return (float)Math.Sin(val);
                case TextureFunctionFunction.Cos:
                    return (float)(Math.Cos(val));
                case TextureFunctionFunction.Saw:
                    return (float)(1.0f - (val - Math.Floor(val)));
                case TextureFunctionFunction.Square:
                    return Math.Sign(Math.Cos(val));
                case TextureFunctionFunction.Triangle:
                    return 1.0f - Math.Abs((val % 2.0f) - 1.0f);
            }
            return 0.0f;
        }
    }

    [Description("Generates a sequential chain pattern")]
    public partial class ChainGenerator : TexGenNode
    {
        Color backgroundColor_ = Color.Transparent;
        Color centerLinkColor_ = Color.White;
        Color connectingLinkColor_ = Color.Black;

        [Description("Color for the background")]
        public Color BackgroundColor { get { return backgroundColor_; } set { backgroundColor_ = value; OnPropertyChanged(); } }
        [Description("Color for the complete link")]
        public Color CenterLinkColor { get { return centerLinkColor_; } set { centerLinkColor_ = value; OnPropertyChanged(); } }
        [Description("Color for the link that spans off the upper and lower bounds")]
        public Color ConnectingLinkColor { get { return connectingLinkColor_; } set { connectingLinkColor_ = value; OnPropertyChanged(); } }

        public ChainGenerator()
        {
        }
        public override void Construct()
        {
            Name = "Chain";
            OutputSockets.Add(new GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Color });
            OutputSockets.Add(new GraphSocket(this) { Name = "Mask", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
        }

        public override void Execute(object param)
        {
            Vector4 p = (Vector4)param;
            Vector2 pos = new Vector2(p.X, p.Y);
            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 upperCenter = new Vector2(0.5f, 0.0f);
            Vector2 lowerCenter = new Vector2(0.5f, 1.0f);

            float radius = 0.5f;
            float thickness = 0.1f;
            float innerRadius = radius - thickness;
            var color = BackgroundColor;
            float mask = 0.0f;
            if (pos.X < 0.5f)
            {
                if (pos.Y > 0.5f)
                {
                    if (Mathf.Between(Vector2.Distance(center, pos), innerRadius, radius))
                    {
                        color = CenterLinkColor;
                        mask = 1.0f;
                    }
                    else if (Mathf.Between(Vector2.Distance(lowerCenter, pos), innerRadius, radius))
                    {
                        color = ConnectingLinkColor;
                        mask = 1.0f;
                    }
                }
                else
                {
                    if (Mathf.Between(Vector2.Distance(upperCenter, pos), innerRadius, radius))
                    {
                        color = ConnectingLinkColor;
                        mask = 1.0f;
                    }
                    else if (Mathf.Between(Vector2.Distance(center, pos), innerRadius, radius))
                    {
                        color = CenterLinkColor;
                        mask = 1.0f;
                    }
                }
            }
            else
            {
                if (pos.Y > 0.5f)
                {
                    if (Mathf.Between(Vector2.Distance(lowerCenter, pos), innerRadius, radius))
                    {
                        color = ConnectingLinkColor;
                        mask = 1.0f;
                    }
                    else if (Mathf.Between(Vector2.Distance(center, pos), innerRadius, radius))
                    {
                        color = CenterLinkColor;
                        mask = 1.0f;
                    }
                }
                else
                {
                    if (Mathf.Between(Vector2.Distance(center, pos), innerRadius, radius))
                    {
                        color = CenterLinkColor;
                        mask = 1.0f;
                    }
                    else if (Mathf.Between(Vector2.Distance(upperCenter, pos), innerRadius, radius))
                    {
                        color = ConnectingLinkColor;
                        mask = 1.0f;
                    }
                }
            }

            OutputSockets[0].Data = color;
            OutputSockets[1].Data = mask;
        }
    }

    [Description("Generates an interlocking chainmail pattern")]
    public partial class ChainMailGenerator : TexGenNode
    {
        Color backgroundColor_ = Color.Transparent;
        Color centerLinkColor_ = Color.White;
        Color connectingLinkColor_ = Color.Black;
        Vector2 chainSize_ = new Vector2(1, 1);

        [Description("Color for the background")]
        public Color BackgroundColor { get { return backgroundColor_; } set { backgroundColor_ = value; OnPropertyChanged(); } }
        [Description("Color for the centermost link")]
        public Color CenterLinkColor { get { return centerLinkColor_; } set { centerLinkColor_ = value; OnPropertyChanged(); } }
        [Description("Color for the links that tie into the centermost link")]
        public Color ConnectingLinkColor { get { return connectingLinkColor_; } set { connectingLinkColor_ = value; OnPropertyChanged(); } }
        [Description("Number of links in X and Y")]
        public Vector2 ChainSize { get { return chainSize_; } set { chainSize_ = value; OnPropertyChanged(); } }

        public ChainMailGenerator()
        {
        }
        public override void Construct()
        {
            Name = "Chain Mail";
            OutputSockets.Add(new GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Color });
            OutputSockets.Add(new GraphSocket(this) { Name = "Mask", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
        }

        public override void Execute(object param)
        {
            Vector4 paramVec = (Vector4)param;
            Vector2 p = new Vector2(paramVec.X, paramVec.Y) * ChainSize;
            float x = p.X % 1;
            float y = p.Y % 1;

            float Radius = 0.47f;
            float Width = 0.08f;

            float Fac = 0.0f;
            float Disp = 0.0f;

            float Rm = Radius - Width;
            float Rp = Radius + Width;

            float r = -1, r1, r2, r3, cr1, cr2, cr3;

            int fx = 0;
            int fy = 0;
            int flip = (y > x) ? 1 : 0;
            int flipt = y > (1 - x) ? 1 : 0;
            if (x > 0.5) { x = 1 - x; fx = 1; }
            if (y > 0.5) { y = 1 - y; fy = 1; }

            r1 = Mathf.Hypot(x - 0.5f, y - 0.5f);
            r2 = Mathf.Hypot(x - 0.5f, y + 0.5f);
            r3 = Mathf.Hypot(x + 0.5f, y - 0.5f);

            float xc = (p.X + 0.5f) % 1;
            float yc = (p.Y + 0.5f) % 1;

            int fxc = 0, fyc = 0, flipc = y > x ? 1 : 0;

            if (xc > 0.5) { xc = 1 - xc; fxc = 1; }
            if (yc > 0.5) { yc = 1 - yc; fyc = 1; }

            cr1 = Mathf.Hypot(xc - 0.5f, yc - 0.5f);
            cr2 = Mathf.Hypot(xc - 0.5f, yc + 0.5f);
            cr3 = Mathf.Hypot(xc + 0.5f, yc - 0.5f);

            Color color = BackgroundColor;

            if ((flip ^ flipt) != 0)
            {
                // base pattern
                r = Pattern(r1, r2, r3, fx, fy, Rm, Rp);
                if (r > -1)
                {
                    Fac = 1;
                    color = CenterLinkColor;
                    Disp = Arc(r);
                }
                else
                {
                    // connecting rings
                    r = Pattern(cr1, cr2, cr3, fxc, fyc, Rm, Rp);
                    if (r > -1)
                    {
                        Fac = 0.5f;
                        color = ConnectingLinkColor;
                        Disp = Fac * Arc(r);
                    }
                }
            }
            else
            {
                // connecting rings
                r = Pattern(cr1, cr2, cr3, fxc, fyc, Rm, Rp);
                if (r > -1)
                {
                    Fac = 0.5f;
                    color = ConnectingLinkColor;
                    Disp = Fac * Arc(r);
                }
                else
                {
                    // base patterm
                    r = Pattern(r1, r2, r3, fx, fy, Rm, Rp);
                    if (r > -1)
                    {
                        Fac = 1;
                        color = CenterLinkColor;
                        Disp = Arc(r);
                    }
                }
            }

            float val = Disp;
            OutputSockets[0].Data = color;
            OutputSockets[1].Data = Fac;
        }

        float Arc(float x)
        {
            return Mathf.Sqrt(1.0f - (x - 0.5f) * (x - 0.5f) / 0.25f);
        }

        float Remap(float a, float b, float[] ra)
        {
            if (Mathf.Between(ra[0], a, b)) { return ra[0]; }
            if (Mathf.Between(ra[1], a, b)) { return ra[1]; }
            if (Mathf.Between(ra[2], a, b)) { return ra[2]; }
            return -1;
        }

        float Pattern(float r1, float r2, float r3, int fx, int fy, float Rm, float Rp)
        {
            float[] x0y0 = { r3, r1, r2 };
            float[] x0y1 = { r2, r3, r1 };
            float[] x1y0 = { r1, r3, r2 };
            float[] x1y1 = { r2, r1, r3 };

            float r = -1;
            if (fx != 0)
            {
                if (fy != 0)
                    r = Remap(Rm, Rp, x1y1);
                else
                    r = Remap(Rm, Rp, x1y0);
            }
            else
            {
                if (fy != 0)
                    r = Remap(Rm, Rp, x0y1);
                else
                    r = Remap(Rm, Rp, x0y0);
            }
            return r;
        }
    }

    [Description("Generates 2 color scales")]
    public partial class ScalesGenerator : TexGenNode
    {
        Color evenColor_ = Color.Black;
        Color oddColor_ = Color.White;
        Vector2 scaleSize_ = new Vector2(1, 1);
        bool matched_ = false;

        [Description("One of the two colors of the scales")]
        public Color EvenColor { get { return evenColor_; } set { evenColor_ = value; OnPropertyChanged(); } }
        [Description("One of the two colors of the scales")]
        public Color OddColor { get { return oddColor_; } set { oddColor_ = value; OnPropertyChanged(); } }
        [Description("Dimensions of each scale")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public Vector2 ScaleSize { get { return scaleSize_; } set { scaleSize_ = value; OnPropertyChanged(); } }
        [Description("Whether scales will align perfectly or be staggered")]
        public bool Matched { get { return matched_; } set { matched_ = value; OnPropertyChanged(); } }

        public ScalesGenerator()
        {
        }
        public override void Construct()
        {
            Name = "Scales";
            OutputSockets.Add(new Data.Graph.GraphSocket(this) { Name = "Color", TypeID = SocketTypeID.Color, IsInput = false, IsOutput = true });
            OutputSockets.Add(new Data.Graph.GraphSocket(this) { Name = "Dist", TypeID = SocketTypeID.Color, IsInput = false, IsOutput = true });
            OutputSockets.Add(new Data.Graph.GraphSocket(this) { Name = "Mask", TypeID = SocketTypeID.Color, IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            Vector4 p = (Vector4)param;
            Vector2 coord = new Vector2(p.X, p.Y);

            float widthFraction = 1.0f / ScaleSize.Y;
            int row = (int)(coord.Y / widthFraction);
            float rowStart = row * widthFraction;
            float rowEnd = (row + 1) * widthFraction;
            float rowHeight = rowEnd - rowStart;
            float yInRow = Mathf.Normalize(coord.Y, rowStart, rowEnd);

            bool evenRow = (row % 2) == 1;
            float dist = 0.0f;
            if (evenRow || matched_) // even row
                dist = Mathf.Abs(Mathf.Cos(coord.X * ScaleSize.X * 3.141596f));
            else // odd row
                dist = Mathf.Abs(Mathf.Sin(coord.X * ScaleSize.X * 3.141596f));

            if (dist > yInRow)
                OutputSockets[0].Data = (evenRow ? EvenColor : OddColor);
            else
                OutputSockets[0].Data = (evenRow ? OddColor : EvenColor);
        }
    }

    [Description("Generates a textile weave pattern based on 'warp' and 'weft'")]
    public partial class WeaveGenerator : TexGenNode
    {
        int underrun_ = 1;
        int overrun_ = 1;
        int skip_ = 1;
        float warpWidth_ = 0.8f;
        float weftWidth_ = 0.8f;
        Color warpColor_ = Color.White;
        Color weftColor_ = Color.Black;
        Color baseColor_ = Color.Transparent;
        IntVector2 tiling_ = new IntVector2(4, 4);

        [PropertyData.AllowPermutations]
        [Description("How many strands of 'warp' the 'weft' strands will overlap")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public int UnderRun { get { return underrun_; } set { underrun_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("How many strands of 'weft' the 'warp' strands will overlap")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public int OverRun { get { return overrun_; } set { overrun_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Controls the alteration and density of the pattern")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public int Skip { get { return skip_; } set { skip_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [PropertyData.ValidStep(Value = -0.2f)]
        [Description("Adjusts the width of the 'warp' strands")]
        public float WarpWidth { get { return warpWidth_; } set { warpWidth_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Adjusts the width of the 'weft' strands")]
        [PropertyData.ValidStep(Value = -0.2f)]
        public float WeftWidth { get { return weftWidth_; } set { weftWidth_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Specifies the color of the 'warp' strands")]
        public Color WarpColor { get { return warpColor_; } set { warpColor_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Specifies the color of the 'weft' strands")]
        public Color WeftColor { get { return weftColor_; } set { weftColor_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Color of the area not covered by the weave pattern")]
        public Color BaseColor { get { return baseColor_; } set { baseColor_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("How dense the weave pattern is")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public IntVector2 Tiling { get { return tiling_; } set { tiling_ = value; OnPropertyChanged(); } }

        public WeaveGenerator()
        {
        }
        public override void Construct()
        {
            Name = "Weave";
            OutputSockets.Add(new GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Channel });
            OutputSockets.Add(new GraphSocket(this) { Name = "Dist", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
            OutputSockets.Add(new GraphSocket(this) { Name = "Mask", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
        }

        public override void Execute(object param)
        {
            Vector4 p = (Vector4)param;
            Vector2 pos = new Vector2(p.X, p.Y);
            int ny = UnderRun + OverRun;
            int nx = Scm(Skip, ny);

            float fx = (pos.X * Tiling.X) % 1.0f;
            float fy = (pos.Y * Tiling.Y) % 1.0f;

            int ix = (int)Math.Floor(fx * nx);
            int iy = (int)Math.Floor(fy * ny);

            float cx = (fx * nx) % 1.0f;
            float cy = (fy * ny) % 1.0f;

            int top = ((iy + Skip * ix) % ny) < OverRun ? 1 : 0;

            float lx = (1.0f - WarpWidth) / 2.0f;
            float hx = 1.0f - lx;
            float ly = (1.0f - WeftWidth) / 2.0f;
            float hy = 1.0f - lx;

            float dist = 0.0f;
            Color color = BaseColor;
            float mask = 0.0f;

            if (top != 0)
            {
                if (cx > lx && cx < hx)
                {
                    color = WarpColor;
                    dist = Mathf.Abs(0.5f - cx);
                    mask = 1.0f;
                }
                else if (cy > ly && cy < hy)
                {
                    color = WeftColor;
                    dist = Mathf.Abs(0.5f - cy);
                    mask = 1.0f;
                }
            }
            else
            {
                if (cy > ly && cy < hy)
                {
                    color = WeftColor;
                    dist = Mathf.Abs(0.5f - cy);
                    mask = 1.0f;
                }
                else if (cx > lx && cx < hx)
                {
                    color = WarpColor;
                    dist = Mathf.Abs(0.5f - cx);
                    mask = 1.0f;
                }
            }

            OutputSockets[0].Data = color;
            OutputSockets[1].Data = dist;
            OutputSockets[2].Data = mask;
        }

        // greatest common divisor
        int Gcd(int A, int B)
        {
            int a = A, b = B;
            if (a == 0) { return b; }
            while (b != 0)
            {
                if (a > b)
                {
                    a = a - b;
                }
                else
                {
                    b = b - a;
                }
            }
            return a;
        }

        // smallest common multiple (assumes a, b > 0 )
        int Scm(int a, int b) { return a * b / Gcd(a, b); }
    }

    public partial class OddBlocksGenerator : TexGenNode
    {

    }

    [Description("Emits several different kinds of basic color gradients")]
    public partial class GradientGenerator : TexGenNode
    {
        public enum GradientType
        {
            Linear,
            Reflected,
            Radial,
            Angle
        }

        Vector2 offset_ = new Vector2(0, 0);
        Color start_ = Color.Black;
        Color end_ = Color.White;
        float length_ = 1.0f;
        float angle_ = 0.0f;
        GradientType gradientType_ = GradientType.Linear;

        [PropertyData.AllowPermutations]
        [Description("Output will be shifted by this amount")]
        public Vector2 Offset { get { return offset_; } set { offset_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Specifies the starting color of the gradient")]
        public Color Start { get { return start_; } set { start_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Specifies the ending color of the gradient")]
        public Color End { get { return end_; } set { end_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("The overall 'reach' of the gradient")]
        [PropertyData.ValidStep(Value = 0.25f)]
        public float Length { get { return length_; } set { length_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Angular orientation of the gradient")]
        [PropertyData.ValidStep(Value = 45.0f)]
        public float Angle { get { return angle_; } set { angle_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Shape of the gradient")]
        public GradientType Type { get { return gradientType_; } set { gradientType_ = value; OnPropertyChanged(); } }

        public GradientGenerator()
        {
        }
        public override void Construct()
        {
            Name = "Gradient";
            OutputSockets.Add(new GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Channel });
        }

        Vector2 Rotate(Vector2 vec, float degrees)
        {
            float cosVal = Mathf.Cos(degrees * 0.0174533f);
            float sinVal = Mathf.Sin(degrees * 0.0174533f);
            float newX = vec.X * cosVal - vec.Y * sinVal;
            float newY = vec.X * sinVal + vec.Y * cosVal;
            return new Vector2(newX, newY);
        }

        Vector2 PerpendicularClockwise(Vector2 vec)
        {
            return new Vector2(-vec.Y, vec.X);
        }

        float Cross(Vector2 lhs, Vector2 rhs)
        {
            return lhs.X * rhs.Y - lhs.Y * rhs.X;
        }

        public override void Execute(object param)
        {
            Vector4 p = (Vector4)param;
            Vector2 UV = new Vector2(p.X, p.Y);

            // Check direction, get offset displacement, then normalize, get angle as DP
            Vector2 dirVec = Rotate(Vector2.UnitY, Angle);
            float sampleToOriginDP = Vector2.Dot(Vector2.Normalize(UV - Offset), dirVec);

            UV -= Offset;

            Color finalColor = Start;

            switch (gradientType_)
            {
                case GradientType.Reflected:
                case GradientType.Linear:
                    {
                        Vector2 emissionLineVec = PerpendicularClockwise(dirVec);
                        float distanceFromLine = (Cross(emissionLineVec, UV) / Length) * -1;
                        if (gradientType_ == GradientType.Reflected)
                            distanceFromLine = Mathf.Abs(distanceFromLine);
                        finalColor = Color.Lerp(Start, End, Mathf.Clamp01(distanceFromLine));
                    }
                    break;
                case GradientType.Radial:
                    {
                        float sampleLen = UV.Length() / Length;
                        finalColor = Color.Lerp(End, Start, Mathf.Clamp01(sampleLen));
                    }
                    break;
                case GradientType.Angle:
                    finalColor = Color.Lerp(End, Start, Mathf.Normalize(sampleToOriginDP, -1.0f, 1.0f) / Length);
                    break;
            }

            OutputSockets[0].Data = finalColor;
        }
    }

    [Description("Generates gabor noise via convolution")]
    [PropertyData.PropertyIgnore("Interpolation")]
    public partial class GaborNoise : NoiseGenerator
    {
        float alpha_ = 0.05f;
        float impules_ = 32.0f;
        float k_ = 1.0f;
        float f0_ = 0.0625f;
        float omega_ = Mathf.PI / 4.0f;
        float offset_ = 0.0f;

        [PropertyData.AllowPermutations]
        [DisplayName("Waviness")]
        [Description("Waviness of the frequency bands")]
        public float Alpha { get { return alpha_; } set { alpha_ = value; OnPropertyChanged(); } }

        [PropertyData.AllowPermutations]
        [DisplayName("Accumulation")]
        [Description("How many iterations to accumulate into each bad")]
        public float Impulses { get { return impules_; } set { impules_ = value; OnPropertyChanged(); } }

        [PropertyData.AllowPermutations]
        [DisplayName("Hardness")]
        [Description("Edge hardness of the bands")]
        public float K { get { return k_; } set { k_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [DisplayName("Frequency")]
        [Description("How densely packed the frequency bands will be")]
        public float F0 { get { return f0_; } set { f0_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [DisplayName("Angle")]
        [Description("Angle of the streaks in the gabor bands, in radians")]
        public float Omega { get { return omega_; } set { omega_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Gabor Noise";
        }

        public override void Execute(object param)
        {
            Vector2 p = ((Vector4)param).XY();

            float kernelRadius = Mathf.Sqrt((float)(-Math.Log(0.05) / Mathf.PI)) / Alpha;
            float impulseDensity = Impulses / (Mathf.PI * kernelRadius * kernelRadius);

            float x = p.X * Period.X;
            float y = p.Y * Period.Y;

            x /= kernelRadius;
            y /= kernelRadius;
            float int_x = (float)Math.Floor(x);
            float int_y = (float)Math.Floor(y);
            float frac_x = x - int_x, frac_y = y - int_y;
            int i = (int)int_x;
            int j = (int)int_y;
            float noise = 0.0f;
            for (int di = -1; di <= +1; ++di)
            {
                for (int dj = -1; dj <= +1; ++dj)
                {
                    noise += CellValue(i + di, j + dj, frac_x - di, frac_y - dj, impulseDensity, kernelRadius);
                }
            }

            if (Inverted)
                OutputSockets[0].Data = 1.0f - noise;
            else
                OutputSockets[0].Data = noise;
            // stupid
            OutputSockets[1].Data = OutputSockets[0].Data;
        }

        float CellValue(int i, int j, float x, float y, float impulseDensity, float kernelRadius)
        {
            int s = Morton(i, j); // nonperiodic noise
            if (s == 0) s = 1;

            float number_of_impulses_per_cell = impulseDensity * kernelRadius * kernelRadius;
            int number_of_impulses = (int)(Mathf.Normalize(noise_.GetValue(i, j), -1, 1) * number_of_impulses_per_cell);
            float noise = 0.0f;
            for (int ii = 0; ii < number_of_impulses; ++ii)
            {
                float x_i = Mathf.Normalize(noise_.GetValue(noise_.m_seed + ii, j * 13), -1, 1);
                float y_i = Mathf.Normalize(noise_.GetValue(noise_.m_seed + ii * 21, j), -1, 1);
                float w_i = noise_.GetValue(noise_.m_seed + i * 37, j * 37463);
                float omega_0_i = (float)(Mathf.Normalize(noise_.GetValue(noise_.m_seed + ii * 471, j * 7), -1, 1) * 2.0 * Mathf.PI);
                float x_i_x = x - x_i;
                float y_i_y = y - y_i;
                if (((x_i_x * x_i_x) + (y_i_y * y_i_y)) < 1.0)
                {
                    noise += w_i * Gabor(K, Alpha, F0, Omega, x_i_x * kernelRadius, y_i_y * kernelRadius); // anisotropic
                                                                                                           //noise += w_i * gabor(K_, a_, F_0_, omega_0_i, x_i_x * kernel_radius_, y_i_y * kernel_radius_); // isotropic
                }
            }
            return noise;
        }

        int Morton(int x, int y)
        {
            int z = 0;
            for (int i = 0; i < (4 * 8); ++i)
            {
                z |= ((x & (1 << i)) << i) | ((y & (1 << i)) << (i + 1));
            }
            return z;
        }

        float Gabor(float K, float a, float F_0, float omega_0, float x, float y)
        {
            float gaussian_envelop = K * Mathf.Exp(-Mathf.PI * (a * a) * ((x * x) + (y * y)));
            float sinusoidal_carrier = Mathf.Cos(2.0f * Mathf.PI * F_0 * ((x * Mathf.Cos(omega_0)) + (y * Mathf.Sin(omega_0))));
            return gaussian_envelop * sinusoidal_carrier;
        }
    }

    [Description("Uses noise derivatives to produce an extremely versatile noise")]
    [PropertyData.PropertyIgnore("Interpolation")]
    public partial class UberNoise : NoiseGenerator
    {
        float lacunarity_ = 2.0f;
        int octaves_ = 3;
        float gain_ = 0.5f;
        float perturbFeatures_ = 0.2f;
        float sharpness_ = 0.7f;
        float amplify_ = 0.7f;
        float altitudeErosion_ = 0.7f;
        float ridgeErosion_ = 0.7f;
        float slopeErosion_ = 0.7f;

        [PropertyData.AllowPermutations]
        [Description("Factor to scale each successive octave by")]
        public float Lacunarity { get { return lacunarity_; } set { lacunarity_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Number of passes to accumulate")]
        public int Octaves { get { return octaves_; } set { octaves_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Power applied to each octave")]
        public float Gain { get { return gain_; } set { gain_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Perturbation creates rippled shapes")]
        public float PerturbFeatures { get { return perturbFeatures_; } set { perturbFeatures_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Brightness of the ridged pattern")]
        public float Sharpness { get { return sharpness_; } set { sharpness_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Intensity of the centers of the ridged pattern")]
        public float AmplifyFeatures { get { return amplify_; } set { amplify_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Dampens noise in the pattern")]
        public float AltitudeErosion { get { return altitudeErosion_; } set { altitudeErosion_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Intensity of the \"clouds\" like effect")]
        public float RidgeErosion { get { return ridgeErosion_; } set { ridgeErosion_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Smoothness of the overall pattern")]
        public float SlopeErosion { get { return slopeErosion_; } set { slopeErosion_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Uber-Noise";
        }

        public override void Execute(object param)
        {
            Vector4 vParam = (Vector4)param;
            Vector4 pos = new Vector4(vParam.X * Period.X, vParam.Y * Period.Y, 0.0f, 0.0f);

            float value = 0.0f;
            var seed = noise_.GetSeed();
            float sum = 0.0f, featureNoise = 0.0f;
            var noiseValue = DerivativeNoise(pos, seed);
            featureNoise = noiseValue.X;

            float max = 0.0f;
            float amp = 1.0f;
            float currentGain = 1.0f;
            int i = 0;

            Vector3 lDerivative = new Vector3(noiseValue.Y, noiseValue.Z, noiseValue.W);

            var aaddToPos = lDerivative * PerturbFeatures;
            pos.X += aaddToPos.X;
            pos.Y += aaddToPos.Y;
            pos.Z += aaddToPos.Z;

            Vector3 ridgeErosionDerivative = new Vector3(0, 0, 0);
            Vector3 slopeErosionDerivative = new Vector3(0, 0, 0);
            while (++i < Octaves)
            {
                // Accumulate max (amp * 2 covers full damped amp range)
                max += amp * 2 + (currentGain * AmplifyFeatures);

                // Accumulate derivatives
                ridgeErosionDerivative += lDerivative * RidgeErosion;
                slopeErosionDerivative += lDerivative * SlopeErosion;

                // Sharpness
                float ridgedNoise = ((1.0f) - Mathf.Abs(featureNoise));
                float billowNoise = featureNoise * featureNoise;
                featureNoise = Mathf.Lerp(featureNoise, billowNoise, Math.Max(0.0f, Sharpness));
                featureNoise = Mathf.Lerp(featureNoise, ridgedNoise, Mathf.Abs(Math.Min(0.0f, Sharpness)));

                // Slope Erosion
                sum += amp * featureNoise * (1.0f / (1.0f + slopeErosionDerivative.LengthSquared()));

                // Amplitude damping
                float dampedAmp = amp * (1.0f - (RidgeErosion / (1.0f * ridgeErosionDerivative.LengthSquared())));
                sum += dampedAmp * featureNoise * (1.0f / (1.0f * slopeErosionDerivative.LengthSquared()));
                amp *= Mathf.Lerp(currentGain, currentGain * Mathf.Lerp(0.0f, 1.0f, sum / max), AltitudeErosion);

                // Amplify features
                sum += featureNoise * currentGain * AmplifyFeatures;
                currentGain = currentGain * Gain * AmplifyFeatures;

                // Prepare for next pass
                seed = (seed + i) & 0x7fffffff;
                var addToPos = lDerivative * PerturbFeatures;
                pos.X += addToPos.X;
                pos.Y += addToPos.Y;
                pos.Z += addToPos.Z;

                pos.X *= Lacunarity;
                pos.Y *= Lacunarity;
                pos.Z *= Lacunarity;
                pos.W *= Lacunarity;

                noiseValue = DerivativeNoise(pos, ++seed);
                featureNoise = noiseValue.X;
                lDerivative = new Vector3(noiseValue.Y, noiseValue.Z, noiseValue.W);
            }
            value = Mathf.Clamp01(sum / max);
            value = Mathf.Normalize(value, -1.0f, 1.0f);

            if (Inverted)
                OutputSockets[0].Data = 1.0f - value;
            else
                OutputSockets[0].Data = value;
            // stupid
            OutputSockets[1].Data = OutputSockets[0].Data;
        }

        Vector4 DerivativeNoise(Vector4 p, int seed)
        {
            float baseNoise = noise_.GetSimplex(p.X, p.Y, p.Z, p.W);
            float H = 0.1f;
            float dx = noise_.GetSimplex(p.X + H, p.Y, p.Z, p.W) - noise_.GetSimplex(p.X - H, p.Y, p.Z, p.W);
            float dy = noise_.GetSimplex(p.X, p.Y + H, p.Z, p.W) - noise_.GetSimplex(p.X, p.Y - H, p.Z, p.W);
            float dz = noise_.GetSimplex(p.X, p.Y, p.Z + H, p.W) - noise_.GetSimplex(p.X, p.Y, p.Z - H, p.W);

            //var v = new Vector3(dx, dy, dz);
            return new Vector4(baseNoise, dx, dy, 1);// v.z);
        }
    }

    [Description("Randomly places the input data into the result. Can use varied rotation and scale")]
    public partial class TextureBombGenerator : TexGenNode
    {
        FastNoise noise_;
        int count_ = 4;
        Vector2 scaleRange_ = new Vector2(0.5f, 2);
        Vector2 rotationRange_ = new Vector2(0, 360);

        [Description("Number of decals to attempt to place")]
        [PropertyData.AllowPermutations]
        [PropertyData.ValidStep(Value = 5)]
        public int Count { get { return count_; } set { count_ = value; OnPropertyChanged(); } }
        [Description("Minimum and maximum scales to resize the inputs to")]
        [PropertyData.AllowPermutations]
        [PropertyData.ValidStep(Value = 0.5f)]
        public Vector2 ScaleRange { get { return scaleRange_; } set { scaleRange_ = value; OnPropertyChanged(); } }

        [Description("Minimum and maximum degrees of rotation to transform the inputs by")]
        [PropertyData.AllowPermutations]
        [PropertyData.ValidStep(Value = 25.0f)]
        public Vector2 RotationRange { get { return rotationRange_; } set { rotationRange_ = value; OnPropertyChanged(); } }

        [Description("Randomness seed for the placement, scale, and orientation of decals")]
        [PropertyData.ValidStep(Value = 5.0f)]
        [PropertyData.AllowPermutations]
        public int Seed { get { return noise_.GetSeed(); } set { noise_.SetSeed(value); OnPropertyChanged(); } }

        public TextureBombGenerator()
        {
            noise_ = new FastNoise();
            noise_.SetNoiseType(FastNoise.NoiseType.Value);
            noise_.m_period = new FastNoise.Float3(8, 8, 8);
            noise_.SetFrequency(8);
        }

        public override bool WillForceExecute()
        {
            return true;
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Texture Bomb";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Source", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Channel });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Probability", IsInput = true, IsOutput = false, TypeID = SocketTypeID.Grayscale });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Channel });
        }

        public override void Execute(object param)
        {
            OutputSockets[0].Data = DoExecute(param);
        }

        public Color DoExecute(object param)
        {
            var coord = (Vector4)param;
            var pos = coord.XY();

            Vector3 offset = new Vector3(5, 7, 11);
            Color current = Color.TransparentBlack;

            float cellDim = 1.0f;

            int maxRetries = 8;
            int count = 1;
            // run n passes for 3x3 cells on each pixel
            int passCt = Math.Max(count_, 1);
            for (int pass = 0; pass < passCt; ++pass)
            {
                int thisCellX = (int)(pos.X);
                int thisCellY = (int)(pos.Y);

                for (int cellX = -1; cellX <= 1; ++cellX)
                {
                    int procCellX = thisCellX + cellX;
                    if (procCellX < 0)
                        procCellX += count;
                    else if (procCellX > count - 1)
                        procCellX = 0;

                    for (int cellY = -1; cellY <= 1; ++cellY)
                    {
                        int procCellY = thisCellY + cellY;
                        if (procCellY < 0)
                            procCellY += count;
                        else if (procCellY > count - 1)
                            procCellY = 0;

                        var offsetCoord = offset * ((procCellX + 1)) * ((procCellY + 1)) * ((pass + 1));

                        float offsetX = GetRandVal(ref offsetCoord, 1);
                        float offsetY = GetRandVal(ref offsetCoord, 1);
                        float r = GetRandVal(ref offsetCoord, 1);
                        float s = GetRandVal(ref offsetCoord, 1);

                        r = r * (rotationRange_.Y - rotationRange_.X) + rotationRange_.X;
                        r *= Mathf.DEGTORAD;
                        s = s * (scaleRange_.Y - scaleRange_.X) + scaleRange_.X;
                        s = 1.0f / s;
                        offsetX *= s;
                        offsetY *= s;

                        var mat = Matrix.CreateScale(s) * Matrix.CreateRotationZ(r) * Matrix.CreateTranslation(offsetX, offsetY, 0);
                        var samplePos = Vector2.Transform(pos + new Vector2(cellX * cellDim, cellY * cellDim), mat);
                        var testPos = Vector2.Transform(Vector2.Zero + new Vector2(cellX * cellDim, cellY * cellDim), mat);
                        if (samplePos.X < 0 || samplePos.X > 1)
                            continue;
                        if (samplePos.Y < 0 || samplePos.Y > 1)
                            continue;

                        // verify whether this pass would output anything
                        ForceExecuteSocketUpstream(new Vector4(testPos.X, testPos.Y, coord.Z, coord.W), InputSockets[1]);
                        if (InputSockets[1].GetFloatData(1.0f) < 0.5f)
                        {
                            if (maxRetries > 0)
                            {
                                maxRetries -= 1;
                                passCt += 1;
                            }
                            continue;
                        }

                        ForceExecuteSocketUpstream(new Vector4(samplePos.X, samplePos.Y, coord.Z, coord.W), InputSockets[0]);
                        var data = InputSockets[0].GetColor();
                        if (data.A > current.A)
                            current = data;
                    }
                }
            }

            //for (int i = 0; i < Count; ++i)
            //{
            //    float px = GetRandVal(ref offset, i);   // X offset
            //    float py = GetRandVal(ref offset, i);   // Y offset
            //    float r =  GetRandVal(ref offset, i);   // rotation
            //    float s =  GetRandVal(ref offset, i);   // scale
            //
            //    //s = s * (ScaleRange.Y - ScaleRange.X);
            //    r = Mathf.Wrap(r * 359.0f, 0, 360);
            //    while (r < 0)
            //        r += 360.0f;
            //    //s = ScaleRange.Clip(s);
            //
            //    var pt = new Vector2(1.0f, 0.0f).Rotate(r);
            //    pt.X += px + pos.X;
            //    pt.Y += py + pos.Y;                
            //
            //    ForceExecuteSocketUpstream(new Vector4(pt.X, pt.Y, coord.Z, coord.W), InputSockets[0]);
            //    var data = InputSockets[0].GetColor();
            //    if (data.A > current.A)
            //        current = data;
            //}
            //OutputSockets[0].Data = current;
            return current;
        }

        float GetRandVal(ref Vector3 offset, int np)
        {
            float ret = Mathf.Normalize(noise_.GetNoise(offset.X, offset.Y * 67, np), -1, 1);
            offset *= 1.2f;
            return ret;
        }
    }

    public enum TextVerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public enum TextHorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    [Description("Draws text to the image")]
    public partial class TextGenerator : TexGenNode
    {
        System.Drawing.Bitmap cache_;
        FontSpec font_ = new FontSpec();
        Color color_ = Color.White;
        string text_ = "Render Text";
        TextHorizontalAlignment horizontal_ = TextHorizontalAlignment.Left;
        TextVerticalAlignment vertical_ = TextVerticalAlignment.Top;

        [Description("Text to render")]
        [PropertyData.AllowPermutations]
        public string Text { get { return text_; } set { text_ = value; FlushCache(); OnPropertyChanged(); } }
        [Description("Font to use for rendering")]
        public FontSpec Font { get { return font_; } set { font_ = value; FlushCache(); OnPropertyChanged(); } }
        [Description("Color to use for the text")]
        [PropertyData.AllowPermutations]
        public Color Color { get { return color_; }set { color_ = value; FlushCache(); OnPropertyChanged(); } }
        [Description("Alignment of the text within the output")]
        [PropertyData.AllowPermutations]
        public TextHorizontalAlignment HorizontalAlignment { get { return horizontal_; }set { horizontal_ = value; FlushCache(); OnPropertyChanged(); } }
        [Description("Alignment of the text within the output")]
        [PropertyData.AllowPermutations]
        public TextVerticalAlignment VerticalAlignment { get { return vertical_; } set { vertical_ = value; FlushCache(); OnPropertyChanged(); } }

        ~TextGenerator()
        {
            FlushCache();
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Text";
            AddOutput(new GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Color });
            AddOutput(new GraphSocket(this) { Name = "Mask", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
        }

        public override void Execute(object param)
        {
            if (cache_ == null)
                RenderText();

            if (cache_.Width == 0 || cache_.Height == 0)
                return;

            Vector4 coord = (Vector4)param;

            float fractX = cache_.Width / coord.Z;
            float fractY = cache_.Height / coord.W;

            if (horizontal_ == TextHorizontalAlignment.Center)
                coord.X -= fractX*0.5f;
            else if (horizontal_ == TextHorizontalAlignment.Right)
                coord.X -= 1.0f - fractX;

            if (vertical_ == TextVerticalAlignment.Center)
                coord.Y -= 0.5f - fractY;
            else if (vertical_ == TextVerticalAlignment.Bottom)
                coord.Y -= 1.0f - fractY;

            int x = (int)(coord.X * coord.Z);
            int y = (int)(coord.Y * coord.W);

            var pixel = cache_.GetPixel(Mathf.Clamp(x, 0, cache_.Width-1), Mathf.Clamp(y, 0, cache_.Height-1)).ToXNAColor();
            OutputSockets[0].Data = pixel;
            OutputSockets[1].Data = pixel.A / 255.0f;
        }

        void FlushCache()
        {
            if (cache_ != null)
                cache_.Dispose();
            cache_ = null;
        }

        void RenderText()
        {
            var img = new System.Drawing.Bitmap(1, 1);
            System.Drawing.Graphics drawing = System.Drawing.Graphics.FromImage(img);

            var font = Font.GetFont();

            //measure the string to see how big the image needs to be
            System.Drawing.SizeF textSize = drawing.MeasureString(text_, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new System.Drawing.Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = System.Drawing.Graphics.FromImage(img);

            //paint the background
            drawing.Clear(System.Drawing.Color.Transparent);

            //create a brush for the text
            var textColor = color_.ToDrawingColor();
            System.Drawing.Brush textBrush = new System.Drawing.SolidBrush(textColor);

            drawing.DrawString(text_, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            cache_ = img;
        }
    }

    [Description("Generates an N-sided polygon with regular edge lengths.")]
    public partial class PolygonGenerator : TexGenNode
    {
        int sideCount_ = 3;
        float rotate_ = 0.0f;

        [Description("Number of side for this polygon")]
        public int SideCount { get { return sideCount_; } set { sideCount_ = value; OnPropertyChanged(); } }
        [Description("Degrees to rotate the polygon by")]
        public float Rotate { get { return rotate_; } set { rotate_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Polygon";
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            base.Execute(param);
            var coord = (Vector4)param;
            // remapped to 0,0 at the center
            var coord2D = new Vector2(coord.X - 0.5f, coord.Y - 0.5f);
            coord2D = coord2D.Rotate(rotate_);

            int sideCt = Math.Max(sideCount_, 3);
            float angStep = (2 * Mathf.PI) / sideCt;
            bool inside = true;
            for (int i = 0; i < sideCt; ++i)
            {
                int nextVert = Mathf.Wrap(i + 1, 0, sideCt);
                Vector2 start = new Vector2(
                    0.5f * Mathf.Cos(angStep * i),
                    0.5f * Mathf.Sin(angStep * i)
                    );
                Vector2 end = new Vector2(
                    0.5f * Mathf.Cos(angStep * nextVert),
                    0.5f * Mathf.Sin(angStep * nextVert)
                    );

                var seg = Vector2.Normalize(end - start).Rotate(90);
                var closest = XNAExt.ClosestPoint(coord2D, start, end);
                var testVec = Vector2.Normalize(closest - coord2D);

                inside &= Vector2.Dot(testVec, seg) <= 0;
            }

            OutputSockets[0].Data = inside ? Color.White : Color.Black;
        }
    }
}
