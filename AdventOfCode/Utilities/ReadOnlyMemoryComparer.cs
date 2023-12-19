namespace AdventOfCode.Utilities;

public abstract class ReadOnlyMemoryComparer<T>
    : IEqualityComparer<ReadOnlyMemory<T>>
{
    public bool Equals(ReadOnlyMemory<T> x, ReadOnlyMemory<T> y)
    {
        return x.Span.SequenceEqual(y.Span);
    }

    public abstract int GetHashCode(ReadOnlyMemory<T> obj);
}
