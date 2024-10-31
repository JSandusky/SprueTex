using System;
using System.Collections.Generic;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework;

namespace SprueKit.Data
{
    /// <summary>
    /// Contains methods for curve supported datatypes.
    /// </summary>
    public static class CurveExtensions
    {

    }

    public enum CurveType
    {
        Constant,               // Fixed value
        Linear,                 // Algebra standard MX+B
        Quadratic,              // Exponential
        Logistic,               // Sigmoid
        Logit,                  // 90 degree Sigmoid (biology/psych origins)
        Threshold,              // Boolean/stair
        Sine,                   // Sine wave
        Parabolic,              // Algebra standard form parabola
        NormalDistribution,     // Probability density function
        Bounce,                 // Bouncing degrading pattern, effectively decaying noise
    }

    [Serializable]
    public class ResponseCurve
    {
        public CurveType CurveShape { get; set; } = CurveType.Linear;

        public float XIntercept { get; set; } = 0.0f;

        public float YIntercept { get; set; } = 0.0f;

        public float SlopeIntercept { get; set; } = 1.0f;

        public float Exponent { get; set; } = 1.0f;

        /// Flips the result value of Y to be 1 - Y (top-bottom mirror)
        public bool FlipY { get; set; } = false;

        /// Flips the value of X to be 1 - X (left-right mirror)
        public bool FlipX { get; set; } = false;

        public float GetValue(float x)
        {
            if (FlipX)
                x = 1.0f - x;

            // Evaluate the curve function for the given inputs.
            float value = 0.0f;
            switch (CurveShape)
            {
                case CurveType.Constant:
                    value = YIntercept;
                    break;
                case CurveType.Linear:
                    // y = m(x - c) + b ... x expanded from standard mx+b
                    value = (SlopeIntercept * (x - XIntercept)) + YIntercept;
                    break;
                case CurveType.Quadratic:
                    // y = mx * (x - c)^K + b
                    value = ((SlopeIntercept * x) * Mathf.Pow(Mathf.Abs(x + XIntercept), Exponent)) + YIntercept;
                    break;
                case CurveType.Logistic:
                    // y = (k * (1 / (1 + (1000m^-1*x + c))) + b
                    value = (Exponent * (1.0f / (1.0f + Mathf.Pow(Mathf.Abs(1000.0f * SlopeIntercept), (-1.0f * x) + XIntercept + 0.5f)))) + YIntercept; // Note, addition of 0.5 to keep default 0 XIntercept sane
                    break;
                case CurveType.Logit:
                    // y = -log(1 / (x + c)^K - 1) * m + b
                    value = (-Mathf.Log((1.0f / Mathf.Pow(Mathf.Abs(x - XIntercept), Exponent)) - 1.0f) * 0.05f * SlopeIntercept) + (0.5f + YIntercept); // Note, addition of 0.5f to keep default 0 XIntercept sane
                    break;
                case CurveType.Threshold:
                    value = x > XIntercept ? (1.0f - YIntercept) : (0.0f - (1.0f - SlopeIntercept));
                    break;
                case CurveType.Sine:
                    // y = sin(m * (x + c)^K + b
                    value = (Mathf.Sin(SlopeIntercept * Mathf.Pow(x + XIntercept, Exponent)) * 0.5f) + 0.5f + YIntercept;
                    break;
                case CurveType.Parabolic:
                    // y = mx^2 + K * (x + c) + b
                    value = Mathf.Pow(SlopeIntercept * (x + XIntercept), 2) + (Exponent * (x + XIntercept)) + YIntercept;
                    break;
                case CurveType.NormalDistribution:
                    // y = K / sqrt(2 * PI) * 2^-(1/m * (x - c)^2) + b
                    value = (Exponent / (Mathf.Sqrt(2 * 3.141596f))) * Mathf.Pow(2.0f, (-(1.0f / (Mathf.Abs(SlopeIntercept) * 0.01f)) * Mathf.Pow(x - (XIntercept + 0.5f), 2.0f))) + YIntercept;
                    break;
                case CurveType.Bounce:
                    value = Mathf.Abs(Mathf.Sin((6.28f * Exponent) * (x + XIntercept + 1f) * (x + XIntercept + 1f)) * (1f - x) * SlopeIntercept) + YIntercept;
                    break;
            }

            // Invert the value if specified as an inverse.
            if (FlipY)
                value = 1.0f - value;

            // Constrain the return to a normal 0-1 range.
            return Mathf.Clamp01(value);
        }

        /// <summary>
        /// Constructs a response curve from a formatted string
        /// Format: CurveType X Y Slope Exponent <FLIPX> <FLIPY>
        /// </summary>
        /// <remarks>
        /// Examples:
        /// Linear 0 0 1 1
        /// Quadratic 0.5 0 0.23 1.3 flipx
        /// Logit -0.15 -0.25 0.3 2.3 flipx flipy
        /// </remarks>
        /// <param name="inputString">String to process</param>
        /// <returns>A response curve created from the input string</returns>
        public static ResponseCurve FromString(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                throw new ArgumentNullException("inputString");

            string[] words = inputString.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 5)
                throw new FormatException("ResponseCurve.FromString requires 5 SPACE seperated inputs: CurveType X Y Slope Exponent <FLIPX> <FLIPY>");

            ResponseCurve ret = new ResponseCurve();
            ret.CurveShape = (CurveType)Enum.Parse(typeof(CurveType), words[0]);

            float fValue = 0.0f;
            if (float.TryParse(words[1], out fValue))
                ret.XIntercept = fValue;
            else
                throw new FormatException("ResponseCurve.FromString; unable to parse X-Intercept: CurveType X Y Slope Exponent <FLIPX> <FLIPY>");

            if (float.TryParse(words[2], out fValue))
                ret.YIntercept = fValue;
            else
                throw new FormatException("ResponseCurve.FromString; unable to parse Y-Intercept: CurveType X Y Slope Exponent <FLIPX> <FLIPY>");

            if (float.TryParse(words[3], out fValue))
                ret.SlopeIntercept = fValue;
            else
                throw new FormatException("ResponseCurve.FromString; unable to parse SLOPE: CurveType X Y Slope Exponent <FLIPX> <FLIPY>");

            if (float.TryParse(words[4], out fValue))
                ret.Exponent = fValue;
            else
                throw new FormatException("ResponseCurve.FromString; unable to parse EXPONENT: CurveType X Y Slope Exponent <FLIPX> <FLIPY>");

            // If there are more parameters then check to see if they're FlipX/FlipY and set accordingly
            for (int i = 5; i < words.Length; ++i)
            {
                string lCase = words[i].ToLowerInvariant();
                if (lCase.Equals("flipx"))
                    ret.FlipX = true;
                else if (lCase.Equals("flipy"))
                    ret.FlipY = true;
            }

            return ret;
        }

        public override string ToString()
        {
            string ret = string.Format("{0} {1} {2} {3} {4}", CurveShape.ToString(), XIntercept, YIntercept, SlopeIntercept, Exponent);
            if (FlipX)
                ret += " flipx";
            if (FlipY)
                ret += " flipy";
            return ret;
        }

        // For the above string parsing split
        private static char[] splitChar = { ' ' };

        public ResponseCurve Clone()
        {
            ResponseCurve ret = new ResponseCurve();
            ret.XIntercept = XIntercept;
            ret.YIntercept = YIntercept;
            ret.SlopeIntercept = SlopeIntercept;
            ret.Exponent = Exponent;
            ret.CurveShape = CurveShape;
            ret.FlipX = FlipX;
            ret.FlipY = FlipY;
            return ret;
        }
    }

    /// <summary>
    /// Base class for knot based curves
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class KnotCurve<T, R>
    {
        public List<T> Knots { get; private set; } = new List<T>();

        public abstract R GetValue(float t);
    }

    public class ColorCurve : KnotCurve<Vector2, float>
    {
        float[] derivatives_;

        public ColorCurve Clone()
        {
            ColorCurve ret = new ColorCurve();
            if (derivatives_.Length > 0)
            {
                ret.derivatives_ = new float[derivatives_.Length];
                Array.Copy(derivatives_, ret.derivatives_, derivatives_.Length);
            }
            ret.Knots.Clear();
            foreach (var knot in Knots)
                ret.Knots.Add(knot);
            return ret;
        }

        public ColorCurve()
        {
            MakeLinear();
        }

        public void Set(ColorCurve rhs)
        {
            if (rhs == this || rhs == null)
                return;
            Knots.Clear();
            foreach (var knot in rhs.Knots)
                Knots.Add(knot);
            CalculateDerivatives();
        }

        public void MakeLinear()
        {
            Knots.Clear();
            Knots.Add(Vector2.Zero);
            Knots.Add(Vector2.One);
            CalculateDerivatives();
        }

        public void SortKnots()
        {
            Knots.Sort(new CurveSorter());
            CalculateDerivatives();
        }

        class CurveSorter : IComparer<Vector2>
        {
            public int Compare(Vector2 x, Vector2 y)
            {
                if (x.X < y.X)
                    return -1;
                else if (x.X > y.X)
                    return 1;
                return 0;
            }
        }

        public override float GetValue(float pos)
        {
            for (int i = 0; i < Knots.Count - 1; ++i)
            {
                Vector2 cur = Knots[i];
                Vector2 next = Knots[i + 1];
                if (pos >= cur.X && pos <= next.X)
                {
                    float t = (pos - cur.X) / (next.X - cur.X);

                    float a = 1.0f - t;
                    float b = t;
                    float h = next.X - cur.X;

                    // Couldn't have figured this bit out without, the interpolation was my error:
                    // http://www.developpez.net/forums/d331608-3/general-developpement/algorithme-mathematiques/contribuez/image-interpolation-spline-cubique/#post3513925
                    return a * cur.Y + b * next.Y + (h * h / 6.0f) * ((a * a * a - a) * derivatives_[i] + (b * b * b - b) * derivatives_[i + 1]);
                }
            }
            // Deal with edges
            if (pos <= 0.0f)
                return 0;
            if (pos >= 0.5f && Knots.Count > 0)
                return Knots[Knots.Count-1].Y;
            return 0.0f;
        }

        public void CalculateDerivatives()
        {
            int count = Knots.Count;
            const float XDiv = 1.0f / 6.0f; // Should be 2Pi?
            const float YDiv = 1.0f / 3.0f; // Should be Pi?
            const float ZDiv = 1.0f / 6.0f; // Shold be 2Pi?

            derivatives_ = new float[count];
            Vector3[] knotTans = new Vector3[count];

            knotTans[0] = new Vector3(0, 1, 0);
            knotTans[count - 1] = new Vector3(0, 1, 0);
            for (int i = 1; i < count - 1; ++i)
            {
                knotTans[i].X = (Knots[i].X - Knots[i - 1].X) * XDiv;
                knotTans[i].Y = (Knots[i + 1].X - Knots[i - 1].X) * YDiv;
                knotTans[i].Z = (Knots[i + 1].X - Knots[i].X) * ZDiv;
                var lhs = (Knots[i + 1] - Knots[i]);
                var rhs = (Knots[i] - Knots[i - 1]);
                derivatives_[i] = ((lhs.Y / lhs.X) - (rhs.Y / rhs.X));
            }

            for (int i = 1; i < count - 1; ++i)
            {
                float m = knotTans[i].X / knotTans[i - 1].Y;
                knotTans[i].Y -= m * knotTans[i - 1].X;
                knotTans[i].X = 0;
                derivatives_[i] -= m * derivatives_[i - 1];
            }

            for (int i = count - 2; i >= 0; --i)
            {
                float m = knotTans[i].Z / knotTans[i + 1].Y;
                knotTans[i].Y -= m * knotTans[i + 1].X;
                knotTans[i].Z = 0;
                derivatives_[i] -= m * derivatives_[i + 1];
            }

            for (int i = 0; i < count; ++i)
                derivatives_[i] /= knotTans[i].Y;
        }
    }

    public class ColorCurves
    {
        public ColorCurve R { get; set; } = new ColorCurve();
        public ColorCurve G { get; set; } = new ColorCurve();
        public ColorCurve B { get; set; } = new ColorCurve();
        public ColorCurve A { get; set; } = new ColorCurve();

        public ColorCurves Clone()
        {
            ColorCurves ret = new ColorCurves();
            ret.R = R.Clone();
            ret.G = G.Clone();
            ret.B = B.Clone();
            ret.A = A.Clone();
            return ret;
        }

        public void SetRGB(ColorCurve refCurve)
        {
            R.Set(refCurve);
            G.Set(refCurve);
            B.Set(refCurve);
        }

        public void SetAll(ColorCurve refCurve)
        {
            R.Set(refCurve);
            G.Set(refCurve);
            B.Set(refCurve);
            A.Set(refCurve);
        }

        public void ResetAll()
        {
            R.MakeLinear();
            G.MakeLinear();
            B.MakeLinear();
            A.MakeLinear();
        }
    }

    public class FloatRamp
    {
        public static float GetValue(float td, float low, float mid, float high)
        {
            if (td < low)
                return 0.0f;
            if (td > high)
                return 1.0f;
            if (td >= low && td < mid)
                return Mathf.Normalize(td, low, mid) * 0.5f;
            else // in the last bit
                return 0.5f + Mathf.Normalize(td, mid, high) * 0.5f;
        }
    }

    public class ColorRamp
    {
        public ColorRamp()
        {
            Colors.Add(new KeyValuePair<float, Color>(0.0f, new Color(0, 0, 0)));
            Colors.Add(new KeyValuePair<float, Color>(1.0f, new Color(1.0f, 1.0f, 1.0f)));
        }

        public List<KeyValuePair<float, Color>> Colors { get; set; } = new List<KeyValuePair<float, Color>>();

        public Color Get(float position)
        {
            Color ret = Color.White;
            for (int i = 0; i < Colors.Count - 1; ++i)
            {
                if (position < Colors[i].Key)
                    return Colors[i].Value;
                if (position > Colors[i + 1].Key)
                    continue;
                float lhs = Colors[i].Key;
                float rhs = Colors[i + 1].Key;
                return Color.Lerp(Colors[i].Value, Colors[i + 1].Value, Mathf.Normalize(position, lhs, rhs));
            }
            if (Colors.Count > 0)
                return Colors[Colors.Count - 1].Value;
            return ret;
        }

        public void Sort()
        {
            Colors.Sort(new RampSorter());
        }

        public ColorRamp Clone()
        {
            ColorRamp ret = new ColorRamp();
            ret.Colors.Clear();
            foreach (var kvp in Colors)
                ret.Colors.Add(kvp);
            return ret;
        }

        class RampSorter : IComparer<KeyValuePair<float, Color>>
        {
            public int Compare(KeyValuePair<float, Color> x, KeyValuePair<float, Color> y)
            {
                if (x.Key < y.Key)
                    return -1;
                else if (x.Key > y.Key)
                    return 1;
                return 0;
            }
        }
    }
}
