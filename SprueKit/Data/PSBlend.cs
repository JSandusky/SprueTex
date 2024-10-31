using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace SprueKit.Data
{
    public enum PSBlendMode
    {
        Normal,
        Additive,
        Subtract,
        Multiply,
        Difference,
        Darken,
        Lighten,
        Divide,
        ColorBurn,
        LinearBurn,
        Screen,
        ColorDodge,
        LinearDodge,
        Dissolve,
        NormalMap,
    }

    public enum PSAlphaMode
    {
        UseWeight,
        UseDestAlpha,
        UseSourceAlpha
    }

    public static class PSBlend
    {
        static Vector3 ToNormal(this Vector4 v)
        {
            return (v.XYZ() - new Vector3(0.5f, 0.5f, 0.5f)) * 2.0f;
        }
        static Vector4 FromNormal(this Vector3 norm)
        {
            return new Vector4(norm.X * 0.5f + 0.5f, norm.Y * 0.5f + 0.5f, norm.Z * 0.5f + 0.5f, 1.0f);
        }
        public static Color Blend(Vector4 src, Vector4 dest, float weightVal, PSBlendMode blendMode, PSAlphaMode alphaMode)
        {
            float blendWeight = 0.0f;
            if (alphaMode == PSAlphaMode.UseWeight)
                blendWeight = weightVal;
            else if (alphaMode == PSAlphaMode.UseDestAlpha)
            {
                blendWeight = dest.W;
                dest.W = 1;
                src.W = 1;
            }
            else if (alphaMode == PSAlphaMode.UseSourceAlpha)
            {
                blendWeight = src.W;
                src.W = 1;
                dest.W = 1;
            }

            Vector4 resultColor = new Vector4();
            bool needsBlend = true;
            switch (blendMode)
            {
                // These two do the same things
                case PSBlendMode.Normal:
                    //alpha * new + (1 - alpha) * old
                    resultColor = (src * blendWeight) + (dest * (1 - blendWeight));
                    needsBlend = false;
                    break;
                case PSBlendMode.Additive:
                case PSBlendMode.LinearDodge:
                    resultColor = dest + src;
                    break;
                case PSBlendMode.Difference:
                    resultColor = new Vector4(Mathf.Abs(dest.X - src.X), Mathf.Abs(dest.Y - src.Y), Mathf.Abs(dest.Z - src.Z), Mathf.Abs(dest.W - src.W));
                    break;
                case PSBlendMode.Darken:
                    resultColor = Vector4.Min(dest, src);
                    break;
                case PSBlendMode.Lighten:
                    resultColor = Vector4.Max(dest, src);
                    break;
                case PSBlendMode.Subtract:
                    resultColor = dest - src;
                    break;
                case PSBlendMode.Multiply:
                    resultColor = dest * src;
                    break;
                case PSBlendMode.Divide:
                    resultColor = dest / src;
                    break;
                case PSBlendMode.ColorBurn:
                    resultColor = Vector4.One - ((Vector4.One - src) / dest);
                    break;
                case PSBlendMode.LinearBurn:
                    resultColor = dest + src - Vector4.One;
                    break;
                case PSBlendMode.Screen:
                    resultColor = Vector4.One - (Vector4.One - dest) * (Vector4.One - src);
                    break;
                case PSBlendMode.ColorDodge:
                    resultColor = src / (Vector4.One - dest);
                    break;
                case PSBlendMode.Dissolve:
                    {
                        /// Get a random float for our position
                        float dissolveWeight = (float)rand.NextDouble();
                        resultColor = dissolveWeight < blendWeight ? dest : src;
                        needsBlend = false;
                    }
                    break;
                case PSBlendMode.NormalMap:
                    {
                        needsBlend = false;
                        // Weighted variation of Ruby 'WhiteOut' style normal map blending
                        float destWeight = 1.0f - blendWeight;
                        Vector3 n1 = Vector3.Normalize(dest.ToNormal());// * destWeight;
                        Vector3 n2 = Vector3.Normalize(src.ToNormal());// * blendWeight;
                        Vector3 newNormal = new Vector3(n1.X + n2.X*blendWeight, n1.Y + n2.Y*blendWeight, n1.Z * n2.Z);
                        resultColor = Vector3.Normalize(newNormal).FromNormal();
                    }
                    break;
            }

            if (needsBlend)
                resultColor = Vector4.Lerp(dest, resultColor, blendWeight);
            // Blend time
            //resultColor = Color.FromNonPremultiplied(SprueLerp(dest, resultColor, blendWeight);
            return Color.FromNonPremultiplied(resultColor);
        }

        static int RandomInt(int seed)
        {
            return seed = (seed * 1103515245 + 12345);
        }

        static float RandomFloat(int seed)
        {
            return ((float)RandomInt(seed)) / ((float)(int.MaxValue));
        }
        static Random rand = new Random();
    }
}
