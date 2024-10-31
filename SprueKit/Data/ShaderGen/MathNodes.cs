using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.ShaderGen
{
    
    public class Tables
    {
        static Tables inst_;
        public static Tables inst() {
            if (inst_ == null)
                inst_ = new Tables();
            return inst_;
        }

        public string Conversion(KeyValuePair<uint, string> lhs, KeyValuePair<uint, string> rhs)
        {
            string lname = IDMapping[lhs.Key];
            string rname = IDMapping[rhs.Key];
            return string.Format(ConversionTable[rname][lname], rhs.Value);
        }

        public Dictionary<string, Dictionary<string, string >> ConversionTable = new Dictionary<string, Dictionary<string, string> >
        {
            { "float", new Dictionary<string, string>  {
                { "float", "{0}" },
                { "float2", "float2({0}, 0.0)" },
                { "float3", "float3({0}, 0.0, 0.0)" },
                { "float4", "float4({0}, 0.0, 0.0, 0.0)" },
            } },
            { "float2", new Dictionary<string, string> {
                { "float", "{0}.x" },
                { "float2", "{0}" },
                { "float3", "float3({0}.xy, 0.0)" },
                { "float4", "float4({0}.xy 0.0, 0.0)" },
            } },
            { "float3", new Dictionary<string, string> {
                { "float", "{0}.x" },
                { "float2", "{0}.xy" },
                { "float3", "{0}" },
                { "float4", "float4({0}.xyz, 0.0)" },
            } },
            { "float4", new Dictionary<string, string> {
                { "float", "{0}.x" },
                { "float2", "{0}.xy" },
                { "float3", "{0}.xyz" },
                { "float4", "{0}" },
            } },
        };

        public string[] Priority = new string[] {
            "int",
            "float",
            "float2",
            "float3",
            "float4",
            "float3x3",
            "float4x4",
        };

        public Dictionary<uint, string> IDMapping = new Dictionary<uint, string>
        {
            { 1,      "int" },
            { 1 << 1, "float" },
            { 1 << 2, "float2" },
            { 1 << 3, "float3" },
            { 1 << 4, "float4" },
            { 1 << 5, "float3x3" },
            { 1 << 6, "float4x4" },
        };

        public Dictionary<string, uint> InverseIDMapping = new Dictionary<string, uint>
        {
            { "int",      1 },
            { "float",    1 << 1 },
            { "float2",   1 << 2 },
            { "float3",   1 << 3 },
            { "float4",   1 << 4 },
            { "float3x3", 1 << 5 },
            { "float4x4", 1 << 6 },
        };

        // This table is used for automatically generating Node bindings based on functions
        public Dictionary<string, string> ToCodeMapping = new Dictionary<string, string>
        {
            { "bool",     "SocketID.Bool" },
            { "int",      "SocketID.Int" },
            { "float",    "SocketID.Float" },
            { "float2",   "SocketID.Float2" },
            { "float3",   "SocketID.Float3" },
            { "float4",   "SocketID.Float4" },
            { "float3x3", "SocketID.Float3x3" },
            { "float4x4", "SocketID.Float4x4" },
            { "scalar",   "SocketID.AnyScalar" },
            { "vector",   "SocketID.AnyVector" },
            { "vmath",     "SocketID.AnyMath" },
            { "matrix",    "SocketID.AnyMatrix" },
        };

        public string GetTarget(string lhs, string rhs)
        {
            if (Array.IndexOf(Priority, lhs) > Array.IndexOf(Priority, rhs))
                return lhs;
            return rhs;
        }

        public string GetTarget(uint lhsType, uint rhsType)
        {
            return GetTarget(IDMapping[lhsType], IDMapping[rhsType]);
        }
    }

    public class BinaryOp : ShaderGenNode
    {
        protected string opChar = "+";
        public override void Construct()
        {
            AddInput("lhs", SocketID.AnyMath);
            AddInput("rhs", SocketID.AnyMath);
            AddOutput("Out", SocketID.AnyMath);
        }
        public override bool EmitsCode()
        {
            // we only emit code if we have more than one connection
            return OutputSockets[0].DownstreamConnectionCount() > 1;
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            if (InputSockets[0].HasConnections() && InputSockets[1].HasConnections())
            {
                var lhs = InputSockets[0].GetSocketInfo();
                var rhs = InputSockets[1].GetSocketInfo();
                compiler.Write(string.Format("{0} {1} = {2};", Tables.inst().IDMapping[lhs.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode()));
            }
        }
        public override void Execute(object param)
        {
            if (InputSockets[0].HasConnections() && InputSockets[1].HasConnections())
            {
                var lhs = InputSockets[0].GetSocketInfo();
                var rhs = InputSockets[1].GetSocketInfo();
                OutputSockets[0].TypeID = lhs.Key;
                if (EmitsCode())
                    OutputSockets[0].StoreSocketInfo(lhs.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
                else
                    OutputSockets[0].StoreSocketInfo(lhs.Key, GetCode());
                IsValid = true;
            }
            else
                IsValid = false;
        }
        protected string GetCode()
        {
            var lhs = InputSockets[0].GetSocketInfo();
            var rhs = InputSockets[1].GetSocketInfo();
            return string.Format("({1} {0} {2})", opChar, lhs.Value, Tables.inst().Conversion(lhs, rhs));
        }
    }

    public partial class AddNode : BinaryOp
    {
        public AddNode() { opChar = "+"; }
        public override void Construct() { base.Construct(); Name = "Add"; }
    }

    public partial class SubtractNode : BinaryOp
    {
        public SubtractNode() { opChar = "-"; }
        public override void Construct() { base.Construct(); Name = "Subtract"; }        
    }

    public partial class MultiplyNode : BinaryOp
    {
        public MultiplyNode() { opChar = "*";  }
        public override void Construct() { base.Construct(); Name = "Multiply"; }
    }

    public partial class DivideNode : BinaryOp
    {
        public DivideNode() { opChar = "/"; }
        public override void Construct() { base.Construct(); Name = "Divide"; }
    }

    public partial class TransformNode : ShaderGenNode
    {
        public TransformNode() { }
    }

    public partial class TransformNormalNode : ShaderGenNode
    {
        public TransformNormalNode() { }
    }
}
