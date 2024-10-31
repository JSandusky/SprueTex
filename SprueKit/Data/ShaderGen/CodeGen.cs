using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.ShaderGen
{
    public static class CodeGen
    {
        public static string GenerateCode(string[] lines)
        {
            StringBuilder ret = new StringBuilder();

            ret.AppendLine("namespace SprueKit.Data.ShaderGen {");

            for (int i = 0; i < lines.Length; ++i)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    string overrideName = null;
                    if (lines[i].StartsWith("//"))
                        continue;
                    else if (lines[i].StartsWith("#"))
                    {
                        overrideName = lines[i].Substring(1);
                        ++i;
                    }

                    string funcDef = lines[i];
                    string headerCode = null;
                    //WARNING!!! Template based code CANNOT use vmath, scalar, or vector keywords
                    if (lines[i].Trim().EndsWith("{"))
                    {
                        StringBuilder funcCode = new StringBuilder();
                        do
                        {
                            funcCode.Append("compiler.WriteRaw(\"");
                                funcCode.Append(lines[i]);
                            funcCode.AppendLine("\\r\\n\");");
                            ++i;
                        } while (!lines[i].StartsWith("}"));
                        funcCode.AppendLine("compiler.WriteRaw(\"}\\r\\n\");");
                        headerCode = funcCode.ToString();
                    }

                    FunctionInfo info = new FunctionInfo(funcDef);
                    ret.AppendFormat(ClassTemplate, 
                        string.Format("{0}", FirstToUpper(info.name)), //class name, {0}
                        info.GetSocketBuild(), // addinput {1}
                        info.GetEmitCode(),    // emit {2}
                        info.GetCode(),        // getcode {3}
                        info.name,             // assured function name {4}
                        overrideName != null ? overrideName : FirstToUpper(info.name), // override name {5}
                        headerCode != null ? headerCode : "", // header code {6}
                        info.GetExecCode(),
                        info.GetAltExecCode()); // execute {7}
                }
            }

            ret.AppendLine("}");

            return ret.ToString();
        }

        static string FirstToUpper(string ret)
        {
            return ret.First().ToString().ToUpper() + ret.Substring(1);
        }

        class FunctionInfo
        {
            public string name;
            public string outputType;
            public string[] param;
            public bool isFirstMatchStyle;

            static readonly char[] splitKeys = new char[] { ' ', '(', ',', ')', ';' };

            public FunctionInfo(string line)
            {
                string[] terms = line.Split(splitKeys, StringSplitOptions.RemoveEmptyEntries);
                outputType = terms[0];
                name = terms[1];

                param = new string[terms.Length - 2];
                for (int i = 0; i < param.Length; ++i)
                    param[i] = terms[i + 2];

                isFirstMatchStyle = param[0].Equals("scalar") || param[0].Equals("vector") || param[0].Equals("matrix") || param[0].Equals("vmath");
            }

            public string GetExecCode()
            {
                if (isFirstMatchStyle)
                    return "OutputSockets[0].StoreSocketInfo(val.Key, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));";
                return "OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));";
            }

            public string GetAltExecCode()
            {
                if (isFirstMatchStyle)
                    return "OutputSockets[0].StoreSocketInfo(OutputSockets[0].TypeID, GetCode());";
                return "OutputSockets[0].StoreSocketInfo(val.Key, GetCode());";
            }

            public string GetEmitCode()
            {
                StringBuilder sb = new StringBuilder();
                if (isFirstMatchStyle)
                    sb.Append("compiler.WriteFormat(\"{0} {1} = ({2});\", Tables.inst().IDMapping[firstVal.Key], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());");
                else
                    sb.Append("compiler.WriteFormat(\"{0} {1} = ({2});\", Tables.inst().IDMapping[OutputSockets[0].TypeID], GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode());");
                return sb.ToString();
            }

            public string GetCode()
            {
                StringBuilder sb = new StringBuilder();

                // grab all of our socket info
                for (int i = 0; i < param.Length/2; ++i)
                    sb.AppendLine(string.Format("var val{0} = InputSockets[{0}].GetSocketInfo();", i));

                sb.Append("return string.Format(\"");
                sb.Append("{0}(");
                for (int i = 0; i < param.Length / 2; ++i)
                {
                    if (i > 0)
                        sb.Append(", ");
                    sb.Append("{" + (i+1).ToString() + "}");
                }
                sb.Append(")\", funcName_, ");

                for (int i = 0; i < param.Length/2; ++i)
                {
                    if (i > 0)
                        sb.Append(", ");
                    sb.AppendFormat("val{0}.Value", i);
                }

                sb.Append(");");

                return sb.ToString();
            }

            public string GetSocketBuild()
            {
                bool hasTables= Tables.inst().InverseIDMapping.ContainsKey("test");
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < param.Length; i += 2)
                    sb.AppendLine(string.Format("AddInput(\"{1}\", {0});", Tables.inst().ToCodeMapping[param[0]], param[1]));
                sb.AppendLine(string.Format("AddOutput(\"Out\", {0});", Tables.inst().ToCodeMapping[outputType]));
                return sb.ToString();
            }
        }

        static readonly string ClassTemplate =
@"public partial class {0}Node : FunctionNode {{
    string funcName_;
    public {0}Node() {{ funcName_ = ""{4}""; }}
    public override void Construct() {{ 
        base.Construct();
        Name = ""{5}"";
        {1}
    }}
    public override void Execute(object param) {{
        for (int i = 0; i < InputSockets.Count; ++i)
            if (!InputSockets[i].HasConnections()) {{ IsValid = false; return; }}
        var val = InputSockets[0].GetSocketInfo();
        if (EmitsCode())
            {7}
        else 
            {8}
    }}
    public override void EmitCode(ShaderCompiler compiler) {{
        for (int i = 0; i < InputSockets.Count; ++i)
            if (!InputSockets[i].HasConnections()) return;
        var firstVal = InputSockets[0].GetSocketInfo();
        {2} 
    }}
    string GetCode() {{
        {3}
    }}
    public override void EmitHeaderBlock(ShaderCompiler compiler)
    {{
        {6}
    }}
}}";

    }
}
