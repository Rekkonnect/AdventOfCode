using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Utilities
{
    public static class IEnumerableExtensions
    {
        public static bool CountAtLeast<T>(this IEnumerable<T> source, Func<T, bool> filter, int occurrences)
        {
            var filtered = source.Where(filter);
            int count = 0;

            foreach (var e in filtered)
            {
                count++;
                if (count >= occurrences)
                    return true;
            }
            return false;
        }
    }
}
