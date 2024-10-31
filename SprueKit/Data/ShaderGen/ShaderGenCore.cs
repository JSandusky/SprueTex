using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Matrix = Microsoft.Xna.Framework.Matrix;
using SprueKit.Data.Graph;

namespace SprueKit.Data.ShaderGen
{
    public static class SocketID
    {
        public static readonly uint Int = 1 << 0;
        public static readonly uint Float = 1 << 1;
        public static readonly uint Float2 = 1 << 2;
        public static readonly uint Float3 = 1 << 3;
        public static readonly uint Float4 = 1 << 4;
        public static readonly uint AnyFloatVector = (1 << 2) | (1 << 3) | (1 << 4);
        public static readonly uint Float3x3 = 1 << 5;
        public static readonly uint Float4x4 = 1 << 6;
        public static readonly uint AnyMatrix = (1 << 5) | (1 << 6);
        public static readonly uint AnyScalar = (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4);
        public static readonly uint AnyMath = (1) | (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4) | (1 << 5) | (1 << 6);
        public static readonly uint Sampler1D = 1 << 7;
        public static readonly uint Sampler2D = 1 << 8;
        public static readonly uint Sampler3D = 1 << 9;
        public static readonly uint SamplerCube = 1 << 10;
        public static readonly uint Texture1D = 1 << 11;
        public static readonly uint Texture2D = 1 << 12;
        public static readonly uint Texture3D = 1 << 13;
        public static readonly uint TextureCube = 1 << 14;
        public static readonly uint Int2 = 1 << 15;
        public static readonly uint Int3 = 1 << 16;
        public static readonly uint Int4 = 1 << 17;
        public static readonly uint AnyIntVector = (1 << 15) | (1 << 16) | (1 << 16);
        public static readonly uint AnyVector = (1 << 2) | (1 << 3) | (1 << 4) | (1 << 15) | (1 << 16) | (1 << 16);
        public static readonly uint Bool = 1 << 18; // no support for bool2, bool3, or bool4 ... approaching insanity

        public static void StoreSocketInfo(this Graph.GraphSocket socket, uint id, string text)
        {
            socket.Data = new KeyValuePair<uint, string>(id, text);
        }
        public static KeyValuePair<uint, string> GetSocketInfo(this Graph.GraphSocket socket)
        {
            return (KeyValuePair<uint, string>)socket.Data;
        }
    }

    public class ShaderGenNode : Graph.GraphNode
    {
        bool isValid_ = true;
        public bool IsValid { get { return isValid_; }set { isValid_ = value;OnPropertyChanged(); } }

        public virtual bool EmitsCode() { return false; }

        /// <summary>
        /// Write out anything that should come at the very beginning
        /// </summary>
        public virtual void EmitUniformBlock(ShaderCompiler compiler) { }
        /// <summary>
        /// Output anything that should come afterwards, function definitions mostly
        /// </summary>
        public virtual void EmitHeaderBlock(ShaderCompiler compiler) { }
        /// <summary>
        /// Write out code for this node
        /// </summary>
        public virtual void EmitCode(ShaderCompiler compiler) { }
        /// <summary>
        /// Called to produce the output that is written when this node is referenced, should match anything produced in 'EmitCode'
        /// </summary>
        public virtual string GetResultCode() { return string.Empty; }

        /// <summary>
        /// Get at automatic default name for a variable
        /// </summary>
        public virtual string GetSocketName(Graph.GraphSocket socket, CompilerStage stage)
        {
            // if we don't have a valid name
            if (string.IsNullOrWhiteSpace(Name))
                return string.Format("{0}{1}_{2}", GetType().Name, NodeID, socket.Name.ToLowerInvariant());
            return string.Format("{0}{1}_{2}", Name.Replace(' ', '_'), NodeID, socket.Name.ToLowerInvariant());
        }

        public void AddInput(string name, uint type) { AddInput(new GraphSocket(this) { Name = name, TypeID = type });}
        public void AddOutput(string name, uint type) { AddOutput(new GraphSocket(this) { Name = name, TypeID = type, IsInput =false,IsOutput=true }); }
    }

    /// <summary>
    /// Base class for all uniform nodes, this is important as we need to be able to find them
    /// </summary>
    public abstract class UniformNode : ShaderGenNode
    {
        public override void Construct() { base.Construct(); }
    }

    public partial class IntUniformNode : UniformNode
    {
        int defaultValue_ = 0;
        public int DefaultValue { get { return defaultValue_; } set { defaultValue_ = value; OnPropertyChanged(); } }
        public IntUniformNode() { }
        public override void Construct()
        {
            base.Construct(); Name = "IntUniform";
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketID.Float });
        }
        public override void EmitUniformBlock(ShaderCompiler compiler) { compiler.Write(string.Format("int {0} = {1};", Name, DefaultValue)); }
    }

    public partial class FloatUniformNode : UniformNode
    {
        float defaultValue_ = 0.0f;
        public float DefaultValue { get { return defaultValue_; } set { defaultValue_ = value; OnPropertyChanged(); } }
        public FloatUniformNode() {  }
        public override void Construct()
        {
            base.Construct(); Name = "FloatUniform";
            AddOutput("Out", SocketID.Float);
        }
        public override void EmitUniformBlock(ShaderCompiler compiler) { compiler.Write(string.Format("float {0} = {1};", Name.ToShaderString(), DefaultValue)); }
        public override void Execute(object param) { OutputSockets[0].StoreSocketInfo(SocketID.Float, Name.ToShaderString()); }
    }

    public partial class Float2UniformNode : UniformNode
    {
        Vector2 defaultValue_ = Vector2.Zero;
        public Vector2 DefaultValue { get { return defaultValue_; } set { defaultValue_ = value; OnPropertyChanged(); } }
        public Float2UniformNode() { }
        public override void Construct()
        {
            base.Construct(); Name = "Float2Uniform";
            AddOutput("Out", SocketID.Float2);
        }
        public override void EmitUniformBlock(ShaderCompiler compiler) { compiler.Write(string.Format("float2 {0} = {1};", Name.ToShaderString(), DefaultValue.ToShaderString())); }
        public override void Execute(object param) { OutputSockets[0].StoreSocketInfo(SocketID.Float2, Name.ToShaderString()); }
    }

    public partial class Float3UniformNode : UniformNode
    {
        Vector3 defaultValue_ = Vector3.Zero;
        public Vector3 DefaultValue { get { return defaultValue_; } set { defaultValue_ = value; OnPropertyChanged(); } }
        public Float3UniformNode() { }
        public override void Construct()
        {
            base.Construct(); Name = "Float3Uniform";
            AddOutput("Out", SocketID.Float3);
        }
        public override void EmitUniformBlock(ShaderCompiler compiler) { compiler.Write(string.Format("float3 {0} = {1};", Name.ToShaderString(), DefaultValue.ToShaderString())); }
        public override void Execute(object param) { OutputSockets[0].StoreSocketInfo(SocketID.Float3, Name.ToShaderString()); }
    }

    public partial class Float4UniformNode : UniformNode
    {
        Vector4 defaultValue_ = Vector4.Zero;
        public Vector4 DefaultValue { get { return defaultValue_; } set { defaultValue_ = value; OnPropertyChanged(); } }
        public Float4UniformNode() { }
        public override void Construct()
        {
            base.Construct(); Name = "Float4Uniform";
            AddOutput("Out", SocketID.Float4);
        }
        public override void EmitUniformBlock(ShaderCompiler compiler) { compiler.Write(string.Format("float4 {0} = {1};", Name.ToShaderString(), DefaultValue.ToShaderString())); }
        public override void Execute(object param) { OutputSockets[0].StoreSocketInfo(SocketID.Float4, Name.ToShaderString()); }
    }

    public partial class MatrixUniformNode : UniformNode
    {
        Matrix defaultValue_ = Matrix.Identity;
        public Matrix DefaultValue { get { return defaultValue_; } set { defaultValue_ = value; OnPropertyChanged(); } }
        public MatrixUniformNode() { }
        public override void Construct()
        {
            base.Construct(); Name = "MatrixUniform";
            AddOutput("Out", SocketID.Float4x4);
        }
        public override void EmitUniformBlock(ShaderCompiler compiler) { compiler.Write(string.Format("matrix {0} = {{ {1} }};", Name, DefaultValue)); }
        public override void Execute(object param) { OutputSockets[0].StoreSocketInfo(SocketID.Float4x4, Name); }
    }

    public partial class Texture1DUniform : UniformNode
    {
        static readonly string CODE =
@"Texture1D {0};

sampler1D {0}_Sampler = sampler_state {{
    Texture = <{0}>;
    AddressU = {1};
    MipFilter = {2};
    MinFilter = {3};
    MagFilter = {4};
}}";

        Microsoft.Xna.Framework.Graphics.TextureFilter mipFilter_ = Microsoft.Xna.Framework.Graphics.TextureFilter.Linear;
        public Microsoft.Xna.Framework.Graphics.TextureFilter MipFilter { get { return mipFilter_; } set { mipFilter_ = value; OnPropertyChanged(); } }
        Microsoft.Xna.Framework.Graphics.TextureFilter minFilter_ = Microsoft.Xna.Framework.Graphics.TextureFilter.Linear;
        public Microsoft.Xna.Framework.Graphics.TextureFilter MinFilter { get { return minFilter_; } set { minFilter_ = value; OnPropertyChanged(); } }
        Microsoft.Xna.Framework.Graphics.TextureFilter magFilter_ = Microsoft.Xna.Framework.Graphics.TextureFilter.Linear;
        public Microsoft.Xna.Framework.Graphics.TextureFilter MagFilter { get { return magFilter_; } set { magFilter_ = value; OnPropertyChanged(); } }

        Microsoft.Xna.Framework.Graphics.TextureAddressMode addressU_ = Microsoft.Xna.Framework.Graphics.TextureAddressMode.Wrap;
        public Microsoft.Xna.Framework.Graphics.TextureAddressMode AddressU { get { return addressU_; } set { addressU_ = value; OnPropertyChanged(); } }

        public Texture1DUniform() { }
        public override void Construct()
        {
            base.Construct(); Name = "tex1DUni";
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketID.Texture1D });
        }
        public override void EmitUniformBlock(ShaderCompiler compiler) {
            compiler.Write(string.Format(CODE, Name, AddressU, MipFilter, MinFilter, MagFilter));
        }
    }

    public partial class Texture2DUniform : UniformNode
    {
        static readonly string CODE =
@"Texture2D {0};

sampler2D {0}_Sampler = sampler_state {{
    Texture = <{0}>;
    AddressU = {1};
    AddressV = {2};
    MipFilter = {3};
    MinFilter = {4};
    MagFilter = {5};
}};";

        Microsoft.Xna.Framework.Graphics.TextureFilter mipFilter_ = Microsoft.Xna.Framework.Graphics.TextureFilter.Linear;
        public Microsoft.Xna.Framework.Graphics.TextureFilter MipFilter { get { return mipFilter_; } set { mipFilter_ = value; OnPropertyChanged(); } }
        Microsoft.Xna.Framework.Graphics.TextureFilter minFilter_ = Microsoft.Xna.Framework.Graphics.TextureFilter.Linear;
        public Microsoft.Xna.Framework.Graphics.TextureFilter MinFilter { get { return minFilter_; } set { minFilter_ = value; OnPropertyChanged(); } }
        Microsoft.Xna.Framework.Graphics.TextureFilter magFilter_ = Microsoft.Xna.Framework.Graphics.TextureFilter.Linear;
        public Microsoft.Xna.Framework.Graphics.TextureFilter MagFilter { get { return magFilter_; } set { magFilter_ = value; OnPropertyChanged(); } }

        Microsoft.Xna.Framework.Graphics.TextureAddressMode addressU_ = Microsoft.Xna.Framework.Graphics.TextureAddressMode.Wrap;
        Microsoft.Xna.Framework.Graphics.TextureAddressMode addressV_ = Microsoft.Xna.Framework.Graphics.TextureAddressMode.Wrap;
        public Microsoft.Xna.Framework.Graphics.TextureAddressMode AddressU { get { return addressU_; } set { addressU_ = value; OnPropertyChanged(); } }
        public Microsoft.Xna.Framework.Graphics.TextureAddressMode AddressV { get { return addressV_; } set { addressV_ = value; OnPropertyChanged(); } }

        public Texture2DUniform() { }
        public override void Construct()
        {
            base.Construct(); Name = "tex2DUni";
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketID.Texture1D });
        }
        public override void EmitUniformBlock(ShaderCompiler compiler) {
            compiler.Write(string.Format(CODE, Name, AddressU, AddressV, MipFilter, MinFilter, MagFilter));
        }
    }

    public partial class Texture3DUniform : UniformNode
    {
        static readonly string CODE =
@"Texture3D {0};

sampler3D {0}_Sampler = sampler_state {{
    Texture = <{0}>;
    AddressU = {1};
    AddressV = {2};
    AddressW = {3};
    MipFilter = {4};
    MinFilter = {5};
    MagFilter = {6};
}};";

        Microsoft.Xna.Framework.Graphics.TextureFilter mipFilter_ = Microsoft.Xna.Framework.Graphics.TextureFilter.Linear;
        public Microsoft.Xna.Framework.Graphics.TextureFilter MipFilter { get { return mipFilter_; } set { mipFilter_ = value; OnPropertyChanged(); } }
        Microsoft.Xna.Framework.Graphics.TextureFilter minFilter_ = Microsoft.Xna.Framework.Graphics.TextureFilter.Linear;
        public Microsoft.Xna.Framework.Graphics.TextureFilter MinFilter { get { return minFilter_; } set { minFilter_ = value; OnPropertyChanged(); } }
        Microsoft.Xna.Framework.Graphics.TextureFilter magFilter_ = Microsoft.Xna.Framework.Graphics.TextureFilter.Linear;
        public Microsoft.Xna.Framework.Graphics.TextureFilter MagFilter { get { return magFilter_; } set { magFilter_ = value; OnPropertyChanged(); } }

        Microsoft.Xna.Framework.Graphics.TextureAddressMode addressU_ = Microsoft.Xna.Framework.Graphics.TextureAddressMode.Wrap;
        Microsoft.Xna.Framework.Graphics.TextureAddressMode addressV_ = Microsoft.Xna.Framework.Graphics.TextureAddressMode.Wrap;
        Microsoft.Xna.Framework.Graphics.TextureAddressMode addressW_ = Microsoft.Xna.Framework.Graphics.TextureAddressMode.Wrap;
        public Microsoft.Xna.Framework.Graphics.TextureAddressMode AddressU { get { return addressU_; } set { addressU_ = value; OnPropertyChanged(); } }
        public Microsoft.Xna.Framework.Graphics.TextureAddressMode AddressV { get { return addressV_; } set { addressV_ = value; OnPropertyChanged(); } }
        public Microsoft.Xna.Framework.Graphics.TextureAddressMode AddressW { get { return addressW_; } set { addressW_ = value; OnPropertyChanged(); } }

        public Texture3DUniform() { }
        public override void Construct()
        {
            base.Construct(); Name = "tex3DUni";
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketID.Texture1D });
        }
        public override void EmitUniformBlock(ShaderCompiler compiler)
        {
            compiler.Write(string.Format(CODE, Name, AddressU, AddressV, AddressW, MipFilter, MinFilter, MagFilter));
        }
    }

    public partial class TextureCubeUniform : ShaderGenNode
    {
        public TextureCubeUniform() { }
        public override void Construct()
        {
            base.Construct(); Name = "texCubeUni";
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketID.Texture1D });
        }
        public override void EmitUniformBlock(ShaderCompiler compiler) { compiler.Write(string.Format("TextureCube {0};", Name)); }
    }

    public partial class TimeUniform : UniformNode
    {
        public TimeUniform() { }
        public override void Construct() {
            base.Construct(); Name = "Time";
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", TypeID = SocketID.Float });
        }
        public override void EmitUniformBlock(ShaderCompiler compiler) { compiler.Write(string.Format("float Time;", Name)); }
        public override string GetSocketName(GraphSocket socket, CompilerStage stage) { return "Time"; }
    }
}
