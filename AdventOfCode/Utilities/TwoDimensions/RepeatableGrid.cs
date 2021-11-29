namespace AdventOfCode.Utilities.TwoDimensions;

public abstract class RepeatableGrid<T> : Grid2D<T>
{
    public RepeatableGrid(int both)
        : this(both, both, default) { }
    public RepeatableGrid(int both, T defaultValue)
        : this(both, both, defaultValue) { }
    public RepeatableGrid(int width, int height)
        : this(width, height, default) { }
    public RepeatableGrid(int width, int height, T defaultValue)
        : base(width, height, defaultValue, true) { }
    public RepeatableGrid(Grid2D<T> other)
        : base(other.Width, other.Height, default, false) { }

    public override T this[int x, int y]
    {
        get => base[x % Width, y % Height];
    }
}
