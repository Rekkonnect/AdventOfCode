using Garyon.DataStructures;
using Garyon.Exceptions;
using Garyon.Extensions;
using Garyon.Objects;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AdventOfCode.Utilities
{
    public class NextValueCounterDictionary<T> : ValueCounterDictionary<T>
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
                else if (comparisonResult == ComparisonResult.Equal)
                {
                    // Reset the best kvp to indicate that there is not a single kvp that has the best value
                    best = default;
                }
            }

            return best;
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
    }
}
