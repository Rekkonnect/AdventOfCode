namespace AdventOfCode.Utilities.TwoDimensions;

public abstract class PrintableGlyphGrid2D<T> : PrintableGrid2D<T>, IGlyphGrid
{
    // Mmmmm yes I love copying ctors
    // A convenient solution against this still beats me unfortunately
    private PrintableGlyphGrid2D(int width, int height, T defaultValue, bool initializeValueCounters)
        : base(width, height, defaultValue, initializeValueCounters) { }

    public PrintableGlyphGrid2D(int both)
        : this(both, both, default) { }
    public PrintableGlyphGrid2D(int both, T defaultValue)
        : this(both, both, defaultValue) { }
    public PrintableGlyphGrid2D(int width, int height)
        : this(width, height, default) { }
    public PrintableGlyphGrid2D(int width, int height, T defaultValue)
        : this(width, height, defaultValue, true) { }
    public PrintableGlyphGrid2D(Location2D dimensions)
        : this(dimensions.X, dimensions.Y) { }
    public PrintableGlyphGrid2D(Location2D dimensions, T defaultValue)
        : this(dimensions.X, dimensions.Y, defaultValue) { }
    public PrintableGlyphGrid2D(PrintableGlyphGrid2D<T> other)
        : base(other) { }
    public PrintableGlyphGrid2D(PrintableGlyphGrid2D<T> other, Location2D dimensions, Location2D offset)
        : base(other, dimensions, offset) { }

    protected abstract bool IsDrawnPixel(T value);

    public sealed override char GetPrintableCharacter(T value)
    {
        return IsDrawnPixel(value) ? '#' : '.';
    }
}
