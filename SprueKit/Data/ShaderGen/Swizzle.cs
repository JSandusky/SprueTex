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
    public enum Swizzle
    {
        XX,
        XY,
        XZ,
        YX,
        YY,
        YZ,
        ZX,
        ZY,
        ZZ,
        XW,
        YW,
        ZW,
        WX,
        WY,
        WZ,

    // Three component
        XXX,
        XXY,
        XXZ,
        XYX,
        XYY,
        XYZ,
        XZX,
        XZY,
        XZZ,
        XXW,
        XYW,
        XZW,
        XWX,
        XWY,
        XWZ,

        YXX,
        YXY,
        YXZ,
        YYX,
        YYY,
        YYZ,
        YZX,
        YZY,
        YZZ,
        YXW,
        YYW,
        YZW,
        YWX,
        YWY,
        YWZ,

        ZXX,
        ZXY,
        ZXZ,
        ZYX,
        ZYY,
        ZYZ,
        ZZX,
        ZZY,
        ZZZ,
        ZXW,
        ZYW,
        ZZW,
        ZWX,
        ZWY,
        ZWZ,

    // Four component
        XXXX,
        XXXY,
        XXXZ,
        XXYX,
        XXYY,
        XXYZ,
        XXZX,
        XXZY,
        XXZZ,
        XXXW,
        XXYW,
        XXZW,
        XXWX,
        XXWY,
        XXWZ,

        YYXX,
        YYXY,
        YYXZ,
        YYYX,
        YYYY,
        YYYZ,
        YYZX,
        YYZY,
        YYZZ,
        YYXW,
        YYYW,
        YYZW,
        YYWX,
        YYWY,
        YYWZ,

        ZZXX,
        ZZXY,
        ZZXZ,
        ZZYX,
        ZZYY,
        ZZYZ,
        ZZZX,
        ZZZY,
        ZZZZ,
        ZZXW,
        ZZYW,
        ZZZW,
        ZZWX,
        ZZWY,
        ZZWZ,

        WXXX,
        WXXY,
        WXXZ,
        WXYX,
        WXYY,
        WXYZ,
        WXZX,
        WXZY,
        WXZZ,
        WXXW,
        WXYW,
        WXZW,
        WXWX,
        WXWY,
        WXWZ,
    }

    public class SwizzleVector : ShaderGenNode
    {
    }

    public class BreakVector : ShaderGenNode
    {
        public BreakVector() { }
        public override void Construct()
        {
            base.Construct();
            Name = "Break Vector";
            AddInput("vec", SocketID.AnyFloatVector);
            AddOutput("x", SocketID.Float);
            AddOutput("y", SocketID.Float);
            AddOutput("z", SocketID.Float);
            AddOutput("w", SocketID.Float);
        }
        // To break a vector we must always emit
        public override bool EmitsCode() { return true; }
        public override void EmitCode(ShaderCompiler compiler)
        {
            var val = InputSockets[0].GetSocketInfo();
            if ((val.Key & SocketID.Float2) != 0)
                compiler.WriteFormat("{0} {1} = {2};", "float2", GetSocketName(InputSockets[0], CompilerStage.PixelShader), val.Value);
            else if ((val.Key & SocketID.Float3) != 0)
                compiler.WriteFormat("{0} {1} = {2};", "float3", GetSocketName(InputSockets[0], CompilerStage.PixelShader), val.Value);
            else if ((val.Key & SocketID.Float4) != 0)
                compiler.WriteFormat("{0} {1} = {2};", "float4", GetSocketName(InputSockets[0], CompilerStage.PixelShader), val.Value);
        }
        public override void Execute(object param)
        {
            if (!InputSockets[0].HasConnections())
            {
                IsValid = false;
                return;
            }
            IsValid = true;

            var inputInfo = InputSockets[0].GetSocketInfo();
            if ((inputInfo.Key & SocketID.Float2) != 0)
            {
                OutputSockets[0].StoreSocketInfo(SocketID.Float, GetSocketName(InputSockets[0], CompilerStage.PixelShader) + ".x");
                OutputSockets[0].TypeID = SocketID.Float;
                OutputSockets[1].StoreSocketInfo(SocketID.Float, GetSocketName(InputSockets[0], CompilerStage.PixelShader) + ".y");
                OutputSockets[1].TypeID = SocketID.Float;
                OutputSockets[2].TypeID = SocketID.Bool;
                OutputSockets[3].TypeID = SocketID.Bool;
            }
            else if ((inputInfo.Key & SocketID.Float3) != 0)
            {
                OutputSockets[0].StoreSocketInfo(SocketID.Float, GetSocketName(OutputSockets[0], CompilerStage.PixelShader) + ".x");
                OutputSockets[0].TypeID = SocketID.Float;
                OutputSockets[1].StoreSocketInfo(SocketID.Float, GetSocketName(OutputSockets[1], CompilerStage.PixelShader) + ".y");
                OutputSockets[1].TypeID = SocketID.Float;
                OutputSockets[2].StoreSocketInfo(SocketID.Float, GetSocketName(OutputSockets[2], CompilerStage.PixelShader) + ".z");
                OutputSockets[2].TypeID = SocketID.Float;
                OutputSockets[3].TypeID = SocketID.Bool;
            }
            else if ((inputInfo.Key & SocketID.Float4) != 0)
            {
                OutputSockets[0].StoreSocketInfo(SocketID.Float, GetSocketName(OutputSockets[0], CompilerStage.PixelShader) + ".x");
                OutputSockets[0].TypeID = SocketID.Float;
                OutputSockets[1].StoreSocketInfo(SocketID.Float, GetSocketName(OutputSockets[1], CompilerStage.PixelShader) + ".y");
                OutputSockets[1].TypeID = SocketID.Float;
                OutputSockets[2].StoreSocketInfo(SocketID.Float, GetSocketName(OutputSockets[2], CompilerStage.PixelShader) + ".z");
                OutputSockets[2].TypeID = SocketID.Float;
                OutputSockets[3].StoreSocketInfo(SocketID.Float, GetSocketName(OutputSockets[3], CompilerStage.PixelShader) + ".w");
                OutputSockets[3].TypeID = SocketID.Float;
            }
        }
    }

    public class MakeVector : ShaderGenNode
    {
        public MakeVector() { }
        public override void Construct()
        {
            base.Construct();
            Name = "MakeVector";
            AddInput("x", SocketID.Float);
            AddInput("z", SocketID.Float);
            AddInput("y", SocketID.Float);
            AddInput("w", SocketID.Float);
            AddOutput("Out", SocketID.AnyFloatVector);
        }
        public override void EmitCode(ShaderCompiler compiler)
        {
            int conCt = 0;
            for (int i = 0; i < InputSockets.Count; ++i)
                if (InputSockets[i].HasConnections())
                    ++conCt;

            var val = OutputSockets[0].GetSocketInfo();
            if (conCt == 2)
                compiler.WriteFormat("{float2 {0} = {1};", GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode(conCt));
            else if (conCt == 3)
                compiler.WriteFormat("{float3 {0} = {1};", GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode(conCt));
            else if (conCt == 4)
                compiler.WriteFormat("{float4 {0} = {1};", GetSocketName(OutputSockets[0], CompilerStage.PixelShader), GetCode(conCt));
        }
        public override void Execute(object param)
        {
            int conCt = 0;
            for (int i = 0; i < InputSockets.Count; ++i)
                if (InputSockets[i].HasConnections())
                    ++conCt;

            if (conCt < 2)
            {
                IsValid = false;
                return;
            }

            if (conCt == 2)
            {
                OutputSockets[0].TypeID = SocketID.Float2;
                if (EmitsCode())
                    OutputSockets[0].StoreSocketInfo(SocketID.Float2, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
                else
                    OutputSockets[0].StoreSocketInfo(SocketID.Float2, GetCode(conCt));
            }
            else if (conCt == 3)
            {
                OutputSockets[0].TypeID = SocketID.Float3;
                if (EmitsCode())
                    OutputSockets[0].StoreSocketInfo(SocketID.Float3, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
                else
                    OutputSockets[0].StoreSocketInfo(SocketID.Float3, GetCode(conCt));
            }
            else if (conCt == 4)
            {
                OutputSockets[0].TypeID = SocketID.Float4;
                if (EmitsCode())
                    OutputSockets[0].StoreSocketInfo(SocketID.Float4, GetSocketName(OutputSockets[0], CompilerStage.PixelShader));
                else
                    OutputSockets[0].StoreSocketInfo(SocketID.Float4, GetCode(conCt));
            }
        }
        string GetCode(int conCt)
        {
            KeyValuePair<uint, string>[] inputs = new KeyValuePair<uint, string>[4] {
                InputSockets[0].GetSocketInfo(),
                InputSockets[1].GetSocketInfo(),
                InputSockets[2].GetSocketInfo(),
                InputSockets[3].GetSocketInfo(),
            };
            if (conCt == 2)
                return string.Format("float2({0}, {1})", inputs[0].Value, inputs[1].Value);
            else if (conCt == 3)
                return string.Format("float3({0}, {1}, {2})", inputs[0].Value, inputs[1].Value, inputs[2].Value);
            else
                return string.Format("float4({0}, {1}, {2}, {3})", inputs[0].Value, inputs[1].Value, inputs[2].Value, inputs[3].Value);
        }
    }
}
