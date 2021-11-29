using Garyon.DataStructures;
using System.Collections.Generic;

namespace AdventOfCode.Utilities;

public class FlexibleHashSetDictionary<TKey, TObject> : FlexibleInitializableValueDictionary<TKey, HashSet<TObject>>
{
    public FlexibleHashSetDictionary() { }
    public FlexibleHashSetDictionary(IEnumerable<TKey> collection)
        : base(collection)
    {
    }
    public FlexibleHashSetDictionary(FlexibleHashSetDictionary<TKey, TObject> other)
        : base(other)
    {
    }
}
