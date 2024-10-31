using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace PluginLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IntVector2
    {
        public int X;
        public int Y;

        public IntVector2(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IntVector4
    {
        public int X;
        public int Y;
        public int Z;
        public int W;

        public IntVector4(int x, int y, int z, int w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ShortVector4
    {
        public short X;
        public short Y;
        public short Z;
        public short W;

        public static Short4 FromIntVector4(IntVector4 v)
        {
            return new Short4(v.X, v.Y, v.Z, v.W);
        }

        public static Short4 Invalid = new Short4(-1, -1, -1, -1);
    }

    public class Mat3x3
    {
        public float[,] m = new float[3, 3] { { 1.0f, 0.0f, 0.0f }, { 0.0f, 1.0f, 0.0f }, { 0.0f, 0.0f, 1.0f } };
    }

    public interface IModelData
    {

        List<IMeshData> MeshData { get; }

        /// <summary>
        /// Can be null/empty.
        /// </summary>
        String[] BoneNames { get; }

        /// <summary>
        /// Must be a 4x4 matrix
        /// </summary>
        Matrix[] BoneTransforms { get; }
    }

    public interface IMeshData
    {
        GeometryData Geometry { get; }
        List<MorphData> MorphTargets { get; }
    }

    [Description("Raw representation of geometry data")]
    public class GeometryData
    {
        public Vector3[] Positions;
        public Vector3[] Normals;
        public Vector4[] Tangents;
        public Vector2[] UVCoordinates;

        [Description("FUTURE: Currently unused")]
        public Vector2[] LightmapUVCoordinates;

        [Description("Colors are not required to be present")]
        public Color[] Colors;

        [Description("Only present if there are bones to be weighted to. Weights correspond to bone-indices below")]
        public Vector4[] BoneWeights;

        [Description("Bone indices for the above weights")]
        public IntVector4[] BoneIndices;

        [Description("Triangle indices in Triangle-list form")]
        public int[] Indices;

        [Description("If not null the Position, Normal, etc data should be used from the Source instead (multiple index buffers for shared vertex buffers)")]
        public GeometryData VertexSource;
    }

    [Description("Contains data used for a morph-target, bitangents are assumed to be calculated as needed")]
    public class MorphData
    {
        [Description("Identifier of this morph target")]
        public string Name;

        [Description("Target indices for each record of morph data")]
        public int[] Indices;

        [Description("Positions to lerp to")]
        public Vector3[] Positions;

        [Description("Vertex normal to blend to")]
        public Vector3[] Normals;

        [Description("Tangent for above normal to blend to, only XYZ are blended")]
        public Vector4[] Tangents;
    }

    [Flags]
    public enum JointMechanics
    {
        JM_None = 0,
        JM_Ball = 1,
        JM_Hinge = 1 << 1,         // like a knee
        JM_Drag = 1 << 2,          // rotates away from velocity
        JM_Anticipates = 1 << 3,   // rotates toward velocity
        JM_Jiggle = 1 << 4         // Uses faux-physics jiggle
    }

    public class JointData
    {
        public SkeletonData Skeleton { get; set; }
        public int Index { get; private set; }
        public string Name { get; set; }
        public uint Flags { get; set; }
        public uint Capabilities { get; set; }
        public JointData Parent { get; set; }
        public JointData SourceJoint { get; set; }
        public List<JointData> Children { get; private set; } = new List<JointData>();

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public Vector3 LocalPosition { get { return Parent != null ? Vector3.Transform(Position, Parent.InverseTransform) : Position; } }

        public List<object> Attachments { get; private set; } = new List<object>();

        public void SetIndex(int idx) { Index = idx; }

        public JointData Duplicate()
        {
            return new JointData { Name = this.Name, Flags = this.Flags, Capabilities = this.Capabilities, Position = this.Position, Rotation = this.Rotation, Scale = this.Scale };
        }

        public Vector3 ModelSpacePosition
        {
            get
            {
                return Vector3.Transform(Position, ModelSpaceTransform);
            }
        }

        public Quaternion ModelSpaceRotation
        {
            get
            {
                return ModelSpaceTransform.Rotation * Rotation;
            }
        }

        public Vector3 ModelSpaceScale
        {
            get
            {
                return ModelSpaceTransform.Scale * Scale;
            }
        }

        public Matrix ModelSpaceTransform
        {
            get
            {
                if (Parent != null)
                    return Matrix.Invert(Parent.ModelSpaceTransform) * Transform;
                return Transform;
            }
        }

        public Matrix InverseTransform { get { return Matrix.Invert(Transform); } }

        public Matrix Transform
        {
            get
            {
                return Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
            }
        }

        public bool HasChildren { get { return Children.Count > 0; } }
    }

    public class SkeletonData
    {
        public JointData Root { get; set; }
        public List<JointData> Inline { get; private set; } = new List<JointData>();

        public void AddJoint(JointData parent, JointData child)
        {
            if (parent == null)
                Root = child;
            else
            {
                parent.Children.Add(child);
                child.Parent = parent;
            }

            child.Skeleton = this;
            child.SetIndex(Inline.Count);
            Inline.Add(child);
        }
    }

    [Description("Interface for loading models")]
    public interface IModelImporter
    {
        [Description("Implementation should set the the ref parameters to the appropriate values, ie: 'FBX' and '*.fbx'")]
        void GetImportSpecification(
            ref string modelTypeName, 
            ref string fileReadMask);

        [Description("Implementation performs the appropriate reading of model data from the given Uri")]
        IModelData ImportModel(
            Uri filePath, 
            IErrorPublisher reporter);
    }

    [Description("Interface for exporting models")]
    public interface IModelExporter
    {
        [Description("Implementation should set the the ref parameters to the appropriate values, ie: 'FBX' and '*.fbx'")]
        void GetExportSpecification(
            ref string modelTypeName, 
            ref string fileWriteMask);

        [Description("Implementation writes the modeldata in this format to the given Uri. Return true if successful")]
        bool ExportModel(
            IModelData data, 
            IErrorPublisher reporter);
    }

    [Description("Model filters can be applied to a set of geometry data to perform custom actions on the data")]
    public interface IModelFilter
    {
        [Description("Process the given model data. Return true if the data has been changed.")]
        bool FilterMesh(
            IModelData data, 
            IErrorPublisher reporter);
    }
}
