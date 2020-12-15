using System;

namespace AdventOfCode.Utilities
{
    public class MinMaxResult<T>
        where T : IComparable<T>
    {
        public T Min { get; init; }
        public T Max { get; init; }

        public MinMaxResult(T min, T max) => (Min, Max) = (min, max);

        public void Deconstruct(out T min, out T max)
        {
            min = Min;
            max = Max;
        }
    }
}