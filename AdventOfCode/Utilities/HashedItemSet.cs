using System.Collections.Generic;

namespace AdventOfCode.Utilities
{
    public class HashedItemSet<T> : HashSet<int>
    {
        public HashedItemSet()
            : base() { }
        public HashedItemSet(int capacity)
            : base(capacity) { }

        public bool Add(T item) => Add(item.GetHashCode());
        public bool Contains(T item) => Contains(item.GetHashCode());
        public bool Remove(T item) => Remove(item.GetHashCode());
    }
}
