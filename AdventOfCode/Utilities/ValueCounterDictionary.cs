using Garyon.Exceptions;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

        public int GetFilteredCountersNumber(int value, InequalityState inequality = InequalityState.Equal)
        {
            inequality &= InequalityState.Any;

            if (inequality == InequalityState.Any)
                return Count;

            if (inequality == default)
                ThrowHelper.Throw<InvalidEnumArgumentException>("There provided inequality state is invalid.");

            return Values.Count(v => inequality switch
            {
                InequalityState.Less => v < value,
                InequalityState.Equal => v == value,
                InequalityState.Greater => v > value,
                InequalityState.LessOrEqual => v <= value,
                InequalityState.GreaterOrEqual => v >= value,
                InequalityState.Different => v != value,
            });
        }

        public override int this[T key]
        {
            get => base[key];
            set => base[key] = value;
        }
    }
}
