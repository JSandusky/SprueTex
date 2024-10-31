using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Util
{
    /// <summary>
    /// Caches a collection of things based on an explicit object type.
    /// </summary>
    /// <typeparam name="T">Type to be stored</typeparam>
    public class TypedCache<T> : Dictionary<Type, T> where T : class, new()
    {
        public T GetOrCreate(object obj)
        {
            // No doc then no result
            if (obj == null)
                return null;

            // First check if we have one stored
            T ret = null;
            if (TryGetValue(obj.GetType(), out ret))
                return ret;

            // Create new
            ret = new T();
            this[obj.GetType()] = ret;

            return ret;
        }
    }
}
