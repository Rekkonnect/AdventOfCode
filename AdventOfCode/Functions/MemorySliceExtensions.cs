namespace AdventOfCode.Functions;

// Copied from Garyon for equivalence with Memory
public static class MemorySliceExtensions
{
    private static int IndexSlicer(int index, int length, out int nextIndex)
    {
        nextIndex = -1;
        if (index > -1)
        {
            nextIndex = index + length;
        }

        return index;
    }

    public static int IndexOf<TSource>(this ReadOnlyMemory<TSource> source, TSource delimiter, out int nextIndex)
        where TSource : IEquatable<TSource>
    {
        int index = source.Span.IndexOf(delimiter);
        return IndexSlicer(index, 1, out nextIndex);
    }

    /// <summary>
    /// Splits the given memory based on the first occurrence of the
    /// delimiter, returning the left and right slices.
    /// </summary>
    /// <param name="memory">The memory to delimit.</param>
    /// <param name="delimiter">The delimiter to find in the memory.</param>
    /// <param name="left">
    /// The left segment of the memory up until before the delimiter.
    /// If the delimiter is not found, this will equal the entire memory.
    /// </param>
    /// <param name="right">
    /// The right segment of the memory starting from the next character
    /// after the first occurrence of the delimiter. If the delimiter is
    /// not found, this will equal <see langword="default"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the delimiter was found at least once,
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool SplitOnce<TSource>(this ReadOnlyMemory<TSource> memory, TSource delimiter, out ReadOnlyMemory<TSource> left, out ReadOnlyMemory<TSource> right)
        where TSource : IEquatable<TSource>
    {
        int index = memory.IndexOf(delimiter, out int nextIndex);
        return SplitOnce(memory, index, nextIndex, out left, out right);
    }

    /// <inheritdoc cref="SplitOnce{TSource}(ReadOnlyMemory{TSource}, TSource, out ReadOnlyMemory{TSource}, out ReadOnlyMemory{TSource})"/>
    public static bool SplitOnce<TSource>(this ReadOnlyMemory<TSource> memory, ReadOnlySpan<TSource> delimiter, out ReadOnlyMemory<TSource> left, out ReadOnlyMemory<TSource> right)
        where TSource : IEquatable<TSource>
    {
        int index = memory.Span.IndexOf(delimiter, out int nextIndex);
        return SplitOnce(memory, index, nextIndex, out left, out right);
    }

    private static bool SplitOnce<TSource>(
        this ReadOnlyMemory<TSource> memory, int index, int nextIndex,
        out ReadOnlyMemory<TSource> left, out ReadOnlyMemory<TSource> right)
        where TSource : IEquatable<TSource>
    {
        left = memory;
        right = default;

        if (index < 0)
            return false;

        left = memory[..index];
        right = memory[nextIndex..];
        return true;
    }
}
