namespace AdventOfCode.Functions;

public static class EnumerableCartesianProducts
{
    public static IEnumerable<(T, T)> CartesianProduct<T>(this IEnumerable<T> source)
    {
        foreach (var a in source)
            foreach (var b in source)
                yield return (a, b);
    }

    public static IReadOnlyList<(T, T)> CachedCartesianProduct<T>(this IReadOnlyList<T> source)
    {
        int count = source.Count;
        var result = new (T, T)[count * count];
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                result[i * count + j] = (source[i], source[j]);
            }
        }
        return result;
    }
    public static IReadOnlyList<(T, T)> CachedCartesianProductExcludingSame<T>(this IReadOnlyList<T> source)
    {
        int count = source.Count;
        int links = count - 1;
        var result = new (T, T)[count * links];
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                if (i == j)
                    continue;

                int index = i * links + j;
                if (j > i)
                    index--;

                result[index] = (source[i], source[j]);
            }
        }
        return result;
    }
}
