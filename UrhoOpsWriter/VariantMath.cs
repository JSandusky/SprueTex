using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrhoOpsWriter
{
    static class VariantMath
    {
        // List of all VariantType values.
        static string[] VT_Names = {
            "VAR_NONE",
            "VAR_INT",
            "VAR_BOOL",
            "VAR_FLOAT",
            "VAR_VECTOR2",
            "VAR_VECTOR3",
            "VAR_VECTOR4",
            "VAR_QUATERNION",
            "VAR_COLOR",
            "VAR_STRING",
            "VAR_BUFFER",
            "VAR_VOIDPTR",
            "VAR_RESOURCEREF",
            "VAR_RESOURCEREFLIST",
            "VAR_VARIANTVECTOR",
            "VAR_VARIANTMAP",
            "VAR_INTRECT",
            "VAR_INTVECTOR2",
            "VAR_PTR",
            "VAR_MATRIX3",
            "VAR_MATRIX3X4",
            "VAR_MATRIX4",
            "VAR_DOUBLE",
            "VAR_STRINGVECTOR",
            "VAR_RECT",
            "VAR_INTVECTOR3",
            "VAR_INT64",
            "VAR_CUSTOM_HEAP",
            "VAR_CUSTOM_STACK",
        };

        /// <summary>
        /// Matching list that translates a VariantType into the corresponding 
        /// "function correct" name of the type
        /// This is NOT a value name. Used for GetFloat(), GetVector3(), etc.
        /// Use "ValueNames" for the real type name.
        /// </summary>
        static string[] VarFuncName =
        {
            "NOP",
            "Int",
            "Bool",
            "Float",
            "Vector2",
            "Vector3",
            "Vector4",
            "Quaternion",
            "Color",
            "String",
            "Buffer",
            "VoidPtr",
            "ResourceRef",
            "ResourceRefList",
            "VariantVector",
            "VariantMap",
            "IntRect",
            "IntVector2",
            "Ptr",
            "Matrix3",
            "Matrix3x4",
            "Matrix4",
            "Double",
            "StringVector",
            "Rect",
            "IntVector3",
            "Int64",
            "NOP",
            "NOP",
        };

        /// <summary>
        /// Remaps "function correct" TypeNames to the actual C++ typename.
        /// Poor naming all around.
        /// </summary>
        static Dictionary<string, string> CTypeName = new Dictionary<string, string>
        {
            { "Int", "int" },
            { "Bool", "bool" },
            { "Double", "double" },
            { "Float", "float" },
            { "Int64", "long long" },
        };

        static bool IsPrimitive(int idx)
        {
            return CTypeName.ContainsKey(VarFuncName[idx]);
        }

        static string GetCTypeName(int idx)
        {
            if (CTypeName.ContainsKey(VarFuncName[idx]))
                return CTypeName[VarFuncName[idx]];
            return VarFuncName[idx];
        }

        static Dictionary<string, int> MemberCount = new Dictionary<string, int>
        {
            { "Vector2", 2 },
            { "Vector3", 3 },
            { "Vector4", 4 },
            { "Color", 4 },
            { "IntVector2", 2},
            { "IntVector3", 3 },
            { "IntRect", 4 },
            { "Quaternion", 4 },
        };

        static Dictionary<string, string[]> MemberNames = new Dictionary<string, string[]>
        {
            { "Vector2",  new string[] { "x_", "y_" } },
            { "Vector3", new string[] { "x_", "y_", "z_" } },
            { "Vector4", new string[] { "x_", "y_", "z_", "w_" } },
            { "Color", new string[] { "r_", "g_", "b_", "a_" } },
            { "IntVector2", new string[] { "x_", "y_" }},
            { "IntVector3", new string[] { "x_", "y_", "z_" } },
            { "IntRect", new string[] { "left_", "top_", "right_", "bottom_" } },
            { "Quaternion", new string[] { "x_", "y_", "z_", "w_" } },
        };

        static bool HasMembers(int idx)
        {
            return MemberNames.ContainsKey(VarFuncName[idx]);
        }

        static string[] BasicMathTypes =
        {
            "Int",
            "Bool",
            "Int64",
            "Float",
            "Double",
            "Vector2",
            "Vector3",
            "Vector4",
            "Color",
            "Quaternion"
        };

        static KeyValuePair<string, string>[] Operands = {
            new KeyValuePair<string,string>("Add", "{0} + {1}"),
            new KeyValuePair<string,string>("Subtract", "{0} - {1}"),
            new KeyValuePair<string,string>("Multiply", "{0} * {1}"),
            new KeyValuePair<string,string>("Divide", "{0} / {1}"),
            //new KeyValuePair<string,string>("Modulo", "{0} % {1}"),
        };

        static KeyValuePair<string, string>[] ElemFunctions =
        {
            new KeyValuePair<string,string>("Floor", "floorf"),
            new KeyValuePair<string,string>("Ceil", "ceilf"),
            new KeyValuePair<string,string>("Fract", "frac"),
        };

        static KeyValuePair<string, Dictionary<string, string>>[] OperandByMember = {

        };

        static KeyValuePair<string, Dictionary<string, string>>[] OperandOverrides =
        {
            new KeyValuePair<string, Dictionary<string, string>>("Modulo",
                new Dictionary<string,string>
                {
                    { "Float", "fmodf({0}, (float){1})" },
                    { "Double", "fmod({0}, (double){1})" }
                }),
            new KeyValuePair<string, Dictionary<string, string>>("Modulo",
                new Dictionary<string,string>
                {

                }),
        };

        static string GetFormatString(string operand, int lhsIdx)
        {
            foreach (var ovr in OperandOverrides)
            {
                if (ovr.Key.Equals(operand))
                {
                    if (ovr.Value.ContainsKey(VarFuncName[lhsIdx]))
                        return ovr.Value[VarFuncName[lhsIdx]];
                }
            }
            return Operands.FirstOrDefault(kvp => kvp.Key.Equals(operand)).Value;
        }

        static Dictionary<string, bool> NeedsCast = new Dictionary<string, bool>
        {
            { "Bool", true }
        };

        public static void Run()
        {
            StringBuilder code = new StringBuilder();
            StringBuilder checkerCode = new StringBuilder();

            foreach (var operand in Operands)
            {
                code.AppendFormat("Variant VariantMath::{0}(const Variant& lhs, const Variant& rhs)\r\n{{\r\n", operand.Key);
                checkerCode.AppendFormat("bool VariantMath::Can{0}(VariantType lhsType, VariantType rhsType)\r\n{{\r\n", operand.Key);
                code.Append("    switch (lhs.GetType())\r\n    {\r\n");
                checkerCode.Append("    switch (lhsType))\r\n    {\r\n");

                for (int lhsIdx = 0; lhsIdx < VT_Names.Length; ++lhsIdx)
                {
                    string lhsTypeName = VarFuncName[lhsIdx];
                    var vtName = VT_Names[lhsIdx];

                    // Reject anything indexed as NOP
                    if (lhsTypeName.Equals("NOP"))
                        continue;

                    if (BasicMathTypes.Contains(lhsTypeName))
                    {
                        // write a case
                        code.AppendFormat("    case {0}:\r\n", vtName);
                        checkerCode.AppendFormat("    case {0}:\r\n", vtName);

                        code.Append("        switch (rhs.GetType())\r\n");
                        code.Append("        {\r\n");

                        checkerCode.Append("        switch (rhsType)\r\n");
                        checkerCode.Append("        {\r\n");

                        bool anyPassed = false;
                        for (int rhsIdx = 0; rhsIdx < VT_Names.Length; ++rhsIdx)
                        {
                            string rhsTypeName = VarFuncName[rhsIdx];
                            var rhsVTName = VT_Names[rhsIdx];
                            if (BasicMathTypes.Contains(rhsTypeName))
                            {
                                bool leftHasMembers = HasMembers(lhsIdx);
                                bool rightHasMembers = HasMembers(rhsIdx);

                                // bypass the bizare cases like Vector3.x only
                                if (rightHasMembers && !leftHasMembers)
                                    continue;

                                anyPassed = true;
                                checkerCode.AppendFormat("        case {0}:\r\n", rhsVTName);

                                string operandAssignment = GetFormatString(operand.Key, lhsIdx);
                                string getLHS = string.Format("lhs.Get{0}()", lhsTypeName);

                                if (rhsVTName.Equals(vtName) || (IsPrimitive(lhsIdx) && IsPrimitive(rhsIdx)))
                                {
                                    // Simplest case, C can take care of it all
                                    code.AppendFormat("            case {0}:\r\n", rhsVTName);
                                    string getRHS = string.Format("rhs.Get{0}()", rhsTypeName);
                                    string assignment = string.Format(operandAssignment, getLHS, getRHS);

                                    if (rhsVTName.Equals(vtName)) // don't cast for doing anything stupid
                                        code.AppendFormat("                return {0};\r\n", assignment);
                                    else // cast to be 100% certain
                                        code.AppendFormat("                return ({1})({0});\r\n", assignment, GetCTypeName(lhsIdx));
                                }
                                else
                                {
                                    // more complex, need to do some reasoning about the types
                                    code.AppendFormat("            case {0}: {{\r\n", rhsVTName);
                                    code.AppendFormat("                {0} rVal = rhs.Get{1}();\r\n", GetCTypeName(rhsIdx), rhsTypeName);

                                    if (leftHasMembers && rightHasMembers)
                                    {
                                        // map an X,Y type onto an X,Y,Z type and vice-versa

                                        var leftMembers = MemberNames[lhsTypeName];
                                        var rightMembers = MemberNames[rhsTypeName];
                                        string getRHS = GetMembersString(GetCTypeName(lhsIdx), leftMembers, rightMembers);

                                        string assignment = string.Format(operandAssignment, getLHS, getRHS);
                                        code.AppendFormat("                return {0};\r\n", assignment);
                                    }
                                    else if (HasMembers(lhsIdx))
                                    {
                                        // map a singular type (float, int, etc) onto an X,Y,Z type
                                        // Outputs: return lhs.GetVector3() + Vector3(rVal, rVal, rVal);

                                        var leftMembers = MemberNames[lhsTypeName];

                                        string getRHS = GetMembersString(GetCTypeName(lhsIdx), leftMembers, null);
                                        string assignment = string.Format(operandAssignment, getLHS, getRHS);

                                        code.AppendFormat("                return {0};\r\n", assignment);
                                    }
                                    else if (HasMembers(rhsIdx))
                                    {
                                        // Case should not be encountered
                                        // Mapping a vector2 onto a float doesn't make sense

                                        code.Append("                ERROR: undesired mapping of a structure onto a singular C-type");
                                        //var rightMembers = MemberNames[rhsTypeName];
                                        //code.AppendFormat("                return lhs.Get{0}() {1} rVal.{2};\r\n", lhsTypeName, operand.Value, rightMembers[0]);
                                    }
                                    else
                                    {
                                        // Fallback and hope it works

                                        code.AppendFormat("                return lhs.Get{0}() {1} rVal;\r\n", lhsTypeName, operand.Value);
                                    }

                                    code.Append("            }\r\n");
                                }

                            }
                            else
                            {
                                // do nothing, we can't process this type
                            }
                        }
                        code.Append("        }\r\n");
                        // end of big block of cases in the Can____ code
                        checkerCode.Append("            return true;\r\n");
                        checkerCode.Append("        }\r\n");
                    }
                    else
                    {
                        // do nothing, skip this type
                    }
                }

                code.Append("    }\r\n    return Variant::EMPTY;\r\n");
                code.Append("}\r\n\r\n");

                checkerCode.Append("    }\r\n    return false;\r\n");
                checkerCode.Append("}\r\n\r\n");
            }

            foreach (var func in ElemFunctions)
            {
                code.AppendFormat("Variant VariantFunctions::{0}(const Variant& value)\r\n{{\r\n", func.Key);
                code.Append("    switch (value.GetType())\r\n");
                code.Append("    {\r\n");
                foreach (var type in BasicMathTypes)
                {
                    int varIdx = Array.IndexOf(VarFuncName, type);
                    code.AppendFormat("    case {0}:\t\n", VT_Names[varIdx]);
                    if (MemberCount.ContainsKey(type))
                    {
                        int ct = MemberCount[type];
                        code.Append("    {\r\n");
                        code.AppendFormat("        {0} val = rhs.Get{1}();\r\n", type, VarFuncName[varIdx]);
                        code.AppendFormat("        return {0}(", type);
                        bool hit = false;
                        foreach (var memSub in MemberNames[type])
                        {
                            if (hit)
                                code.Append(", ");
                            code.AppendFormat("{0}(val.{1})", func.Value, memSub);
                            hit = true;
                        }
                        code.Append(");\r\n");
                        code.Append("    } break;\r\n");
                    }
                    else
                    {
                        code.AppendFormat("        return ({1}){0}(value.GetFloat());\r\n", func.Value, GetCTypeName(varIdx));
                    }
                }
                code.Append("    default: return Variant();\r\n");
                code.Append("    }\r\n");
                code.Append("}\r\n");
            }


            string outText = code.ToString();
            outText.Replace("\t", "    ");

            string outChecker = checkerCode.ToString();
            outChecker.Replace("\t", "    ");

            System.IO.File.WriteAllText("Var.txt", outText.ToString());
            System.Console.Write(outText);
        }

        static string GetMembersString(string targetType, string[] leftMembers, string[] rightMembers)
        {
            if (leftMembers != null && rightMembers != null)
            {
                string getRHS = string.Format("{0}(", targetType);
                int i = 0;
                for (; i < rightMembers.Length && i < leftMembers.Length; ++i)
                {
                    if (i > 0)
                        getRHS += ", ";
                    getRHS += "rVal." + rightMembers[i];
                }
                for (; i < leftMembers.Length; ++i)
                {
                    if (i > 0)
                        getRHS += ", ";
                    getRHS += "0";
                }
                getRHS += ")";

                return getRHS;
            }
            else if (leftMembers != null)
            {
                string getRHS = string.Format("{0}(", targetType);
                for (int i = 0; i < leftMembers.Length; ++i)
                {
                    if (i > 0)
                        getRHS += ", ";
                    getRHS += "rVal";
                }
                getRHS += ")";
                return getRHS;
            }
            else
                throw new Exception("Encountered error with struct handling");
            return "ERROR";
        }
    }
}
