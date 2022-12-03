namespace AdventOfCode.Functions;

// For convenience when typing the type out
using SpanString = ReadOnlySpan<char>;

public static class SpanStringExtensions
{
    // And of course more overloads
    public static int ParseInt32(this SpanString spanString)
    {
        return int.Parse(spanString);
    }
    public static long ParseInt64(this SpanString spanString)
    {
        return long.Parse(spanString);
    }
    public static uint ParseUInt32(this SpanString spanString)
    {
        return uint.Parse(spanString);
    }
    public static ulong ParseUInt64(this SpanString spanString)
    {
        return ulong.Parse(spanString);
    }
}
