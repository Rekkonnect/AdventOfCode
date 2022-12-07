namespace AdventOfCode.Utilities.FourDimensions;

public readonly ref struct LinearSpan4D<T>
{
    public Span<T> Span { get; }

    public int Width { get; }
    public int Height { get; }
    public int Depth { get; }
    public int DepthW { get; }

    public int TotalLength => Span.Length;

    public LinearSpan4D(Span<T> span, int width, int height, int depth, int depthW)
    {
        if (width * height * depth * depthW != span.Length)
        {
            LinearSpanThrowHelpers.ThrowInconsistentDimensions();
        }

        Span = span;
        Width = width;
        Height = height;
        Depth = depth;
        DepthW = depthW;
    }

    public void Fill(T value)
    {
        Span.Fill(value);
    }

    private int IndexAt(int x, int y, int z, int w)
    {
        // It feels physically impossible to nicely format this shit
        return ((x * Height + y)
                   * Depth + z)
                   * DepthW + w;
    }

    public ref T this[Location4D location]
    {
        get
        {
            return ref this[location.X, location.Y, location.Z, location.W];
        }
    }
    public ref T this[int x, int y, int z, int w]
    {
        get
        {
            return ref Span[IndexAt(x, y, z, w)];
        }
    }
}
