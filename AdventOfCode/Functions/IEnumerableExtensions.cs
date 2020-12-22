using AdventOfCode.Utilities;
using Garyon.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Functions
{
    public static class IEnumerableExtensions
    {
        /// <summary>Flattens a collection of collections into a single collection. The resulting elements are contained in the order they are enumerated.</summary>
        /// <typeparam name="T">The type of elements contained in the collections.</typeparam>
        /// <param name="enumerable">The collection of collections.</param>
        /// <returns>The flattened collection.</returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> enumerable)
        {
            foreach (var e in enumerable)
                foreach (var v in e)
                    yield return v;
        }

        #region MinMax
        // Behold the true copy-paste hell
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<byte> MinMax(this IEnumerable<byte> source)
        {
            VerifyCollection(source);

            byte min = byte.MaxValue;
            byte max = byte.MinValue;

            foreach (var v in source)
            {
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }

            return new(min, max);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<sbyte> MinMax(this IEnumerable<sbyte> source)
        {
            VerifyCollection(source);

            sbyte min = sbyte.MaxValue;
            sbyte max = sbyte.MinValue;

            foreach (var v in source)
            {
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }

            return new(min, max);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<short> MinMax(this IEnumerable<short> source)
        {
            VerifyCollection(source);

            short min = short.MaxValue;
            short max = short.MinValue;

            foreach (var v in source)
            {
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }

            return new(min, max);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<ushort> MinMax(this IEnumerable<ushort> source)
        {
            VerifyCollection(source);

            ushort min = ushort.MaxValue;
            ushort max = ushort.MinValue;

            foreach (var v in source)
            {
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }

            return new(min, max);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<int> MinMax(this IEnumerable<int> source)
        {
            VerifyCollection(source);

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
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<uint> MinMax(this IEnumerable<uint> source)
        {
            VerifyCollection(source);

            uint min = uint.MaxValue;
            uint max = uint.MinValue;

            foreach (var v in source)
            {
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }

            return new(min, max);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<long> MinMax(this IEnumerable<long> source)
        {
            VerifyCollection(source);

            long min = long.MaxValue;
            long max = long.MinValue;

            foreach (var v in source)
            {
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }

            return new(min, max);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<ulong> MinMax(this IEnumerable<ulong> source)
        {
            VerifyCollection(source);

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
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<float> MinMax(this IEnumerable<float> source)
        {
            VerifyCollection(source);

            float min = float.MaxValue;
            float max = float.MinValue;

            foreach (var v in source)
            {
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }

            return new(min, max);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<double> MinMax(this IEnumerable<double> source)
        {
            VerifyCollection(source);

            double min = double.MaxValue;
            double max = double.MinValue;

            foreach (var v in source)
            {
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }

            return new(min, max);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<decimal> MinMax(this IEnumerable<decimal> source)
        {
            VerifyCollection(source);

            decimal min = decimal.MaxValue;
            decimal max = decimal.MinValue;

            foreach (var v in source)
            {
                if (v < min)
                    min = v;
                if (v > max)
                    max = v;
            }

            return new(min, max);
        }

        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values of the objects in the sequence that are non-<see langword="null"/>, otherwise <seealso cref="MinMaxResult{T}.Default"/>.</returns>
        public static MinMaxResult<byte?> MinMax(this IEnumerable<byte?> source)
        {
            var filtered = source.Where(e => e.HasValue);
            if (!filtered.Any())
                return MinMaxResult<byte?>.Default;
            return MinMax(filtered);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values of the objects in the sequence that are non-<see langword="null"/>, otherwise <seealso cref="MinMaxResult{T}.Default"/>.</returns>
        public static MinMaxResult<sbyte?> MinMax(this IEnumerable<sbyte?> source)
        {
            var filtered = source.Where(e => e.HasValue);
            if (!filtered.Any())
                return MinMaxResult<sbyte?>.Default;
            return MinMax(filtered);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values of the objects in the sequence that are non-<see langword="null"/>, otherwise <seealso cref="MinMaxResult{T}.Default"/>.</returns>
        public static MinMaxResult<short?> MinMax(this IEnumerable<short?> source)
        {
            var filtered = source.Where(e => e.HasValue);
            if (!filtered.Any())
                return MinMaxResult<short?>.Default;
            return MinMax(filtered);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values of the objects in the sequence that are non-<see langword="null"/>, otherwise <seealso cref="MinMaxResult{T}.Default"/>.</returns>
        public static MinMaxResult<ushort?> MinMax(this IEnumerable<ushort?> source)
        {
            var filtered = source.Where(e => e.HasValue);
            if (!filtered.Any())
                return MinMaxResult<ushort?>.Default;
            return MinMax(filtered);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values of the objects in the sequence that are non-<see langword="null"/>, otherwise <seealso cref="MinMaxResult{T}.Default"/>.</returns>
        public static MinMaxResult<int?> MinMax(this IEnumerable<int?> source)
        {
            var filtered = source.Where(e => e.HasValue);
            if (!filtered.Any())
                return MinMaxResult<int?>.Default;
            return MinMax(filtered);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values of the objects in the sequence that are non-<see langword="null"/>, otherwise <seealso cref="MinMaxResult{T}.Default"/>.</returns>
        public static MinMaxResult<uint?> MinMax(this IEnumerable<uint?> source)
        {
            var filtered = source.Where(e => e.HasValue);
            if (!filtered.Any())
                return MinMaxResult<uint?>.Default;
            return MinMax(filtered);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values of the objects in the sequence that are non-<see langword="null"/>, otherwise <seealso cref="MinMaxResult{T}.Default"/>.</returns>
        public static MinMaxResult<long?> MinMax(this IEnumerable<long?> source)
        {
            var filtered = source.Where(e => e.HasValue);
            if (!filtered.Any())
                return MinMaxResult<long?>.Default;
            return MinMax(filtered);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values of the objects in the sequence that are non-<see langword="null"/>, otherwise <seealso cref="MinMaxResult{T}.Default"/>.</returns>
        public static MinMaxResult<ulong?> MinMax(this IEnumerable<ulong?> source)
        {
            var filtered = source.Where(e => e.HasValue);
            if (!filtered.Any())
                return MinMaxResult<ulong?>.Default;
            return MinMax(filtered);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values of the objects in the sequence that are non-<see langword="null"/>, otherwise <seealso cref="MinMaxResult{T}.Default"/>.</returns>
        public static MinMaxResult<float?> MinMax(this IEnumerable<float?> source)
        {
            var filtered = source.Where(e => e.HasValue);
            if (!filtered.Any())
                return MinMaxResult<float?>.Default;
            return MinMax(filtered);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values of the objects in the sequence that are non-<see langword="null"/>, otherwise <seealso cref="MinMaxResult{T}.Default"/>.</returns>
        public static MinMaxResult<double?> MinMax(this IEnumerable<double?> source)
        {
            var filtered = source.Where(e => e.HasValue);
            if (!filtered.Any())
                return MinMaxResult<double?>.Default;
            return MinMax(filtered);
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <returns>The minimum and maximum values of the objects in the sequence that are non-<see langword="null"/>, otherwise <seealso cref="MinMaxResult{T}.Default"/>.</returns>
        public static MinMaxResult<decimal?> MinMax(this IEnumerable<decimal?> source)
        {
            var filtered = source.Where(e => e.HasValue);
            if (!filtered.Any())
                return MinMaxResult<decimal?>.Default;
            return MinMax(filtered);
        }

        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<byte> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, byte> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<sbyte> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, sbyte> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<short> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, short> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<ushort> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, ushort> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<int> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<uint> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, uint> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<long> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<ulong> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<float> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<double> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<decimal> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            return MinMax(source.Select(selector));
        }
        
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<byte?> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, byte?> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<sbyte?> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, sbyte?> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<short?> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, short?> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<ushort?> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, ushort?> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<int?> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<uint?> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, uint?> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<long?> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<ulong?> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong?> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<float?> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<double?> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            return MinMax(source.Select(selector));
        }
        /// <summary>Gets the minimum and maximum values within the collection.</summary>
        /// <param name="source">The collection. It must be non-<see langword="null"/>, and contain at least one element.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The minimum and maximum values.</returns>
        public static MinMaxResult<decimal?> MinMax<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            return MinMax(source.Select(selector));
        }
        #endregion

        private static void VerifyCollection<T>(IEnumerable<T> source)
        {
            if (source?.Any() ?? false)
                ThrowHelper.Throw<ArgumentException>("The collection must be non-null and contain at least one element.");
        }
    }
}