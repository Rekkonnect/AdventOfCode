using Garyon.Exceptions;
using System;
using System.Collections.Generic;

namespace AdventOfCode.Functions
{
    public static class PendingLinqExtensions
    {
        // When this goes to System.Linq.Enumerable (if at all), change the code responsible for the ThrowHelper

        public static uint Sum(this IEnumerable<uint> source)
        {
            if (source == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            uint sum = 0;
            checked
            {
                foreach (uint v in source)
                {
                    sum += v;
                }
            }

            return sum;
        }

        public static uint? Sum(this IEnumerable<uint?> source)
        {
            if (source == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            uint? sum = 0;
            checked
            {
                foreach (uint? v in source)
                {
                    sum += v;
                }
            }

            return sum;
        }

        public static ulong Sum(this IEnumerable<ulong> source)
        {
            if (source == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            ulong sum = 0;
            checked
            {
                foreach (ulong v in source)
                {
                    sum += v;
                }
            }

            return sum;
        }

        public static ulong? Sum(this IEnumerable<ulong?> source)
        {
            if (source == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            ulong? sum = 0;
            checked
            {
                foreach (ulong? v in source)
                {
                    sum += v;
                }
            }

            return sum;
        }

        // Selectors

        public static uint Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, uint> selector)
        {
            if (source == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            if (selector == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            uint sum = 0;
            checked
            {
                foreach (TSource item in source)
                {
                    sum += selector(item);
                }
            }

            return sum;
        }

        public static uint? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, uint?> selector)
        {
            if (source == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            if (selector == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            uint? sum = 0;
            checked
            {
                foreach (TSource item in source)
                {
                    sum += selector(item);
                }
            }

            return sum;
        }

        public static ulong Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong> selector)
        {
            if (source == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            if (selector == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            ulong sum = 0;
            checked
            {
                foreach (TSource item in source)
                {
                    sum += selector(item);
                }
            }

            return sum;
        }

        public static ulong? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong?> selector)
        {
            if (source == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            if (selector == null)
            {
                ThrowHelper.Throw<ArgumentNullException>();
            }

            ulong? sum = 0;
            checked
            {
                foreach (TSource item in source)
                {
                    sum += selector(item);
                }
            }

            return sum;
        }
    }
}
