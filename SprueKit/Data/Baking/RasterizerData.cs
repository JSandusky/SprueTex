using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.Baking
{
    /// <summary>
    /// POD container for rasterization data.
    /// </summary>
    public struct RasterizerData
    {
        /// Pixel data that is to be written to.
        public ColorF[] Pixels;
        /// Optional mask that will record which pixels have been written and which have not. May be null.
        public bool[] WrittenMask;
        /// Width of the texture data.
        public int Width;
        /// Height of the texture data.
        public int Height;
        /// Depth of the texture data (in pseudo3d this is used as a hack value).
        public int Depth;
        /// Indicates whether anything was written
        public bool AnythingWritten;

        public void Init(int width, int height, bool withMask)
        {
            Pixels = new ColorF[width * height];
            Width = width;
            Height = height;
            AnythingWritten = false;
            
            if (withMask)
                WrittenMask = new bool[width * height];
        }

        #region Rasterization Masking
        /// Optional mask that will modulate the written values. May be null.
        public float[] Mask;
        /// Width of the "Mask" data.
        public int MaskWidth;
        /// Height of the "Mask" data.
        public int MaskHeight;
        /// Depth of the "Mask" data.
        public int MaskDepth;
        #endregion

        /// <summary>
        /// Confirms whether or not this data is valid for 2d rasterization
        /// </summary>
        public bool ValidFor2D()
        {
            return Width > 0 && Height > 0 && Pixels.Length == Width * Height;
        }

        /// <summary>
        /// Confirms whether or not this data is valid for 3d rasterization
        /// </summary>
        public bool ValidFor3D()
        {
            return Width > 0 && Height > 0 && Depth > 0 && Pixels.Length == Width * Height * Depth;
        }

        public int PixelCoordinate(float x, float y)
        {
            return PixelCoordinate(x * Width, y * Height);
        }

        public int PixelCoordinate(float x, float y, float z)
        {
            return PixelCoordinate(x * Width, y * Height, z * Depth);
        }

        public int PixelCoordinate(int x, int y)
        {
            return x + (y * Width);
        }

        public int PixelCoordinate(int x, int y, int z)
        {
            return x + (y * Width) + (z * Height * Width);
        }

        static int[] Lateral = { -1, 0, 1 };

        /// <summary>
        /// Fills the given color data with a specific value.
        /// </summary>
        public static void Fill(ref RasterizerData rasterData, ColorF value)
        {
            for (int i = 0; i < rasterData.Pixels.Length; ++i)
                rasterData.Pixels[i] = value;
        }

        /// <summary>
        /// Uses the boolean "written" mask to apply a 1 pixel padding to around the borders of written pixels.
        /// </summary>
        public static void Pad(ref RasterizerData rasterData)
        {
            if (rasterData.WrittenMask == null)
                return;
            for (int x = 0; x < rasterData.Width; ++x)
            {
                for (int y = 0; y < rasterData.Height; ++y)
                {
                    if (!rasterData.WrittenMask[x + y * rasterData.Width])
                    {
                        ColorF sum = new ColorF();
                        int sumCt = 0;
                        for (int xx = x - 1; xx < x + 1; ++xx)
                        {
                            if (xx >= rasterData.Width || xx < 0)
                                continue;
                            for (int yy = y - 1; yy < y + 1; ++yy)
                            {
                                if (yy >= rasterData.Height || yy < 0)
                                    continue;
                                // Only concerned about properly written pixels
                                if (rasterData.WrittenMask[xx + yy * rasterData.Width])
                                {
                                    sumCt += 1;
                                    sum += rasterData.Pixels[xx + yy * rasterData.Width];
                                }
                            }
                        }
                        if (sumCt > 0)
                            rasterData.Pixels[x + y * rasterData.Width] = sum * (1.0f / (float)sumCt);
                    }
                }
            }
        }


        public static void Blur(ref RasterizerData rasterData)
        {
            if (rasterData.WrittenMask == null)
                return;

            for (int y = 0; y < rasterData.Height; ++y)
            {
                for (int x = 0; x < rasterData.Width; ++x)
                {
                    int writeIndex = rasterData.PixelCoordinate(x, y);
                    if (rasterData.WrittenMask[writeIndex])
                    {
                        ColorF sum = new SprueKit.ColorF();
                        int sumCt = 0;

                        for (int yy = y - 1; yy <= y + 1; ++yy)
                        {
                            if (yy >= rasterData.Height || yy < 0)
                                continue;
                            for (int xx = x - 1; xx <= x + 1; ++xx)
                            {
                                if (xx >= rasterData.Width || xx < 0)
                                    continue;
                                if (xx == x && /*zz == z &&*/ yy == y)
                                    continue;

                                // Only concerned about properly written pixels
                                int readIdx = rasterData.PixelCoordinate(xx, yy);
                                if (rasterData.WrittenMask[readIdx])
                                {
                                    sumCt += 1;
                                    sum += rasterData.Pixels[readIdx];
                                }
                            }
                        }
                        if (sumCt > 0)
                        {
                            rasterData.Pixels[writeIndex] = sum * (1.0f / (float)sumCt);
                        }
                    }
                }
            }
        }

        public static void PadEdges(ref RasterizerData rasterData, int passes)
        {
            if (rasterData.WrittenMask == null)
                return;

            for (int i = 0; i < passes; ++i)
            {
                bool[] newWrittenMask = new bool[rasterData.Width * rasterData.Height];
                for (int w = 0; w < rasterData.Height * rasterData.Width; ++w)
                    newWrittenMask[w] = false;

                for (int y = 0; y < rasterData.Height; ++y)
                {
                    for (int x = 0; x < rasterData.Width; ++x)
                    {
                        int writeIndex = rasterData.PixelCoordinate(x, y);
                        if (!rasterData.WrittenMask[writeIndex])
                        {
                            ColorF sum = new SprueKit.ColorF(0.0f, 0.0f, 0.0f, 0.0f);
                            int sumCt = 0;

                            for (int yy = y - 1; yy <= y + 1; ++yy)
                            {
                                if (yy >= rasterData.Height || yy < 0)
                                    continue;
                                for (int xx = x - 1; xx <= x + 1; ++xx)
                                {
                                    if (xx >= rasterData.Width || xx < 0)
                                        continue;
                                    if (xx == x && /*zz == z &&*/ yy == y)
                                        continue;

                                    // Only concerned about properly written pixels
                                    int readIdx = rasterData.PixelCoordinate(xx, yy);
                                    if (rasterData.WrittenMask[readIdx])
                                    {
                                        sumCt += 1;
                                        sum += rasterData.Pixels[readIdx];
                                    }
                                }
                            }
                            if (sumCt > 0)
                            {
                                rasterData.Pixels[writeIndex] = sum * (1.0f / (float)sumCt);
                                newWrittenMask[writeIndex] = true;
                            }
                            else
                                newWrittenMask[writeIndex] = false;
                        }
                        else
                            newWrittenMask[writeIndex] = true;// rasterData.WrittenMask[writeIndex];
                    }
                }

                rasterData.WrittenMask = newWrittenMask;
            }
        }

        /// <summary>
        /// Calculates normalization values for UV coordinates to fit them to a range of 0-1
        /// </summary>
        public static void CalculateUVNormalization(Vector2[] uvs, out Vector2 min, out Vector2 max)
        {
            float minX = float.MaxValue,
                minY = float.MaxValue,
                maxX = float.MinValue,
                maxY = float.MinValue;

            foreach (Vector2 uv in uvs)
            {
                minX = Math.Min(uv.X, minX);
                maxX = Math.Max(uv.X, maxX);

                minY = Math.Min(uv.Y, minY);
                maxY = Math.Max(uv.Y, maxY);
            }

            min = new Vector2(minX, minY);
            max = new Vector2(maxX, maxY);
        }
    }
}
