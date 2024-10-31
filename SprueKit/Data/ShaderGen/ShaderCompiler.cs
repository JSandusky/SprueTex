using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.ShaderGen
{
    public enum CompilerStage
    {
        Header,
        Uniform,
        VertexShader,
        PixelShader
    }

    public class ShaderCompiler
    {
        StringBuilder builder_ = new StringBuilder();
        int indentDepth_ = 0;

        public ShaderCompiler(Graph.Graph graph)
        {
            GenerateCode(graph);
        }

        public string GetCode()
        {
            return builder_.ToString();
        }

        /// <summary>
        /// Writes a line of code
        /// </summary>
        public void Write(string text)
        {
            WriteIndent();
            builder_.AppendLine(text);
        }

        public void WriteRaw(string text)
        {
            builder_.Append(text);
        }

        public void WriteFormat(string fmtString, params object[] args)
        {
            WriteIndent();
            builder_.AppendFormat("{0}\r\n", string.Format(fmtString, args));
        }

        public void WriteFormatRaw(string fmtString, params object[] args)
        {
            builder_.AppendFormat(fmtString, args);
        }

        public void NewLine()
        {
            builder_.Append("\r\n");
        }

        public void ScopeIn()
        {
            WriteIndent();
            builder_.AppendLine("{");
            Indent();
        }

        public void ScopeOut()
        {
            Unindent();
            WriteIndent();
            builder_.AppendLine("}");
        }

        public void Indent()
        {
            ++indentDepth_;
        }

        public void Unindent()
        {
            --indentDepth_;
        }

        public void WriteIndent()
        {
            for (int i = 0; i < indentDepth_; ++i)
                builder_.Append("    ");
        }

        void GenerateCode(Graph.Graph graph)
        {
            List<ShaderGen.ShaderGenNode> nodes = new List<ShaderGenNode>();
            foreach (var nd in graph.Nodes)
            {
                if (nd is ShaderGenNode)
                    nodes.Add(nd as ShaderGenNode);
            }

            HeaderPass(nodes);
            VertexShaderPass(graph);
            PixelShaderPass(graph);
            FinishGraph();
        }

        /// <summary>
        /// First we have to emit the list of everything that is a static/external object
        /// </summary>
        /// <param name="nodes"></param>
        void HeaderPass(List<ShaderGen.ShaderGenNode> nodes)
        {
            // Emit top of file constants
            foreach (var node in nodes)
                node.EmitUniformBlock(this);

            // Emit function definitions and the like, only once per type
            HashSet<Type> visited = new HashSet<Type>();
            foreach (var node in nodes)
            {
                if (visited.Contains(node.GetType()))
                    continue;
                node.EmitHeaderBlock(this);
                visited.Add(node.GetType());
            }
        }

        List<ShaderGenNode> GetSortedVertexShaderNodes(Graph.Graph graph)
        {
            var vs = graph.Nodes.FirstOrDefault(p => p is VertexShaderOutputNode);
            if (vs == null)
                return new List<ShaderGenNode>();
            return GraphSort.UpstreamDepthSort(vs).Select(g => g as ShaderGenNode).ToList();
        }

        void VertexShaderPass(Graph.Graph graph)
        {
            var ordered = GetSortedVertexShaderNodes(graph);

            builder_.Append(
@"
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float4 Tangent  : TANGENT0;
    float3 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal   : TEXCOORD0;
    float4 Tangent  : TEXCOORD1;
    float4 UVCoord  : TEXCOORD2;
    float4 UV2      : TEXCOORD3;
    float4 UV3      : TEXCOORD4;
    float4 UV4      : TEXCOORD5;
    float4 Color    : TEXCOORD6;
};

VertexShaderOutput MainVS(in VertexShaderInput vsInput)
{
    VertexShaderOutput vsOutput = (VertexShaderOutput)0;
");



            Indent();
            var vs = graph.Nodes.FirstOrDefault(p => p is VertexShaderOutputNode);
            if (vs != null)
            {
                vs.ExecuteUpstream(1, null);
                for (int i = 0; i < ordered.Count; ++i)
                {
                    if (ordered[i].EmitsCode())
                    {
                        if (!string.IsNullOrWhiteSpace(ordered[i].Description))
                            WriteFormat("// {0}", ordered[i].Description);
                        ordered[i].EmitCode(this);
                    }
                }
            }
            Unindent();

            builder_.Append(
@"}
");
        }

        List<ShaderGenNode> GetSortedPixelShaderNodes(Graph.Graph graph)
        {
            var vs = graph.Nodes.FirstOrDefault(p => p is PixelShaderOutputNode);
            if (vs == null)
                return new List<ShaderGenNode>();
            return GraphSort.UpstreamDepthSort(vs).Select(g => g as ShaderGenNode).ToList();
        }

        void PixelShaderPass(Graph.Graph graph)
        {
            var ordered = GetSortedPixelShaderNodes(graph);

            builder_.Append(
@"
float4 MainPS(VertexShaderOutput psInput) : COLOR
{
");

            Indent();
            var ps = graph.Nodes.FirstOrDefault(p => p is PixelShaderOutputNode);
            if (ps != null)
            {
                ps.ExecuteUpstream(1, null);
                for (int i = 0; i < ordered.Count; ++i)
                {
                    if (ordered[i].EmitsCode())
                    {
                        if (!string.IsNullOrWhiteSpace(ordered[i].Description))
                            WriteFormat("// {0}", ordered[i].Description);
                        ordered[i].EmitCode(this);
                    }
                }
            }
            Unindent();

            builder_.Append(
@"}
");
        }

        void WriteCommonStart()
        {
            string CODE =
@"#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

";
            builder_.Append(CODE);
        }

        void FinishGraph()
        {
            string CODE =
@"

technique ShaderGraphTech
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
";
            builder_.Append(CODE);
        }
    }
}
