using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data
{
    /// <summary>
    /// Maintains a cache of default objects and helpers
    /// </summary>
    public sealed class DefaultPool
    {
        static Dictionary<Type, object> defaults_ = new Dictionary<Type, object>();


        static object _GetDefaultObject(Type t)
        {
            if (defaults_.ContainsKey(t))
                return defaults_[t];

            object ret = Activator.CreateInstance(t);
            defaults_[ret.GetType()] = ret;
            return ret;
        }

        public static T GetDefaultObject<T>() where T : new()
        {
            if (defaults_.ContainsKey(typeof(T)))
                return (T)defaults_[typeof(T)];

            T ret = new T();
            defaults_[typeof(T)] = ret;
            return ret;
        }

        public static bool IsPropertyDefaultValue<T>(T obj, PropertyInfo property) where T : new()
        {
            object referenceObj = GetDefaultObject<T>();
            if (referenceObj == null)
                return false;

            object referenceVal = property.GetValue(referenceObj);
            object compareVal = property.GetValue(obj);
            if (referenceVal == null && compareVal != null)
                return true;
            if (referenceVal != null && compareVal == null)
                return true;
            if (referenceVal == null && compareVal == null)
                return false;

            return referenceVal.Equals(compareVal);
        }

        public static bool IsPropertyDefault(object obj, PropertyInfo property)
        {
            object referenceObj = _GetDefaultObject(obj.GetType());
            if (referenceObj == null)
                return false;

            object referenceVal = property.GetValue(referenceObj);
            object compareVal = property.GetValue(obj);
            if (referenceVal == null && compareVal != null)
                return true;
            if (referenceVal != null && compareVal == null)
                return true;
            if (referenceVal == null && compareVal == null)
                return false;

            return referenceVal.Equals(compareVal);
        }
    }
}
