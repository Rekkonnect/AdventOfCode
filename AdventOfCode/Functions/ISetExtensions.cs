using System.Collections.Generic;

namespace AdventOfCode.Functions
{
    public static class ISetExtensions
    {
        public static void AddRange<T>(this ISet<T> set, IEnumerable<T> range)
        {
            foreach (var element in range)
                set.Add(element);
        }
    }
}
