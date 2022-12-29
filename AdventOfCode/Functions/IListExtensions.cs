namespace AdventOfCode.Functions;

public static class IListExtensions
{
    public static void RemoveAtDecrement<T>(this IList<T> list, ref int index)
    {
        list.RemoveAt(index);
        index--;
    }

    public static T Pop<T>(this IList<T> list)
    {
        var last = list.Last();
        list.RemoveLast();
        return last;
    }
}
