namespace AdventOfCode.Utilities.TwoDimensions;

public readonly ref struct LinearSpan2D<T>
{
    public Span<T> Span { get; }

    public int Width => Span.Length / Height;
    public int Height { get; }

    public int TotalLength => Span.Length;

    public LinearSpan2D(Span<T> span, int width, int height)
    {
        if (width * height != span.Length)
        {
            LinearSpanThrowHelpers.ThrowInconsistentDimensions();
        }

        Span = span;
        Height = height;
    }

    public void Fill(T value)
    {
        Span.Fill(value);
    }

    private int IndexAt(int x, int y)
    {
        return x * Height + y;
    }

    public ref T this[Location2D location]
    {
        get
        {
            return ref this[location.X, location.Y];
        }
    }
    public ref T this[int x, int y]
    {
        get
        {
            return ref Span[IndexAt(x, y)];
        }
    }

    public static LinearSpan2D<T> SplitOnWidth(Span<T> span, int width)
    {
        int height = Math.DivRem(span.Length, width, out var remainder);
        LinearSpanThrowHelpers.ThrowNonDivisibleDimensionality(remainder);

        return new(span, width, height);
    }
    public static LinearSpan2D<T> SplitOnHeight(Span<T> span, int height)
    {
        int width = Math.DivRem(span.Length, height, out var remainder);
        LinearSpanThrowHelpers.ThrowNonDivisibleDimensionality(remainder);

        return new(span, width, height);
    }
}
