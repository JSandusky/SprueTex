using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprueKit.Data.Graph;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using IntVector2 = PluginLib.IntVector2;
using System.ComponentModel;

namespace SprueKit.Data.TexGen
{
    [Description("Blurs the input with a gaussian blur")]
    public partial class BlurModifier : CachingNode
    {
        float[,] kernel = null;

        int blurRadius_ = 3;
        float blurStepSize_ = 1.0f;
        float sigma_ = 1.5f;

        public override void Construct()
        {
            base.Construct();
            Name = "Blur";
        }

        [PropertyData.AllowPermutations]
        [Description("Size of the convolution kernel")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public int BlurRadius { get { return blurRadius_; } set { blurRadius_ = value; kernel = null; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Specifies what fraction of the image dimensions to use for stepping the blur radius")]
        [PropertyData.ValidStep(Value = 0.5f)]
        public float BlurStepSize { get { return blurStepSize_; } set { blurStepSize_ = value; kernel = null; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Intensity of the blur's mixing")]
        [PropertyData.ValidStep(Value = 0.2f)]
        public float Sigma { get { return sigma_; } set { sigma_ = value; kernel = null; OnPropertyChanged(); } }

        public override bool WillForceExecute() { return true; }
        public override void Execute(object param)
        {
            Vector4 coord = (Vector4)param;

            PrepareCache((int)coord.Z, (int)coord.W);

            Vector2 pos = new Vector2(coord.X, coord.Y);
            float stepping = CalculateStepSize(BlurStepSize, coord);

            if (kernel == null)
                kernel = Calculate(BlurRadius * 2 + 1, Sigma);


            Vector4 sum = new Vector4(0,0,0,0);
            int blurHalf = BlurRadius;
            for (int y = -blurHalf; y <= blurHalf; ++y)
            {
                for (int x = -blurHalf; x <= blurHalf; ++x)
                {
                    sum += cache_.GetPixelBilinear(pos.X + x * stepping, pos.Y + y * stepping).ToVector4() * kernel[blurHalf + x, blurHalf + y];
                    //ForceExecuteUpstream(new Vector4(pos.X + x * stepping, pos.Y + y * stepping, coord.Z, coord.W));
                    //sum += InputSockets[0].GetColor().ToVector4() *  kernel[blurHalf + x, blurHalf + y];

                }
            }
            OutputSockets[0].Data = sum;
        }

        protected void CalculateKernel(ref float[,] target)
        {
            float sum = 0;

            float calculatedEuler = 1.0f / (2.0f * (float)Math.PI * (float)Math.Pow(Sigma, 2));

            int kernelRadius = BlurRadius;

            for (int filterY = -kernelRadius; filterY <= kernelRadius; ++filterY)
            {
                for (int filterX = -kernelRadius; filterX <= kernelRadius; ++filterX)
                {
                    float distance = ((filterX * filterX) + (filterY * filterY)) / (2 * (Sigma * Sigma));

                    float value = calculatedEuler * (float)Math.Exp(-distance);
                    target[kernelRadius + filterX, kernelRadius + filterY] = Math.Abs(value);
                    sum += value;
                }
            }

            float inverseSum = 1.0f / sum;
            for (int x = 0; x < target.GetLength(0); ++x)
            {
                for (int y = 0; y < target.GetLength(1); ++y)
                {
                    target[x, y] = target[x,y] * (inverseSum);
                }
            }
        }

        protected static float[,] Calculate(int length, double weight)
        {
            float[,] Kernel = new float[length, length];
            float sumTotal = 0;


            int kernelRadius = length / 2;
            float distance = 0;

            float calculatedEuler = 1.0f / (float)(2.0 * Math.PI * Math.Pow(weight, 2));


            for (int filterY = -kernelRadius; filterY <= kernelRadius; filterY++)
            {
                for (int filterX = -kernelRadius; filterX <= kernelRadius; filterX++)
                {
                    distance = (float)(((filterX * filterX) + (filterY * filterY)) / (2.0f * (weight * weight)));
                    Kernel[filterY + kernelRadius, filterX + kernelRadius] = calculatedEuler * (float)Math.Exp(-distance);
                    sumTotal += Kernel[filterY + kernelRadius, filterX + kernelRadius];
                }
            }


            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    Kernel[y, x] = Kernel[y, x] * (1.0f / sumTotal);
                }
            }


            return Kernel;
        }
    }

    [Description("Mixes in an offset sampling to produce a streaked blur")]
    public partial class StreakModifier : CachingNode
    {
        float streakAngle_ = 0.0f;
        float streakLength_ = 0.1f;
        int samples_ = 3;
        bool fadeOff_ = true;

        [PropertyData.AllowPermutations]
        [Description("Angular direction for the streak, in degrees")]
        [PropertyData.ValidStep(Value = 45.0f)]
        public float StreakAngle { get { return streakAngle_; } set { streakAngle_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Specifices how long each streak should be")]
        [PropertyData.ValidStep(Value = 0.1f)]
        public float StreakLength { get { return streakLength_; } set { streakLength_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Number of samples to take per streak")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public int Samples { get { return samples_; } set { samples_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Streak";
        }

        public override bool WillForceExecute() { return true; }

        public override void Execute(object param)
        {
            Vector4 sum = new Vector4(0, 0, 0, 0);

            Vector4 coord = (Vector4)param;
            PrepareCache((int)coord.Z, (int)coord.W);
            Vector2 angleVec = Vector2.Normalize(new Vector2(0,1).Rotate(StreakAngle));

            for (int i = 0; i < Samples; ++i)
            {
                Vector4 off = new Vector4(angleVec.X * StreakLength * i, angleVec.Y * StreakLength * i, 0, 0);
                sum += cache_.GetPixelBilinear(coord.X + off.X, coord.Y + off.Y).ToVector4();
            }

            if (Samples > 1)
                sum *= (1.0f / ((float)Samples));

            OutputSockets[0].Data = sum;
        }
    }

    [Description("Uses an edge detection filter to control where a gaussian blur is applied, this filter is extremely slow")]
    public partial class AnisotropicBlur : BlurModifier
    {
        ResponseCurve edgeCurve_ = new ResponseCurve();
        [Description("Controls the bias of the underlying Sobel filter used to decide where to blur")]
        public ResponseCurve EdgeCurve { get { return edgeCurve_; } set { edgeCurve_ = value; OnPropertyChanged(); } }


        float sobelEdge_;
        [Description("Determines the step size to use for edge detection")]
        [PropertyData.ValidStep(Value =1.0f)]
        public float SobelEdgeStep { get { return sobelEdge_; } set { sobelEdge_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Aniso Blur";
            AddOutput(new Data.Graph.GraphSocket(this) { IsOutput = true, IsInput = false, Name = "Flipped", TypeID = SocketTypeID.Channel });
        }

        public override void Execute(object param)
        {
            Vector4 pos = (Vector4)param;
            PrepareCache((int)pos.Z, (int)pos.W);

            float edgeSteeping = CalculateStepSize(SobelEdgeStep, pos);
            float stepping = CalculateStepSize(BlurStepSize, pos);
            float l = SobelTextureModifier.SampleSobel(this, 0, -1, 0, pos, edgeSteeping);
            float r = SobelTextureModifier.SampleSobel(this, 0, 1, 0, pos, edgeSteeping);
            float t = SobelTextureModifier.SampleSobel(this, 0, 0, -1, pos, edgeSteeping);
            float b = SobelTextureModifier.SampleSobel(this, 0, 0, 1, pos, edgeSteeping);
            float tl = SobelTextureModifier.SampleSobel(this, 0, -1, -1, pos, edgeSteeping);
            float tr = SobelTextureModifier.SampleSobel(this, 0, 1, -1, pos, edgeSteeping);
            float bl = SobelTextureModifier.SampleSobel(this, 0, -1, 1, pos, edgeSteeping);
            float br = SobelTextureModifier.SampleSobel(this, 0, 1, 1, pos, edgeSteeping);

            float dX = tr + 2 * r + br - tl - 2 * l - bl;
            float dY = bl + 2 * b + br - tl - 2 * t - tr;
            float fVal = Mathf.Clamp01(dX * dY);
            fVal = edgeCurve_.GetValue(fVal);

            //ForceExecuteUpstream(pos);
            Vector4 trueColor = cache_.GetPixelBilinear(pos.X, pos.Y).ToVector4();// InputSockets[0].GetColor().ToVector4();
            base.Execute(pos);
            Vector4 blendColor = OutputSockets[0].GetColor().ToVector4();

            OutputSockets[0].Data = Vector4.Lerp(trueColor, blendColor, fVal);
            OutputSockets[1].Data = Vector4.Lerp(blendColor, trueColor, fVal);
        }
    }
}
