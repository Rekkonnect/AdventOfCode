using System;
using System.Collections;
using System.Collections.Generic;

namespace AdventOfCode.Utilities
{
    public class FlexibleDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public IEnumerable<TKey> Keys => dictionary.Keys;
        public IEnumerable<TValue> Values => dictionary.Values;

        public int Count => dictionary.Count;

        public FlexibleDictionary() { }
        public FlexibleDictionary(IEnumerable collection)
        {
            foreach (var v in collection)
                Add((TKey)v);
        }
        public FlexibleDictionary(IEnumerable<TKey> collection)
        {
            foreach (var v in collection)
                Add(v);
        }
        public FlexibleDictionary(IEnumerable<TValue> collection, Func<TValue, TKey> keySelector)
        {
            foreach (var v in collection)
                Add(keySelector(v), v);
        }
        public FlexibleDictionary(FlexibleDictionary<TKey, TValue> other)
        {
            foreach (var kvp in other.dictionary)
                dictionary.Add(kvp.Key, kvp.Value);
        }

        public virtual void Add(TKey key, TValue value = default) => dictionary.TryAdd(key, value);
        public virtual void Add(KeyValuePair<TKey, TValue> kvp) => dictionary.Add(kvp.Key, kvp.Value);
        public virtual void Remove(TKey key) => dictionary.Remove(key);
        public void Clear() => dictionary.Clear();

        public FlexibleDictionary<TKey, TValue> Clone() => new FlexibleDictionary<TKey, TValue>(this);

        public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => dictionary.GetEnumerator();

        public virtual TValue this[TKey key]
        {
            get
            {
                if (!ContainsKey(key))
                    dictionary.Add(key, default);
                return dictionary[key];
            }
            set
            {
                if (!ContainsKey(key))
                    dictionary.Add(key, value);
                else
                    dictionary[key] = value;
            }
        }
    }
}
