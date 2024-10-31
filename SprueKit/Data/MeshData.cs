using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.Xml;
using SprueKit.Util;

namespace SprueKit.Data
{
    public class MeshBone
    {

    }

    public class MeshData : IDisposable
    {
        public BoundingBox Bounds { get; private set; }
        public Effect Effect { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public VertexBuffer VertexBuffer { get; set; }

        List<int> indices;
        List<PluginLib.VertexData> vertices;

        public List<int> GetIndices() { return indices; }
        public List<PluginLib.VertexData> GetVertices() { return vertices; }

        public PluginLib.SkeletonData Skeleton { get; set; }
        Material texture_;
        public Material Texture { get { return texture_; } set { if (texture_ != null) texture_.Dispose(); texture_ = value; } }

        public MeshData(List<int> indices, List<PluginLib.VertexData> vertices)
        {
            this.indices = indices;
            this.vertices = vertices;

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var vert in vertices)
            {
                min.X = Math.Min(min.X, vert.Position.X);
                min.Y = Math.Min(min.Y, vert.Position.Y);
                min.Z = Math.Min(min.Z, vert.Position.Z);

                max.X = Math.Max(max.X, vert.Position.X);
                max.Y = Math.Max(max.Y, vert.Position.Y);
                max.Z = Math.Max(max.Z, vert.Position.Z);
            }
            Bounds = new BoundingBox { Min = min, Max = max };
        }

        ~MeshData()
        {
            Dispose();
        }

        public void SetData(List<int> indices, List<PluginLib.VertexData> vertices)
        {
            lock (this)
            {
                this.indices = indices;
                this.vertices = vertices;
                if (VertexBuffer != null)
                    VertexBuffer.Dispose();
                if (IndexBuffer != null)
                    IndexBuffer.Dispose();
            }
        }

        bool disposed_ = false;
        public void Dispose()
        {
            lock (this)
            {
                if (IndexBuffer != null)
                    IndexBuffer.Dispose();
                IndexBuffer = null;
                if (VertexBuffer != null)
                    VertexBuffer.Dispose();
                VertexBuffer = null;
                disposed_ = true;
            }
        }

        public int TriangleCount { get { return indices != null ? indices.Count / 3 : 0; } }
        public int IndexCount { get { return indices != null ? indices.Count : 0; } }
        public int VertexCount { get { return vertices != null ? vertices.Count : 0; } }

        public void Initialize(GraphicsDevice device)
        {
            if (device == null)
                return;
            IndexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            VertexBuffer = new VertexBuffer(device, typeof(PluginLib.VertexData), vertices.Count, BufferUsage.WriteOnly);

            IndexBuffer.SetData<int>(indices.ToArray());
            VertexBuffer.SetData(vertices.ToArray());
        }

        /// <summary>
        /// Render to the given graphics device
        /// </summary>
        /// <param name="device"></param>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        /// <param name="leaveTextures"></param>
        public void Draw(GraphicsDevice device, Matrix view, Matrix projection, bool leaveTextures = false)
        {
            if (Effect == null)
                return;

            lock (this)
            {
                if (indices.Count > 0 && vertices.Count > 0)
                {
                    if (VertexBuffer == null || IndexBuffer == null || VertexBuffer.IsDisposed || IndexBuffer.IsDisposed)
                        Initialize(device);
                    if (Texture != null && Texture.AnyNeedInitialization)
                        Texture.Initialize(device);

                    if (VertexBuffer == null || IndexBuffer == null)
                        return;

                    if (Effect is Graphics.Materials.TexturedEffect)
                    {
                        Graphics.Materials.TexturedEffect fx = Effect as Graphics.Materials.TexturedEffect;
                        if (Texture != null && fx.ViewChannel == Graphics.Materials.RenderTextureChannel.DiffuseOnly)
                            ((Graphics.Materials.TexturedEffect)Effect).DiffuseTexture = Texture.DiffuseTexture;
                        else if (Texture != null && fx.ViewChannel == Graphics.Materials.RenderTextureChannel.NormalMapOnly)
                            ((Graphics.Materials.TexturedEffect)Effect).DiffuseTexture = Texture.NormalMapTexture;
                        else if (Texture != null && fx.ViewChannel == Graphics.Materials.RenderTextureChannel.RoughnessOnly)
                            ((Graphics.Materials.TexturedEffect)Effect).DiffuseTexture = Texture.RoughnessTexture;
                        else if (Texture != null && fx.ViewChannel == Graphics.Materials.RenderTextureChannel.MetalnessOnly)
                            ((Graphics.Materials.TexturedEffect)Effect).DiffuseTexture = Texture.MetallicTexture;
                        else if (Texture != null && fx.ViewChannel == Graphics.Materials.RenderTextureChannel.HeightOnly)
                            ((Graphics.Materials.TexturedEffect)Effect).DiffuseTexture = Texture.DisplacementTexture;
                        else
                            ((Graphics.Materials.TexturedEffect)Effect).DiffuseTexture = null;
                    }
                    else if (Effect is Graphics.Materials.PBREffect && !leaveTextures)
                    {
                        Graphics.Materials.PBREffect fx = Effect as Graphics.Materials.PBREffect;
                        if (Texture != null)
                        {
                            fx.DiffuseTexture = Texture.DiffuseTexture;
                            fx.NormalMapTexture = Texture.NormalMapTexture;
                            fx.RoughnessTexture = Texture.RoughnessTexture;
                            fx.MetalnessTexture = Texture.MetallicTexture;
                            fx.HeightTexture = Texture.DisplacementTexture;
                        }
                        else
                        {
                            fx.DiffuseTexture = fx.NormalMapTexture = fx.RoughnessTexture = fx.MetalnessTexture = fx.HeightTexture = null;
                        }
                    }

                    device.BlendState = BlendState.NonPremultiplied;
                    device.SetVertexBuffer(VertexBuffer);
                    device.Indices = IndexBuffer;
                    foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount / 3);
                    }

                    device.SetVertexBuffer(null);
                    device.Indices = null;

                    //device.RasterizerState = originalState;
                }
            }

            if (disposed_)
                Dispose();
        }

        public void Draw(GraphicsDevice device, Effect effect)
        {
            device.BlendState = BlendState.NonPremultiplied;
            device.SetVertexBuffer(VertexBuffer);
            device.Indices = IndexBuffer;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                if (IndexBuffer == null || IndexBuffer.GraphicsDevice == null || device == null)
                    break;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount / 3);
            }

            device.SetVertexBuffer(null);
            device.Indices = null;
        }

        public MeshData Clone() {
            List<PluginLib.VertexData> cloneVerts = new List<PluginLib.VertexData>(vertices.Count);
            for (int i = 0; i < vertices.Count; ++i)
                cloneVerts.Add(new PluginLib.VertexData(vertices[i].Position, vertices[i].Normal, vertices[i].TextureCoordinate));
            return new MeshData(indices, cloneVerts);
        }

        public static void CreateFromHeightField(System.Drawing.Bitmap bmp, string fileName, float heightScale, bool decimate, float power)
        {
            if (bmp == null)
                return;

            int dimX = bmp.Width;
            int dimY = bmp.Height;
            List<int> indices = GenerateTerrainIndices(dimX, dimY);
            List<PluginLib.VertexData> vertices = GenerateTerrainVertices(bmp, new Vector2(-1.0f, -1.0f), dimX, dimY, 1.0f, heightScale);
            
            MeshData ret = new Data.MeshData(indices, vertices);

            var bindingMdl = BindingUtil.ToModel(ret);
            if (decimate)
            {
                bindingMdl.Meshes[0].Decimate(power);
                bindingMdl.Meshes[0].ComputeNormalUVs();
            }
            //bindingMdl.Meshes[0].ReadFromAPI();
            SprueBindings.ModelData.SaveOBJ(bindingMdl, fileName, ErrorHandler.inst());
            ret.Dispose();
        }

        public static void CreateMarchingSquares(System.Drawing.Bitmap bmp, bool useEdgeIntercept, string fileName, byte heightCutoff, bool decimate, float power)
        {
            MarchingSquares.VoxelGrid grid = new MarchingSquares.VoxelGrid();
            grid.Initialize(Math.Max(bmp.Width, bmp.Height), (float)Math.Max(bmp.Width, bmp.Height), 135.0f);
            grid.Apply(bmp, heightCutoff, useEdgeIntercept);

            var ret = grid.ToMesh();
            var bindingMdl = BindingUtil.ToModel(ret);
            if (decimate)
                bindingMdl.Meshes[0].Decimate(power);
            bindingMdl.Meshes[0].ComputeNormalUVs();
            //bindingMdl.Meshes[0].ReadFromAPI();
            SprueBindings.ModelData.SaveOBJ(bindingMdl, fileName, ErrorHandler.inst());
            ret.Dispose();
        }

        static List<PluginLib.VertexData> GenerateTerrainVertices(System.Drawing.Bitmap heightmap, Vector2 startPosition, int vertexCountX, int vertexCountZ, float blockScale = 3.0f, float heightScale = 0.5f)
        {
            float halfTerrainWidth = (vertexCountX - 1) * blockScale * .5f;
            float halfTerrainDepth = (vertexCountZ - 1) * blockScale * .5f;
            float tuDerivative = 1.0f / (vertexCountX - 1);
            float tvDerivative = 1.0f / (vertexCountZ - 1);

            PluginLib.VertexData[] vertices = new PluginLib.VertexData[vertexCountX * vertexCountZ];
            int vertexCount = 0;
            float tu = 0;
            float tv = 0;
            for (float i = -halfTerrainDepth; i <= halfTerrainDepth; i += blockScale)
            {
                tu = 0.0f;
                for (float j = -halfTerrainWidth; j <= halfTerrainWidth; j += blockScale)
                {
                    var heightValue = heightmap.GetPixelBilinear(tu, tv).R;
                    vertices[vertexCount].Position = new Vector3(j, heightValue * heightScale * 0.1f, i);
                    vertices[vertexCount].TextureCoordinate = new Vector2(tu, tv);
                    vertices[vertexCount].Normal = Vector3.UnitY;
                    tu += tuDerivative;
                    vertexCount++;
                }
                tv += tvDerivative;
            }

            return vertices.ToList();
        }

        // remember, dim + 1
        static List<int> GenerateTerrainIndices(int vertexCountX, int vertexCountZ, float blockScale = 3.0f, float heightScale = 0.5f)
        {
            int numTriangles = numTriangles = (vertexCountX - 1) * (vertexCountZ - 1) * 2;
            int numIndices = numTriangles * 3;
            int[] indices = new int[numIndices];
            int indicesCount = 0;
            for (int i = 0; i < (vertexCountZ - 1); i++) //All Rows except last
                for (int j = 0; j < (vertexCountX - 1); j++) //All Columns except last
                {
                    int index = j + i * vertexCountZ; //2D coordinates to linear
                                                      //First Triangle Vertices
                    indices[indicesCount++] = index;
                    indices[indicesCount++] = index + 1;
                    indices[indicesCount++] = index + vertexCountX + 1;

                    //Second Triangle Vertices
                    indices[indicesCount++] = index + vertexCountX + 1;
                    indices[indicesCount++] = index + vertexCountX;
                    indices[indicesCount++] = index;
                }
            return indices.ToList();
        }

        #region Debugging Utilities
        public void DrawTangentFrames(Graphics.DebugDraw debugDraw, Vector3 offset)
        {
            lock (this)
            {
                if (vertices != null)
                {
                    foreach (var v in vertices)
                    {
                        debugDraw.DrawLine(v.Position + offset, v.Position + offset + v.Normal * 0.07f, Color.CornflowerBlue);
                        debugDraw.DrawLine(v.Position + offset, v.Position + offset + v.Tangent.XYZ() * 0.07f, Color.Magenta);
                        Vector3 cross = Vector3.Cross(v.Normal, v.Tangent.XYZ()) * v.Tangent.W;
                        debugDraw.DrawLine(v.Position + offset, v.Position + cross * 0.07f + offset, Color.Yellow);
                    }
                }
            }
        }
        #endregion
    }

    public class ForeignModelMorphTarget : BaseClass
    {
        public string Name { get; set; }
        float appliedValue_ = 0.0f;

        public float AppliedValue { get { return appliedValue_; } set { appliedValue_ = value; OnPropertyChanged(); } }

        internal ForeignModelMorphTarget Clone()
        {
            return new ForeignModelMorphTarget { Name = this.Name, AppliedValue = this.AppliedValue };
        }
    }

    public class ForeignModelSubMesh : BaseClass
    {
        public string Name { get; set; }
        bool included_ = true;
        public bool Included { get { return included_; }set { included_ = value; OnPropertyChanged(); } }

        internal ForeignModelSubMesh Clone()
        {
            return new ForeignModelSubMesh { Name = this.Name, Included = this.Included };
        }
    }

    public class ForeignModel : BaseClass
    {
        class ModelDataCacheRecord
        {
            public SprueBindings.ModelData Model { get; set; }
            public List<MeshData> Meshes { get; private set; } = new List<MeshData>();

            public void UpdateModel()
            {
                foreach (var mesh in Meshes)
                    mesh.Dispose();
                Meshes.Clear();

                if (Model != null)
                {
                    foreach (var mesh in Model.Meshes)
                    {
                        mesh.ReadFromAPI();
                        Meshes.Add(BindingUtil.ToMesh(mesh));
                    }
                }
            }
        }
        static Dictionary<Uri, ModelDataCacheRecord> Cache = new Dictionary<Uri, ModelDataCacheRecord>();
        HashCache<MeshData> morphCache_ = new HashCache<MeshData>();

        ~ForeignModel()
        {
            morphCache_.Dispose();
        }

        public static SprueBindings.ModelData GetOrLoad(Uri fileUri)
        {
            ModelDataCacheRecord ret = null;
            if (Cache.TryGetValue(fileUri, out ret))
                return ret.Model;

            ret = new ModelDataCacheRecord { Model = SprueBindings.ModelData.LoadModel(fileUri.AbsolutePath, ErrorHandler.inst()) };
            if (ret != null && ret.Model != null)
            {
                ret.UpdateModel();
                Cache[fileUri] = ret;
            }
            return ret.Model;
        }

        public int TriangleCount
        {
            get
            {
                if (modelData_ == null || modelData_.Meshes == null) return 0;
                int sum = 0;
                for (int i = 0; i < modelData_.Meshes.Count; ++i)
                {
                    if (SubMeshes[i].Included)
                    {
                        if (modelData_.Meshes[i].Indices == null)
                            modelData_.Meshes[i].ReadFromAPI();
                        sum += modelData_.Meshes[i].Indices.Length / 3;
                    }
                }
                return sum;
            }
        }

        public int VertexCount
        {
            get
            {
                if (modelData_ == null || modelData_.Meshes == null) return 0;
                int sum = 0;
                for (int i = 0; i < modelData_.Meshes.Count; ++i)
                {
                    if (SubMeshes[i].Included)
                        sum += modelData_.Meshes[i].Positions.Length;
                }
                return sum;
            }
        }

        SprueBindings.ModelData modelData_;
        Uri modelFile_;

        [PropertyData.PropertyIgnore]
        public SprueBindings.ModelData ModelData { get { return modelData_; } set { modelData_ = value; OnPropertyChanged(); } }

        [PropertyData.PropertyIgnore]
        public bool AnyMeshesIncluded {
            get {
                bool yes = false;
                for (int i = 0; i < SubMeshes.Count; ++i)
                    yes |= SubMeshes[i].Included;
                return yes;
            }
        }

        public ObservableCollection<ForeignModelMorphTarget> MorphTargets { get; private set; } = new ObservableCollection<ForeignModelMorphTarget>();

        public ObservableCollection<ForeignModelSubMesh> SubMeshes { get; private set; } = new ObservableCollection<ForeignModelSubMesh>();

        [PropertyData.AllowPermutations]
        public Uri ModelFile { get { return modelFile_; } set
            {
                var oldModelFile = modelFile_;
                if (value != oldModelFile)
                {
                    ReleaseModel();
                    modelFile_ = value;
                    UpdateModel();
                    OnPropertyChanged();
                }
            }
        }

        void ReleaseModel()
        {
            if (modelData_ != null)
            {
                modelData_ = null;
            }
        }

        void UpdateModel()
        {
            lock (this)
            {
                if (modelFile_ != null)
                    modelData_ = GetOrLoad(modelFile_);

                SubMeshes.Clear();
                MorphTargets.Clear();
                if (modelData_ != null && modelData_.Meshes != null)
                {
                    foreach (var mesh in modelData_.Meshes)
                    {
                        SubMeshes.Add(new ForeignModelSubMesh { Name = mesh.Name, Included = true });
                        if (mesh.MorphTargets != null)
                        {
                            foreach (var morph in mesh.MorphTargets)
                                MorphTargets.Add(new ForeignModelMorphTarget { Name = morph.Name, AppliedValue = 0.0f });
                        }
                    }
                }

            }
        }

        internal ForeignModel Clone()
        {
            lock (this)
            {
                ForeignModel ret = new ForeignModel();
                ret.modelFile_ = this.modelFile_;
                ret.modelData_ = this.modelData_;

                foreach (var morph in this.MorphTargets)
                    ret.MorphTargets.Add(morph.Clone());

                foreach (var sub in this.SubMeshes)
                    ret.SubMeshes.Add(sub.Clone());
                return ret;
            }
        }

        public List<MeshData> GetMeshes()
        {
            lock (this)
            {
                List<MeshData> ret = new List<MeshData>();
                if (ModelData == null)
                    return ret;

                ModelDataCacheRecord cacheRecord = null;
                if (Cache.TryGetValue(ModelFile, out cacheRecord))
                {
                    for (int i = 0; i < ModelData.Meshes.Count; ++i)
                    {
                        if (SubMeshes[i].Included)
                            ret.Add(cacheRecord.Meshes[i]);
                    }

                    for (int mdl = 0; mdl < ModelData.Meshes.Count; ++mdl)
                    {
                        if (!SubMeshes[mdl].Included)
                            continue;

                        List<float> morphHashing = new List<float>();
                        for (int i = 0; i < MorphTargets.Count; ++i)
                        {
                            if (MorphTargets[i].AppliedValue != 0.0f)
                                morphHashing.Add(MorphTargets[i].AppliedValue);
                        }
                        if (morphHashing.Count > 0)
                        {
                            var found = morphCache_.Get(mdl, morphHashing.Hash());
                            if (found != null)
                                ret[mdl] = found;
                        }
                    }

                    int slotIdx = 0;
                    for (int mdl = 0; mdl < ModelData.Meshes.Count; ++mdl)
                    {
                        MeshData outMdl = null;
                        if (!SubMeshes[mdl].Included)
                            continue;

                        // we grabbed it from the morph target cache
                        if (ret[mdl] != cacheRecord.Meshes[mdl])
                        {
                            ++slotIdx;
                            continue;
                        }

                        // have to process morph targets
                        List<float> morphHashing = new List<float>();
                        bool anyMorphs = false;
                        for (int i = 0; i < MorphTargets.Count; ++i)
                        {
                            if (MorphTargets[i].AppliedValue != 0.0f)
                            {
                                if (outMdl == null)
                                    outMdl = ret[slotIdx].Clone();
                                anyMorphs = true;
                                ApplyMorph(outMdl, ModelData.Meshes[mdl], i, MorphTargets[i].AppliedValue);
                                ret[slotIdx] = outMdl;
                                morphHashing.Add(MorphTargets[i].AppliedValue);
                            }
                        }

                        if (anyMorphs)
                            morphCache_.Store(mdl, morphHashing.Hash(), ret[slotIdx]);
                        ++slotIdx;
                    }
                }
                return ret;
            }
        }

        public void Write(SerializationContext context, XmlElement intoParent)
        {
            intoParent.AddStringElement("uri", context.GetRelativePathString(ModelFile));
            var subMeshelem = intoParent.CreateChild("submeshes");
            foreach (var sm in SubMeshes)
            {
                var submesh = subMeshelem.CreateChild("submesh");
                submesh.AddStringElement("name", sm.Name);
                submesh.AddStringElement("included", sm.Included.ToString());
            }

            var morphElems = intoParent.CreateChild("morphtargets");
            foreach (var morph in MorphTargets)
            {
                var morpht = morphElems.CreateChild("target");
                morpht.AddStringElement("name", morph.Name);
                morpht.AddStringElement("weight", morph.AppliedValue.ToString());
            }
        }

        public void Read(SerializationContext context, XmlElement fromElement)
        {
            string uriText = fromElement.GetStringElement("uri");
            ModelFile = context.GetAbsolutePath(new Uri(uriText, UriKind.RelativeOrAbsolute), this, "ModelFile", "3D Model", FileData.ModelFileMask);

            if (ModelData != null)
            {
                var subMeshesElem = fromElement.SelectSingleNode("submeshes") as XmlElement;
                var submeshes = subMeshesElem.SelectNodes("submesh");
                foreach (var elem in submeshes)
                {
                    var xmlElem = elem as XmlElement;
                    string name = xmlElem.GetStringElement("name");
                    var submeshTarget = SubMeshes.FirstOrDefault((a) => a.Name.Equals(name));
                    if (submeshTarget != null)
                        submeshTarget.Included = xmlElem.GetBoolElement("included", true);
                }

                var morphsElem = fromElement.SelectSingleNode("morphtargets") as XmlElement;
                var morphs = morphsElem.SelectNodes("target");
                foreach (var elem in morphs)
                {
                    var xmlElem = elem as XmlElement;
                    string name = xmlElem.GetStringElement("name");
                    var morphTarget = MorphTargets.FirstOrDefault((a) => a.Name.Equals(name));
                    if (morphTarget != null)
                        morphTarget.AppliedValue = xmlElem.GetFloatElement("weight", 0);
                }
            }
        }

        public void Write(SerializationContext context, System.IO.BinaryWriter strm)
        {
            if (ModelFile != null)
            {
                strm.Write(true);
                strm.Write(context.GetRelativePathString(ModelFile));
                strm.Write(SubMeshes.Count);
                for (int i = 0; i < SubMeshes.Count; ++i)
                {
                    strm.Write(SubMeshes[i].Name);
                    strm.Write(SubMeshes[i].Included);
                }
                strm.Write(MorphTargets.Count);
                for (int i = 0; i < MorphTargets.Count; ++i)
                {
                    strm.Write(MorphTargets[i].Name);
                    strm.Write(MorphTargets[i].AppliedValue);
                }
            }
            else strm.Write(false);
        }

        public void Read(SerializationContext context, System.IO.BinaryReader strm)
        {
            if (strm.ReadBoolean())
            {
                string file = strm.ReadString();
                ModelFile = context.GetAbsolutePath(new Uri(file, UriKind.Relative), this, "Model file", "Model", FileData.ModelFileMask);
                int shapeCt = strm.ReadInt32();
                for (int i = 0; i < shapeCt; ++i)
                {
                    string targetName = strm.ReadString();
                    bool isIncluded = strm.ReadBoolean();
                    if (SubMeshes != null)
                    {
                        var target = SubMeshes.FirstOrDefault(s => s.Name.Equals(targetName));
                        if (target != null)
                            target.Included = isIncluded;
                    }
                }
                int morphCt = strm.ReadInt32();
                for (int i = 0; i < morphCt; ++i)
                {
                    string morphName = strm.ReadString();
                    float weight = strm.ReadSingle();
                    if (MorphTargets != null)
                    {
                        var target = MorphTargets.FirstOrDefault(m => m.Name.Equals(morphName));
                        if (target != null)
                            target.AppliedValue = weight;
                    }
                }
            }
        }

        void ApplyMorph(MeshData onto, SprueBindings.MeshData src, int target, float weight)
        {
            if (src.MorphTargets != null && target < src.MorphTargets.Length)
            {
                var tgt = src.MorphTargets[target];
                for (int i = 0; i < tgt.Positions.Length; ++i)
                {
                    if (src.Positions[i].Equals(tgt.Positions))
                        continue;

                    var verts = onto.GetVertices();
                    verts[i] = new PluginLib.VertexData(
                        verts[i].Position + (tgt.Positions[i] - src.Positions[i]) * weight,
                        verts[i].Normal, 
                        verts[i].TextureCoordinate);
                }
            }
        }
    }
}
