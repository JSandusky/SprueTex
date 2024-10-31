using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data
{
    public class HashCache<T> : Dictionary<int, KeyValuePair<int, T>>, IDisposable where T : class, IDisposable
    {
        public T Get(int idx, int hash)
        {
            if (ContainsKey(idx))
            {
                if (this[idx].Key == hash)
                    return this[idx].Value;
                else
                    this[idx].Value.Dispose();
                Remove(idx);
            }
            return null;
        }

        public T GetOrCreate(int idx, int hash, Func<T> factory)
        {
            T ret = Get(idx, hash);
            if (ret == null)
            {
                var val = factory();
                if (val != null)
                    this[idx] = new KeyValuePair<int, T>(hash, val);
            }
            return null;
        }

        public void Store(int idx, int hash, T obj)
        {
            if (ContainsKey(idx) && this[idx].Value != null)
                this[idx].Value.Dispose();
            this[idx] = new KeyValuePair<int, T>(hash, obj);
        }

        public void Dispose()
        {
            foreach (var kvp in this)
                if (kvp.Value.Value != null)
                    kvp.Value.Value.Dispose();
            base.Clear();
        }

        public new void Clear()
        {
            Dispose();
        }
    }
}
