namespace AdventOfCode.Functions;

public static class SpanSliceExtensions
{
    public static Span<T> AdvanceSlice<T>(this Span<T> s, int count)
    {
        return s[count..];
    }
    public static void AdvanceSliceRef<T>(this ref Span<T> s, int count)
    {
        s = s.AdvanceSlice(count);
    }

    public static ReadOnlySpan<T> AdvanceSlice<T>(this ReadOnlySpan<T> s, int count)
    {
        return s[count..];
    }
    public static void AdvanceSliceRef<T>(this ref ReadOnlySpan<T> s, int count)
    {
        s = s.AdvanceSlice(count);
    }
}
