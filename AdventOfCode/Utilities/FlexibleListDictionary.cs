using System.Collections.Generic;

namespace AdventOfCode.Utilities
{
    public class FlexibleListDictionary<TKey, TObject> : FlexibleDictionary<TKey, List<TObject>>
    {
        public FlexibleListDictionary()
            : base() { }
        public FlexibleListDictionary(IEnumerable<TKey> collection)
            : base()
        {
            foreach (var k in collection)
                Add(k, new List<TObject>());
        }
        public FlexibleListDictionary(FlexibleListDictionary<TKey, TObject> other)
            : base()
        {
            foreach (var kvp in other)
                Add(kvp.Key, new List<TObject>(kvp.Value));
        }

        public virtual void Add(TKey key, TObject value = default)
        {
            if (!ContainsKey(key))
                base.Add(key, new List<TObject> { value });
            else
                base[key].Add(value);
        }

        public virtual void Add(KeyValuePair<TKey, TObject> kvp) => Add(kvp.Key, kvp.Value);

        public virtual void Remove(TKey key, TObject value)
        {
            if (!ContainsKey(key))
                return;

            base[key].Remove(value);
        }
        
        public bool TryGetValue(TKey key, int index, out TObject value)
        {
            value = default;
            if (!TryGetValue(key, out var list))
                return false;
            if (index >= list.Count)
                return false;
            value = list[index];
            return true;
        }
    }
}
