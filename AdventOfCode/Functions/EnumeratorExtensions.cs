namespace AdventOfCode.Functions;

public static class EnumeratorExtensions
{
    public static List<T> ToList<T>(this IEnumerator<T> enumerator, bool resetEnumerator = false)
    {
        var list = new List<T>();
        LoadIntoList(enumerator, list, resetEnumerator);
        return list;
    }
    public static ImmutableArray<T> ToImmutableArray<T>(this IEnumerator<T> enumerator, bool resetEnumerator = false)
    {
        var builder = ImmutableArray.CreateBuilder<T>();
        LoadIntoList(enumerator, builder, resetEnumerator);
        return builder.ToImmutable();
    }

    private static void LoadIntoList<T>(this IEnumerator<T> enumerator, IList<T> list, bool resetEnumerator)
    {
        if (resetEnumerator)
            enumerator.Reset();

        while (true)
        {
            bool hasNext = enumerator.MoveNext();
            if (!hasNext)
                break;

            list.Add(enumerator.Current);
        }
    }
}
