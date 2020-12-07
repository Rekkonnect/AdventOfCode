using System.Collections;
using System.Collections.Generic;

namespace AdventOfCode.Utilities
{
    public class FlexibleHashSetDictionary<TKey, TObject> : FlexibleDictionary<TKey, HashSet<TObject>>
    {
        public FlexibleHashSetDictionary() { }
        public FlexibleHashSetDictionary(IEnumerable<TKey> collection)
            : base(collection)
        {
        }
        public FlexibleHashSetDictionary(IEnumerable collection)
            : base(collection)
        {
        }
        public FlexibleHashSetDictionary(FlexibleHashSetDictionary<TKey, TObject> other)
            : base(other)
        {
        }

        public void Add(TKey key, TObject element)
        {
            if (this[key] == null)
                this[key] = new HashSet<TObject>();
            this[key].Add(element);
        }
        public void Remove(TKey key, TObject element)
        {
            if (this[key] == null)
                return;
            this[key].Remove(element);
        }

        public override HashSet<TObject> this[TKey key]
        {
            get
            {
                if (base[key] == null)
                    base[key] = new HashSet<TObject>();
                return base[key];
            }
        }
    }
}
