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
    public partial class FunctionNode : ShaderGenNode
    {
        public override bool EmitsCode()
        {
            return OutputSockets[0].DownstreamConnectionCount() > 1;
        }
    }
}

namespace SprueKit.Data.ShaderGen
{
    public partial class RadiansNode : FunctionNode
    {
        string funcName_;
        public RadiansNode() { funcName_ = "radians"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Radians";
            AddInput("value", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class DegreesNode : FunctionNode
    {
        string funcName_;
        public DegreesNode() { funcName_ = "degrees"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Degrees";
            AddInput("value", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class CosNode : FunctionNode
    {
        string funcName_;
        public CosNode() { funcName_ = "cos"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Cosine";
            AddInput("value", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class AcosNode : FunctionNode
    {
        string funcName_;
        public AcosNode() { funcName_ = "acos"; }
        public override void Construct()
        {
            base.Construct();
            Name = "ArcCosine";
            AddInput("value", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class SinNode : FunctionNode
    {
        string funcName_;
        public SinNode() { funcName_ = "sin"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Sine";
            AddInput("value", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class AsinNode : FunctionNode
    {
        string funcName_;
        public AsinNode() { funcName_ = "asin"; }
        public override void Construct()
        {
            base.Construct();
            Name = "ArcSine";
            AddInput("value", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class TanNode : FunctionNode
    {
        string funcName_;
        public TanNode() { funcName_ = "tan"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Tang";
            AddInput("value", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class AtanNode : FunctionNode
    {
        string funcName_;
        public AtanNode() { funcName_ = "atan"; }
        public override void Construct()
        {
            base.Construct();
            Name = "ArcTan";
            AddInput("value", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class Atan2Node : FunctionNode
    {
        string funcName_;
        public Atan2Node() { funcName_ = "atan2"; }
        public override void Construct()
        {
            base.Construct();
            Name = "ArcTan2";
            AddInput("y", SocketID.AnyMath);
            AddInput("y", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class CoshNode : FunctionNode
    {
        string funcName_;
        public CoshNode() { funcName_ = "cosh"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Hyperbolic Cosine";
            AddInput("value", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class SinhNode : FunctionNode
    {
        string funcName_;
        public SinhNode() { funcName_ = "sinh"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Hypberbolic Sine";
            AddInput("value", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class TanhNode : FunctionNode
    {
        string funcName_;
        public TanhNode() { funcName_ = "tanh"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Hypberbolic Tan";
            AddInput("value", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class PowNode : FunctionNode
    {
        string funcName_;
        public PowNode() { funcName_ = "pow"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Pow";
            AddInput("x", SocketID.AnyMath);
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class ExpNode : FunctionNode
    {
        string funcName_;
        public ExpNode() { funcName_ = "exp"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Exp";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class Exp2Node : FunctionNode
    {
        string funcName_;
        public Exp2Node() { funcName_ = "exp2"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Exp2";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class LogNode : FunctionNode
    {
        string funcName_;
        public LogNode() { funcName_ = "log"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Log";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class Log2Node : FunctionNode
    {
        string funcName_;
        public Log2Node() { funcName_ = "log2"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Log2";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class SqrtNode : FunctionNode
    {
        string funcName_;
        public SqrtNode() { funcName_ = "sqrt"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Sqrt";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class RsqrtNode : FunctionNode
    {
        string funcName_;
        public RsqrtNode() { funcName_ = "rsqrt"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Rsqrt";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class AbsNode : FunctionNode
    {
        string funcName_;
        public AbsNode() { funcName_ = "abs"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Abs";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class SignNode : FunctionNode
    {
        string funcName_;
        public SignNode() { funcName_ = "sign"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Sign";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class FloorNode : FunctionNode
    {
        string funcName_;
        public FloorNode() { funcName_ = "floor"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Floor";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class CeilNode : FunctionNode
    {
        string funcName_;
        public CeilNode() { funcName_ = "ceil"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Ceil";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class FracNode : FunctionNode
    {
        string funcName_;
        public FracNode() { funcName_ = "frac"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Frac";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class FmodNode : FunctionNode
    {
        string funcName_;
        public FmodNode() { funcName_ = "fmod"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Fmod";
            AddInput("x", SocketID.AnyMath);
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class RoundNode : FunctionNode
    {
        string funcName_;
        public RoundNode() { funcName_ = "round"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Round";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class MinNode : FunctionNode
    {
        string funcName_;
        public MinNode() { funcName_ = "min"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Min";
            AddInput("x", SocketID.AnyMath);
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class MaxNode : FunctionNode
    {
        string funcName_;
        public MaxNode() { funcName_ = "max"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Max";
            AddInput("x", SocketID.AnyMath);
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class ClampNode : FunctionNode
    {
        string funcName_;
        public ClampNode() { funcName_ = "clamp"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Clamp";
            AddInput("val", SocketID.AnyMath);
            AddInput("val", SocketID.AnyMath);
            AddInput("val", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            var val2 = InputSockets[2].GetSocketInfo();
            return string.Format("{0}({1}, {2}, {3})", funcName_, val0.Value, val1.Value, val2.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class LerpNode : FunctionNode
    {
        string funcName_;
        public LerpNode() { funcName_ = "lerp"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Lerp";
            AddInput("a", SocketID.AnyMath);
            AddInput("a", SocketID.AnyMath);
            AddInput("a", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            var val2 = InputSockets[2].GetSocketInfo();
            return string.Format("{0}({1}, {2}, {3})", funcName_, val0.Value, val1.Value, val2.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class StepNode : FunctionNode
    {
        string funcName_;
        public StepNode() { funcName_ = "step"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Step";
            AddInput("a", SocketID.AnyMath);
            AddInput("a", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class SmoothstepNode : FunctionNode
    {
        string funcName_;
        public SmoothstepNode() { funcName_ = "smoothstep"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Smoothstep";
            AddInput("edgeA", SocketID.AnyMath);
            AddInput("edgeA", SocketID.AnyMath);
            AddInput("edgeA", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            var val2 = InputSockets[2].GetSocketInfo();
            return string.Format("{0}({1}, {2}, {3})", funcName_, val0.Value, val1.Value, val2.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class TruncNode : FunctionNode
    {
        string funcName_;
        public TruncNode() { funcName_ = "trunc"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Truncate";
            AddInput("v", SocketID.AnyVector);
            AddOutput("Out", SocketID.AnyVector);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class LengthNode : FunctionNode
    {
        string funcName_;
        public LengthNode() { funcName_ = "length"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Length";
            AddInput("x", SocketID.AnyMath);
            AddOutput("Out", SocketID.Float);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class DistanceNode : FunctionNode
    {
        string funcName_;
        public DistanceNode() { funcName_ = "distance"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Distance";
            AddInput("p0", SocketID.AnyMath);
            AddInput("p0", SocketID.AnyMath);
            AddOutput("Out", SocketID.Float);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class DstNode : FunctionNode
    {
        string funcName_;
        public DstNode() { funcName_ = "dst"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Dst";
            AddInput("a", SocketID.AnyVector);
            AddInput("a", SocketID.AnyVector);
            AddOutput("Out", SocketID.AnyVector);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class DotNode : FunctionNode
    {
        string funcName_;
        public DotNode() { funcName_ = "dot"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Dot";
            AddInput("x", SocketID.AnyScalar);
            AddInput("x", SocketID.AnyScalar);
            AddOutput("Out", SocketID.Float);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class CrossNode : FunctionNode
    {
        string funcName_;
        public CrossNode() { funcName_ = "cross"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Cross";
            AddInput("x", SocketID.Float3);
            AddInput("x", SocketID.Float3);
            AddOutput("Out", SocketID.Float3);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(val.Key, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[OutputSockets[0].TypeID], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class NormalizeNode : FunctionNode
    {
        string funcName_;
        public NormalizeNode() { funcName_ = "normalize"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Normalize";
            AddInput("v", SocketID.AnyVector);
            AddOutput("Out", SocketID.AnyVector);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class DeterminantNode : FunctionNode
    {
        string funcName_;
        public DeterminantNode() { funcName_ = "determinant"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Determinant";
            AddInput("m", SocketID.AnyMatrix);
            AddOutput("Out", SocketID.Float);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class TransposeNode : FunctionNode
    {
        string funcName_;
        public TransposeNode() { funcName_ = "transpose"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Transpose";
            AddInput("m", SocketID.AnyMatrix);
            AddOutput("Out", SocketID.AnyMatrix);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class FaceforwardNode : FunctionNode
    {
        string funcName_;
        public FaceforwardNode() { funcName_ = "faceforward"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Face Forward";
            AddInput("N", SocketID.AnyScalar);
            AddInput("N", SocketID.AnyScalar);
            AddInput("N", SocketID.AnyScalar);
            AddOutput("Out", SocketID.AnyScalar);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            var val2 = InputSockets[2].GetSocketInfo();
            return string.Format("{0}({1}, {2}, {3})", funcName_, val0.Value, val1.Value, val2.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class ReflectNode : FunctionNode
    {
        string funcName_;
        public ReflectNode() { funcName_ = "reflect"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Reflect";
            AddInput("I", SocketID.AnyScalar);
            AddInput("I", SocketID.AnyScalar);
            AddOutput("Out", SocketID.AnyScalar);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class RefractNode : FunctionNode
    {
        string funcName_;
        public RefractNode() { funcName_ = "refract"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Refract";
            AddInput("I", SocketID.AnyScalar);
            AddInput("I", SocketID.AnyScalar);
            AddInput("I", SocketID.AnyScalar);
            AddOutput("Out", SocketID.AnyScalar);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            var val2 = InputSockets[2].GetSocketInfo();
            return string.Format("{0}({1}, {2}, {3})", funcName_, val0.Value, val1.Value, val2.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class DdxNode : FunctionNode
    {
        string funcName_;
        public DdxNode() { funcName_ = "ddx"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Ddx";
            AddInput("coord", SocketID.AnyScalar);
            AddOutput("Out", SocketID.Float);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class DdyNode : FunctionNode
    {
        string funcName_;
        public DdyNode() { funcName_ = "ddy"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Ddy";
            AddInput("coord", SocketID.AnyScalar);
            AddOutput("Out", SocketID.Float);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class IsinfiniteNode : FunctionNode
    {
        string funcName_;
        public IsinfiniteNode() { funcName_ = "isinfinite"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Is Infinite";
            AddInput("value", SocketID.AnyScalar);
            AddOutput("Out", SocketID.Bool);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class IsnanNode : FunctionNode
    {
        string funcName_;
        public IsnanNode() { funcName_ = "isnan"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Is Not a Number";
            AddInput("value", SocketID.AnyScalar);
            AddOutput("Out", SocketID.Bool);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class SaturateNode : FunctionNode
    {
        string funcName_;
        public SaturateNode() { funcName_ = "saturate"; }
        public override void Construct()
        {
            base.Construct();
            Name = "Saturate";
            AddInput("value", SocketID.AnyScalar);
            AddOutput("Out", SocketID.AnyScalar);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            return string.Format("{0}({1})", funcName_, val0.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {

        }
    }
    public partial class GetSpecularIBLNode : FunctionNode
    {
        string funcName_;
        public GetSpecularIBLNode() { funcName_ = "GetSpecularIBL"; }
        public override void Construct()
        {
            base.Construct();
            Name = "GetSpecularIBL";
            AddInput("val", SocketID.Float3);
            AddInput("val", SocketID.Float3);
            AddInput("val", SocketID.Float3);
            AddOutput("Out", SocketID.Float3);

        }
        public override void Execute(object param)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) { IsValid = false; return; }
            var val = InputSockets[0].GetSocketInfo();
            if (EmitsCode())
                OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
            else
                OutputSockets[0].StoreSocketInfo(val.Key, GetCode());
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
                if (!InputSockets[i].HasConnections()) return;
            var firstVal = InputSockets[0].GetSocketInfo();
            compiler.WriteFormat("{0} {1} = ({2});", Tables.inst().IDMapping[OutputSockets[0].TypeID], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());
        }
        string GetCode()
        {
            var val0 = InputSockets[0].GetSocketInfo();
            var val1 = InputSockets[1].GetSocketInfo();
            return string.Format("{0}({1}, {2})", funcName_, val0.Value, val1.Value);
        }
        public override void EmitHeaderBlock(ShaderCompiler compiler)
        {
            compiler.WriteRaw("float3 GetSpecularIBL(float3 val, float3 oVal) {\r\n");
            compiler.WriteRaw("    return val * oVal + distance(val, oVal);\r\n");
            compiler.WriteRaw("}\\r");
        }
    }
}
