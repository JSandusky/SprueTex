using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.Baking
{
    public struct Rect
    {
        public float xMin;
        public float yMin;
        public float xMax;
        public float yMax;

        public float width { get { return xMax - xMin; } }
        public float height { get { return yMax - yMin; } }

        public Rect(float xmin, float ymin, float width, float height)
        {
            xMin = xmin;
            yMin = ymin;
            xMax = xMin + width;
            yMax = yMin + height;
        }
    }

    public static class MeshRasterizer
    {
        public enum RasterizerBlend
        {
            Replace,
            Add,
            Subtract,
            Multiply,
            Max
        }

        /// <summary>
        /// Allows hooking into triangle rasterization to modify the value before it is written.
        /// </summary>
        /// <param name="pixelColor">Pixel that is about to be rasterized</param>
        public delegate void PixelCallback(ref ColorF pixelColor);

        public delegate ColorF? ElementSampleCallback(ref Vector3 pos, ref Vector3 nor);
        public delegate ColorF? ElementSampleCallbackWithUV(ref Vector3 pos, ref Vector3 nor, ref Vector2 uv);

        /// <summary>
        /// Allows hooking into triangle rasterization to modify the value before it is written.
        /// </summary>
        /// <param name="pixelColor">Pixel that is about to be rasterized</param>
        /// <param name="otherColor">Computed "other" value from RasterTriangleWithRef</param>
        public delegate void DoublePixelCallback(ref ColorF pixelColor, ColorF otherColor);

        public static void RasterizeModelSpaceTriangle(ref RasterizerData rasterData, Vector3[] pos, Vector3[] norm, Vector2[] uvs, ElementSampleCallback callback)
        {
            float xSize = 1.0f / rasterData.Width;
            float ySize = 1.0f / rasterData.Height;

            Rect triBounds = CalculateTriangleImageBounds(rasterData.Width, rasterData.Height, uvs);
            for (float y = triBounds.yMin; y <= triBounds.yMax && y < rasterData.Height; y += 1.0f)
            {
                if (y < 0 || y >= rasterData.Height)
                    continue;

                float yCoord = y / rasterData.Height;
                for (float x = triBounds.xMin; x <= triBounds.xMax; x += 1.0f)
                {
                    if (x < 0 || x >= rasterData.Width)
                        continue;

                    float xCoord = x / rasterData.Width;
                    Vector2 pt;
                    pt.X = xCoord; pt.Y = yCoord;

                    float mul = 1.0f;
                    Vector2[] pixelCorners = {
                        new Vector2(pt.X, pt.Y * mul),
                        new Vector2(pt.X + xSize, pt.Y * mul),
                        new Vector2(pt.X, (pt.Y + ySize) * mul),
                        new Vector2(pt.X + xSize, (pt.Y + ySize) * mul),
                        new Vector2(pt.X + xSize*0.5f, (pt.Y+ySize*0.5f) * mul),
                    };

                    // If any corner is overlapped then we pass for rendering under conservative rasterization
                    bool anyCornersContained = false;
                    for (int i = 0; i < 5; ++i)
                        anyCornersContained |= IsPointContained(uvs, pixelCorners[i].X, pixelCorners[i].Y);

                    if (anyCornersContained) //if (IsPointContained(uvs, xCoord, yCoord))
                    {
                        Vector2 samplePt = pt;
                        samplePt.X += xSize * 0.5f;
                        samplePt.Y += ySize * 0.5f;

                        Vector3 samplePos = InterpolateVertexAttribute(uvs, samplePt, pos);
                        Vector3 sampleNor = InterpolateVertexAttribute(uvs, samplePt, norm);
                        sampleNor.Normalize();

                        ColorF? writeColor = callback(ref samplePos, ref sampleNor);
                        if (!writeColor.HasValue)
                            continue;

                        // If there's a mask than grab the appropriate mask pixel and multiply it with the write target
                        if (rasterData.Mask != null)
                        {
                            float maskWidthFraction = (float)rasterData.MaskWidth / (float)rasterData.Width;
                            float maskHeightFraction = (float)rasterData.MaskHeight / (float)rasterData.Height;
                            int maskIndex = Mathf.Clamp((int)(x * maskWidthFraction), 0, rasterData.MaskWidth - 1) + Mathf.Clamp((int)(y * maskHeightFraction), 0, rasterData.MaskHeight - 1) * rasterData.MaskWidth;
                            rasterData.Pixels[(int)x + (int)y * rasterData.Width] = writeColor.Value * rasterData.Mask[maskIndex];
                        }
                        else
                        {
                            rasterData.Pixels[(int)x + (int)y * rasterData.Width] = writeColor.Value;
                        }

                        // If we have a mask then record that we've written there
                        if (rasterData.WrittenMask != null)
                            rasterData.WrittenMask[(int)x + (int)y * rasterData.Width] = true;
                        rasterData.AnythingWritten = true;
                    }
                }
            }
        }

        public static void RasterizeModelSpaceTriangle(ref RasterizerData rasterData, Vector3[] pos, Vector3[] norm, Vector2[] uvs, ElementSampleCallbackWithUV callback)
        {
            float xSize = 1.0f / rasterData.Width;
            float ySize = 1.0f / rasterData.Height;

            Rect triBounds = CalculateTriangleImageBounds(rasterData.Width, rasterData.Height, uvs);
            for (float y = triBounds.yMin; y <= triBounds.yMax && y < rasterData.Height; y += 1.0f)
            {
                if (y < 0 || y >= rasterData.Height)
                    continue;

                float yCoord = y / rasterData.Height;
                for (float x = triBounds.xMin; x <= triBounds.xMax; x += 1.0f)
                {
                    if (x < 0 || x >= rasterData.Width)
                        continue;

                    float xCoord = x / rasterData.Width;
                    Vector2 pt;
                    pt.X = xCoord; pt.Y = yCoord;

                    float mul = 1.0f;
                    Vector2[] pixelCorners = {
                        new Vector2(pt.X, pt.Y * mul),
                        new Vector2(pt.X + xSize, pt.Y * mul),
                        new Vector2(pt.X, (pt.Y + ySize) * mul),
                        new Vector2(pt.X + xSize, (pt.Y + ySize) * mul),
                        new Vector2(pt.X + xSize*0.5f, (pt.Y+ySize*0.5f) * mul), // center
                    };

                    // If any corner is overlapped then we pass for rendering under conservative rasterization
                    bool anyCornersContained = false;
                    for (int i = 0; i < 5; ++i)
                        anyCornersContained |= IsPointContained(uvs, pixelCorners[i].X, pixelCorners[i].Y);

                    if (anyCornersContained) //if (IsPointContained(uvs, xCoord, yCoord))
                    {
                        Vector2 samplePt = pt;
                        samplePt.X += xSize * 0.5f;
                        samplePt.Y += ySize * 0.5f;
                        Vector3 samplePos = InterpolateVertexAttribute(uvs, pt, pos);
                        Vector3 sampleNor = InterpolateVertexAttribute(uvs, pt, norm);
                        Vector2 sampleUV = InterpolateVertexAttribute(uvs, pt, uvs);
                        sampleNor.Normalize();

                        ColorF? writeColor = callback(ref samplePos, ref sampleNor, ref sampleUV);
                        if (!writeColor.HasValue)
                            continue;

                        // If there's a mask than grab the appropriate mask pixel and multiply it with the write target
                        if (rasterData.Mask != null)
                        {
                            float maskWidthFraction = (float)rasterData.MaskWidth / (float)rasterData.Width;
                            float maskHeightFraction = (float)rasterData.MaskHeight / (float)rasterData.Height;
                            int maskIndex = Mathf.Clamp((int)(x * maskWidthFraction), 0, rasterData.MaskWidth - 1) + Mathf.Clamp((int)(y * maskHeightFraction), 0, rasterData.MaskHeight - 1) * rasterData.MaskWidth;
                            rasterData.Pixels[(int)x + (int)y * rasterData.Width] = writeColor.Value * rasterData.Mask[maskIndex];
                        }
                        else
                        {
                            rasterData.Pixels[(int)x + (int)y * rasterData.Width] = writeColor.Value;
                        }

                        // If we have a mask then record that we've written there
                        if (rasterData.WrittenMask != null)
                            rasterData.WrittenMask[(int)x + (int)y * rasterData.Width] = true;
                        rasterData.AnythingWritten = true;
                    }
                }
            }
        }

        static ColorF DoBlend(RasterizerBlend blendMode, ColorF dst, ColorF src)
        {
            switch (blendMode)
            {
                case RasterizerBlend.Add:
                    return dst + src;
                case RasterizerBlend.Subtract:
                    return new SprueKit.ColorF {
                        R = dst.R - src.R,
                        G = dst.G - src.G,
                        B = dst.B - src.B,
                        A = Math.Max(dst.A, src.A),
                    };
                case RasterizerBlend.Multiply:
                    return dst * src;
                case RasterizerBlend.Max:
                    return new SprueKit.ColorF
                    {
                        R = Math.Max(dst.R, src.R),
                        G = Math.Max(dst.G, src.G),
                        B = Math.Max(dst.B, src.B),
                        A = Math.Max(dst.A, src.A)
                    };
            }
            return src;
        }

        public static void Blit(ref RasterizerData dst, ref RasterizerData src, RasterizerBlend blendMode)
        {
            if (src.Width != dst.Width || src.Height != dst.Width)
                return;

            for (int idx = 0; idx < dst.Width * dst.Height; ++idx)
            {
                if (src.WrittenMask[idx])
                {
                    dst.Pixels[idx] = DoBlend(blendMode, dst.Pixels[idx], src.Pixels[idx]);
                    dst.AnythingWritten = true;
                }
            }
        }

        /// <summary>
        /// Rasterizes the given colors into the given texture pixels and marks the "written" mask as needed
        /// </summary>
        /// <param name="texture">Pixel data to write into</param>
        /// <param name="mask">boolean mask to mark as "written" when writing pixels</param>
        /// <param name="width">Width of the pixel data</param>
        /// <param name="height">Height of the pixel data</param>
        /// <param name="uvs">UV coordinates to write to</param>
        /// <param name="inputs">Color inputs to interpolate for the vertex UVs</param>
        public static void RasterizeTriangle(ref RasterizerData rasterData, Vector2[] uvs, ColorF[] inputs, PixelCallback callback)
        {
            Rect triBounds = CalculateTriangleImageBounds(rasterData.Width, rasterData.Height, uvs);
            for (float y = triBounds.yMin; y <= triBounds.yMax && y < rasterData.Height; y += 1.0f)
            {
                if (y < 0 || y >= rasterData.Height)
                    continue;

                float yCoord = y / rasterData.Height;
                for (float x = triBounds.xMin; x <= triBounds.xMax; x += 1.0f)
                {
                    if (x < 0 || x >= rasterData.Width)
                        continue;

                    float xCoord = x / rasterData.Width;
                    Vector2 pt;
                    pt.X = xCoord; pt.Y = yCoord;
                    if (IsPointContained(uvs, xCoord, yCoord))
                    {
                        ColorF writeColor = TexelToWorldSpace(uvs, pt, inputs);
                        if (callback != null)
                            callback(ref writeColor);

                        // If there's a mask than grab the appropriate mask pixel and multiply it with the write target
                        if (rasterData.Mask != null)
                        {
                            float maskWidthFraction = (float)rasterData.MaskWidth / (float)rasterData.Width;
                            float maskHeightFraction = (float)rasterData.MaskHeight / (float)rasterData.Height;
                            int maskIndex = Mathf.Clamp((int)(x * maskWidthFraction), 0, rasterData.MaskWidth - 1) + Mathf.Clamp((int)(y * maskHeightFraction), 0, rasterData.MaskHeight - 1) * rasterData.MaskWidth;
                            rasterData.Pixels[(int)x + (int)y * rasterData.Width] = writeColor * rasterData.Mask[maskIndex];
                        }
                        else
                        {
                            rasterData.Pixels[(int)x + (int)y * rasterData.Width] = writeColor;
                        }

                        // If we have a mask then record that we've written there
                        if (rasterData.WrittenMask != null)
                            rasterData.WrittenMask[(int)x + (int)y * rasterData.Width] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Rasterizes the given colors into the given texture pixels and marks the "written" mask as needed.
        /// This variation of the function uses a different callback and accepts another set of parameters that can be used with the callback.
        /// </summary>
        /// <param name="texture">Pixel data to write into</param>
        /// <param name="mask">boolean mask to mark as "written" when writing pixels</param>
        /// <param name="width">Width of the pixel data</param>
        /// <param name="height">Height of the pixel data</param>
        /// <param name="uvs">UV coordinates to write to</param>
        /// <param name="inputs">Color inputs to interpolate for the vertex UVs</param>
        public static void RasterizeTriangleWithRef(ref RasterizerData rasterData, Vector2[] uvs, ColorF[] inputs, ColorF[] reference, DoublePixelCallback callback)
        {
            Rect triBounds = CalculateTriangleImageBounds(rasterData.Width, rasterData.Height, uvs);
            for (float y = triBounds.yMin; y <= triBounds.yMax; y += 1.0f)
            {
                if (y < 0 || y >= rasterData.Height)
                    continue;

                float yCoord = y / rasterData.Height;
                for (float x = triBounds.xMin; x <= triBounds.xMax; x += 1.0f)
                {
                    if (x < 0 || x >= rasterData.Width)
                        continue;

                    float xCoord = x / rasterData.Width;
                    Vector2 pt = new Vector2(xCoord, yCoord);
                    if (IsPointContained(uvs, xCoord, yCoord))
                    {
                        ColorF writeColor = TexelToWorldSpace(uvs, pt, inputs);
                        ColorF refColor = TexelToWorldSpace(uvs, pt, reference);
                        if (callback != null)
                            callback(ref writeColor, refColor);

                        // If there's a mask than grab the appropriate mask pixel and multiply it with the write target
                        if (rasterData.Mask != null)
                        {
                            float maskWidthFraction = (float)rasterData.MaskWidth / (float)rasterData.Width;
                            float maskHeightFraction = (float)rasterData.MaskHeight / (float)rasterData.Height;
                            int maskIndex = Mathf.Clamp((int)(x * maskWidthFraction), 0, rasterData.MaskWidth - 1) + Mathf.Clamp((int)(y * maskHeightFraction), 0, rasterData.MaskHeight - 1) * rasterData.MaskWidth;
                            rasterData.Pixels[(int)x + (int)y * rasterData.Width] = writeColor * rasterData.Mask[maskIndex];
                        }
                        else
                            rasterData.Pixels[(int)x + (int)y * rasterData.Width] = writeColor;

                        if (rasterData.WrittenMask != null)
                            rasterData.WrittenMask[(int)x + (int)y * rasterData.Width] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates an integral rect that best fits the triangle to minimize the area to attempt rasterizing to
        /// <summary>
        public static Rect CalculateTriangleImageBounds(int width, int height, Vector2[] uvs)
        {
            int xMin = (int)Math.Min(uvs[0].X * width, Math.Min(uvs[1].X * width, uvs[2].X * width));
            int xMax = (int)Math.Max(uvs[0].X * width, Math.Max(uvs[1].X * width, uvs[2].X * width));

            int yMax = (int)Math.Max(uvs[0].Y * height, Math.Max(uvs[1].Y * height, uvs[2].Y * height));
            int yMin = (int)Math.Min(uvs[0].Y * height, Math.Min(uvs[1].Y * height, uvs[2].Y * height));
            Rect r = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            r.xMin = xMin - 1; r.xMax = xMax + 1;
            r.yMin = yMin - 1; r.yMax = yMax + 1;
            return r;
        }

        /// <summary>
        /// Utility for checking UV space point containment.
        /// </summary>
        public static bool IsPointContained(Vector2[] uvs, float x, float y)
        {
            Vector2 pt = new Vector2(x, y);
            Vector2 v0 = uvs[2] - uvs[0];
            Vector2 v1 = uvs[1] - uvs[0];
            Vector2 v2 = pt - uvs[0];

            float dot00 = Vector2.Dot(v0, v0); //??
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);

            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            return (u >= 0) && (v >= 0) && (u + v < 1.0f);
        }

        /// <summary>
        /// Returns the barycentric coordinates for a given UV set at a point.
        /// </summary>
        static Vector3 GetBarycentricFactors(Vector2[] uvs, Vector2 point)
        {
            Vector2 v0 = uvs[2] - uvs[0];
            Vector2 v1 = uvs[1] - uvs[0];
            Vector2 v2 = point - uvs[0];

            float dot00 = Vector2.Dot(v0, v0); //??
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);

            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            float w = 1.0f - v - u;

            return new Vector3(u, v, w);
        }

        /// <summary>
        /// Returns the barycentric coordinates for the given 3d vertices at a point.
        /// </summary>
        static Vector3 GetBarycentricFactors(Vector3[] points, Vector3 forPoint)
        {
            Vector3 f1 = points[0] - forPoint;
            Vector3 f2 = points[1] - forPoint;
            Vector3 f3 = points[2] - forPoint;

            float a = Vector3.Cross((points[0] - points[1]), (points[0] - points[2])).Length();
            float a1 = Vector3.Cross(f2, f3).Length() / a;
            float a2 = Vector3.Cross(f3, f1).Length() / a;
            float a3 = Vector3.Cross(f1, f2).Length() / a;

            Vector3 r;
            r.X = a1;
            r.Y = a2;
            r.Z = a3;
            return r;
        }

        public static Vector3 InterpolateVertexAttribute(Vector2[] uvCoords, Vector2 pointCoords, Vector3[] points)
        {
            Vector3 vec = GetBarycentricFactors(uvCoords, pointCoords);
            float x = points[0].X * vec.X + points[1].X * vec.Y + points[2].X * vec.Z;
            float y = points[0].Y * vec.X + points[1].Y * vec.Y + points[2].Y * vec.Z;
            float z = points[0].Z * vec.X + points[1].Z * vec.Y + points[2].Z * vec.Z;
            return new Vector3(x,y,z);
        }

        public static Vector2 InterpolateVertexAttribute(Vector2[] uvCoords, Vector2 pointCoords, Vector2[] points)
        {
            Vector3 vec = GetBarycentricFactors(uvCoords, pointCoords);
            float x = points[0].X * vec.X + points[1].X * vec.Y + points[2].X * vec.Z;
            float y = points[0].Y * vec.X + points[1].Y * vec.Y + points[2].Y * vec.Z;
            return new Vector2(x, y);
        }

        /// <summary>
        /// Performs texture rasterization of inputs.
        /// </summary>
        public static ColorF TexelToWorldSpace(Vector2[] uvCoords, Vector2 pointCoords, Vector3[] points)
        {
            Vector3 vec = GetBarycentricFactors(uvCoords, pointCoords);            
            float x = points[0].X * vec.X + points[1].X * vec.Y + points[2].X * vec.Z;
            float y = points[0].Y * vec.X + points[1].Y * vec.Y + points[2].Y * vec.Z;
            float z = points[0].Z * vec.X + points[1].Z * vec.Y + points[2].Z * vec.Z;
            ColorF ret;
            ret.R = x;
            ret.G = y;
            ret.B = z;
            ret.A = 1.0f;
            return ret;
        }

        /// <summary>
        /// Performs texture rasterization of inputs.
        /// </summary>
        public static Vector3 AttributeToWorldSpace(Vector2[] uvCoords, Vector2 pointCoords, Vector3[] points)
        {
            Vector3 vec = GetBarycentricFactors(uvCoords, pointCoords);
            float x = points[0].X * vec.X + points[1].X * vec.Y + points[2].X * vec.Z;
            float y = points[0].Y * vec.X + points[1].Y * vec.Y + points[2].Y * vec.Z;
            float z = points[0].Z * vec.X + points[1].Z * vec.Y + points[2].Z * vec.Z;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Performs texture rasterization of inputs.
        /// </summary>
        public static ColorF TexelToWorldSpace(Vector2[] uvCoords, Vector2 pointCoords, ColorF[] points)
        {
            float[,] T = new float[2, 2];
            float[,] iT = new float[2, 2];

            T[0, 0] = uvCoords[0].X - uvCoords[2].X;
            T[0, 1] = uvCoords[1].X - uvCoords[2].X;
            T[1, 0] = uvCoords[0].Y - uvCoords[2].Y;
            T[1, 1] = uvCoords[1].Y - uvCoords[2].Y;

            float d = T[0, 0] * T[1, 1] - T[0, 1] * T[1, 0];

            iT[0, 0] = T[1, 1] / d;
            iT[0, 1] = -T[0, 1] / d;
            iT[1, 0] = -T[1, 0] / d;
            iT[1, 1] = T[0, 0] / d;

            float lambda0 = iT[0, 0] * (pointCoords.X - uvCoords[2].X) + iT[0, 1] * (pointCoords.Y - uvCoords[2].Y);
            float lambda1 = iT[1, 0] * (pointCoords.X - uvCoords[2].X) + iT[1, 1] * (pointCoords.Y - uvCoords[2].Y);
            float lambda2 = 1.0f - lambda0 - lambda1;

            float x = points[0].R * lambda0 + points[1].R * lambda1 + points[2].R * lambda2;
            float y = points[0].G * lambda0 + points[1].G * lambda1 + points[2].G * lambda2;
            float z = points[0].B * lambda0 + points[1].B * lambda1 + points[2].B * lambda2;
            float a = points[0].A * lambda0 + points[1].A * lambda1 + points[2].A * lambda2;
            ColorF ret;
            ret.R = x;
            ret.G = y;
            ret.B = z;
            ret.A = a;
            return ret;
        }

        public static Color ToColor(this Vector3 vec)
        {
            return new Color(vec.X, vec.Y, vec.Z, 1.0f);
        }
    }
}
