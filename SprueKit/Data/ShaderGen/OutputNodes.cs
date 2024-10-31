using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprueKit.Data.Graph;

namespace SprueKit.Data.ShaderGen
{
    public partial class VertexShaderOutputNode : ShaderGenNode
    {
        public VertexShaderOutputNode() { }

        public override void Construct()
        {
            base.Construct();
            Name = "Vertex Shader Out";

            // these 3 are the only available vertex shader nodes
            AddInput("Position", SocketID.Float3);
            AddInput("Normal", SocketID.Float3);
            AddInput("UVCoord", SocketID.Float4);
            AddInput("UV2", SocketID.Float4);
            AddInput("UV3", SocketID.Float4);
            AddInput("UV4", SocketID.Float4);
            AddInput("Color", SocketID.Float);
            // hidden are Tangent and Bitangent
        }
        public override bool EmitsCode() { return true; }
        public override void EmitCode(ShaderCompiler compiler)
        {
            for (int i = 0; i < InputSockets.Count; ++i)
            {
                if (InputSockets[i].HasConnections())
                {
                    var socketData = InputSockets[i].GetSocketInfo();
                    compiler.WriteFormat("{0}.{1} = {2};", "vsOutput", InputSockets[i].Name, socketData.Value);
                }
            }
            compiler.Write("return vsOutput;");
        }
    }

    public partial class InputData : ShaderGenNode
    {
        public InputData() { }
        public override void Construct()
        {
            base.Construct();
            Name = "Inputs";
            AddOutput("Position", SocketID.Float3);
            AddOutput("Normal",   SocketID.Float3);
            AddOutput("Tangent",  SocketID.Float4);
            AddOutput("TexCoord", SocketID.Float3);
        }

        public override string GetSocketName(GraphSocket socket, CompilerStage stage)
        {
            if (stage == CompilerStage.VertexShader)
            {
                if (socket == OutputSockets[0])
                    return "vsInput.Position";
                else if (socket == OutputSockets[1])
                    return "vsInput.Normal";
                else if (socket == OutputSockets[2])
                    return "vsInput.Tangent";
                else
                    return "vsInput.UVCoord";
            }
            else
            {
                if (socket == OutputSockets[0])
                    return "psInput.Position";
                else if (socket == OutputSockets[1])
                    return "psInput.Normal";
                else if (socket == OutputSockets[2])
                    return "psInput.Tangent";
                else
                    return "psInput.UVCoord";
            }
        }
    }

    public partial class PixelShaderOutputNode : ShaderGenNode
    {
        public PixelShaderOutputNode() { }
        public override void Construct()
        {
            base.Construct();
            Name = "Pixel Shader Out";
            AddInput("Diffuse", SocketID.Float4);
            AddInput("Roughness", SocketID.Float);
            AddInput("Metalness", SocketID.Float);
            AddInput("Displacement", SocketID.Float);
        }
        public override bool EmitsCode() { return true; }
        public override void EmitCode(ShaderCompiler compiler)
        {
            
        }
    }
}
