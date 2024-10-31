using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Matrix = Microsoft.Xna.Framework.Matrix;
using SprueKit.Data.ShaderGen;

namespace SprueKit.Data.ShaderGen
{
    #region Utility constants

    public partial class ConstantNode : ShaderGenNode
    {
        string text_;
        uint outputType_ = 0;
        string name_;
        protected ConstantNode(uint outputType, string name, string text)
        {
            outputType_ = outputType;
            text_ = text;
            name_ = name;
        }

        public override bool EmitsCode() { return true; }

        public override void Construct()
        {
            base.Construct();
            Name = name_;
            AddOutput("Value", outputType_);
        }

        public override void EmitUniformBlock(ShaderCompiler compiler)
        {
            compiler.WriteFormat("#define {0} {1}", name_.ToShaderString(), text_);
        }

        public override string GetResultCode() { return name_; }
        public override void Execute(object param) { OutputSockets[0].Data = new KeyValuePair<uint, string>(SocketID.Float, name_.ToShaderString()); }
    }

    public partial class PiNode : ConstantNode
    {
        public PiNode() : base(SocketID.Float, "PI", "3.141593") { }
    }

    public partial class TauNode : ConstantNode
    {
        public TauNode() : base(SocketID.Float, "Tau", "6.283185") { }
    }

    public partial class PhiNode : ConstantNode
    {
        public PhiNode() : base(SocketID.Float, "Phi", "1.618034") { }
    }

    public partial class Root2Node : ConstantNode
    {
        public Root2Node() : base(SocketID.Float, "Root2", "1.414214") { }
    }

    public partial class EulersConstantNode : ConstantNode
    {
        public EulersConstantNode() : base(SocketID.Float, "Eulers", "2.718282") { }
    }

    #endregion

    #region Custom constants

    public abstract class SimpleConstant<T> : ShaderGenNode
    {
        T value_ = default(T);
        public T Value { get { return value_; } set { value_ = value; OnPropertyChanged(); } }

        public SimpleConstant() { }
        public override void EmitUniformBlock(ShaderCompiler compiler) { compiler.WriteFormat("#define {0} {1}", Name.ToShaderString(), ValueText()); }
        protected abstract string ValueText();
    }

    public partial class IntConstant : SimpleConstant<int>
    {
        public IntConstant() { }
        public override void Construct()
        {
            base.Construct();
            Name = "New Constant Int";
            AddOutput("Out", SocketID.Int);
        }
        protected override string ValueText() { return Value.ToShaderString(); }
        public override void Execute(object param) { OutputSockets[0].Data = new KeyValuePair<uint, string>(SocketID.Int, Name.ToShaderString()); }
    }

    public partial class FloatConstant : SimpleConstant<float>
    {
        public FloatConstant() { }
        public override void Construct()
        {
            base.Construct();
            Name = "New Constant Float";
            AddOutput("Out", SocketID.Float);
        }
        protected override string ValueText() { return Value.ToShaderString(); }
        public override void Execute(object param) { OutputSockets[0].Data = new KeyValuePair<uint, string>(SocketID.Float, Name.ToShaderString()); }
    }

    public partial class Float2Constant : SimpleConstant<Vector2>
    {
        public Float2Constant() { }
        public override void Construct()
        {
            base.Construct();
            Name = "New Constant float2";
            AddOutput("Out", SocketID.Float2);
        }
        protected override string ValueText() { return Value.ToShaderString(); }
        public override void Execute(object param) { OutputSockets[0].Data = new KeyValuePair<uint, string>(SocketID.Float2, Name.ToShaderString()); }
    }

    public partial class Float3Constant : SimpleConstant<Vector3>
    {
        public Float3Constant() { }
        public override void Construct()
        {
            base.Construct();
            Name = "New Constant float3";
            AddOutput("Out", SocketID.Float3);
        }
        protected override string ValueText() { return Value.ToShaderString(); }
        public override void Execute(object param) { OutputSockets[0].Data = new KeyValuePair<uint, string>(SocketID.Float3, Name.ToShaderString()); }
    }

    public partial class Float4Constant : SimpleConstant<Vector4>
    {
        public Float4Constant() { }
        public override void Construct()
        {
            base.Construct();
            Name = "New Constant float4";
            AddOutput("Out", SocketID.Float4);
        }
        protected override string ValueText() { return Value.ToShaderString(); }
        public override void Execute(object param) { OutputSockets[0].Data = new KeyValuePair<uint, string>(SocketID.Float4, Name.ToShaderString()); }
    }

    #endregion
}
