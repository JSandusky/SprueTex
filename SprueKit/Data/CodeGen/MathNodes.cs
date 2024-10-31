using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.CodeGen
{
    public class Tables
    {
        static Tables inst_;
        public static Tables inst()
        {
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

        public Dictionary<string, Dictionary<string, string>> ConversionTable = new Dictionary<string, Dictionary<string, string>>
        {
            { "int", new Dictionary<string,string> {
                { "int", "{0}" },
                { "float", "(float){0}" },
                { "Vector2", "new Vector2({0}, {0})" },
                { "Vector3", "new Vector3({0}, {0}, {0})" },
                { "Vector4", "new Vector4({0}, {0}, {0}, {0})" },
            } },
            { "float", new Dictionary<string, string>  {
                { "float", "{0}" },
                { "Vector2", "float2({0}, 0.0)" },
                { "Vector3", "float3({0}, 0.0, 0.0)" },
                { "Vector4", "float4({0}, 0.0, 0.0, 0.0)" },
            } },
            { "float2", new Dictionary<string, string> {
                { "float", "{0}.x" },
                { "Vector2", "{0}" },
                { "Vector3", "float3({0}.xy, 0.0)" },
                { "Vector4", "float4({0}.xy 0.0, 0.0)" },
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
            "Vector2",
            "Vector3",
            "Vector4",
            "float3x3",
            "Matrix",
        };

        public Dictionary<uint, string> IDMapping = new Dictionary<uint, string>
        {
            { 1,      "int" },
            { 1 << 1, "float" },
            { 1 << 2, "Vector2" },
            { 1 << 3, "Vector3" },
            { 1 << 4, "Vector4" },
            { 1 << 5, "float3x3" },
            { 1 << 6, "Matrix" },
        };

        public Dictionary<string, uint> InverseIDMapping = new Dictionary<string, uint>
        {
            { "int",       1 },
            { "float",     1 << 1 },
            { "Vector2",   1 << 2 },
            { "Vector3",   1 << 3 },
            { "Vector4",   1 << 4 },
            { "float3x3",  1 << 5 },
            { "Matrix",    1 << 6 },
        };

        public Dictionary<string, string> ToCodeMapping = new Dictionary<string, string>
        {
            { "bool",       "SocketID.Bool" },
            { "int",        "SocketID.Int" },
            { "float",      "SocketID.Float" },
            { "Vector2",    "SocketID.Float2" },
            { "Vector3",    "SocketID.Float3" },
            { "Vector4",    "SocketID.Float4" },
            { "float3x3",   "SocketID.Float3x3" },
            { "Matrix",     "SocketID.Float4x4" },
            { "scalar",     "SocketID.AnyScalar" },
            { "vector",     "SocketID.AnyVector" },
            { "vmath",      "SocketID.AnyMath" },
            { "matrix",     "SocketID.AnyMatrix" },
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
}
