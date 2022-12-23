namespace AdventOfCode.Functions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> PickSingles<T>(this IEnumerable<ICollection<T>> collections)
    {
        foreach (var collection in collections)
        {
            if (collection.Count is 1)
                yield return collection.First();
        }
    }
    public static IEnumerable<ICollection<T>> PickNonEmptyCollections<T>(this IEnumerable<ICollection<T>> collections)
    {
        foreach (var collection in collections)
        {
            if (collection.Count is 0)
                continue;

            yield return collection;
        }
    }

    // Glorious copy-pasting
    public static List<T> PrepareListForCount<T>(this IEnumerable<T> enumerable)
    {
        return PrepareListForCount<T, T>(enumerable);
    }
    public static List<TList> PrepareListForCount<TSource, TList>(this IEnumerable<TSource> enumerable)
    {
        bool hasCount = enumerable.TryGetNonEnumeratedCount(out int count);

        if (!hasCount)
            return new();

        return new(count);
    }

    public static HashSet<T> PrepareHashSetForCount<T>(this IEnumerable<T> enumerable)
    {
        return PrepareHashSetForCount<T, T>(enumerable);
    }
    public static HashSet<TList> PrepareHashSetForCount<TSource, TList>(this IEnumerable<TSource> enumerable)
    {
        bool hasCount = enumerable.TryGetNonEnumeratedCount(out int count);

        if (!hasCount)
            return new();

        return new(count);
    }
}
