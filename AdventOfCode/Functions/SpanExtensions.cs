namespace AdventOfCode.Functions;

public static class SpanExtensions
{
    // Can't wait for roles and extensions to be introduced
    public static TResult[] SelectArray<TSource, TResult>(this Span<TSource> source, Func<TSource, TResult> selector)
    {
        var result = new TResult[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            result[i] = selector(source[i]);
        }
        return result;
    }
    public static TResult[] SelectArray<TSource, TResult>(this ReadOnlySpan<TSource> source, Func<TSource, TResult> selector)
    {
        var result = new TResult[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            result[i] = selector(source[i]);
        }
        return result;
    }

    public static IReadOnlyList<int> AllIndicesOf<TSource>(this Span<TSource> source, TSource value)
        where TSource : IEquatable<TSource>
    {
        var result = new List<int>();

        while (true)
        {
            int index = source.IndexOf(value, out int nextIndex);
            if (index < 0)
                break;

            result.Add(nextIndex);
            source = source[nextIndex..];
        }

        return result;
    }
    public static IReadOnlyList<int> AllIndicesOf<TSource>(this ReadOnlySpan<TSource> source, TSource value)
        where TSource : IEquatable<TSource>
    {
        var result = new List<int>();

        while (true)
        {
            int index = source.IndexOf(value, out int nextIndex);
            if (index < 0)
                break;

            result.Add(nextIndex);
            source = source[nextIndex..];
        }

        return result;
    }

    public static int IndexOf<TSource>(this Span<TSource> s, TSource delimiter, out int nextIndex)
        where TSource : IEquatable<TSource>
    {
        int index = s.IndexOf(delimiter);
        nextIndex = -1;
        if (index > -1)
        {
            nextIndex = index + 1;
        }

        return index;
    }
    public static int IndexOf<TSource>(this ReadOnlySpan<TSource> s, TSource delimiter, out int nextIndex)
        where TSource : IEquatable<TSource>
    {
        int index = s.IndexOf(delimiter);
        nextIndex = -1;
        if (index > -1)
        {
            nextIndex = index + 1;
        }

        return index;
    }
}
