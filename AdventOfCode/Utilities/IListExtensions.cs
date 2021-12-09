#nullable enable

using System.Collections.Generic;

namespace AdventOfCode.Utilities;

public static class IListExtensions
{
    public static void RemoveAtDecrement<T>(this IList<T> list, ref int index)
    {
        list.RemoveAt(index);
        index--;
    }
}
