using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using SprueKit.Data.Graph;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Color = Microsoft.Xna.Framework.Color;
using IntVector2 = PluginLib.IntVector2;
using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using Microsoft.Xna.Framework;

namespace SprueKit.Data.TexGen
{
    [Description("Contains a 3d meshes for use by mesh baking nodes")]
    public partial class ModelNode : TexGenNode
    {
        protected ForeignModel modelFile_ = new ForeignModel();
        [Description("3d model file to emit from this node")]
        public ForeignModel ModelFile { get { return modelFile_; } set { modelFile_ = value; OnPropertyChanged(); } }

        protected bool dirty_ = true;

        public ModelNode()
        {
            ModelFile.PropertyChanged += (o, e) => { dirty_ = true; OnPropertyChanged("DisplayName"); };
        }

        public override string DisplayName
        {
            get
            {
                if (ModelFile.ModelFile != null)
                    return string.Format("{0}\r\n{1}", Name, System.IO.Path.GetFileName(ModelFile.ModelFile.AbsolutePath));
                else
                    return Name;
            }
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Model";
            AddOutput(new GraphSocket(this) { Name = "Out", IsOutput = true, IsInput = false, TypeID = SocketTypeID.Model });
        }

        public override void PrimeEarly(object param)
        {
            base.PrimeBeforeExecute(param);
            Execute(null);
        }

        public override void Execute(object param)
        {
            if (ModelFile != null)
                OutputSockets[0].Data = ModelFile.GetMeshes();
            else
                OutputSockets[0].Data = null;
        }
    }

    [PropertyData.PreviewRequiresMesh]
    public partial class BakerNode : TexGenNode
    {
        bool noDispose_ = false;
        protected System.Drawing.Bitmap cache_;

        //protected ForeignModel modelFile_ = new ForeignModel();
        //[Description("3d model file to use for this bake")]
        //public ForeignModel ModelFile { get { return modelFile_; } set { modelFile_ = value; OnPropertyChanged(); } }

        protected bool dirty_ = true;

        public BakerNode()
        {
           // ModelFile.PropertyChanged += (o, e) => dirty_ = true;
        }

        ~BakerNode()
        {
            if (cache_ != null && !noDispose_)
                cache_.Dispose();
        }

        IntVector2 cacheSize_ = new IntVector2(256, 256);
        public IntVector2 TextureSize { get { return cacheSize_; } set { cacheSize_ = value; OnPropertyChanged(); } }

        public override void SocketConnectivityChanged(GraphSocket socket)
        {
            if (socket.IsInput == true)
                dirty_ = true;
        }

        public override void Execute(object param)
        {
            base.Execute(param);
            if (cache_ != null)
            {
                Vector4 p = (Vector4)param;
                OutputSockets[0].Data = cache_.GetPixelBilinear(p.X, p.Y).ToXNAColor();
            }
            else
                OutputSockets[0].Data = 0.0f;
        }

        protected bool PrePrime()
        {
            bool ret = dirty_;
            if (dirty_)
            {
                if (cache_ != null)
                {
                    cache_.Dispose();
                    cache_ = null;
                }
                //??PostPrime();
            }
            if (InputSockets[0].CachedInputSource != null)
                InputSockets[0].Data = InputSockets[0].CachedInputSource.Data;
            return ret;
        }

        /// <summary>
        /// After we prime we need to transfer our cache back up to the main
        /// </summary>
        protected void PostPrime()
        {
            if (dirty_)
            {
                if (cache_ != null)
                {
                    if (CloneSource != null)
                    {
                        lock (CloneSource)
                        {
                            var clone = ((BakerNode)CloneSource);
                            if (clone.cache_ != null)
                            {
                                clone.cache_.Dispose();
                                ((BakerNode)CloneSource).cache_ = cache_;
                                noDispose_ = true;
                            }
                        }
                    }
                }
                dirty_ = false;
                if (CloneSource != null)
                    ((BakerNode)CloneSource).dirty_ = false;
            }
        }

        protected System.Drawing.Bitmap CreateCache()
        {
            if (dirty_)
            {
                if (cache_ != null)
                    cache_.Dispose();
                cache_ = new System.Drawing.Bitmap(Math.Max(cacheSize_.X, 1), Math.Max(cacheSize_.Y, 1));
            }
            return cache_;
        }

        protected void Rasterize(ref Baking.RasterizerData rasterData, Vector3[] pos, Vector3[] nor, Vector2[] uv)
        {
            Baking.MeshRasterizer.RasterizeModelSpaceTriangle(ref rasterData, pos, nor, uv, (ref Vector3 tPos, ref Vector3 tNor) =>
            {
                //??Color? value = data.Key.Sample(tPos, tNor, data.Value?.Texture);
                //if (!value.HasValue)
                //    return null;
                //return ColorF.FromMonoGame(value.Value);
                return new ColorF();
            });
        }
    }

    [Description("Outputs a finite element based solution to the ambient occlusion of the mesh")]
    public partial class AmbientOcclusionBaker : BakerNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Ambient Occlusion";
            AddInput(new GraphSocket(this) { Name = "Model", TypeID = SocketTypeID.Model, IsInput = true, IsOutput = false });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            if (!PrePrime())
                return;

            if (dirty_)
            {
                var cache = CreateCache();

                var meshes = InputSockets[0].Data as List<MeshData>;
                if (meshes != null)
                {
                    List<SprueBindings.MeshData> outMeshData = new List<SprueBindings.MeshData>();
                    if (meshes.Count > 0)
                    {
                        foreach (var mesh in meshes)
                            outMeshData.Add(BindingUtil.ToMeshData(mesh));
                        var imgData = SprueBindings.Bakers.AmbientOcclusion(outMeshData.ToArray(), Math.Max(1, TextureSize.X), Math.Max(1, TextureSize.Y), null);
                        imgData.WriteInto(cache);
                    }
                }
            }
            PostPrime();
        }
    }

    [Description("Outputs a finite element based solution to finding an estimated thickness of the mesh")]
    public partial class ThicknessBaker : BakerNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Thickness";
            AddInput(new GraphSocket(this) { Name = "Model", TypeID = SocketTypeID.Model, IsInput = true, IsOutput = false });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            if (!PrePrime())
                return;

            if (dirty_)
            {
                var cache = CreateCache();

                var meshes = InputSockets[0].Data as List<MeshData>;
                if (meshes != null)
                {
                    List<SprueBindings.MeshData> outMeshData = new List<SprueBindings.MeshData>();
                    if (meshes.Count > 0)
                    {
                        foreach (var mesh in meshes)
                            outMeshData.Add(BindingUtil.ToMeshData(mesh));
                        var imgData = SprueBindings.Bakers.Thickness(outMeshData.ToArray(), Math.Max(1, TextureSize.X), Math.Max(1, TextureSize.Y), null);
                        imgData.WriteInto(cache);
                    }
                }
            }
            PostPrime();
        }
    }

    [Description("Outputs the MSP of a mesh in normalized coordinates")]
    public partial class ModelSpacePositionBaker : BakerNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Model Space Position";
            AddInput(new GraphSocket(this) { Name = "Model", TypeID = SocketTypeID.Model, IsInput = true, IsOutput = false });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            if (!PrePrime())
                return;

            if (dirty_)
            {
                var cache = CreateCache();

                var meshes = InputSockets[0].Data as List<MeshData>;
                if (meshes != null)
                {
                    List<SprueBindings.MeshData> outMeshData = new List<SprueBindings.MeshData>();
                    if (meshes.Count > 0)
                    {
                        foreach (var mesh in meshes)
                            outMeshData.Add(BindingUtil.ToMeshData(mesh));
                        var imgData = SprueBindings.Bakers.ObjectSpacePosition(outMeshData.ToArray(), Math.Max(1, TextureSize.X), Math.Max(1, TextureSize.Y), null);
                        imgData.WriteInto(cache);
                    }
                }
            }
            PostPrime();
        }
    }

    [Description("Outputs the normals of the mesh in modelspace")]
    public partial class ModelSpaceNormalBaker : BakerNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Model Space Normal";
            AddInput(new GraphSocket(this) { Name = "Model", TypeID = SocketTypeID.Model, IsInput = true, IsOutput = false });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            if (!PrePrime())
                return;

            if (dirty_)
            {
                var cache = CreateCache();
                var meshes = InputSockets[0].Data as List<MeshData>;
                if (meshes != null)
                {
                    List<SprueBindings.MeshData> outMeshData = new List<SprueBindings.MeshData>();
                    if (meshes.Count > 0)
                    {
                        foreach (var mesh in meshes)
                            outMeshData.Add(BindingUtil.ToMeshData(mesh));
                        var imgData = SprueBindings.Bakers.ObjectSpaceNormal(outMeshData.ToArray(), Math.Max(1, TextureSize.X), Math.Max(1, TextureSize.Y), null);
                        imgData.WriteInto(cache);
                    }
                }
            }
            PostPrime();
        }
    }

    public abstract class TextureMapBaker : BakerNode
    {
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public System.Windows.Media.Imaging.BitmapSource Thumbnail { get; set; }

        protected SprueBindings.ImageData data_;
        protected Uri imageFile_;
        protected bool bilinearFilter_ = true;

        [PropertyData.ResourceTag(Type = PropertyData.ResourceTagType.RasterTexture)]
        [Description("Source texture to use for sampling")]
        public Uri Texture
        {
            get { return imageFile_; }
            set
            {
                imageFile_ = value;
                data_ = null;
                if (value != null)
                {
                    data_ = SprueBindings.ImageData.Load(value.AbsolutePath, ErrorHandler.inst());
                    if (data_ == null)
                    {
                        imageFile_ = null;
                        Thumbnail = null;
                    }
                    else
                    {
                        Thumbnail = BindingUtil.ToBitmap(data_, 512);
                    }
                }
                OnPropertyChanged("Thumbnail");
                OnPropertyChanged();
            }
        }

        Vector2 tiling_ = Vector2.One;
        [Description("How many times the image will tile per axis")]
        public Vector2 Tiling { get { return tiling_; }set { tiling_ = value; OnPropertyChanged(); } }

        public virtual bool WorksWithoutTexture() { return false; }

        public virtual bool WantsBlur() { return false; }

        public override void Construct()
        {
            base.Construct();
            AddInput(new GraphSocket(this) { Name = "Model", TypeID = SocketTypeID.Model, IsInput = true, IsOutput = false });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            if ((Texture == null || data_ == null) && !WorksWithoutTexture())

            if (!PrePrime())
                return;

            if (dirty_)
            {
                var cache = CreateCache();
                var meshes = InputSockets[0].Data as List<MeshData>;
                if (meshes != null)
                {
                    int Width = Math.Max(1, TextureSize.X);
                    int Height = Math.Max(1, TextureSize.Y);

                    Baking.RasterizerData rasterData = new Baking.RasterizerData();
                    rasterData.Init(Width, Height, true);

                    foreach (var meshData in meshes)
                    {
                        var meshIndices = meshData.GetIndices();
                        var vertices = meshData.GetVertices();
                        for (int vertIndex = 0; vertIndex < meshIndices.Count; vertIndex += 3)
                        {
                            int[] indices = {
                                    meshIndices[vertIndex],
                                    meshIndices[vertIndex + 1],
                                    meshIndices[vertIndex + 2],
                                };
                            Vector2[] uvs = {
                                    vertices[indices[2]].TextureCoordinate,
                                    vertices[indices[1]].TextureCoordinate,
                                    vertices[indices[0]].TextureCoordinate,
                                };
                            Vector3[] pos = {
                                    vertices[indices[0]].Position,
                                    vertices[indices[1]].Position,
                                    vertices[indices[2]].Position,
                                };
                            Vector3[] nor = {
                                    vertices[indices[0]].Normal,
                                    vertices[indices[1]].Normal,
                                    vertices[indices[2]].Normal,
                                };

                            Baking.MeshRasterizer.RasterizeModelSpaceTriangle(ref rasterData, pos, nor, uvs, Sample);
                        }
                    }

                    if (WantsBlur())
                        Baking.RasterizerData.Blur(ref rasterData);
                    Baking.RasterizerData.PadEdges(ref rasterData, 4);
                    
                    for (int y = 0; y < Height; ++y)
                    {
                        for (int x = 0; x < Width; ++x)
                            cache.SetPixel(x, y, rasterData.Pixels[y * Width + x].ToMonoGame().ToDrawingColor());
                    }
                }
            }
            PostPrime();
        }

        protected abstract ColorF? Sample(ref Vector3 pos, ref Vector3 nor, ref Vector2 uv);

        protected override void CloneFields(GraphNode node)
        {
            base.CloneFields(node);
            var self = node as TextureMapBaker;
            self.tiling_ = tiling_;
            self.imageFile_ = imageFile_;
            self.data_ = data_;
            self.bilinearFilter_ = bilinearFilter_;
        }
    }

    [Description("Projects a texture onto a mesh using triplanar or cube mapping")]
    public partial class TriplanarTextureBaker : TextureMapBaker
    {
        bool useTriplanar_ = true;
        
        [Description("Whether to use triplanar sampling or cubemap projection")]
        public bool TriplanarSampling { get { return useTriplanar_; } set { useTriplanar_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Triplanar Bake";
        }

        protected override ColorF? Sample(ref Vector3 pos, ref Vector3 nor, ref Vector2 uv)
        {
            if (TriplanarSampling)
            {
                Vector3 interpolatedNormal = nor.Abs();
                float weightSum = interpolatedNormal.X + interpolatedNormal.Y + interpolatedNormal.Z;
                interpolatedNormal /= weightSum;

                Vector3 scaling = new Vector3(1, 1, 1);

                Vector2 coord1 = new Vector2(pos.Y, pos.Z);// * scaling.X;
                Vector2 coord2 = new Vector2(pos.X, pos.Z);// * scaling.Y;
                Vector2 coord3 = new Vector2(pos.X, pos.Y);// * scaling.Z;

                Color col1 = data_.GetPixelBilinear(coord1.X * Tiling.X, coord1.Y * Tiling.Y);
                Color col2 = data_.GetPixelBilinear(coord2.X * Tiling.X, coord2.Y * Tiling.Y);
                Color col3 = data_.GetPixelBilinear(coord3.X * Tiling.X, coord3.Y * Tiling.Y);

                Color writeColor = col1.Mul(interpolatedNormal.X).Add(col2.Mul(interpolatedNormal.Y)).Add(col3.Mul(interpolatedNormal.Z));
                writeColor.A = 255;
                return new SprueKit.ColorF(writeColor.R, writeColor.G, writeColor.B, writeColor.A);
            }
            else
            {
                // cubemap style mapping
                Vector3 absVec = nor.Abs();
                int maxElem = absVec.MaxElementIndex();
                Vector2 coord = Vector2.Zero;
                if (maxElem == 0) // X axis
                    coord = new Vector2(pos.Z, pos.Y);
                else if (maxElem == 1) // Y axis
                    coord = new Vector2(pos.X, pos.Z);
                else if (maxElem == 2) // Z axis
                    coord = new Vector2(pos.X, pos.Y);

                var writeColor = data_.GetPixelBilinear(coord.X * Tiling.X, coord.Y * Tiling.Y);
                return new SprueKit.ColorF(writeColor.R, writeColor.G, writeColor.B, writeColor.A);
            }
        }
    }

    [Description("Projects a texture onto a mesh as a cylinder rolled around it")]
    public partial class CylindricalTextureBaker : TextureMapBaker
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Cylindrical Bake";
        }

        protected override ColorF? Sample(ref Vector3 pos, ref Vector3 nor, ref Vector2 uv)
        {
            float u = Mathf.Atan2(pos.X, pos.Z) / (2 * Mathf.PI) + 0.5f;
            float v = (pos.Y * 0.5f) + 0.5f;
            var colValue = data_.GetPixelBilinear(u * Tiling.X, v * Tiling.Y);
            return new SprueKit.ColorF(colValue.R, colValue.G, colValue.B, colValue.A);
        }
    }

    [Description("Bakes a directional light into a texture")]
    [PropertyData.PropertyIgnore]
    [PropertyData.OverrideName("Texture", "Normal Map")]
    public partial class FauxLightBaker : TextureMapBaker
    {
        bool flipNormals_ = false;
        bool blur_ = false;
        Color lightColor_ = Color.White;
        Vector3 lightDirection_ = -Vector3.UnitY;

        [Description("If the mesh requires flipping culling then it will also require flipping the normals")]
        public bool FlipNormals { get { return flipNormals_; } set { flipNormals_ = value; OnPropertyChanged(); } }

        [Description("Run an LCSM relaxation step to soften the hardness of the light, can be problematic around poorly placed UV seams")]
        public bool LCSMRelax { get { return blur_; }set { blur_ = value; OnPropertyChanged(); } }

        public override bool WantsBlur()
        {
            return LCSMRelax;
        }

        [Description("Color of the light to bake onto the mesh")]
        public Color LightColor { get { return lightColor_; }set { lightColor_ = value; OnPropertyChanged(); } }

        [PropertyData.HasGizmo(GizmoType = PropertyData.GizmoProperties.DirectionOnly)]
        [Description("Direction the light source will come from")]
        public Vector3 LightDirection { get { return lightDirection_; }set { lightDirection_ = value; OnPropertyChanged(); } }

        public override bool WorksWithoutTexture() { return true; }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            lightDirection_ = Vector3.Normalize(lightDirection_);
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Faux Light";
        }

        protected override ColorF? Sample(ref Vector3 pos, ref Vector3 nor, ref Vector2 uv)
        {
            float dp = -Vector3.Dot(Vector3.Normalize(lightDirection_), flipNormals_ ? -nor : nor);
            if (data_ != null)
            {
                // this is not correct ... but I don't have the tangents to do it correctly
                float norDot = -Vector3.Dot(data_.GetPixelBilinear(uv.X * Tiling.X, uv.Y * Tiling.Y).ToNormal(), Vector3.UnitZ);
                if (norDot > 0)
                    dp *= norDot;
            }
            if (dp > 0.0f)
                return new ColorF(
                    dp * (lightColor_.R / 255.0f), 
                    dp * (lightColor_.G / 255.0f), 
                    dp * (lightColor_.B / 255.0f), 
                    1.0f);
            return null;
        }
    }

    [Description("Bakes a positionable decal")]
    public partial class DecalBaker : TextureMapBaker
    {
        Matrix decalTransform_ = Matrix.Identity;
        bool passThrough_ = false;

        [Description("If active the decal will appear on backfacing triangles as well as front facing")]
        public bool PassThrough { get { return passThrough_; }set { passThrough_ = value; OnPropertyChanged(); } }

        [PropertyData.HasGizmo(GizmoType = PropertyData.GizmoProperties.Full)]
        [Description("Positioning and scale of the decal in 3-space")]
        public Matrix DecalTransform { get { return decalTransform_; } set { decalTransform_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Decal";
        }

        protected override ColorF? Sample(ref Vector3 pos, ref Vector3 nor, ref Vector2 uv)
        {

            return null;
        }
    }

    [Description("Outputs the deviation of each vertex from the plane of it's one-ring neighbors")]
    public partial class CurvatureBaker : BakerNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Curvature";
            AddInput(new GraphSocket(this) { Name = "Model", TypeID = SocketTypeID.Model, IsInput = true, IsOutput = false });

            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
            AddOutput(new GraphSocket(this) { Name = "Concave", TypeID = SocketTypeID.Grayscale, IsInput = false, IsOutput = true });
            AddOutput(new GraphSocket(this) { Name = "Convex", TypeID = SocketTypeID.Grayscale, IsInput = false, IsOutput = true });
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            if (!PrePrime())
                return;

            if (dirty_)
            {
                var cache = CreateCache();

                var meshes = InputSockets[0].Data as List<MeshData>;
                if (meshes != null)
                {
                    List<SprueBindings.MeshData> outMeshData = new List<SprueBindings.MeshData>();
                    if (meshes.Count > 0)
                    {
                        foreach (var mesh in meshes)
                            outMeshData.Add(BindingUtil.ToMeshData(mesh));
                        var imgData = SprueBindings.Bakers.Curvature(outMeshData.ToArray(), Math.Max(1, TextureSize.X), Math.Max(1, TextureSize.Y), null);
                        imgData.WriteInto(cache);
                    }
                }
            }
            PostPrime();
        }

        public override void Execute(object param)
        {
            base.Execute(param);
            var colValue = OutputSockets[0].GetColor().ToVector4();
            OutputSockets[1].Data = colValue.X;
            OutputSockets[2].Data = colValue.Z;
        }
    }

    [Description("Outputs the dominant plane of each triangle")]
    public partial class DominantPlaneBaker : BakerNode
    {
        static readonly Vector3[] PlaneAxes = {
            new Vector3(0,1,0),
            new Vector3(0,-1,0),
            new Vector3(-1,0,0),
            new Vector3(1,0,0),
            new Vector3(0,0,1),
            new Vector3(0,0,-1)
        };

        static readonly ColorF[] PlaneColors = {
            new ColorF(0.5f, 1.0f, 0.5f),
            new ColorF(0.5f, 0.0f, 0.5f),
            new ColorF(0.0f, 0.5f, 0.5f),
            new ColorF(1.0f, 0.5f, 0.5f),
            new ColorF(0.5f, 0.5f, 1.0f),
            new ColorF(0.5f, 0.5f, 0.0f)
        };

        public override void Construct()
        {
            base.Construct();
            Name = "Dominant Plane";
            AddInput(new GraphSocket(this) { Name = "Model", TypeID = SocketTypeID.Model, IsInput = true, IsOutput = false });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            if (!PrePrime())
                return;

            if (dirty_)
            {
                var cache = CreateCache();
                var meshes = InputSockets[0].Data as List<MeshData>;
                if (meshes != null)
                {
                    int Width = Math.Max(1, TextureSize.X);
                    int Height = Math.Max(1, TextureSize.Y);

                    Baking.RasterizerData rasterData = new Baking.RasterizerData();
                    rasterData.Init(Width, Height, true);

                    foreach (var meshData in meshes)
                    {
                        var meshIndices = meshData.GetIndices();
                        var vertices = meshData.GetVertices();
                        for (int vertIndex = 0; vertIndex < meshIndices.Count; vertIndex += 3)
                        {
                            int[] indices = {
                                    meshIndices[vertIndex],
                                    meshIndices[vertIndex + 1],
                                    meshIndices[vertIndex + 2],
                                };
                            Vector2[] uvs = {
                                    vertices[indices[2]].TextureCoordinate,
                                    vertices[indices[1]].TextureCoordinate,
                                    vertices[indices[0]].TextureCoordinate,
                                };
                            Vector3[] pos = {
                                    vertices[indices[2]].Position,
                                    vertices[indices[1]].Position,
                                    vertices[indices[0]].Position,
                                };

                            Vector3 triNormal = Vector3.Normalize(Vector3.Cross(pos[1] - pos[0], pos[2] - pos[0]));
                            float bestDot = float.MinValue;
                            int bestIndex = 0;
                            for (int i = 0; i < PlaneAxes.Length; ++i)
                            {
                                float dot = Vector3.Dot(triNormal, PlaneAxes[i]);
                                if (dot > bestDot)
                                {
                                    bestDot = dot;
                                    bestIndex = i;
                                }
                            }

                            ColorF[] col =
                            {
                                PlaneColors[bestIndex],
                                PlaneColors[bestIndex],
                                PlaneColors[bestIndex],
                            };

                            Baking.MeshRasterizer.RasterizeTriangle(ref rasterData, uvs, col, null);
                        }
                    }

                    for (int y = 0; y < Height; ++y)
                    {
                        for (int x = 0; x < Width; ++x)
                            cache.SetPixel(x, y, rasterData.Pixels[y * Width + x].ToMonoGame().ToDrawingColor());
                    }
                }
            }
            PostPrime();
        }
    }

    [Description("Outputs the vertex colors in the model")]
    public partial class VertexColorBaker : BakerNode
    {
        bool triangleAverage_ = false;
        [Description("If enabled the color of each triangle will be the average of the vertex colors, otherwise vertex colors will be emitted")]
        public bool TriangleAverage { get { return triangleAverage_; } set { triangleAverage_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Vertex Color";
            AddInput(new GraphSocket(this) { Name = "Model", TypeID = SocketTypeID.Model, IsInput = true, IsOutput = false });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Color, IsInput = false, IsOutput = true });
        }
    }

    [Description("Outputs the polygonal faces of the mesh")]
    public partial class FacetBaker : BakerNode
    {
        float angularTolerance_ = 0.5f;
        bool forceAllEdges_ = false;

        [Description("Edges whose faces angles fall below this dot-product result will be ignored")]
        public float AngularTolerance { get { return angularTolerance_; } set { angularTolerance_ = value; OnPropertyChanged(); } }
        [Description("All edges will be drawn without concern for angular properties")]
        public bool DrawAllEdges { get { return forceAllEdges_; } set { forceAllEdges_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Facets";
            AddInput(new GraphSocket(this) { Name = "Model", TypeID = SocketTypeID.Model, IsInput = true, IsOutput = false });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            if (!PrePrime())
                return;

            if (dirty_)
            {
                var cache = CreateCache();

                var meshes = InputSockets[0].Data as List<MeshData>;
                if (meshes != null)
                {
                    List<SprueBindings.MeshData> outMeshData = new List<SprueBindings.MeshData>();
                    if (meshes.Count > 0)
                    {
                        foreach (var mesh in meshes)
                            outMeshData.Add(BindingUtil.ToMeshData(mesh));
                        var imgData = SprueBindings.Bakers.Facet(outMeshData.ToArray(), Math.Max(1, TextureSize.X), Math.Max(1, TextureSize.Y), AngularTolerance, DrawAllEdges, null);
                        imgData.WriteInto(cache);
                    }
                }
            }
            PostPrime();
        }
    }

    [Description("Normalized model space gradient of the mesh")]
    public partial class ModelSpaceGradientBaker : BakerNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Modelspace Gradient";
            AddInput(new GraphSocket(this) { Name = "Model", TypeID = SocketTypeID.Model, IsInput = true, IsOutput = false });
            AddOutput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            if (!PrePrime())
                return;

            if (dirty_)
            {
                var cache = CreateCache();

                var meshes = InputSockets[0].Data as List<MeshData>;
                if (meshes != null)
                {
                    List<SprueBindings.MeshData> outMeshData = new List<SprueBindings.MeshData>();
                    if (meshes.Count > 0)
                    {
                        foreach (var mesh in meshes)
                            outMeshData.Add(BindingUtil.ToMeshData(mesh));
                        var imgData = SprueBindings.Bakers.ObjectSpaceGradient(outMeshData.ToArray(), Math.Max(1, TextureSize.X), Math.Max(1, TextureSize.Y), null);
                        imgData.WriteInto(cache);
                    }
                }
            }
            PostPrime();
        }
    }

    public abstract class VolumetricNoiseBaker : BakerNode
    {
        protected FastNoise noise_ = new FastNoise();
        protected bool inverted_ = false;
        protected Vector3 period_ = new Vector3(80, 80, 80);

        [PropertyData.AllowPermutations]
        [Description("Controls the seed value for the RNG")]
        [PropertyData.ValidStep(Value = 5.0f)]
        public int Seed { get { return noise_.GetSeed(); } set { noise_.SetSeed(value); OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Output will be inverted")]
        public bool Inverted { get { return inverted_; } set { inverted_ = value; OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("Method of interpolating the noise values")]
        public FastNoise.Interp Interpolation { get { return noise_.m_interp; } set { noise_.SetInterp(value); OnPropertyChanged(); } }
        [PropertyData.AllowPermutations]
        [Description("How densely packed the noise is")]
        [PropertyData.ValidStep(Value = 1.0f)]
        public float Frequency { get { return noise_.m_frequency; } set { noise_.SetFrequency(value); OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            AddInput(new GraphSocket(this) { Name = "Model", TypeID = SocketTypeID.Model, IsInput = true, IsOutput = false });
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = true });
        }

        public override void Execute(object param)
        {
            base.Execute(param);
            if (Inverted)
            {
                var val = OutputSockets[0].GetColor().ToVector4();
                OutputSockets[0].Data = new Vector4(1 - val.X, 1 - val.Y, 1 - val.Z, val.W);
            }
        }

        public override void PrimeBeforeExecute(object param)
        {
            base.PrimeBeforeExecute(param);
            if (!PrePrime())
                return;

            if (dirty_)
            {
                var cache = CreateCache();
                var meshes = InputSockets[0].Data as List<MeshData>;
                if (meshes != null)
                {
                    int Width = Math.Max(1, TextureSize.X);
                    int Height = Math.Max(1, TextureSize.Y);

                    Baking.RasterizerData rasterData = new Baking.RasterizerData();
                    rasterData.Init(Width, Height, true);

                    foreach (var meshData in meshes)
                    {
                        var meshIndices = meshData.GetIndices();
                        var vertices = meshData.GetVertices();
                        for (int vertIndex = 0; vertIndex < meshIndices.Count; vertIndex += 3)
                        {
                            int[] indices = {
                                    meshIndices[vertIndex],
                                    meshIndices[vertIndex + 1],
                                    meshIndices[vertIndex + 2],
                                };
                            Vector2[] uvs = {
                                    vertices[indices[2]].TextureCoordinate,
                                    vertices[indices[1]].TextureCoordinate,
                                    vertices[indices[0]].TextureCoordinate,
                                };
                            Vector3[] pos = {
                                    vertices[indices[0]].Position,
                                    vertices[indices[1]].Position,
                                    vertices[indices[2]].Position,
                                };
                            Vector3[] nor = {
                                    vertices[indices[0]].Normal,
                                    vertices[indices[1]].Normal,
                                    vertices[indices[2]].Normal,
                                };

                            Baking.MeshRasterizer.RasterizeModelSpaceTriangle(ref rasterData, pos, nor, uvs, Sample);
                        }
                    }

                    Baking.RasterizerData.PadEdges(ref rasterData, 3);

                    for (int y = 0; y < Height; ++y)
                    {
                        for (int x = 0; x < Width; ++x)
                            cache.SetPixel(x, y, rasterData.Pixels[y * Width + x].ToMonoGame().ToDrawingColor());
                    }
                }
            }
            PostPrime();
        }

        protected abstract ColorF? Sample(ref Vector3 pos, ref Vector3 nor);
    }

    [Description("Generates an octave sum noise over the volume of the model")]
    public partial class VolumetricFBM : VolumetricNoiseBaker
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

        public VolumetricFBM()
        {
            noise_.SetNoiseType(FastNoise.NoiseType.ValueFractal);
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Volumetric FBM";
            Frequency = 4.0f;
        }

        protected override ColorF? Sample(ref Vector3 pos, ref Vector3 nor)
        {
            float fVal = noise_.GetValueFractal(pos.X, pos.Y, pos.Z);
            fVal = Mathf.Normalize(fVal, -1, 1);
            return new ColorF(fVal, fVal, fVal, 1.0f);
        }
    }

    [Description("Generates a cellular texture over the volume of the model")]
    [PropertyData.PropertyIgnore("Interpolation")]
    public partial class VolumetricVoronoi : VolumetricNoiseBaker
    {
        bool knockOutMode_ = false;
        [Description("How the distance between points is calculated")]
        public FastNoise.CellularDistanceFunction Function { get { return noise_.m_cellularDistanceFunction; } set { noise_.SetCellularDistanceFunction(value); OnPropertyChanged(); } }
        [Description("The output style of the cellular function")]
        public FastNoise.CellularReturnType CellType { get { return noise_.m_cellularReturnType; } set { noise_.SetCellularReturnType(value); OnPropertyChanged(); } }
        [Description("Cells will be randomly dropped from the output to increase the sparsity")]
        public bool KnockOutMode { get { return knockOutMode_; } set { knockOutMode_ = value; OnPropertyChanged(); } }

        public VolumetricVoronoi()
        {
            noise_.SetNoiseType(FastNoise.NoiseType.Cellular);
            noise_.SetCellularNoiseLookup(new FastNoise(7865));
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Volumetric Voronoi";
            Frequency = 4.0f;
            noise_.SetNoiseType(FastNoise.NoiseType.Cellular);
            noise_.SetCellularNoiseLookup(new FastNoise(7865));
        }

        protected override ColorF? Sample(ref Vector3 pos, ref Vector3 nor)
        {
            float fVal = noise_.GetNoise(pos.X, pos.Y, pos.Z);
            if (KnockOutMode)
                fVal = fVal > 0.5f ? fVal : 0.0f;
            return new ColorF(fVal, fVal, fVal, 1.0f);
        }
    }

    [Description("Generates Perlin noise over the volume of the model")]
    public partial class VolumetricPerlin : VolumetricNoiseBaker
    {
        public VolumetricPerlin()
        {
            noise_.SetNoiseType(FastNoise.NoiseType.Perlin);
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Volumetric Perlin";
            Frequency = 4.0f;
        }

        protected override ColorF? Sample(ref Vector3 pos, ref Vector3 nor)
        {
            float fVal = noise_.GetNoise(pos.X, pos.Y, pos.Z);
            fVal = Mathf.Normalize(fVal, -1, 1);
            return new ColorF(fVal, fVal, fVal, 1.0f);
        }
    }
}
