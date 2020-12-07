using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Utilities
{
    public class KeyedObjectDictionary<TKey, TObject> : IEnumerable<TObject>
        where TObject : IKeyedObject<TKey>
    {
        private Dictionary<TKey, TObject> d = new Dictionary<TKey, TObject>();

        public TObject[] Values => d.Values.ToArray();
        public int Count => d.Count;

        public KeyedObjectDictionary() { }
        public KeyedObjectDictionary(KeyedObjectDictionary<TKey, TObject> other) => d = new Dictionary<TKey, TObject>(other.d);

        public void Add(TObject value) => d.Add(value.Key, value);
        public bool Remove(TKey key) => d.Remove(key);
        public void Clear() => d.Clear();

        public bool Contains(TObject item) => d.ContainsKey(item.Key);
        public bool ContainsKey(TKey key) => d.ContainsKey(key);

        public bool TryGetValue(TKey key, out TObject value) => d.TryGetValue(key, out value);

        public IEnumerator<TObject> GetEnumerator() => d.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TObject this[TKey key]
        {
            get => d[key];
            set => d[key] = value;
        }
    }
}
