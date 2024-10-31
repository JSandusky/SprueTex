using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PluginLib
{
    public static class MathExt
    {
        public static string ToTightString(this Mat3x3 mat)
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                mat.m[0, 0], mat.m[0, 1], mat.m[0, 2],
                mat.m[1, 0], mat.m[1, 1], mat.m[1, 2],
                mat.m[2, 0], mat.m[2, 1], mat.m[2, 2]);
        }

        public static Mat3x3 ToMat3x3(this string mat)
        {
            string[] terms = mat.Split(' ');
            if (terms.Length != 9)
                return new Mat3x3();
            Mat3x3 ret = new Mat3x3();
            ret.m[0, 0] = float.Parse(terms[0]);
            ret.m[0, 1] = float.Parse(terms[1]);
            ret.m[0, 2] = float.Parse(terms[2]);

            ret.m[1, 0] = float.Parse(terms[3]);
            ret.m[1, 1] = float.Parse(terms[4]);
            ret.m[1, 2] = float.Parse(terms[5]);

            ret.m[2, 0] = float.Parse(terms[6]);
            ret.m[2, 1] = float.Parse(terms[7]);
            ret.m[2, 2] = float.Parse(terms[8]);
            return ret;
        }

        public static string ToTightString(this IntVector2 vec)
        {
            return string.Format("{0} {1}", vec.X, vec.Y);
        }

        public static IntVector2 ToIntVector2(this string str)
        {
            string[] terms = str.Split(' ');
            if (terms.Length != 2)
                return new IntVector2();
            return new PluginLib.IntVector2 { X = int.Parse(terms[0]), Y = int.Parse(terms[1]) };
        }

        public static string ToTightString(this IntVector4 vec)
        {
            return string.Format("{0} {1} {2} {3}", vec.X, vec.Y, vec.Z, vec.W);
        }

        public static IntVector4 ToIntVector4(this string str)
        {
            string[] terms = str.Split(' ');
            if (terms.Length != 4)
                return new IntVector4();
            return new PluginLib.IntVector4 { X = int.Parse(terms[0]), Y = int.Parse(terms[1]), Z = int.Parse(terms[2]), W = int.Parse(terms[3]) };
        }

        public static void Write(this BinaryWriter strm, Mat3x3 mat)
        {
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                    strm.Write(mat.m[i, j]);
        }

        public static Mat3x3 ReadMat3x3(this BinaryReader strm)
        {
            Mat3x3 ret = new Mat3x3();
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                    ret.m[i,j] = strm.ReadSingle();
            return ret;
        }

        public static void Write(this BinaryWriter strm, IntVector2 mat)
        {
            strm.Write(mat.X);
            strm.Write(mat.Y);
        }

        public static IntVector2 ReadIntVector2(this BinaryReader strm)
        {
            IntVector2 ret = new IntVector2();
            ret.X = strm.ReadInt32();
            ret.Y = strm.ReadInt32();
            return ret;
        }

        public static void Write(this BinaryWriter strm, IntVector4 mat)
        {
            strm.Write(mat.X);
            strm.Write(mat.Y);
            strm.Write(mat.Z);
            strm.Write(mat.W);
        }

        public static IntVector4 ReadIntVector4(this BinaryReader strm)
        {
            IntVector4 ret = new IntVector4();
            ret.X = strm.ReadInt32();
            ret.Y = strm.ReadInt32();
            ret.Z = strm.ReadInt32();
            ret.W = strm.ReadInt32();
            return ret;
        }
    }
}
