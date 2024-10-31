using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyData
{
    public class PropertyGrouping
    {
        public string GroupName { get; set; }
        public List<CachedPropertyInfo> Properties { get; private set; } = new List<CachedPropertyInfo>();
    }

    public class CachedPropertyInfo
    {
        public PropertyInfo Property;
        public string DisplayName;
    }

    public static class PropertyHelpers
    {
        static Dictionary<Type, List<PropertyGrouping>> GroupedCache = new Dictionary<Type, List<PropertyGrouping>>();
        static Dictionary<Type, List<CachedPropertyInfo>> AlphabeticalCache = new Dictionary<Type, List<CachedPropertyInfo>>();
        static Dictionary<Type, List<CachedPropertyInfo>> Cache = new Dictionary<Type, List<CachedPropertyInfo>>();

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
                if (createdGroupings.ContainsKey(grpAttr.Category))
                    target = createdGroupings[grpAttr.Category];
                else
                {
                    target = new PropertyGrouping { GroupName = grpAttr.Category };
                    createdGroupings[grpAttr.Category] = target;
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
                var lhsAttr = lhs.GetCustomAttribute<PropertyPriorityAttribute>();
                var rhsAttr = rhs.GetCustomAttribute<PropertyPriorityAttribute>();
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

            List<CachedPropertyInfo> retVal = new List<CachedPropertyInfo>();
            foreach (var val in processing)
            {
                CachedPropertyInfo info = new CachedPropertyInfo { Property = val };
                var lbl = val.GetCustomAttribute<DisplayNameAttribute>();
                if (lbl != null)
                    info.DisplayName = lbl.DisplayName;
                else
                    info.DisplayName = val.Name.SplitCamelCase();
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

        static List<PropertyInfo> FilterByAttributes(Type type, PropertyInfo[] infos)
        {
            List<PropertyInfo> processing = new List<PropertyInfo>();
            processing.AddRange(infos.Where((p) => {
                if (p.GetCustomAttribute<PropertyIgnoreAttribute>() != null)
                    return false;
                var browsable = p.GetCustomAttribute<BrowsableAttribute>();
                if (browsable != null && browsable.Browsable == false)
                    return false;
                return true;
            }));

            var typeLevelIgnores = type.GetCustomAttributes<PropertyIgnoreAttribute>();
            if (typeLevelIgnores != null)
            {
                var ignoreNames = typeLevelIgnores.Select(p => p.PropName);
                processing = processing.Where((p) => { return !ignoreNames.Contains(p.Name); }).ToList();
            }

            return processing;
        }

        static string SplitCamelCase(this string input)
        {
            StringBuilder ret = new StringBuilder();

            bool lastWasLower = false;
            for (int i = 0; i < input.Length; ++i)
            {
                if (char.IsUpper(input[i]))
                {
                    if (lastWasLower)
                    {
                        ret.Append(' ');
                        ret.Append(input[i]);
                    }
                    else if (char.IsLower(input[i + 1]))
                    {
                        ret.Append(' ');
                        ret.Append(input[i]);
                    }
                    else
                        ret.Append(input[i]);
                    lastWasLower = false;
                }
                else
                {
                    ret.Append(input[i]);
                    lastWasLower = true;
                }
            }
            return ret.ToString();
        }
    }
}
