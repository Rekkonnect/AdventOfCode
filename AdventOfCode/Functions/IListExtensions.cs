using System;
using System.Collections.Generic;

namespace AdventOfCode.Functions
{
    public static class IListExtensions
    {
        public static void RemoveAt<T>(this IList<T> list, Index index)
        {
            list.RemoveAt(index.GetOffset(list.Count));
        }
    }
}
