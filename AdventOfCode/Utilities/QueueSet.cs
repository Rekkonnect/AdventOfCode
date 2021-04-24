using System.Collections;
using System.Collections.Generic;

namespace AdventOfCode.Utilities
{
    public class QueueSet<T> : IEnumerable<T>
    {
        private readonly Queue<T> queue;
        private readonly HashSet<T> set;

        public int Count => set.Count;
        public bool IsEmpty => Count == 0;

        public QueueSet()
        {
            queue = new Queue<T>();
            set = new HashSet<T>();
        }
        public QueueSet(int capacity)
        {
            queue = new Queue<T>(capacity);
            set = new HashSet<T>(capacity);
        }
        public QueueSet(IEqualityComparer<T> comparer)
        {
            queue = new Queue<T>();
            set = new HashSet<T>(comparer);
        }
        public QueueSet(int capacity, IEqualityComparer<T> comparer)
        {
            queue = new Queue<T>(capacity);
            set = new HashSet<T>(comparer);
        }

        public bool Contains(T item) => set.Contains(item);

        public bool Enqueue(T item)
        {
            if (!set.Add(item))
                return false;

            queue.Enqueue(item);
            return true;
        }
        public T Dequeue()
        {
            var item = queue.Dequeue();
            set.Remove(item);
            return item;
        }
        public T Peek()
        {
            return queue.Peek();
        }

        public void Clear()
        {
            set.Clear();
            queue.Clear();
        }

        public void CopyTo(T[] array, int arrayIndex) => queue.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => queue.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
