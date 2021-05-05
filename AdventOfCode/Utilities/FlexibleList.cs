using System;
using System.Collections;
using System.Collections.Generic;

namespace AdventOfCode.Utilities
{
    public class FlexibleList<T> : IList<T>
    {
        private readonly List<T> list;

        public int Count => list.Count;
        public int Capacity => list.Capacity;

        bool ICollection<T>.IsReadOnly => false;

        public FlexibleList()
            : this(16) { }
        public FlexibleList(int capacity)
        {
            list = new(capacity);
        }
        public FlexibleList(IEnumerable<T> elements)
        {
            list = new(elements);
        }
        public FlexibleList(FlexibleList<T> other)
        {
            list = new(other.list);
        }

        public bool Contains(T item) => list.Contains(item);

        public int IndexOf(T item) => list.IndexOf(item);

        public void Add(T item) => list.Add(item);
        public void AddRange(IEnumerable<T> items) => list.AddRange(items);
        public bool Remove(T item) => list.Remove(item);
        public void RemoveRange(int start, int count)
        {
            if (start >= Count)
                return;

            count = Math.Min(count, Count - start);
            list.RemoveRange(start, count);
        }
        public void RemoveAt(int index)
        {
            if (index < Count)
                list.RemoveAt(index);
        }

        public void Insert(int index, T item)
        {
            ExpandToCount(index + 1);
            list.Insert(index, item);
        }

        public void Clear() => list.Clear();

        public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

        protected void ExpandToIndex(int desiredIndex) => ExpandToCount(desiredIndex + 1);
        protected void ExpandToCount(int newCount)
        {
            if (Count >= newCount)
                return;

            var collection = new List<T>(newCount - Count);
            for (int i = 0; i < collection.Count; i++)
                collection.Add(GetDefaultInitializedValue());
            list.AddRange(collection);
        }

        protected virtual T GetDefaultInitializedValue() => default;

        public T this[int index]
        {
            get
            {
                ExpandToIndex(index);
                return list[index];
            }
            set
            {
                ExpandToIndex(index);
                list[index] = value;
            }
        }

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
    }
}
