using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconColorizer
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = args[0];
            string outName = args[1];
            float maxLumen = GetArg("max", 255, args);
            float forceLumen = GetArg("force", 0, args);
            Color recolorValue = GetColorArg("color", args);

            System.Console.WriteLine("Reading: " + fileName);
            Bitmap bmp = new Bitmap(fileName);
            for (int y = 0; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    var pixelColor = bmp.GetPixel(x, y);
                    pixelColor = pixelColor.ToLuminance(maxLumen, forceLumen).Colorize(recolorValue);
                    bmp.SetPixel(x, y, pixelColor);
                }
            }
            bmp.Save(outName);
            System.Console.WriteLine("Wrote image: " + outName);
        }

        static Color GetColorArg(string argName, params string[] args)
        {
            string targetArgStr = argName + "=";
            foreach (string str in args)
                if (str.StartsWith(targetArgStr))
                {
                    string subStr = str.Remove(0, targetArgStr.Length);
                    string[] terms = subStr.Split(',');
                    if (terms.Length == 3)
                    {
                        Color ret = Color.FromArgb(byte.Parse(terms[0]), byte.Parse(terms[1]), byte.Parse(terms[2]));
                        System.Console.WriteLine(ret.ToString());
                        return ret;
                    }
                }

            return Color.White;
        }

        static float GetArg(string argName, float defaultValue, params string[] args)
        {
            string targetArgStr = argName + "=";
            foreach (string str in args)
                if (str.StartsWith(targetArgStr))
                {
                    string subStr = str.Remove(0, targetArgStr.Length);
                    float fVal = defaultValue;
                    if (float.TryParse(subStr, out fVal))
                        return fVal;
                }

            return defaultValue;
        }
    }

    public static class ColorExt
    {
        public static Color ToLuminance(this Color src, float maxLuminance, float forceLumen)
        {
            float r = src.R / 255.0f;
            float g = src.G / 255.0f;
            float b = src.B / 255.0f;
            float brightness = (r * 0.2126f + g * 0.7152f + b * 0.0722f);
            //brightness = brightness > maxLuminance ? maxLuminance : brightness;
            byte brightByte = (byte)(brightness * 255);
            if (brightByte > maxLuminance)
                brightByte = (byte)maxLuminance;
            if (forceLumen > 0)
                brightByte = (byte)forceLumen;
            return Color.FromArgb(src.A, brightByte, brightByte, brightByte);
        }

        public static Color Colorize(this Color src, Color colorizingColor)
        {
            float r = src.R / 255.0f;
            float g = src.G / 255.0f;
            float b = src.B / 255.0f;

            float or = colorizingColor.R / 255.0f;
            float og = colorizingColor.G / 255.0f;
            float ob = colorizingColor.B / 255.0f;

            byte nr = (byte)(r * or * 255);
            byte ng = (byte)(g * og * 255);
            byte nb = (byte)(b * ob * 255);

            return Color.FromArgb(src.A, nr, ng, nb);
        }
    }
}
