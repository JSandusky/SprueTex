using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLib
{
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        Dictionary<TKey, TValue> backingDictionary = new Dictionary<TKey, TValue>();

        public EventHandler CollectionChanged;
        public EventHandler RecordChanged;

        void SignalChange() { if (CollectionChanged != null) CollectionChanged(this, null); }
        public void SignalRecordChange() { if (RecordChanged != null) RecordChanged(this, null); }

        public TValue this[TKey key]
        {
            get { return backingDictionary[key]; }
            set { backingDictionary[key] = value; SignalRecordChange(); }
        }

        public int Count { get { return backingDictionary.Count; } }

        public bool IsReadOnly { get { return false; } }

        public ICollection<TKey> Keys { get { return backingDictionary.Keys; } }

        public ICollection<TValue> Values { get { return backingDictionary.Values; } }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            backingDictionary.Add(item.Key, item.Value);
            SignalChange();
        }

        public void Add(TKey key, TValue value)
        {
            backingDictionary.Add(key, value);
            SignalChange();
        }

        public void Clear()
        {
            backingDictionary.Clear();
            SignalChange();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return backingDictionary.ContainsKey(item.Key);
        }

        public bool ContainsKey(TKey key)
        {
            return backingDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return backingDictionary.GetEnumerator(); }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool ret = backingDictionary.Remove(item.Key);
            if (ret)
                SignalChange();
            return ret;
        }

        public bool Remove(TKey key)
        {
            bool ret = backingDictionary.Remove(key);
            if (ret)
                SignalChange();
            return ret;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return backingDictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return backingDictionary.GetEnumerator();
        }
    }
}
