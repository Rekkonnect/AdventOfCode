using Garyon.DataStructures;
using Garyon.Exceptions;
using Garyon.Extensions;
using Garyon.Extensions.ArrayExtensions;
using Garyon.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AdventOfCode.Utilities
{
    public class NextValueCounterDictionary<T> : ValueCounterDictionary<T>, IEquatable<NextValueCounterDictionary<T>>
    {
        public NextValueCounterDictionary() { }
        public NextValueCounterDictionary(IEnumerable<T> collection, int initial = 1)
        {
            foreach (var v in collection)
                Add(v, initial);
        }
        public NextValueCounterDictionary(IEnumerable collection, int initial = 1)
        {
            foreach (var v in collection)
                Add((T)v, initial);
        }
        public NextValueCounterDictionary(NextValueCounterDictionary<T> other) : base(other) { }

        public KeyValuePair<T, int> Max() => Best(ComparisonResult.Greater);
        public KeyValuePair<T, int> Min() => Best(ComparisonResult.Less);

        public KeyValuePair<T, int> Best(ComparisonResult matchingResult)
        {
            KeyValuePair<T, int> best = default;
            int bestValue = int.MaxValue * -(int)matchingResult;

            foreach (var kvp in this)
            {
                var comparisonResult = kvp.Value.GetComparisonResult(bestValue);
                if (comparisonResult == matchingResult)
                {
                    best = kvp;
                    bestValue = kvp.Value;
                }
                else if (comparisonResult is ComparisonResult.Equal)
                {
                    // Reset the best kvp to indicate that there is not a single kvp that has the best value
                    best = default;
                }
            }

            return best;
        }

        public int GetFilteredCountersNumber(int value, ComparisonType comparison = ComparisonType.Equal)
        {
            comparison &= ComparisonType.Any;

            if (comparison is ComparisonType.Any)
                return Count;

            if (comparison is ComparisonType.None)
                ThrowHelper.Throw<InvalidEnumArgumentException>("There provided comparison type is invalid.");

            return Values.Count(v => comparison switch
            {
                ComparisonType.Less => v < value,
                ComparisonType.Equal => v == value,
                ComparisonType.Greater => v > value,
                ComparisonType.LessOrEqual => v <= value,
                ComparisonType.GreaterOrEqual => v >= value,
                ComparisonType.Different => v != value,
            });
        }

        public bool Equals(NextValueCounterDictionary<T> other)
        {
            foreach (var kvp in Dictionary)
                if (!other.Dictionary.TryGetValue(kvp.Key, out int value) || !kvp.Value.Equals(value))
                    return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is NextValueCounterDictionary<T> d && Equals(d);
        }
        public override int GetHashCode()
        {
            // Clearly a "hack" for 
            var result = new HashCode();
            var values = Dictionary.Values.ToArray();
            var sortedValues = values.Sort();
            foreach (var value in sortedValues)
                result.Add(value);
            return result.ToHashCode();
        }
    }
}
