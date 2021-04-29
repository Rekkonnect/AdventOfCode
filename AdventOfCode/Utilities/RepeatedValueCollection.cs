using System.Collections;
using System.Collections.Generic;

namespace AdventOfCode.Utilities
{
    public class RepeatedValueCollection<T> : IReadOnlyCollection<T>
    {
        public T Value { get; }
        public int Count { get; set; }

        public RepeatedValueCollection(T value, int count)
        {
            Value = value;
            Count = count;
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            public RepeatedValueCollection<T> Collection { get; }

            private int index;

            public T Current => Collection.Value;
            object IEnumerator.Current => Current;

            public Enumerator(RepeatedValueCollection<T> collection)
            {
                Collection = collection;
                index = -1;
            }

            public bool MoveNext()
            {
                index++;
                return index < Collection.Count;
            }
            public void Reset()
            {
                index = -1;
            }
            public void Dispose() { }
        }
    }
}
