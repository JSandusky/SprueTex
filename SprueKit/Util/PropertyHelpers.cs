using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.ComponentModel;

namespace SprueKit
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class EnumNamesAttribute : System.Attribute
    {
        public string[] EnumNames;

        public EnumNamesAttribute(string text)
        {
            EnumNames = text.Split(',');
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ShapeDataAttribute : System.Attribute
    {
        public ShapeDataAttribute(string propname)
        {
            PropertyName = propname;
        }
        public string PropertyName { get; set; }
    }    

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : System.Attribute
    {
        public CommandAttribute(string txt)
        {
            Text = txt;
        }
        public string Text { get; set; }
    }

    public class PropertyGrouping
    {
        public string GroupName { get; set; }
        public List<CachedPropertyInfo> Properties { get; private set; } = new List<CachedPropertyInfo>();
        public List<FloatMethodInfo> FloatMethods { get; private set; } = new List<FloatMethodInfo>();
    }

    public class CachedPropertyInfo
    {
        public PropertyInfo Property;
        public string DisplayName;
        public string Tip;
    }

    public class FloatMethodInfo
    {
        public string DisplayName;
        public MethodInfo Method;

        public void Invoke(object onWho, float param)
        {
            Method.Invoke(onWho, new object[] { param });
        }
    }

    public class TypeCommandInfo
    {
        internal System.Reflection.MethodInfo method_;
        public string DisplayText { get; set; }
        public string Tip { get; set; }

        public void Execute(object onWho)
        {
            method_.Invoke(onWho, null);
        }
    }

    public static class PropertyHelpers
    {
        static Dictionary<Type, List<FloatMethodInfo>> Methods = new Dictionary<Type, List<FloatMethodInfo>>();
        static Dictionary<Type, List<PropertyGrouping>> GroupedCache = new Dictionary<Type, List<PropertyGrouping>>();
        static Dictionary<Type, List<CachedPropertyInfo>> AlphabeticalCache = new Dictionary<Type, List<CachedPropertyInfo>>();
        static Dictionary<Type, List<CachedPropertyInfo>> Cache = new Dictionary<Type, List<CachedPropertyInfo>>();
        static Dictionary<Type, List<TypeCommandInfo>> Commands = new Dictionary<Type, List<TypeCommandInfo>>();

        public static List<FloatMethodInfo> GetMethods(Type type)
        {
            if (Methods.ContainsKey(type))
                return Methods[type];

            List<FloatMethodInfo> floatMethods = new List<FloatMethodInfo>();
            foreach (var method in type.GetMethods())
            {
                if (!method.IsStatic && method.GetCustomAttribute<PropertyData.GUIMethodAttribute>() != null)
                {
                    var attr = method.GetCustomAttribute<PropertyData.GUIMethodAttribute>();
                    floatMethods.Add(new FloatMethodInfo { DisplayName = attr.Name, Method = method });
                }
            }

            Methods[type] = floatMethods;
            return floatMethods;
        }

        /// <summary>
        /// Gets a collection of property groups for a given type
        /// </summary>
        public static List<PropertyGrouping> GetGrouped(Type type)
        {
            if (GroupedCache.ContainsKey(type))
                return GroupedCache[type];

            List<CachedPropertyInfo> properties = GetOrdered(type);
            HashSet<PropertyInfo> used = new HashSet<PropertyInfo>();

            List<PropertyGrouping> ret = new List<PropertyGrouping>();
            Dictionary<string, PropertyGrouping> createdGroupings = new Dictionary<string, PropertyGrouping>();

            foreach (var propInfo in properties)
            {
                var grpAttr = propInfo.Property.GetCustomAttribute<CategoryAttribute>();
                string catName = grpAttr != null ? grpAttr.Category : "Misc";

                PropertyGrouping target = null;
                if (createdGroupings.ContainsKey(catName))
                    target = createdGroupings[catName];
                else
                {
                    target = new PropertyGrouping { GroupName = catName };
                    createdGroupings[catName] = target;
                    ret.Add(target);
                }

                target.Properties.Add(propInfo);
            }

            // order by group name
            ret = ret.OrderBy((o) => o.GroupName).ToList();
            GroupedCache[type] = ret;
            return ret;
        }

        public static List<CachedPropertyInfo> GetOrdered(Type type)
        {
            if (Cache.ContainsKey(type))
                return Cache[type];

            PropertyInfo[] infos = type.GetProperties();
            List<PropertyInfo> processing = FilterByAttributes(type, infos);

            processing.Sort((lhs, rhs) => {
                var lhsAttr = lhs.GetCustomAttribute<PropertyData.PropertyPriorityAttribute>();
                var rhsAttr = rhs.GetCustomAttribute<PropertyData.PropertyPriorityAttribute>();
                if (lhsAttr != null && rhsAttr == null)
                    return -1;
                else if (lhsAttr == null && rhsAttr != null)
                    return 1;
                else if (lhsAttr != null && rhsAttr != null)
                {
                    if (lhsAttr.Level == rhsAttr.Level)
                        return 0;
                    return lhsAttr.Level < rhsAttr.Level ? -1 : 1;
                }
                return 1;
            });

            List<PropertyData.OverrideNameAttribute> nameOverrides = new List<PropertyData.OverrideNameAttribute>();
            var foundOverrides = type.GetCustomAttributes<PropertyData.OverrideNameAttribute>();
            if (foundOverrides != null)
                foreach (var ov in foundOverrides)
                    nameOverrides.Add(ov);
            List<CachedPropertyInfo> retVal = new List<CachedPropertyInfo>();
            foreach (var val in processing)
            {
                CachedPropertyInfo info = new CachedPropertyInfo { Property = val };

                var lbl = val.GetCustomAttribute<DisplayNameAttribute>();
                var overrideName = nameOverrides.FirstOrDefault(o => o.Target.Equals(val.Name));

                if (overrideName != null)
                    info.DisplayName = overrideName.NewName;
                else if (lbl != null)
                    info.DisplayName = lbl.DisplayName;
                else
                    info.DisplayName = val.Name.SplitCamelCase();

                var tip = val.GetCustomAttribute<DescriptionAttribute>();
                if (tip != null)
                    info.Tip = tip.Description;

                retVal.Add(info);
            }

            Cache[type] = retVal;
            return retVal;
        }

        public static List<CachedPropertyInfo> GetAlphabetical(Type type)
        {
            if (AlphabeticalCache.ContainsKey(type))
                return AlphabeticalCache[type];

            PropertyInfo[] infos = type.GetProperties();
            List<PropertyInfo> processing = FilterByAttributes(type, infos);

            List<CachedPropertyInfo> ret = new List<CachedPropertyInfo>();
            foreach (var val in processing)
            {
                CachedPropertyInfo info = new CachedPropertyInfo { Property = val };
                var lbl = val.GetCustomAttribute<DisplayNameAttribute>();
                if (lbl != null)
                    info.DisplayName = lbl.DisplayName;
                else
                    info.DisplayName = val.Name.SplitCamelCase();
                ret.Add(info);
            }

            return ret.OrderBy(o => o.DisplayName).ToList();
        }

        public static List<CachedPropertyInfo> Sort(Type type, List<PropertyInfo> infos)
        {
            if (Cache.ContainsKey(type))
                return Cache[type];

            List<CachedPropertyInfo> retVal = new List<CachedPropertyInfo>();
            List<PropertyInfo> processing = new List<PropertyInfo>();

            var typeLevelIgnores = type.GetCustomAttributes<PropertyData.PropertyIgnoreAttribute>();

            // First take the properties that not marked to ignore
            processing.AddRange(infos.Where((p) => { return p.GetCustomAttribute<PropertyData.PropertyIgnoreAttribute>() == null; }));

            if (typeLevelIgnores != null)
            {
                var ignoreNames = typeLevelIgnores.Select(p => p.PropName);
                processing = processing.Where((p) => { return !ignoreNames.Contains(p.Name); }).ToList();
            }

            processing.Sort((lhs, rhs) => {
                var lhsAttr = lhs.GetCustomAttribute<PropertyData.PropertyPriorityAttribute>();
                var rhsAttr = rhs.GetCustomAttribute<PropertyData.PropertyPriorityAttribute>();
                if (lhsAttr != null && rhsAttr == null)
                    return -1;
                else if (lhsAttr == null && rhsAttr != null)
                    return 1;
                else if (lhsAttr != null && rhsAttr != null)
                {
                    if (lhsAttr.Level == rhsAttr.Level)
                        return 0;
                    return lhsAttr.Level < rhsAttr.Level ? -1 : 1;
                }
                return 1;
            });

            foreach (var val in processing)
            {
                CachedPropertyInfo info = new CachedPropertyInfo { Property = val };
                var lbl = val.GetCustomAttribute<PluginLib.PropertyLabelAttribute>();
                if (lbl != null)
                    info.DisplayName = lbl.Label;
                else
                    info.DisplayName = val.Name.SplitCamelCase();
                retVal.Add(info);
            }

            Cache[type] = retVal;
            return retVal;
        }

        static List<PropertyInfo> FilterByAttributes(Type type, PropertyInfo[] infos)
        {
            List<PropertyInfo> processing = new List<PropertyInfo>();
            processing.AddRange(infos.Where((p) => {
                if (p.GetCustomAttribute<PropertyData.PropertyIgnoreAttribute>() != null)
                    return false;
                var browsable = p.GetCustomAttribute<BrowsableAttribute>();
                if (browsable != null && browsable.Browsable == false)
                    return false;
                return true;
            }));

            var typeLevelIgnores = type.GetCustomAttributes<PropertyData.PropertyIgnoreAttribute>();
            if (typeLevelIgnores != null)
            {
                var ignoreNames = typeLevelIgnores.Select(p => p.PropName);
                processing = processing.Where((p) => { return !ignoreNames.Contains(p.Name); }).ToList();
            }

            return processing;
        }

        public static List<TypeCommandInfo> GetCommands(Type type)
        {
            if (Commands.ContainsKey(type))
                return Commands[type];
            List<TypeCommandInfo> newList = new List<TypeCommandInfo>();

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<CommandAttribute>();
                if (attr != null)
                {
                    var tip = method.GetCustomAttribute<DescriptionAttribute>();
                    newList.Add(new TypeCommandInfo { DisplayText = attr.Text, Tip = tip != null ? tip.Description : "", method_ = method });
                }
            }

            Commands.Add(type, newList);
            return newList;
        }
    }

    public static class PropertyPrefetch
    {
        public static void Run()
        {
            foreach (var grp in Data.TexGen.TextureGenDocument.NodeGroups)
            {
                foreach (var subGrp in grp.Types)
                {
                    PropertyHelpers.GetAlphabetical(subGrp);
                }
            }
        }
    }
}
