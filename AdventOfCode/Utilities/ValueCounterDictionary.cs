using System.Collections;
using System.Collections.Generic;

namespace AdventOfCode.Utilities
{
    public class ValueCounterDictionary<T> : FlexibleDictionary<T, int>
    {
        public ValueCounterDictionary() { }
        public ValueCounterDictionary(IEnumerable<T> collection, int initial = 1)
        {
            foreach (var v in collection)
                Add(v, initial);
        }
        public ValueCounterDictionary(IEnumerable collection, int initial = 1)
        {
            foreach (var v in collection)
                Add((T)v, initial);
        }
        public ValueCounterDictionary(ValueCounterDictionary<T> other) : base(other) { }

        public override void Add(T value, int count = 1) => this[value] += count;
        public void Remove(T value, int count = 1) => this[value] -= count;
        public void AdjustValue(T oldValue, T newValue)
        {
            Remove(oldValue);
            Add(newValue);
        }

        public override int this[T key]
        {
            get => base[key];
            set => base[key] = value;
        }
    }
}
