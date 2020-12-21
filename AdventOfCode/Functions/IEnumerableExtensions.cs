using AdventOfCode.Utilities;
using System;
using System.Collections.Generic;

namespace AdventOfCode.Functions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> enumerable)
        {
            var flattened = new List<T>();
            foreach (var e in enumerable)
                flattened.AddRange(e);
            return flattened;
        }

        // TODO: Implement for other types too kekw
        public static MinMaxResult<int> MinMax(this IEnumerable<int> source)
        {
            int min = int.MaxValue;
            int max = int.MinValue;

            foreach (var v in source)
            {
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }

            return new(min, max);
        }
        public static MinMaxResult<ulong> MinMax(this IEnumerable<ulong> source)
        {
            ulong min = ulong.MaxValue;
            ulong max = ulong.MinValue;

            foreach (var v in source)
            {
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }

            return new(min, max);
        }

        public static MinMaxResult<int> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            int min = int.MaxValue;
            int max = int.MinValue;

            foreach (var v in source)
            {
                var transformed = selector(v);
                if (transformed < min)
                    min = transformed;
                if (transformed > max)
                    max = transformed;
            }

            return new(min, max);
        }
        public static MinMaxResult<ulong> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong> selector)
        {
            ulong min = ulong.MaxValue;
            ulong max = ulong.MinValue;

            foreach (var v in source)
            {
                var transformed = selector(v);
                if (transformed < min)
                    min = transformed;
                if (transformed > max)
                    max = transformed;
            }

            return new(min, max);
        }
    }
}