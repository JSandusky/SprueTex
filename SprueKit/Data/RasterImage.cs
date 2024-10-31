using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data
{
    public class RasterImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Color[] Pixels { get; set; }

        public int ToIndex(int x, int y)
        {
            x = Mathf.Clamp(x, 0, Width);
            y = Mathf.Clamp(y, 0, Height);
            return y * Width + x;
        }

        public Color GetPixel(int x, int y)
        {
            return Pixels[ToIndex(x, y)];
        }

        public void SetPixel(int x, int y, Color value)
        {
            Pixels[ToIndex(x, y)] = value;
        }
    }
}
