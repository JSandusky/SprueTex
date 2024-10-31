using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework;

namespace SprueKit.Data
{
    public static class BindingUtil
    {
        public static Vector4 ToVec4(this PluginLib.IntVector4 v)
        {
            return new Vector4(v.X, v.Y, v.Z, v.W);
        }

        public static void ToMesh(SprueBindings.MeshData model, ref List<int> indices, ref List<PluginLib.VertexData> verts)
        {
            indices.AddRange(model.Indices);

            for (int i = 0; i < model.Positions.Length; ++i)
            {
                bool hasNorm = model.Normals != null && i < model.Normals.Length;
                bool hasUV = model.UVCoordinates != null && i < model.UVCoordinates.Length;
                bool hasBoneWeights = model.BoneWeights != null && model.BoneIndices != null && i < model.BoneWeights.Length && i < model.BoneIndices.Length;
                bool hasTangents = model.Tangents != null && i < model.Tangents.Length;
                verts.Add(new PluginLib.VertexData
                {
                    Position = model.Positions[i],
                    Normal = hasNorm ? model.Normals[i] : new Microsoft.Xna.Framework.Vector3(),
                    Tangent = hasTangents ? model.Tangents[i] : new Vector4(),
                    TextureCoordinate = hasUV ? model.UVCoordinates[i] : new Microsoft.Xna.Framework.Vector2(),
                    BoneWeights = hasBoneWeights ? model.BoneWeights[i] : new Vector4(),
                    BoneIndices = hasBoneWeights ? model.BoneIndices[i].ToVec4() : new Vector4(-1, -1, -1, -1)
                });
            }
        }

        public static MeshData ToMesh(SprueBindings.MeshData mesh)
        {
            List<int> indices = new List<int>();
            List<PluginLib.VertexData> verts = new List<PluginLib.VertexData>();
            PluginLib.SkeletonData skeleton = null;
            mesh.ReadFromAPI();
            ToMesh(mesh, ref indices, ref verts);
            return new Data.MeshData(indices, verts) { Skeleton = skeleton };
        }

        public static MeshData ToMesh(SprueBindings.ModelData model)
        {
            List<int> indices = new List<int>();
            List<PluginLib.VertexData> verts = new List<PluginLib.VertexData>();
            PluginLib.SkeletonData skeleton = null;
            foreach (var mesh in model.Meshes)
            {
                mesh.ReadFromAPI();
                ToMesh(mesh, ref indices, ref verts);
                if (mesh.Skeleton != null)
                    skeleton = mesh.Skeleton;
            }
            return new Data.MeshData(indices, verts) { Skeleton = skeleton };
        }

        public static SprueBindings.MeshData ToMeshData(MeshData meshData)
        {
            SprueBindings.MeshData data = new SprueBindings.MeshData
            {
                Positions = new Vector3[meshData.VertexCount],
                Normals = new Vector3[meshData.VertexCount],
                Indices = new int[meshData.IndexCount],
                UVCoordinates = new Vector2[meshData.VertexCount],
                BoneWeights = new Vector4[meshData.VertexCount],
                BoneIndices = new PluginLib.IntVector4[meshData.VertexCount]
            };

            var vertices = meshData.GetVertices();
            var indices = meshData.GetIndices();

            for (int i = 0; i < data.Positions.Length; ++i)
                data.Positions[i] = vertices[i].Position;
            for (int i = 0; i < data.Normals.Length; ++i)
                data.Normals[i] = vertices[i].Normal;
            for (int i = 0; i < data.Indices.Length; ++i)
                data.Indices[i] = indices[i];
            for (int i = 0; i < data.UVCoordinates.Length; ++i)
                data.UVCoordinates[i] = vertices[i].TextureCoordinate;
            for (int i = 0; i < data.BoneIndices.Length; ++i)
            {
                var inds = vertices[i].BoneIndices;
                data.BoneIndices[i] = new PluginLib.IntVector4((int)inds.X, (int)inds.Y, (int)inds.Z, (int)inds.W);
                data.BoneWeights[i] = vertices[i].BoneWeights;
            }

            data.Skeleton = meshData.Skeleton;
            data.WriteToAPI();
            return data;
        }

        public static SprueBindings.ModelData ToModel(MeshData meshData)
        {
            SprueBindings.ModelData ret = new SprueBindings.ModelData();
            ret.Meshes = new List<SprueBindings.MeshData>();
            ret.Meshes.Add(ToMeshData(meshData));
            return ret;
        }

        public static SprueBindings.ModelData ToModel(SprueModel model)
        {
            SprueBindings.ModelData ret = new SprueBindings.ModelData();
            ret.Meshes = new List<SprueBindings.MeshData>();
            FillModel(ret, model);
            return ret;
        }

        static void FillModel(SprueBindings.ModelData target, SpruePiece current)
        {
            if (current is SprueModel)
            {
                var self = current as SprueModel;
                if (self.MeshData != null)
                    target.Meshes.Add(ToMeshData(self.MeshData));
            }
            else if (current is ModelPiece)
            {
                var self = current as ModelPiece;
                // don't include merge meshes
                if (self.Combine == SprueBindings.CSGTask.Independent || self.Combine == SprueBindings.CSGTask.ClipIndependent)
                {
                    List<MeshData> meshes = self.GetMeshes();
                    if (meshes != null && meshes.Count > 0)
                    {
                        foreach (var meshData in meshes)
                            target.Meshes.Add(ToMeshData(meshData));
                    }
                }
            }

            foreach (SpruePiece p in current.FlatChildren)
                FillModel(target, p);
        }

        public static BitmapSource ToBitmap(SprueBindings.ImageData image, int maxDim)
        {
            if (image.Width < maxDim && image.Height < maxDim)
                return ToBitmap(image);

            float widthFactor = ((float)maxDim) / image.Width;
            float heightFactor = ((float)maxDim) / image.Height;

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap((int)(image.Width * widthFactor), (int)(image.Height * heightFactor), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //WriteableBitmap bitmap = WriteableBitmap.Create(128, 128, 72, 72, System.Windows.Media.PixelFormats.Rgba128Float, null, IntPtr.Zero, 0, 0) as WriteableBitmap;
            //int yFactor = (int)(1.0f / (128.0f / image.Height));
            //int xFactor = (int)(1.0f / (128.0f / image.Width));
            for (int y = 0; y < bmp.Height; ++y)
            {
                float fY = ((float)y) / bmp.Height;
                for (int x = 0; x < bmp.Width; ++x)
                {
                    float fX = ((float)x) / bmp.Width;
                    var col = image.GetPixelBilinear(fX, fY);
                    bmp.SetPixel(x, y, col.ToDrawingColor());
                }
            }

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(image.Width, image.Height));
        }

        public static System.Drawing.Bitmap ToDrawingBitmap(this SprueBindings.ImageData image)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            image.WriteInto(bmp);
            return bmp;
        }

        public static void WriteInto(this SprueBindings.ImageData image, System.Drawing.Bitmap bmp)
        {
            for (int y = 0; y < image.Height; ++y)
            {
                for (int x = 0; x < image.Width; ++x)
                {
                    var col = image.Pixels[y * image.Width + x];
                    bmp.SetPixel(x, y, col.ToDrawingColor());
                }
            }
        }

        public static BitmapSource ToBitmap(SprueBindings.ImageData image)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            image.WriteInto(bmp);
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(image.Width, image.Height));
        }

        public static SprueBindings.ImageData ToImageData(Baking.RasterizerData data)
        {
            SprueBindings.ImageData ret = new SprueBindings.ImageData();
            ret.Width = data.Width;
            ret.Height = data.Height;

            ret.Pixels = new Color[ret.Width * ret.Height];
            for (int i = 0; i < data.Pixels.Length; ++i)
                ret.Pixels[i] = data.Pixels[i].ToMonoGame();

            return ret;
        }

        public static SprueBindings.ImageData ToImageData(this System.Drawing.Bitmap bmp)
        {
            SprueBindings.ImageData ret = new SprueBindings.ImageData();
            ret.Width = bmp.Width;
            ret.Height = bmp.Height;
            ret.Pixels = new Color[ret.Width * ret.Height];
            for (int y = 0; y < bmp.Height; ++y)
                for (int x = 0; x < bmp.Width; ++x)
                    ret.Pixels[y * bmp.Width + x] = bmp.GetPixel(x, y).ToXNAColor();
            return ret;
        }

        //public static Color GetBilinear(SprueBindings.ImageData data, float x, float y)
        //{
        //
        //}
    }
}
