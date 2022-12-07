namespace AdventOfCode.Utilities.ThreeDimensions;

public readonly ref struct LinearSpan3D<T>
{
    public Span<T> Span { get; }

    public int Width => Span.Length / Height / Depth;
    public int Height { get; }
    public int Depth { get; }

    public int TotalLength => Span.Length;

    public LinearSpan3D(Span<T> span, int width, int height, int depth)
    {
        if (width * height * depth != span.Length)
        {
            LinearSpanThrowHelpers.ThrowInconsistentDimensions();
        }

        Span = span;
        Height = height;
        Depth = depth;
    }

    public void Fill(T value)
    {
        Span.Fill(value);
    }

    private int IndexAt(int x, int y, int z)
    {
        return (x * Height + y)
                  * Depth + z;
    }

    public ref T this[Location3D location]
    {
        get
        {
            return ref this[location.X, location.Y, location.Z];
        }
    }
    public ref T this[int x, int y, int z]
    {
        get
        {
            return ref Span[IndexAt(x, y, z)];
        }
    }
}
