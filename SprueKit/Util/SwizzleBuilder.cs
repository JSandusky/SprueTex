using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Util
{
    public class SwizzleBuilder
    {
        // meh, after the fact it looks like this could all work as a few calls to one function
        public static string Build()
        {
            StringBuilder sb = new StringBuilder();

            WriteSwizzle(sb, "Vector2", 2, new char[] { 'X', 'Y', 'Z', 'W' });
            WriteSwizzle(sb, "Vector3", 3, new char[] { 'X', 'Y', 'Z', 'W' });
            WriteSwizzle(sb, "Vector4", 4, new char[] { 'X', 'Y', 'Z', 'W' });

            return sb.ToString();
        }

        static void WriteSwizzle(StringBuilder sb, string sourceType, int primCount, char[] terms)
        {
            sb.AppendLine(string.Format("// {0} into Vector2 swizzles", sourceType));
            // 2 component
            for (int i = 0; i < primCount; ++i)
            {
                char first = terms[i];
                for (int j = 0; j < primCount; ++j)
                {
                    if (primCount == 2 && i == 0 && j == 1)
                        continue; // redundant
                    char second = terms[j];
                    sb.AppendLine(string.Format("public static Vector2 {0}{1}(this {2} v) {{ return new Vector2(v.{0}, v.{1}); }}", first, second, sourceType));
                }
            }

            sb.AppendLine(string.Format("// {0} into Vector3 swizzles", sourceType));
            // 3 component
            for (int i = 0; i < primCount; ++i)
            {
                char first = terms[i];
                for (int j = 0; j < primCount; ++j)
                {
                    char second = terms[j];
                    for (int k = 0; k < primCount; ++k)
                    {
                        if (primCount == 3 && i == 0 && j == 1 && k == 2)
                            continue; // redundant
                        char third = terms[k];
                        sb.AppendLine(string.Format("public static Vector3 {0}{1}{2}(this {3} v) {{ return new Vector3(v.{0}, v.{1}, v.{2}); }}", first, second, third, sourceType));
                    }
                }
            }

            sb.AppendLine(string.Format("// {0} into Vector4 swizzles", sourceType));
            // 4 component
            for (int i = 0; i < primCount; ++i)
            {
                char first = terms[i];
                for (int j = 0; j < primCount; ++j)
                {
                    char second = terms[j];
                    for (int k = 0; k < primCount; ++k)
                    {
                        char third = terms[k];
                        for (int l = 0; l < primCount; ++l)
                        {
                            if (primCount == 4 && i == 0 && j == 1 && k == 2 && l == 3)
                                continue; // redundant
                            char fourth = terms[l];
                            sb.AppendLine(string.Format("public static Vector4 {0}{1}{2}{3}(this {4} v) {{ return new Vector4(v.{0}, v.{1}, v.{2}, v.{3}); }}", first, second, third, fourth, sourceType));
                        }
                    }
                }
            }
        }
    }
}
