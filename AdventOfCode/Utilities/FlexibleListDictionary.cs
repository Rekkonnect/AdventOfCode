using Garyon.DataStructures;
using System.Collections.Generic;

namespace AdventOfCode.Utilities
{
    public class FlexibleListDictionary<TKey, TObject> : FlexibleInitializableValueDictionary<TKey, List<TObject>>
    {
        public FlexibleListDictionary()
            : base() { }
        public FlexibleListDictionary(IEnumerable<TKey> collection)
            : base(collection) { }
        public FlexibleListDictionary(FlexibleListDictionary<TKey, TObject> other)
            : base()
        {
            foreach (var kvp in other)
                Add(kvp.Key, new List<TObject>(kvp.Value));
        }
    }
}
