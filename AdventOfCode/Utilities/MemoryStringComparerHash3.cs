namespace AdventOfCode.Utilities;

public sealed class MemoryStringComparerHash3 : ReadOnlyMemoryComparer<char>
{
    public static MemoryStringComparerHash3 Instance { get; } = new();

    public override int GetHashCode(MemoryString obj)
    {
        if (obj.Length is 0)
            return 0;

        var span = obj.Span;

        int result = span[0];
        if (span.Length is 1)
            return result;

        result |= span[1] << 8;
        if (span.Length is 2)
            return result;

        result |= span[2] << 16;
        return result;
    }
}
