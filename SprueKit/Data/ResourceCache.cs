using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data
{
    public interface IResourceObject : IDisposable
    {

    }

    public class ResourceCacheEntry
    {
        public IResourceObject DataObject { get; set; }
        public int ReferenceCount { get; set; }

        public ResourceCacheEntry(IResourceObject newObject)
        {
            DataObject = newObject;
            ReferenceCount = 1;
        }

        public IResourceObject AddRef() { ReferenceCount = ReferenceCount + 1; return DataObject; }
        public bool SubRef() { ReferenceCount = ReferenceCount - 1; return ReferenceCount == 0; }
        public void Dispose() {
            if (DataObject != null)
                DataObject.Dispose();
            DataObject = null;
        }
    }

    public class ResourceCache
    {
        static ResourceCache inst_;
        private ResourceCache() { }

        public static ResourceCache inst()
        {
            if (inst_ == null)
                inst_ = new ResourceCache();
            return inst_;
        }

        Dictionary<Type, Dictionary<string, ResourceCacheEntry>> resourceTable_ = new Dictionary<Type, Dictionary<string, ResourceCacheEntry>>();
        Dictionary<object, Dictionary<string, ResourceCacheEntry>> objectResourceTable_ = new Dictionary<object, Dictionary<string, ResourceCacheEntry>>();

        Dictionary<string, ResourceCacheEntry> GetTable(Type forType)
        {
            if (resourceTable_.ContainsKey(forType))
                return resourceTable_[forType];
            else
            {
                Dictionary<string, ResourceCacheEntry> ret = new Dictionary<string, ResourceCacheEntry>();
                resourceTable_[forType] = ret;
                return ret;
            }
        }

        Dictionary<string, ResourceCacheEntry> GetObjectDataTable(object forID)
        {
            if (objectResourceTable_.ContainsKey(forID))
                return objectResourceTable_[forID];
            else
            {
                Dictionary<string, ResourceCacheEntry> ret = new Dictionary<string, ResourceCacheEntry>();
                objectResourceTable_[forID] = ret;
                return ret;
            }
        }

        public T GetResource<T>(string name) where T : class, IResourceObject
        {
            var table = GetTable(typeof(T));
            ResourceCacheEntry entry = null;
            if (table.TryGetValue(name, out entry))
            {
                if (entry == null)
                    return default(T);

                T ret = entry.DataObject as T;
                if (ret != null)
                    entry.AddRef();
                return ret;
            }
            return default(T);
        }

        public T GetObjectResource<T>(object keyObject, string name) where T : class, IResourceObject
        {
            var table = GetObjectDataTable(keyObject);
            if (table != null)
            {
                ResourceCacheEntry entry = null;
                if (table.TryGetValue(name, out entry))
                {
                    T ret = entry.DataObject as T;
                    if (ret != null)
                        entry.AddRef();
                    return ret;
                }
            }
            return default(T);
        }

        public void StoreResource<T>(T obj, string name) where T : class, IResourceObject
        {
            var table = GetTable(typeof(T));
            if (table.ContainsKey(name))
                throw new Exception(string.Format("Resource management error: duplicate key '{0}'", name));
            table[name] = new ResourceCacheEntry(obj);
        }

        public void StoreObjectResource<T>(object keyObject, T dataObject, string name) where T : class, IResourceObject
        {
            var table = GetObjectDataTable(keyObject);
            if (table.ContainsKey(name))
            {
                table[name].Dispose();
                //throw new Exception(string.Format("Resource management error: duplicate key '{0}'", name));
            }
            table[name] = new ResourceCacheEntry(dataObject);
        }

        public void RemoveResource<T>(string name) where T : class
        {
            var table = GetTable(typeof(T));
            ResourceCacheEntry entry = null;
            if (table.TryGetValue(name, out entry))
            {
                if (entry != null && entry.SubRef())
                {
                    entry.Dispose();
                    table.Remove(name);
                }
            }
        }

        public void RemoveObjectResource(object keyObject, string name)
        {
            var table = GetObjectDataTable(keyObject);
            ResourceCacheEntry entry = null;
            if (table.TryGetValue(name, out entry))
            {
                if (entry != null && entry.SubRef())
                {
                    entry.Dispose();
                    table.Remove(name);
                }
            }
        }

        public void RemoveAllObjectResources(object keyObject)
        {
            var table = GetObjectDataTable(keyObject);
            foreach (var kvp in table)
                kvp.Value.Dispose();
            table.Clear();
        }

        public T GetOrLoadResource<T>(string name, Func<T> loadFunc) where T : class, IResourceObject
        {
            T ret = GetResource<T>(name);
            if (ret == null)
            {
                ret = loadFunc();
                if (ret != null)
                    StoreResource<T>(ret, name);
            }
            return ret;
        }

        public T GetOrLoadObjectResource<T>(object keyObject, string name, Func<T> loadFunc) where T :  class, IResourceObject
        {
            T ret = GetObjectResource<T>(keyObject, name);
            if (ret != null)
                return ret;

            ret = loadFunc();
            if (ret != null)
                StoreObjectResource<T>(keyObject, ret, name);

            return null;
        }
    }

    public static class ResourceCacheExt
    {
        public static T GetResource<T>(this Dictionary<string, ResourceCacheEntry> table, string name) where T : class, IResourceObject
        {
            if (table.ContainsKey(name))
            {
                var ret = table[name];
                return ret as T;
            }
            return null;
        }
    }

}
