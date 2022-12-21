namespace AdventOfCode.Functions;

public static class IListExtensions
{
    public static T Pop<T>(this IList<T> list)
    {
        var last = list.Last();
        list.RemoveLast();
        return last;
    }

    public static List<T> ToListOrExisting<T>(this IEnumerable<T> source)
    {
        if (source is List<T> list)
            return list;

        return source.ToList();
    }
}
