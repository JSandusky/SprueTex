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
    public static class ShaderExt
    {
        public static string ToShaderString(this int val)
        {
            return val.ToString();
        }

        public static string ToShaderString(this float val)
        {
            return val.ToString();
        }

        public static string ToShaderString(this Vector2 val)
        {
            return string.Format("float2({0}, {1})", val.X.ToString(), val.Y.ToString());
        }

        public static string ToShaderString(this Vector3 val)
        {
            return string.Format("float3({0}, {1})", val.X.ToString(), val.Y.ToString(), val.Z.ToString());
        }

        public static string ToShaderString(this Vector4 val)
        {
            return string.Format("float4({0}, {1})", val.X.ToString(), val.Y.ToString(), val.Z.ToString(), val.W.ToString());
        }

        public static string ToShaderString(this string str)
        {
            return str.Replace(' ', '_');
        }
    }
}
