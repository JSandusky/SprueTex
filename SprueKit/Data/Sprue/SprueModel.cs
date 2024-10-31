using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using MonoGame;
using Microsoft.Xna.Framework;
using SprueKit.Graphics;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using SprueKit.Graphics.Materials;
using System.ComponentModel;

namespace SprueKit.Data
{
    public enum SymmetricAxis
    {
        None,
        XAxis,
        YAxis,
        ZAxis
    }

    public enum DebugDrawMode
    {
        Passive,
        Selected,
        Hover
    }

    public enum CSGOperation
    {
        Add,
        Subtract,
        Intersect,
        Blend
    }

    public interface IHaveDensity
    {
        CSGOperation CSGType();
        void PrepareDensity();
        float GetDensity(Vector3 position);
    }

    public static class DensityRayTracer
    {
        public static bool TraceRay(this IHaveDensity densitySource, Ray ray, out float hitTime)
        {
            hitTime = 100000.0f;
            Ray testRay = ray;// new Ray(Vector3.Transform(ray.Position, InverseTransform), Vector3.TransformNormal(ray.Direction, InverseTransform));
            float t = 0.0f;

            const int MAX_RAY_STEPS = 32;
            for (int i = 0; i < MAX_RAY_STEPS; ++i)
            {
                Vector3 testPoint = testRay.Position + testRay.Direction * t;
                if (!testPoint.X.IsFinite() || !testPoint.Y.IsFinite() || !testPoint.Z.IsFinite())
                    continue;

                float d = densitySource.GetDensity(testPoint);
                if (d < 0.001f)
                {
                    hitTime = (testPoint - testRay.Position).Length();
                    return true;
                }
                t += d;
            }
            return false;
        }
    }

    public interface IMousePickable
    {
        bool DoMousePick(Ray ray, out float hitDist);
    }

    public class Transform : BaseClass
    {
        Transform parent_ = null;
        Vector3 position_ = Vector3.Zero;
        Quaternion rotation_ = Quaternion.Identity;
        Vector3 scale_ = Vector3.One;

        public Vector3 Position { get { return position_; } set { position_ = value; OnPropertyChanged(); } }
        public Quaternion Rotation { get { return rotation_; } set { rotation_ = value; OnPropertyChanged(); } }
        public Vector3 EulerRotation { get { return rotation_.ToEuler(); } set { Rotation = value.QuaternionFromEuler(); OnPropertyChanged(); } }
        public Vector3 Scale { get { return scale_; } set { scale_ = value; OnPropertyChanged(); } }

        public Vector3 WorldPosition { get { return Vector3.Transform(Position, WorldTransform); } }
        public Quaternion WorldRotation { get { return rotation_ * ParentRotation; } }
        public Vector3 WorldScale { get { return scale_ * ParentScale; } }

        public Matrix LocalTransform {
            get { return Matrix.CreateScale(scale_) * Matrix.CreateFromQuaternion(rotation_) * Matrix.CreateTranslation(position_); }
            set
            {
                value.Decompose(out scale_, out rotation_, out scale_);
                // round scale
                scale_.X = (float)Math.Round(scale_.X, 4, MidpointRounding.ToEven);
                scale_.Y = (float)Math.Round(scale_.Y, 4, MidpointRounding.ToEven);
                scale_.Z = (float)Math.Round(scale_.Z, 4, MidpointRounding.ToEven);
                OnPropertyChanged("LocalTransform");

                using (var v = new Notify.Tracker.TrackingSideEffects())
                {
                    OnPropertyChanged("WorldTransform");
                    OnPropertyChanged("Position");
                    OnPropertyChanged("Rotation");
                    OnPropertyChanged("EulerRotation");
                    OnPropertyChanged("Scale");
                }
            }
        }
        public Matrix ParentTransform { get { return parent_ != null ? parent_.WorldTransform : Matrix.Identity; } }
        public Quaternion ParentRotation { get { return parent_ != null ? parent_.WorldRotation : Quaternion.Identity; } }
        public Vector3 ParentScale { get { return parent_ != null ? parent_.WorldScale : Vector3.One; } }
        public Matrix WorldTransform {
            get { return LocalTransform * ParentTransform; }
            set { LocalTransform = Matrix.Invert(ParentTransform) * value; }
        }
    }

    public partial class SpruePiece : BaseClass, IPermutable
    {
        public event EventHandler PermutationsChanged;
        public void SignalPermutationChange() { if (PermutationsChanged != null) PermutationsChanged(this, null); }

        SpruePiece parent_ = null;
        string name_ = "";
        Vector3 position_ = new Vector3(0, 0, 0);
        Quaternion rotation_ = new Quaternion();
        Vector3 scale_ = new Vector3(1, 1, 1);
        uint capabilities_ = 0;
        uint flags_ = 0;
        bool isEnabled_ = true;
        SymmetricAxis symmetric_ = SymmetricAxis.None;
        Dictionary<string, PermutationSet> permutations_ = new Dictionary<string, PermutationSet>();

        public SprueModel GetModel()
        {
            var cur = Parent;
            var last = cur;
            while (cur != null)
            {
                last = cur;
                cur = cur.Parent;
            }

            if (cur == null)
                return (SprueModel)last;
            return null;
        }

        public SpruePiece()
        {
            Children.CollectionChanged += (o, e) =>
            {
                OnPropertyChanged("FlatChildren");
                foreach (var child in Children)
                    child.Parent = this;
            };
            Components.CollectionChanged += (o, e) =>
            {
                OnPropertyChanged("FlatChildren");
                foreach (var child in Components)
                    child.Parent = this;
            };
        }

        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public Dictionary<string, PermutationSet> Permutations { get { return permutations_; } }

        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public IEnumerable<PermutationRecord> FlatPermutations { get { return PermutationRecord.GetRecords(permutations_); } }

        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public bool Expanded { get; set; } = true;

        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public bool Selected { get; set; } = false;

        [Notify.TrackMember(IsExcluded = true)]
        [PropertyData.PropertyIgnore]
        public new SpruePiece Parent
        {
            get { return parent_; }
            set
            { // When we set our parent we have to deal with a bunch of consequences
                if (parent_ != null && parent_ != value)
                {
                    var oldParent = parent_;
                    parent_ = value;
                    SprueComponent comp = this as SprueComponent;
                    if (comp != null)
                        oldParent.Components.Remove(comp);
                    else
                        oldParent.Children.Remove(this);
                }
                parent_ = value;
            }
        }

        #region GUI only properties

        [PropertyData.PropertyIgnore]
        public string DisplayName { get { return string.Format("{0} - {1}", string.IsNullOrWhiteSpace(Name) ? "<unnamed>" : Name, GetType().Name); } }

        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public virtual IEnumerable<object> FlatChildren
        {
            get
            {
                List<object> ret = new List<object>();
                ret.AddRange(Components);
                ret.AddRange(Children);
                return ret;
            }
        }

        #endregion

        [PropertyData.PropertyPriority(1)]
        [Category("General")]
        [Description("Identifier for reference and convenience")]
        public string Name { get { return name_; } set { name_ = value; OnPropertyChanged(); OnPropertyChanged("DisplayName"); } }

        [PropertyData.PropertyPriority(2)]
        [PropertyData.PropertyGroup("Transform")]
        [PropertyData.AllowPermutations]
        [Category("General")]
        [Description("Location in the scene")]
        public Vector3 Position {
            get { return Vector3.Transform(position_, ParentTransform); }
            set { position_ = Vector3.Transform(value, InverseParentTransform); OnPropertyChanged(); PostTransformChange(); }
        }

        [PropertyData.PropertyIgnore]
        [Category("General")]
        public Quaternion Rotation {
            get { return rotation_ * ParentTransform.Rotation; }
            set { rotation_ = value * InverseParentTransform.Rotation; OnPropertyChanged(); PostTransformChange(); }
        }

        [PropertyData.PropertyPriority(3)]
        [PropertyData.AllowPermutations]
        [PropertyData.PropertyGroup("Transform")]
        [PluginLib.PropertyLabel("Rotation")]
        [Category("General")]
        [DisplayName("Rotation")]
        [Description("XYZ Rotation in degrees")]
        public Vector3 EulerRotation {
            get { return Rotation.ToEuler(); }
            set { rotation_ = value.QuaternionFromEuler() * InverseParentTransform.Rotation; OnPropertyChanged(); PostTransformChange(); }
        }

        [PropertyData.PropertyPriority(4)]
        [PropertyData.AllowPermutations]
        [PropertyData.PropertyGroup("Transform")]
        [Category("General")]
        [Description("Scaling factor shrink or enlarge")]
        public Vector3 Scale {
            get { return scale_ * ParentTransform.Scale; }
            set { scale_ = value * InverseParentTransform.Scale; OnPropertyChanged(); PostTransformChange(); }
        }

        protected void PostTransformChange()
        {
            using (var v = new Notify.Tracker.TrackingSideEffects())
                OnPropertyChanged("Transform");
        }

        [PropertyData.PropertyIgnore]
        public Vector3 LocalPosition { get { return position_; } set { position_ = value;  OnPropertyChanged(); PostTransformChange(); } }
        [PropertyData.PropertyIgnore]
        public Quaternion LocalRotation { get { return rotation_; } set { rotation_ = value;  OnPropertyChanged(); PostTransformChange(); } }
        [PropertyData.PropertyIgnore]
        public Vector3 LocalScale { get { return scale_; } set { scale_ = value;  OnPropertyChanged(); PostTransformChange(); } }

        [PropertyData.PropertyIgnore]
        public Matrix Transform
        {
            get { return (Matrix.CreateScale(LocalScale) * Matrix.CreateFromQuaternion(LocalRotation) * Matrix.CreateTranslation(LocalPosition)) * ParentTransform; }
            set {
                var oldMat = Transform;
                var newMat = value;
                value = value * InverseParentTransform;
                value.Decompose(out scale_, out rotation_, out position_);
                // round scale
                scale_.X = (float)Math.Round(scale_.X, 4, MidpointRounding.ToEven);
                scale_.Y = (float)Math.Round(scale_.Y, 4, MidpointRounding.ToEven);
                scale_.Z = (float)Math.Round(scale_.Z, 4, MidpointRounding.ToEven);
                OnPropertyChanged("Transform");
                using (var v = new Notify.Tracker.TrackingSideEffects()) {
                    OnPropertyChanged("Position");
                    OnPropertyChanged("Rotation");
                    OnPropertyChanged("EulerRotation");
                    OnPropertyChanged("Scale");
                }
                MatrixTransformChanged(oldMat, newMat);
            }
        }
        // Gives derived types a chance to handle special matrix issues
        protected virtual void MatrixTransformChanged(Matrix oldMat, Matrix newMat) { }


        [PropertyData.PropertyIgnore]
        public Matrix InverseTransform { get { return Matrix.Invert(Transform); } }
        [PropertyData.PropertyIgnore]
        public Matrix ParentTransform { get { return Parent != null ? Parent.Transform : Matrix.Identity; } }
        [PropertyData.PropertyIgnore]
        public Matrix InverseParentTransform { get { return Matrix.Invert(ParentTransform); } }
        [PropertyData.PropertyIgnore]
        public Matrix LocalTransform { get { return (Matrix.CreateScale(LocalScale) * Matrix.CreateFromQuaternion(LocalRotation) * Matrix.CreateTranslation(LocalPosition)); } }
        
        [PropertyData.PropertyIgnore]
        public Matrix InverseLocalTransform { get { return Matrix.Invert(LocalTransform); } }


        [PropertyData.PropertyPriority(5)]
        [PropertyData.AllowPermutations]
        [PropertyData.PropertyGroup("Bit Masks")]
        [Category("Bit Masks")]
        [Description("If disabled this object will be ignored")]
        public bool IsEnabled { get { return isEnabled_; } set { isEnabled_ = value; OnPropertyChanged(); } }

        [PropertyData.PropertyPriority(6)]
        [PropertyData.AllowPermutations]
        [PropertyData.PropertyGroup("Bit Masks")]
        [Category("Shape")]
        [Description("Generate a symmetric copy along the chosen axis")]
        public SymmetricAxis Symmetric { get { return symmetric_; } set { symmetric_ = value; OnPropertyChanged(); } }

        /// <summary>
        /// Is inverse matrix
        /// </summary>
        /// <returns></returns>
        public Matrix GetSymmetricMatrix()
        {
            if (Symmetric == SymmetricAxis.XAxis)
                return Matrix.Invert(Matrix.CreateScale(scale_) * Matrix.CreateFromQuaternion(rotation_.Mirror(Symmetric)) * Matrix.CreateTranslation(new Vector3(-position_.X, position_.Y, position_.Z)));
            else if (Symmetric == SymmetricAxis.YAxis)
                return Matrix.Invert(Matrix.CreateScale(scale_) * Matrix.CreateFromQuaternion(rotation_.Mirror(Symmetric)) * Matrix.CreateTranslation(new Vector3(position_.X, -position_.Y, position_.Z)));
            else
                return Matrix.Invert(Matrix.CreateScale(scale_) * Matrix.CreateFromQuaternion(rotation_.Mirror(Symmetric)) * Matrix.CreateTranslation(new Vector3(position_.X, position_.Y, -position_.Z)));
        }

        /// <summary>
        /// Version of GetSymmetricMatrix that is NOT inverted
        /// </summary>
        /// <returns></returns>
        public Matrix GetSymmetricTransformMatrix()
        {
            if (Symmetric == SymmetricAxis.XAxis)
                return Matrix.CreateScale(scale_) * Matrix.CreateFromQuaternion(rotation_.Mirror(Symmetric)) * Matrix.CreateTranslation(new Vector3(-position_.X, position_.Y, position_.Z));
            else if (Symmetric == SymmetricAxis.YAxis)
                return Matrix.CreateScale(scale_) * Matrix.CreateFromQuaternion(rotation_.Mirror(Symmetric)) * Matrix.CreateTranslation(new Vector3(position_.X, -position_.Y, position_.Z));
            else
                return Matrix.CreateScale(scale_) * Matrix.CreateFromQuaternion(rotation_.Mirror(Symmetric)) * Matrix.CreateTranslation(new Vector3(position_.X, position_.Y, -position_.Z));
        }

        public Vector3 GetSymmetricVector(Vector3 v)
        {
            if (Symmetric == SymmetricAxis.XAxis)
                return v * new Vector3(-1, 1, 1);
            else if (Symmetric == SymmetricAxis.YAxis)
                return v * new Vector3(1, -1, 1);
            else
                return v * new Vector3(1, 1, -1);
        }

        [PropertyData.PropertyPriority(7)]
        [PropertyData.PropertyFlags(BitNames = "Capabilities")]
        [PropertyData.PropertyGroup("Bit Masks")]
        [Category("Bit Masks")]
        [Description("Defines intrinsic features of this object, such as a capability to animate")]
        public uint Capabilities { get { return capabilities_; } set { capabilities_ = value; OnPropertyChanged(); } }

        [PropertyData.PropertyPriority(8)]
        [PropertyData.AllowPermutations]
        [PropertyData.PropertyFlags(BitNames = "Flags")]
        [Category("Bit Masks")]
        [Description("Arbitrary user traits, such as 'sharp' or 'dark' to be used for classifying things")]
        public uint Flags { get { return flags_; } set { flags_ = value; OnPropertyChanged(); } }

        [PropertyData.PropertyIgnore]
        public ObservableCollection<SpruePiece> Children { get; private set; } = new ObservableCollection<SpruePiece>();

        [PropertyData.PropertyIgnore]
        public ObservableCollection<SprueComponent> Components { get; private set; } = new ObservableCollection<SprueComponent>();

        public virtual void DebugDraw(SprueKit.Graphics.DebugRenderer renderer, DebugDrawMode mode)
        {

        }

        public virtual void Render(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, Effect effect, Matrix view, Matrix projection)
        {
            foreach (var child in Children)
                child.Render(device, effect, view, projection);
        }

        public virtual void WriteMeshGenStream(ref BinaryWriter paramStream, ref BinaryWriter transformStream)
        {

        }

        protected void WriteTransforms(ref BinaryWriter transformStream)
        {
            // Write the transform stream
            Matrix mat = Matrix.Transpose(Transform);
            transformStream.Write(mat.M11);
            transformStream.Write(mat.M12);
            transformStream.Write(mat.M13);
            transformStream.Write(mat.M14);

            transformStream.Write(mat.M21);
            transformStream.Write(mat.M22);
            transformStream.Write(mat.M23);
            transformStream.Write(mat.M24);

            transformStream.Write(mat.M31);
            transformStream.Write(mat.M32);
            transformStream.Write(mat.M33);
            transformStream.Write(mat.M34);
        }

        protected static Color GetDrawColor(DebugDrawMode mode)
        {
            switch (mode)
            {
                case DebugDrawMode.Hover:
                    return new Color(Color.Gold, 0.75f);
                case DebugDrawMode.Selected:
                    return new Color(Color.LimeGreen, 0.25f);
            }
            return new Color(255, 255, 255, 10);
        }

        [PropertyData.PropertyIgnore]
        public virtual bool IsSprueModel { get { return false; } }

        public void VisitAll(Action<SpruePiece> visitor)
        {
            visitor(this);
            var subs = FlatChildren;
            foreach (SpruePiece child in subs)
                child.VisitAll(visitor);
        }

        public T FindParent<T>() where T : SpruePiece
        {
            if (Parent == null)
                return null;
            if (Parent is T)
                return Parent as T;
            return FindParent<T>();
        }

        public void VisitAllTested(Action<SpruePiece> visitor, Func<SpruePiece, bool> testFunc)
        {
            if (testFunc(this))
            {
                visitor(this);
                var subs = FlatChildren;
                foreach (SpruePiece child in subs)
                    child.VisitAllTested(visitor, testFunc);
            }
        }

        public void VisitAllDensitySources(Action<IHaveDensity> visitor)
        {
            if (this is IHaveDensity)
                visitor(this as IHaveDensity);
            var subs = FlatChildren;
            foreach (SpruePiece child in subs)
                child.VisitAllDensitySources(visitor);
        }

        public void VisitAllPickableNonDensity(Action<IMousePickable> visitor)
        {
            if (this is IMousePickable && !(this is IHaveDensity))
                visitor(this as IMousePickable);
            var subs = FlatChildren;
            foreach (SpruePiece child in subs)
                child.VisitAllPickables(visitor);
        }

        public void VisitAllPickables(Action<IMousePickable> visitor)
        {
            if (this is IMousePickable)
                visitor(this as IMousePickable);
            var subs = FlatChildren;
            foreach (SpruePiece child in subs)
                child.VisitAllPickables(visitor);
        }

        public void VisitAll<T>(Action<T> visitor) where T : SpruePiece
        {
            if (this is T)
                visitor(this as T);
            var subs = FlatChildren;
            foreach (SpruePiece child in subs)
                child.VisitAll<T>(visitor);
        }

        public void VisitChildren(Action<SpruePiece> visitor)
        {
            visitor(this);
            for (int i = 0; i < Children.Count; ++i)
                Children[i].VisitChildren(visitor);
        }

        public void VisitChildren<T>(Action<T> visitor) where T : SpruePiece
        {
            if (this is T)
                visitor(this as T);
            for (int i = 0; i < Children.Count; ++i)
                Children[i].VisitChildren<T>(visitor);
        }

        public void VisitComponents(Action<SprueComponent> visitor)
        {
            for (int i = 0; i < Components.Count; ++i)
                visitor(Components[i]);
            for (int i = 0; i < Children.Count; ++i)
                Children[i].VisitComponents(visitor);
        }

        public void VisitComponents<T>(Action<T> visitor) where T : SprueComponent
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                T c = Components[i] as T;
                if (c != null)
                    visitor(c);
            }
            for (int i = 0; i < Children.Count; ++i)
                Children[i].VisitComponents<T>(visitor);
        }

        public virtual bool AcceptsChild(SpruePiece piece)
        {
            /*
             * Conventionally it's assumed that GUI covers these responsibilities
             * but 
             */
            var parent = Parent;
            while (parent != null)
            {
                if (!parent.AcceptsChild(piece))
                    return false;
                parent = parent.Parent;
            }
            return true;
        }

        [PropertyData.PropertyIgnore]
        public virtual bool ChildrenAreLocked { get { return false; } }
        [PropertyData.PropertyIgnore]
        public virtual bool IsLocked
        {
            get
            {
                var parent = Parent;
                while (parent != null)
                {
                    if (parent.ChildrenAreLocked)
                        return true;
                    parent = parent.Parent;
                }
                return false;
            }
        }

        public virtual System.Collections.IList GetAppropriateList(SpruePiece piece)
        {
            if (piece is ChainPiece.ChainBone)
                return null;
            if (piece is SprueComponent)
                return Components;
            return Children;
        }

        protected virtual int CalculateStructuralHash() { return 0; }

        public virtual int StructuralHash(int hash = 17)
        {
            hash = hash * 31 + CalculateStructuralHash();
            foreach (var child in Children)
                hash = hash * 31 + child.StructuralHash();
            return hash;
        }

        public virtual SpruePiece Clone() { return null; }

        protected void CloneInto(SpruePiece into)
        {
            into.Parent = Parent;
            into.Name = Name;
            into.Position = Position;
            into.Rotation = Rotation;
            into.Scale = Scale;
            into.Flags = Flags;
            into.Capabilities = Capabilities;
            into.IsEnabled = IsEnabled;
            into.Symmetric = Symmetric;

            foreach (var child in Children)
            {
                var cloneChild = child.Clone();
                if (cloneChild != null)
                {
                    cloneChild.Parent = into;
                    into.Children.Add(cloneChild);
                }
            }

            foreach (var comp in Components)
            {
                var cloneChild = comp.Clone() as SprueComponent;
                if (cloneChild != null)
                {
                    cloneChild.Parent = into;
                    into.Components.Add(cloneChild);
                }
            }
        }
    }

    /// <summary>
    /// SprueComponents always appear before children.
    /// Components deal with non-standard items, such as texturing, meshing tasks, etc.
    /// </summary>
    public class SprueComponent : SpruePiece
    {

    }

    [PropertyData.PropertyIgnore("Symmetric")]
    public partial class SprueModel : SpruePiece, IHaveDensity
    {
        public override bool IsSprueModel { get { return true; } }

        MeshData meshData_;
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public MeshData MeshData {
            get { return meshData_; }
            set {
                var oldMesh = meshData_;
                meshData_ = value;
                if (oldMesh != null)
                    DisposalQueue.Inst.Queue(oldMesh);
                    //oldMesh.Dispose();
                OnPropertyChanged("TotalTriangleCount");
                OnPropertyChanged("TotalVertexCount");
                OnPropertyChanged("MeshData");
            }
        }

        bool useBMesh_ = false;
        [Category("Experimental")]
        [Description("B-Mesh algorithm will be used instead of voxelization")]
        [DisplayName("Use B-Mesh")]
        public bool UseBMesh { get { return useBMesh_; } set { useBMesh_ = value; OnPropertyChanged(); } }
        int bmeshSubdivs_ = 1;
        [Category("Experimental")]
        [Description("Iterations of Catmull-Clark subdivision to be applied to B-Mesh")]
        [DisplayName("B-Mesh Subdivisions")]
        public int BMeshSubdivisions { get { return bmeshSubdivs_; } set { bmeshSubdivs_ = value; OnPropertyChanged(); } }

        #region Information

        [Notify.TrackMember(IsExcluded = true)]
        public int TotalTriangleCount
        {

            get {
                int total = 0;
                VisitChildren<SprueModel>((m) => { total += m.TriangleCount; });
                VisitChildren<ModelPiece>((m) => { total += m.TriangleCount; });
                return total;
            }
        }

        [Notify.TrackMember(IsExcluded = true)]
        public int TotalVertexCount
        {
            get
            {
                int total = 0;
                VisitChildren<SprueModel>((m) => { total += m.VertexCount; });
                VisitChildren<ModelPiece>((m) => { total += m.VertexCount; });
                return total;
            }
        }

        [Notify.TrackMember(IsExcluded = true)]
        public int TriangleCount { get { return MeshData != null ? MeshData.TriangleCount : 0; } }
        [Notify.TrackMember(IsExcluded = true)]
        public int VertexCount { get { return MeshData != null ? MeshData.VertexCount : 0; } }

        #endregion

        #region Export Settings

        PluginLib.IntVector2 textureSize_ = new PluginLib.IntVector2(1024, 1024);
        [Category("Export")]
        [Description("Desired texture output size")]
        public PluginLib.IntVector2 TextureSize { get { return textureSize_; } set { textureSize_ = value; OnPropertyChanged(); } }

        bool packMeshes_ = true;
        [Category("Export")]
        [Description("Mesh textures will be packed together and geometry merged into one object")]
        public bool PackMeshes { get { return packMeshes_; } set { packMeshes_ = value; OnPropertyChanged(); } }

        PluginLib.IntVector2 packedTextureSize_ = new PluginLib.IntVector2(1024, 1024);
        [Category("Export")]
        [Description("Texture space to be allowed for packing textures when \"Pack Meshes\" is enabled, maximum budget for the entire model")]
        public PluginLib.IntVector2 PackedTextureSize { get { return packedTextureSize_; } set { packedTextureSize_ = value; OnPropertyChanged(); } }

        bool calculateLOD_ = false;
        [Category("Export")]
        [Description("LODs will be generated for 3 levels")]
        [Notify.DontSignalWork]
        public bool CalculateLOD { get { return calculateLOD_; } set { calculateLOD_ = value; OnPropertyChanged(); } }

        bool generateReport_ = true;
        [Category("Export")]
        [Description("When exporting a contents report will be generated alongside the output model")]
        [Notify.DontSignalWork]
        public bool GenerateReportWithExport { get { return generateReport_; } set { generateReport_ = value; OnPropertyChanged(); } }

        ResponseCurve weightsCurve_ = new ResponseCurve { CurveShape = CurveType.Logistic, YIntercept = -0.06f, Exponent = 1.1f };
        [Category("Export")]
        [Description("Bone weights will be use this curve as a fall-off function")]
        public ResponseCurve BoneWeightsCurve { get { return weightsCurve_; } set { weightsCurve_ = value; OnPropertyChanged(); } }

        #endregion

        #region Density Function

        public void PrepareDensity()
        {
            VisitChildren((p) =>
            {
                if (p is IHaveDensity && !(p is SprueModel))
                    ((IHaveDensity)p).PrepareDensity();
            });
        }

        public float GetDensity(Vector3 pos)
        {
            float currentDensity = 1000.0f;
            foreach (SpruePiece child in Children)
                currentDensity = RecurseDensity(child, child.Children, pos, currentDensity);
            return currentDensity;
        }

        public static float RecurseDensity(SpruePiece piece, IEnumerable<SpruePiece> targetList, Vector3 pos, float inputDensity)
        {
            if (piece is IHaveDensity)
                inputDensity = DensityFunctions.CombineDensity(inputDensity, ((IHaveDensity)piece).GetDensity(pos), ((IHaveDensity)piece).CSGType());
            foreach (SpruePiece child in targetList)
                inputDensity = RecurseDensity(child, child.Children, pos, inputDensity);
            return inputDensity;
        }

        public CSGOperation CSGType() { return CSGOperation.Add; }

        #endregion
    }

    public enum ShapeFunctionType
    {
        Sphere,
        Box,
        RoundedBox,
        Capsule,
        Cylinder,
        Cone,
        Ellipsoid,
        Plane,
        Torus,
        SuperShape,
    }

    /// <summary>
    /// Primitive shape
    /// </summary>
    public partial class SimplePiece : SpruePiece, IHaveDensity, IMousePickable
    {
        CSGOperation csgOp_;
        ShapeFunctionType type_;
        Vector4 params_ = new Vector4(1,1,1,0);

        [PropertyData.AllowPermutations]
        [Category("Shape")]
        [Description("Controls how this shape will be composited into the final mesh")]
        public CSGOperation CSGOperation { get { return csgOp_; } set { csgOp_ = value;  OnPropertyChanged(); } }

        /// <summary>
        /// Type of shape function to use.
        /// </summary>
        [PropertyData.AllowPermutations]
        [Category("Shape")]
        [Description("Primitive shape that will be used")]
        public ShapeFunctionType ShapeType { get { return type_; } set { type_ = value; OnPropertyChanged(); } }
        /// <summary>
        /// Shapes have a maximum of 4 parameter.
        /// </summary>
        [PropertyData.AllowPermutations]
        [ShapeData("ShapeType")]
        [Category("Shape")]
        public Vector4 Params { get { return params_; } set { params_ = value; OnPropertyChanged(); } }

        public override void DebugDraw(SprueKit.Graphics.DebugRenderer renderer, DebugDrawMode mode)
        {
            switch (ShapeType)
            {
            case ShapeFunctionType.Sphere:
                renderer.DrawWireSphere(Transform, Params.X, GetDrawColor(mode), DebugDrawDepth.Always);
                //renderer.DrawWireSphere(new BoundingSphere(Position, Params.X), Color.Green);
                break;
            case ShapeFunctionType.Box:
                renderer.DrawWireBox(Transform, Params.X/2, Params.Y/2, Params.Z/2, GetDrawColor(mode), DebugDrawDepth.Always);
                break;
            case ShapeFunctionType.RoundedBox:
                renderer.DrawWireBox(Transform, Params.X/2, Params.Y/2, Params.Z/2, GetDrawColor(mode), DebugDrawDepth.Always);
                break;
            case ShapeFunctionType.Capsule:
                renderer.DrawCapsule(Transform, Params.Y, Params.X, GetDrawColor(mode), DebugDrawDepth.Always);
                break;
            case ShapeFunctionType.Ellipsoid:
                renderer.DrawEllipsoid(Transform, Params.X, Params.Y, Params.Z, GetDrawColor(mode), DebugDrawDepth.Always);
                break;
            case ShapeFunctionType.Plane:
                //renderer.DrawPlane(Transform, Params.X, Params.Y, Params.Z, Params.W, Color.LimeGreen);
                break;
            case ShapeFunctionType.SuperShape:
                break;
            case ShapeFunctionType.Cone:
                renderer.DrawCone(Transform, Params.X, Params.Y, GetDrawColor(mode), DebugDrawDepth.Always);
                break;
            case ShapeFunctionType.Cylinder:
                renderer.DrawCylinder(Transform, Params.X, Params.Y, GetDrawColor(mode), DebugDrawDepth.Always);
                break;
            case ShapeFunctionType.Torus:
                renderer.DrawTorus(Transform, Params.X, Params.Y, GetDrawColor(mode), DebugDrawDepth.Always);
                break;
            }
        }

        public override void WriteMeshGenStream(ref BinaryWriter paramStream, ref BinaryWriter transformStream)
        {
            paramStream.Write(Params.X);
            paramStream.Write(Params.Y);
            paramStream.Write(Params.Z);
            paramStream.Write(Params.W);
            // write shape data
            //switch (ShapeType)
            //{
            //    case ShapeFunctionType.Box:
            //    case ShapeFunctionType.Ellipsoid:
            //        paramStream.Write(Params.X);
            //        paramStream.Write(Params.Y);
            //        paramStream.Write(Params.Z);
            //        break;
            //    case ShapeFunctionType.Cone:
            //    case ShapeFunctionType.Capsule:
            //    case ShapeFunctionType.Cylinder:
            //    case ShapeFunctionType.Torus:
            //        paramStream.Write(Params.X);
            //        paramStream.Write(Params.Y);
            //        break;
            //    case ShapeFunctionType.Plane:
            //    case ShapeFunctionType.RoundedBox:
            //    case ShapeFunctionType.SuperShape:
            //        paramStream.Write(Params.X);
            //        paramStream.Write(Params.Y);
            //        paramStream.Write(Params.Z);
            //        paramStream.Write(Params.W);
            //        break;
            //    case ShapeFunctionType.Sphere:
            //        paramStream.Write(Params.X);
            //        break;
            //}

            base.WriteTransforms(ref transformStream);
        }

        public CSGOperation CSGType() { return csgOp_; }

        float InternalGetDensity(Vector3 localPos)
        {
            switch (ShapeType)
            {
                case ShapeFunctionType.Box:
                    return DensityFunctions.CubeDensity(localPos, Params);
                case ShapeFunctionType.Capsule:
                    return DensityFunctions.CapsuleDensity(localPos, Params);
                case ShapeFunctionType.Cone:
                    return DensityFunctions.ConeDensity(localPos, Params);
                case ShapeFunctionType.Cylinder:
                    return DensityFunctions.CylinderDensity(localPos, Params);
                case ShapeFunctionType.Ellipsoid:
                    return DensityFunctions.EllipsoidDistance(localPos, Params);
                case ShapeFunctionType.Plane:
                    return DensityFunctions.PlaneDistance(localPos, Params);
                case ShapeFunctionType.RoundedBox:
                    return DensityFunctions.RoundedBoxDensity(localPos, Params);
                case ShapeFunctionType.Sphere:
                    return DensityFunctions.SphereDensity(localPos, Params);
                case ShapeFunctionType.SuperShape:
                case ShapeFunctionType.Torus:
                    return DensityFunctions.TorusDensity(localPos, Params);
            }
            return 1000.0f;
        }

        public void PrepareDensity()
        {

        }

        public float GetDensity(Vector3 pos)
        {
            Vector3 localPos = Vector3.Transform(pos, InverseTransform);
            float ret = InternalGetDensity(localPos);

            if (Symmetric != SymmetricAxis.None)
            {
                //Console.WriteLine(string.Format("{0} -> {1}", localPos.ToString(), symmetricPosition.ToString()));
                Vector3 symmetricPosition = Vector3.Transform(pos, GetSymmetricMatrix());
                float symValue = InternalGetDensity(symmetricPosition);
                return Math.Min(ret, symValue);
            }

            return ret;
        }

        public bool DoMousePick(Ray ray, out float hitTime)
        {
            hitTime = 100000.0f;
            Ray testRay = ray;// new Ray(Vector3.Transform(ray.Position, InverseTransform), Vector3.TransformNormal(ray.Direction, InverseTransform));
            float t = 0.0f;

            const int MAX_RAY_STEPS = 32;
            for (int i = 0; i < MAX_RAY_STEPS; ++i)
            {
                Vector3 testPoint = testRay.Position + testRay.Direction * t;
                if (!testPoint.X.IsFinite() || !testPoint.Y.IsFinite() || !testPoint.Z.IsFinite())
                    continue;

                float d = GetDensity(testPoint);
                if (d < 0.001f)
                {
                    hitTime = (testPoint - testRay.Position).Length();
                    return true;
                }
                t += d;
            }
            return false;
        }

        protected override int CalculateStructuralHash()
        {
            int curHash = SprueKit.Util.HashHelper.Start(CSGOperation.GetHashCode());
            curHash = SprueKit.Util.HashHelper.Hash(curHash, ShapeType.GetHashCode());
            curHash = SprueKit.Util.HashHelper.Hash(curHash, Symmetric.GetHashCode());
            return curHash;
        }

        public override SpruePiece Clone()
        {
            var ret = new SimplePiece();
            CloneInto(ret);
            ret.Params = Params;
            ret.ShapeType = ShapeType;
            ret.CSGOperation = CSGOperation;
            return ret;
        }
    }

    /// <summary>
    /// A piece that comes from a plugin function. Only available in the editor.
    /// </summary>
    public class PluginPiece : SpruePiece
    {

    }

    public abstract class SequencePiece<CHILD_TYPE> : SpruePiece where CHILD_TYPE : SpruePiece
    {
        public override IEnumerable<object> FlatChildren
        {
            get
            {
                List<object> ret = new List<object>();

                ret.AddRange(Nodes);
                ret.AddRange(Components);
                ret.AddRange(Children);

                return ret;
            }
        }

        [PropertyData.PropertyIgnore]
        public ObservableCollection<CHILD_TYPE> Nodes { get; private set; } = new ObservableCollection<CHILD_TYPE>();

        public abstract CHILD_TYPE AddNode(CHILD_TYPE after = null);
        public abstract CHILD_TYPE AddNodeBefore(CHILD_TYPE before);
        public abstract void RemoveNode(CHILD_TYPE who);

        public override System.Collections.IList GetAppropriateList(SpruePiece piece)
        {
            if (piece is SprueComponent)
                return Components;
            else if (piece is CHILD_TYPE)
                return Nodes;
            return Children;
        }
    }

    /// <summary>
    /// Node based chain of connections using a split elipse function.
    /// </summary>
    public partial class ChainPiece : SpruePiece, IHaveDensity
    {
        [PropertyData.PropertyIgnore("Scale")]
        [PropertyData.PropertyIgnore("Symmetric")]
        public partial class ChainBone : SpruePiece, IMousePickable
        {
            Vector3 crossSection_ = new Vector3(2,2,2); //4 component ellipsoid

            //public Vector3 Position { get { return position_; } set { position_ = value; } }
            [Category("Shape")]
            [Description("Adjusts the extruded cross-section of the segment")]
            public Vector3 CrossSection { get { return crossSection_; } set { crossSection_ = value; OnPropertyChanged(); } }

            public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
            {
                if (Parent != null)
                    Parent.DebugDraw(renderer, mode);
            }

            public void DebugDraw(ChainPiece next, DebugRenderer renderer, DebugDrawMode mode)
            {

            }

            public bool DoMousePick(Ray ray, out float hitDist)
            {
                hitDist = 1000.0f;
                if (Parent != null)
                {
                    ChainPiece parent = Parent as ChainPiece;
                    if (parent != null)
                    {
                        BoundingSphere sphere = new BoundingSphere(Position, crossSection_.MaxElement());
                        float? hit = sphere.Intersects(ray);
                        if (hit.HasValue)
                        {
                            hitDist = hit.Value;
                            return true;
                        }

                        if (parent.Symmetric != SymmetricAxis.None)
                        {
                            sphere.Center = parent.GetSymmetricVector(sphere.Center);
                            hit = sphere.Intersects(ray);
                            if (hit.HasValue)
                            {
                                hitDist = hit.Value;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            public override SpruePiece Clone()
            {
                ChainBone ret = new ChainBone();
                CloneInto(ret);
                ret.CrossSection = CrossSection;
                return ret;
            }

            protected override void MatrixTransformChanged(Matrix oldMat, Matrix newMat)
            {
                var parent = Parent as ChainPiece;
                if (parent != null)
                {
                    int idx = parent.Bones.IndexOf(this);
                    if (idx != -1 && idx < parent.Bones.Count - 1)
                    {
                        using (var v = new Notify.Tracker.TrackingSideEffects())
                        {
                            var invSelf = Matrix.Invert(oldMat);
                            Matrix nextMat = (parent.Bones[idx + 1].Transform * invSelf) * LocalTransform; // relative to us
                            nextMat.Scale = nextMat.Scale.Abs();
                            parent.Bones[idx + 1].Transform = nextMat * ParentTransform;
                        }
                    }
                }
            }
        }

        public ChainPiece()
        {
            Bones.CollectionChanged += Bones_CollectionChanged;
        }

        public static ChainPiece New()
        {
            ChainPiece ret = new Data.ChainPiece();
            ret.Bones.Add(new ChainBone { Position = new Vector3(0, 0, -4), Parent = ret });
            ret.Bones.Add(new ChainBone { Position = new Vector3(0, 2, 0), Parent = ret });
            ret.Bones.Add(new ChainBone { Position = new Vector3(0, 0, 4), Parent = ret });
            return ret;
        }

        private void Bones_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var bone in Bones)
                bone.Parent = this;
            OnPropertyChanged("FlatChildren");
        }

        //[PropertyIgnore]
        public override IEnumerable<object> FlatChildren
        {
            get
            {
                List<object> ret = new List<object>();

                ret.AddRange(Bones);
                ret.AddRange(Components);
                ret.AddRange(Children);

                return ret;
            }
        }

        [PropertyData.PropertyIgnore]
        public ObservableCollection<ChainBone> Bones { get; private set; } = new ObservableCollection<ChainBone>();

        public ChainBone AddSpineBone(ChainBone after = null)
        {
            if (after == null || Bones.Last() == after)
            {
                ChainBone last = Bones.Last();
                ChainBone prev = Bones[Bones.Count - 2];

                Vector3 delta = last.Position - prev.Position;

                var newBone = new ChainBone { Parent = this, Position = last.Position + delta, CrossSection = last.CrossSection };
                Bones.Add(newBone);
                return newBone;
            }
            else
            {
                int idx = Bones.IndexOf(after);
                ChainBone nextBone = Bones[idx + 1];
                var newBone = new ChainBone { Parent = this, Position = Vector3.Lerp(after.Position, nextBone.Position, 0.5f), CrossSection = Vector3.Lerp(after.CrossSection, nextBone.CrossSection, 0.5f) };
                Bones.Insert(idx + 1, newBone);
                return newBone;
            }
            return null;
        }

        public ChainBone AddSpineBoneBefore(ChainBone before)
        {
            int idx = Bones.IndexOf(before);
            if (idx > 0 && idx <= Bones.Count)
                return AddSpineBone(Bones[idx - 1]);
            else
            {
                //???if (Bones.Count > 1)
                //???{
                //???    
                //???}
                //???else
                {
                    var newBone = new ChainBone
                    {
                        Parent = this,
                        Position = before.Position - Vector3.UnitZ * 3,
                        CrossSection = before.CrossSection
                    };
                    Bones.Insert(0, newBone);
                    return newBone;
                }
            }
        }

        public void RemoveSpineBone(ChainBone bone)
        {
            if (Bones.Count <= 2)
                return;
            Bones.Remove(bone);
        }

        public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
        {
            Matrix symmetricMatrix = GetSymmetricMatrix();
            for (int boneIdx = 0; boneIdx < Bones.Count - 1; ++boneIdx)
            {
                Vector3 dir = Bones[boneIdx+1].Position - Bones[boneIdx].Position;
                float r = Bones[boneIdx].CrossSection.MaxElement();

                Vector3 heightVec = new Vector3(0, 0, dir.Length());
                Vector3 offsetXVec = new Vector3(r, 0, 0);
                Vector3 offsetZVec = new Vector3(0, r, 0);

                Vector3 fDir = dir;
                fDir.Normalize();

                Matrix transform = Matrix.CreateBillboard(Bones[boneIdx].Position, Bones[boneIdx+1].Position, Bones[boneIdx+1].Transform.Up, null);

                for (float i = 0; i < 360; i += 90.0f)
                {
                    Vector3 pt1 = Graphics.DebugRenderer.PointOnSphere(Vector3.Zero, r, MathHelper.ToRadians(90), MathHelper.ToRadians(i));
                    Vector3 pt2 = Graphics.DebugRenderer.PointOnSphere(Vector3.Zero, r, MathHelper.ToRadians(90), MathHelper.ToRadians(i + 90.0f));

                    Vector3 p1 = Vector3.Transform(pt1, transform);
                    Vector3 p2 = Vector3.Transform(pt2, transform);
                    renderer.DrawLine(p1, p2, GetDrawColor(mode), DebugDrawDepth.Always);
                }

                Vector3 fHeightVec = new Vector3(0, 0, -dir.Length());
                Vector3 fOffsetXVec = new Vector3(r, 0, 0);
                Vector3 fOffsetZVec = new Vector3(0, r, 0);

                renderer.DrawLine(Vector3.Transform(fOffsetXVec, transform), Vector3.Transform(fHeightVec, transform), GetDrawColor(mode), DebugDrawDepth.Always);
                renderer.DrawLine(Vector3.Transform(-fOffsetXVec, transform), Vector3.Transform(fHeightVec, transform), GetDrawColor(mode), DebugDrawDepth.Always);
                renderer.DrawLine(Vector3.Transform(fOffsetZVec, transform), Vector3.Transform(fHeightVec, transform), GetDrawColor(mode), DebugDrawDepth.Always);
                renderer.DrawLine(Vector3.Transform(-fOffsetZVec, transform), Vector3.Transform(fHeightVec, transform), GetDrawColor(mode), DebugDrawDepth.Always);
            }
        }

        public override System.Collections.IList GetAppropriateList(SpruePiece piece)
        {
            if (piece is ChainBone)
                return Bones;
            if (piece is SprueComponent)
                return Components;
            return Children;
        }

        public CSGOperation CSGType()
        {
            return CSGOperation.Add;
        }

        bool spine_ = false;
        [Category("Shape")]
        [Description("Spines use 'closest' behaviour in linking to parent bones instead of linking by the first chain")]
        public bool IsSpine { get { return spine_; } set { spine_ = value;  OnPropertyChanged(); } }

        bool generateBones_ = true;
        [Category("Shape")]
        [Description("This chain will generate a skeleton bones for animation")]
        public bool GenerateBones { get { return generateBones_; } set { generateBones_ = value;  OnPropertyChanged(); } }

        uint smoothingLevels_ = 0;
        [Category("Shape")]
        [Description("Geometry will use an interpolated curve to smooth the chain")]
        public uint SmoothingLevels { get { return smoothingLevels_; } set { smoothingLevels_ = value; OnPropertyChanged(); } }

        float InternalGetDensity(Vector3 samplePos, bool useSymmetric)
        {
            float currentDistance = 100000.0f;
            if (Bones.Count < 1)
                return currentDistance;

            for (int i = 0; i < positions.Count; i += 2)
            //float step = 1.0f / (1.0f + (float)SmoothingLevels);
            //for (float i = 0; i < Bones.Count - step; i += step)
            {
                Vector3 thisBonePos = positions[i].MakeSymmetric(useSymmetric ? Symmetric : SymmetricAxis.None);
                Vector3 nextBonePos = positions[i+1].MakeSymmetric(useSymmetric ? Symmetric : SymmetricAxis.None);

                Vector3 pa = samplePos - thisBonePos;
                Vector3 ba = (nextBonePos - thisBonePos);
                float h = Mathf.Clamp(Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba), 0.0f, 1.0f);
                if (!h.IsFinite())
                    continue;

                Vector3 thisCross = crosses[i];
                Vector3 nextCross = crosses[i+1];

                Vector3 interpRad = Vector3.Lerp(thisCross, nextCross, h);
                float distance = (((pa - ba * h) / interpRad).Length() - 1.0f) * Math.Min(interpRad.X, Math.Min(interpRad.Y, interpRad.Z));
                currentDistance = Math.Min(currentDistance, distance);
            }

            //for (int i = 0; i < Bones.Count - 1; ++i)
            //{
            //    Vector3 thisBonePos = Bones[i].Position.MakeSymmetric(useSymmetric ? Symmetric : SymmetricAxis.None);
            //    Vector3 nextBonePos = Bones[i + 1].Position.MakeSymmetric(useSymmetric ? Symmetric : SymmetricAxis.None);
            //
            //    Vector3 pa = samplePos - thisBonePos;
            //    Vector3 ba = (nextBonePos - thisBonePos);
            //    float h = Mathf.Clamp(Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba), 0.0f, 1.0f);
            //
            //    // Calculate an ellipsoid distance at the given point
            //    Vector3 interpRad = Vector4.Lerp(Bones[i].CrossSection, Bones[i + 1].CrossSection, h).XYZ();
            //    float distance = (((pa - ba * h) / interpRad).Length() - 1.0f) * Math.Min(interpRad.X, Math.Min(interpRad.Y, interpRad.Z));
            //    currentDistance = Math.Min(currentDistance, distance);
            //}

            return currentDistance;
        }

        List<Vector3> positions = new List<Vector3>();
        List<Vector3> crosses = new List<Vector3>();
        public void PrepareDensity()
        {
            Data.TripletCurve meshingPositions = new Data.TripletCurve();
            Data.TripletCurve meshingCross = new Data.TripletCurve();
            for (int i = 0; i < Bones.Count; ++i)
            {
                meshingPositions.AddKey(i, Bones[i].LocalPosition);
                meshingCross.AddKey(i, Bones[i].CrossSection);
            }
            meshingPositions.ComputeTangents();
            meshingCross.ComputeTangents();

            positions.Clear();
            crosses.Clear();

            float step = 1.0f / (1.0f + (float)SmoothingLevels);
            for (float i = 0; i < Bones.Count - step; i += step)
            {
                positions.Add(meshingPositions.Evaluate(i));
                positions.Add(meshingPositions.Evaluate(i + step));

                crosses.Add(meshingCross.Evaluate(i));
                crosses.Add(meshingCross.Evaluate(i + step));
            }

            foreach (var bone in Bones)
            {
                foreach (var child in bone.Children)
                {
                    child.VisitChildren((p) =>
                    {
                        if (p is IHaveDensity)
                            ((IHaveDensity)p).PrepareDensity();
                    });
                }
            }    
        }

        public List<Vector3> GetPositions() { return positions; }
        public List<Vector3> GetCrosses() { return crosses; }

        public float GetDensity(Vector3 position)
        {
            Vector3 localPos = Vector3.Transform(position, InverseTransform);
            float ret = InternalGetDensity(localPos, false);

            foreach (ChainBone bone in Bones)
                ret = Math.Min(ret, SprueModel.RecurseDensity(bone, bone.Children, position, ret));

            if (Symmetric != SymmetricAxis.None)
            {
                float symValue = InternalGetDensity(localPos, true);
                ret = Math.Min(ret, symValue);

                foreach (ChainBone bone in Bones)
                    ret = Math.Min(ret, SprueModel.RecurseDensity(bone, bone.Children, position, ret));
            }

            return ret;
        }

        public override void Render(GraphicsDevice device, Effect effect, Matrix view, Matrix projection)
        {
            base.Render(device, effect, view, projection);
            foreach (var bone in Bones)
                bone.Render(device, effect, view, projection);
        }

        protected override int CalculateStructuralHash()
        {
            int hash = SprueKit.Util.HashHelper.Start(Bones.Count.GetHashCode());
            hash = SprueKit.Util.HashHelper.Hash(hash, Symmetric.GetHashCode());
            hash = SprueKit.Util.HashHelper.Hash(hash, SmoothingLevels.GetHashCode());
            foreach (var bone in Bones)
                hash = SprueKit.Util.HashHelper.Hash(hash, bone.StructuralHash());
            return hash;
        }

        public override SpruePiece Clone()
        {
            ChainPiece ret = new ChainPiece();
            ret.generateBones_ = generateBones_;
            ret.smoothingLevels_ = smoothingLevels_;
            ret.spine_ = spine_;
            CloneInto(ret);
            foreach (var bone in Bones)
            {
                var newBone = bone.Clone() as ChainBone;
                if (newBone != null)
                {
                    newBone.Parent = ret;
                    ret.Bones.Add(newBone);
                }
            }
            return ret;
        }

        [Notify.TrackMember(IsExcluded = true)]
        public int NodeCount
        {
            get
            {
                return Bones.Count;
            }
        }

        [Notify.TrackMember(IsExcluded = true)]
        public float Length
        {
            get
            {
                float len = 0.0f;
                for (int i = 0; i < Bones.Count - 1; ++i)
                    len += (Bones[i + 1].Position - Bones[i].Position).Length();
                return len;
            }
        }


    }

    /// <summary>
    /// Uses an external model for the piece.
    /// </summary>
    public partial class ModelPiece : SpruePiece, IMousePickable
    {
        [Category("Shape")]
        [Description("3d model file to use for this mesh")]
        public ForeignModel ModelFile { get; set; } = new ForeignModel();

        SprueBindings.CSGTask combination_ = SprueBindings.CSGTask.Merge;
        [PropertyData.AllowPermutations]
        [Category("Shape")]
        [Description("Determines how this mesh will be combined into the final output")]
        public SprueBindings.CSGTask Combine { get { return combination_; } set { combination_ = value; OnPropertyChanged(); } }

        bool isDetail_ = false;
        [Category("Shape")]
        [Description("'Detail' meshes are only included in the highest LOD level")]
        public bool IsDetail { get { return isDetail_; } set { isDetail_ = value; OnPropertyChanged(); } }

        [Notify.TrackMember(IsExcluded = true)]
        [PropertyData.PropertyIgnore]
        public int TriangleCount { get { return IsEnabled ? ModelFile.TriangleCount : 0; } }

        [Notify.TrackMember(IsExcluded = true)]
        [PropertyData.PropertyIgnore]
        public int VertexCount { get { return IsEnabled ? ModelFile.VertexCount : 0; } }

        ~ModelPiece()
        {
        }

        public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
        {
            if (mode == DebugDrawMode.Passive)
                return;
            if (ModelFile != null && ModelFile.ModelData != null)
            {
                Color drawColor = GetDrawColor(mode);
            
                Matrix transform = Transform;
                foreach (var mesh in ModelFile.ModelData.Meshes)
                {
                    if (mesh.Indices == null)
                        mesh.ReadFromAPI();

                    for (int i = 0; i < mesh.Indices.Length; i += 3)
                    {
                        var vertA = Vector3.Transform(mesh.Positions[mesh.Indices[i]], transform);
                        var vertB = Vector3.Transform(mesh.Positions[mesh.Indices[i + 1]], transform);
                        var vertC = Vector3.Transform(mesh.Positions[mesh.Indices[i + 2]], transform);

                        renderer.DrawLine(vertA, vertB, drawColor, DebugDrawDepth.Always);
                        renderer.DrawLine(vertB, vertC, drawColor, DebugDrawDepth.Always);
                        renderer.DrawLine(vertC, vertA, drawColor, DebugDrawDepth.Always);
                    }
                }
            }
        }

        public override void Render(GraphicsDevice device, Effect effect, Matrix view, Matrix projection)
        {
            if (!IsEnabled)
                return;
            if (Combine != SprueBindings.CSGTask.Independent && Combine != SprueBindings.CSGTask.ClipIndependent)
                return;

            base.Render(device, effect, view, projection);
            var meshes = GetMeshes();
            if (meshes != null && meshes.Count > 0)
            {
                foreach (var mesh in meshes)
                {
                    mesh.Effect = effect;
                    ((ICommonEffect)effect).Transform = this.Transform;
                    mesh.Draw(device, view, projection);

                    if (Symmetric != SymmetricAxis.None)
                    {
                        Matrix sym = GetSymmetricTransformMatrix();
                        ((ICommonEffect)effect).Transform = sym;
                        mesh.Draw(device, view, projection);
                    }
                }
            }
        }

        float? InternalMousePick(Ray ray, Matrix transform)
        {
            float hitDist = float.MaxValue;
            bool ret = false;
            List<MeshData> meshes = GetMeshes();
            foreach (var mesh in meshes)
            {
                Matrix inverseTransform = Matrix.Invert(transform);
                Ray newRay = new Ray(ray.Position, ray.Direction);
                newRay.Position = Vector3.Transform(ray.Position, inverseTransform);
                newRay.Direction = Vector3.TransformNormal(ray.Direction, inverseTransform);
                newRay.Direction.Normalize();
            
                if (!newRay.Intersects(mesh.Bounds).HasValue)
                    return null;

                var vertices = mesh.GetVertices();
                var indices = mesh.GetIndices();
                for (int i = 0; i < indices.Count; i += 3)
                {
                    var vertA = vertices[indices[i]].Position;
                    var vertB = vertices[indices[i + 1]].Position;
                    var vertC = vertices[indices[i + 2]].Position;
            
                    float hit = 0.0f;
                    Vector2 hitUV = new Vector2();
                    if (newRay.Intersects(vertA, vertB, vertC, ref hit, ref hitUV))
                    {
                        if (hit < hitDist)
                        {
                            hitDist = hit;
                            ret = true;
                        }
                    }
                }
            }

            if (ret)
                return hitDist;
            return null;
        }

        public bool DoMousePick(Ray ray, out float hitDist)
        {
            hitDist = float.MaxValue;
            float? mainHit = InternalMousePick(ray, Transform);
            if (mainHit.HasValue)
            {
                hitDist = mainHit.Value;
                return true;
            }

            if (Symmetric != SymmetricAxis.None)
            {
                float? symHit = InternalMousePick(ray, GetSymmetricTransformMatrix());
                if (symHit.HasValue)
                {
                    hitDist = symHit.Value;
                    return true;
                }
            }

            return false;
        }

        public List<MeshData> GetMeshes() { return ModelFile != null ? ModelFile.GetMeshes() : new List<MeshData>(); }

        public override SpruePiece Clone()
        {
            var ret = new ModelPiece();
            ret.ModelFile = ModelFile.Clone();
            ret.Combine = Combine;
            ret.IsDetail = IsDetail;
            CloneInto(ret);
            return ret;
        }
    }

    public partial class InstancePiece : SpruePiece
    {
        Uri filePath_;

        [PropertyData.AllowPermutations]
        [PropertyData.PropertyGroup("Shape")]
        public Uri SprueModel { get { return filePath_; } set { filePath_ = value; UpdateModel();  OnPropertyChanged(); } }

        public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
        {
            
        }

        public override SpruePiece Clone()
        {
            var ret = new InstancePiece();
            ret.SprueModel = SprueModel;
            if (Children.Count > 0)
                ret.Children.Add(Children[0].Clone());
            return ret;
        }

        // We cannot change these objects, limitation
        public override bool AcceptsChild(SpruePiece piece) { return false; }
        // We cannot change these objects, limitation
        public override bool ChildrenAreLocked { get { return true; } }

        void UpdateModel()
        {
            using (var v = new Notify.Tracker.TrackingSideEffects())
            {
                Children.Clear();
                if (filePath_ != null && System.IO.File.Exists(filePath_.AbsolutePath))
                {
                    SprueModel loadedModel = Data.SprueModel.LoadFile(filePath_.AbsolutePath);
                    Children.Add(loadedModel);
                }
            }
        }
    }

    public partial class MarkerPiece : SpruePiece
    {
        bool showForward_ = false;

        [Notify.DontSignalWork]
        [Description("The forward direction of the marker will be displayed in the 3d viewport")]
        public bool ShowForward { get { return showForward_; } set { showForward_ = value; OnPropertyChanged(); } }

        public override void DebugDraw(DebugRenderer renderer, DebugDrawMode mode)
        {
            Color drawColor = mode == DebugDrawMode.Selected ? Color.LimeGreen : Color.LightCyan;
            if (ShowForward)
                renderer.DrawLine(Position, Vector3.Transform(Vector3.UnitZ * 2, Transform), Color.Gold, DebugDrawDepth.Always);
            renderer.DrawCross(Position, 0.25f * Scale.MaxElement(), drawColor, DebugDrawDepth.Always);
        }

        public override SpruePiece Clone()
        {
            var ret = new MarkerPiece();
            CloneInto(ret);
            ret.ShowForward = ShowForward;
            return ret;
        }
    }
}
