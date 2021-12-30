namespace AdventOfCode.Functions;

public static class IListExtensions
{
    public static T Pop<T>(this IList<T> list)
    {
        var last = list.Last();
        list.RemoveLast();
        return last;
    }
}
