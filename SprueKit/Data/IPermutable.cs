using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Color = Microsoft.Xna.Framework.Color;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Matrix = Microsoft.Xna.Framework.Matrix;
using System.Reflection;

namespace SprueKit.Data
{
    public interface IPermutable
    {
        Dictionary<string, PermutationSet> Permutations { get; }
        IEnumerable<PermutationRecord> FlatPermutations { get; }

        event EventHandler PermutationsChanged;
        void SignalPermutationChange();
    }

    public static class PermutationHelpers
    {
        public static int GetPermutationCount(IPermutable perm)
        {
            return perm.FlatPermutations.Count();
        }

        public static void GetUsedPermutationNames(IPermutable perm, HashSet<string> tags)
        {
            if (perm == null)
                return;
            foreach (var p in perm.FlatPermutations)
            {
                string name = p.Value.Name;
                if (!string.IsNullOrEmpty(name))
                {
                    if (!tags.Contains(name.Trim()))
                        tags.Add(name.Trim());
                }
            }
        }

        public static void GetUsedPermutationFlags(IPermutable perm, HashSet<string> tags)
        {
            if (perm == null)
                return;
            foreach (var p in perm.FlatPermutations)
            {
                string flags = p.Value.TextFlags;
                if (!string.IsNullOrEmpty(flags))
                {
                    string[] list = flags.Split(',');
                    foreach (var str in list)
                    {
                        if (!tags.Contains(str.Trim()))
                            tags.Add(str.Trim());
                    }
                }
            }
        }

        public static void Randomize(IPermutable perm)
        {
            if (perm == null)
                return;
            var perms = perm.FlatPermutations;
            if (perms != null && perms.Count() > 0)
            {
                Random r = new Random(perm.GetHashCode() * DateTime.Now.Millisecond);
                int randCt = r.Next(0, perms.Count());
                for (int i = 0; i < randCt; ++i)
                {
                    int idx = r.Next(0, perms.Count());
                    var p = perms.ElementAt(idx);
                    perm.GetType().GetProperty(p.Property).SetValue(perm, p.Value.Value);
                }
            }
        }

        public static void ApplyName(IPermutable perm, string name)
        {
            if (perm == null)
                return;
            var perms = perm.FlatPermutations;
            if (perms != null && perms.Count() > 0)
            {
                foreach (var p in perms)
                    if (p.Value.Name.ToLowerInvariant().Equals(name.ToLowerInvariant()))
                        perm.GetType().GetProperty(p.Property).SetValue(perm, p.Value.Value);
            }
        }
    }

    public static class PermutationSerialization
    {
        public static void SerializePermutations(SerializationContext context, BinaryWriter writer, object target, Dictionary<string, PermutationSet> Permutations)
        {
            writer.Write(Permutations.Count);
            foreach (var permSet in Permutations)
            {
                var permutations = permSet.Value;
                var dataType = permutations.DataType;

                writer.Write(permSet.Key);
                writer.Write(dataType.Name);
                writer.Write(permutations.Values.Count);
                foreach (var perm in permutations.Values)
                {
                    writer.Write(perm.Name);
                    writer.Write(perm.Weight);
                    writer.Write(perm.Flags);
                    writer.Write(perm.Value == null);
                    if (perm.Value != null)
                    {
                        if (dataType == typeof(int))
                            writer.Write((int)perm.Value);
                        else if (dataType == typeof(uint))
                            writer.Write((uint)perm.Value);
                        else if (dataType == typeof(bool))
                            writer.Write((bool)perm.Value);
                        else if (dataType == typeof(float))
                            writer.Write((float)perm.Value);
                        else if (dataType == typeof(double))
                            writer.Write((double)perm.Value);
                        else if (dataType == typeof(Vector2))
                            writer.Write((Vector2)perm.Value);
                        else if (dataType == typeof(Vector3))
                            writer.Write((Vector3)perm.Value);
                        else if (dataType == typeof(Vector4))
                            writer.Write((Vector4)perm.Value);
                        else if (dataType == typeof(Microsoft.Xna.Framework.Quaternion))
                            writer.Write((Quaternion)perm.Value);
                        else if (dataType == typeof(Color))
                            writer.Write((Color)perm.Value);
                        else if (dataType == typeof(string))
                            writer.Write((string)perm.Value);
                    }
                }
            }
        }

        public static void DeserializePermutations(SerializationContext context, BinaryReader reader, object target, Dictionary<string, PermutationSet> Permutations)
        {
            int permutationSetCt = reader.ReadInt32();
            for (int i = 0; i < permutationSetCt; ++i)
            {
                string key = reader.ReadString();
                Type dataType = reader.ReadString().ToType();
                var newPerms = new PermutationSet(dataType, key);

                PropertyInfo pi = target.GetType().GetProperty(key);

                int permCt = reader.ReadInt32();
                for (int p = 0; p < permCt; ++p)
                {
                    PermutationValue perm = PermutationValue.CreateFromFile(pi, dataType);
                    newPerms.Values.Add(perm);
                    perm.Name = reader.ReadString();
                    perm.Weight = reader.ReadInt32();
                    perm.Flags = reader.ReadUInt32();

                    if (dataType == typeof(int))
                        perm.Value = reader.ReadInt32();
                    else if (dataType == typeof(uint))
                        perm.Value = reader.ReadUInt32();
                    else if (dataType == typeof(bool))
                        perm.Value = reader.ReadBoolean();
                    else if (dataType == typeof(float))
                        perm.Value = reader.ReadSingle();
                    else if (dataType == typeof(double))
                        perm.Value = reader.ReadDouble();
                    else if (dataType == typeof(Vector2))
                        perm.Value = reader.ReadVector2();
                    else if (dataType == typeof(Vector3))
                        perm.Value = reader.ReadVector3();
                    else if (dataType == typeof(Vector4))
                        perm.Value = reader.ReadVector4();
                    else if (dataType == typeof(Quaternion))
                        perm.Value = reader.ReadQuaternion();
                    else if (dataType == typeof(Color))
                        perm.Value = reader.ReadColor();
                    else if (dataType == typeof(string))
                        perm.Value = reader.ReadString();
                    else
                        perm.Value = Activator.CreateInstance(dataType);
                }

                if (newPerms.Values.Count > 0)
                    Permutations.Add(key, newPerms);
            }
        }

        public static void SerializePermutations(SerializationContext context, XmlElement into, object target, Dictionary<string, PermutationSet> Permutations)
        {
            if (Permutations.Count > 0)
            {
                XmlElement permsTopElement = into.CreateChild("permutations");

                foreach (var set in Permutations)
                {
                    if (set.Value.Values.Count > 0)
                    {
                        XmlElement setElement = permsTopElement.CreateChild("set");
                        setElement.AddStringElement("key", set.Key);
                        setElement.AddStringElement("datatype", set.Value.DataType.Name);

                        foreach (var perm in set.Value.Values)
                        {
                            XmlElement permElem = setElement.CreateChild("permutation");
                            permElem.AddStringElement("name", perm.Name);
                            permElem.AddStringElement("flags", perm.Flags.ToString());
                            permElem.AddStringElement("weight", perm.Weight.ToString());

                            if (set.Value.DataType == typeof(bool))
                                permElem.AddStringElement("value", perm.Value.ToString());
                            else if (set.Value.DataType == typeof(int))
                                permElem.AddStringElement("value", perm.Value.ToString());
                            else if (set.Value.DataType == typeof(uint))
                                permElem.AddStringElement("value", perm.Value.ToString());
                            else if (set.Value.DataType == typeof(float))
                                permElem.AddStringElement("value", perm.Value.ToString());
                            else if (set.Value.DataType == typeof(double))
                                permElem.AddStringElement("value", perm.Value.ToString());
                            else if (set.Value.DataType == typeof(Vector2))
                                permElem.AddStringElement("value", ((Vector2)perm.Value).ToTightString());
                            else if (set.Value.DataType == typeof(Vector3))
                                permElem.AddStringElement("value", ((Vector3)perm.Value).ToTightString());
                            else if (set.Value.DataType == typeof(Vector4))
                                permElem.AddStringElement("value", ((Vector4)perm.Value).ToTightString());
                            else if (set.Value.DataType == typeof(Quaternion))
                                permElem.AddStringElement("value", ((Quaternion)perm.Value).ToTightString());
                            else if (set.Value.DataType == typeof(Color))
                                permElem.AddStringElement("value", ((Color)perm.Value).ToTightString());
                            else if (set.Value.DataType == typeof(string))
                                permElem.AddStringElement("value", perm.Value.ToString());
                        }
                    }
                }
            }
        }

        public static void DeserializePermutations(SerializationContext context, XmlElement from, object target, Dictionary<string, PermutationSet> Permutations)
        {
            var node = from.SelectSingleNode("permutations");
            if (node == null)
                return;

            var permutationSets = node.SelectNodes("set");
            if (permutationSets == null || permutationSets.Count == 0)
                return;

            foreach (XmlNode permutationSet in permutationSets)
            {
                string key = (permutationSet as XmlElement).GetStringElement("key");
                string typeName = (permutationSet as XmlElement).GetStringElement("datatype");
                Type dataType = typeName.ToType();

                PermutationSet newPermSet = new PermutationSet(dataType, key);

                PropertyInfo pi = target.GetType().GetProperty(key);

                var permutations = permutationSet.SelectNodes("permutation");
                if (permutations == null || permutations.Count == 0)
                    continue;

                foreach (XmlNode permutation in permutations)
                {
                    PermutationValue newPerm = PermutationValue.CreateFromFile(pi, dataType);
                    newPerm.Name = (permutation as XmlElement).GetStringElement("name");
                    newPerm.Flags = uint.Parse((permutation as XmlElement).GetStringElement("flags", "0"));
                    newPerm.Weight = (permutation as XmlElement).GetIntElement("weight");
                    string valueString = (permutation as XmlElement).GetStringElement("value");

                    if (dataType == typeof(bool))
                        newPerm.Value = bool.Parse(valueString);
                    else if (dataType == typeof(int))
                        newPerm.Value = int.Parse(valueString);
                    else if (dataType == typeof(uint))
                        newPerm.Value = uint.Parse(valueString);
                    else if (dataType == typeof(float))
                        newPerm.Value = float.Parse(valueString);
                    else if (dataType == typeof(double))
                        newPerm.Value = double.Parse(valueString);
                    else if (dataType == typeof(Vector2))
                        newPerm.Value = valueString.ToVector2();
                    else if (dataType == typeof(Vector3))
                        newPerm.Value = valueString.ToVector3();
                    else if (dataType == typeof(Vector4))
                        newPerm.Value = valueString.ToVector4();
                    else if (dataType == typeof(Quaternion))
                        newPerm.Value = valueString.ToQuaternion();
                    else if (dataType == typeof(Color))
                        newPerm.Value = valueString.ToColor();
                    else if (dataType == typeof(string))
                        newPerm.Value = valueString;
                    else
                        newPerm.Value = Activator.CreateInstance(dataType);

                    newPermSet.Values.Add(newPerm);
                }

                if (newPermSet.Values.Count > 0)
                    Permutations.Add(key, newPermSet);
            }
        }
    }
}
