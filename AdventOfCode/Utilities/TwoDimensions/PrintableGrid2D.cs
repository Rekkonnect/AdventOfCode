﻿namespace AdventOfCode.Utilities.TwoDimensions;

public abstract class PrintableGrid2D<T> : Grid2D<T>, IPrintableGrid<T>
{
    protected PrintableGrid2D(int width, int height, T defaultValue, bool initializeValueCounters)
        : base(width, height, defaultValue, initializeValueCounters) { }

    public PrintableGrid2D(int both)
        : this(both, both, default) { }
    public PrintableGrid2D(int both, T defaultValue)
        : this(both, both, defaultValue) { }
    public PrintableGrid2D(int width, int height)
        : this(width, height, default) { }
    public PrintableGrid2D(int width, int height, T defaultValue)
        : this(width, height, defaultValue, true) { }
    public PrintableGrid2D(Location2D dimensions)
        : this(dimensions.X, dimensions.Y) { }
    public PrintableGrid2D(Location2D dimensions, T defaultValue)
        : this(dimensions.X, dimensions.Y, defaultValue) { }
    public PrintableGrid2D(PrintableGrid2D<T> other)
        : base(other) { }
    public PrintableGrid2D(PrintableGrid2D<T> other, Location2D dimensions, Location2D offset)
        : base(other, dimensions, offset) { }

    public virtual void PrintGrid() => Console.WriteLine(ToString());

    public abstract char GetPrintableCharacter(T value);
    protected virtual string FinalizeResultingString(StringBuilder builder) => builder.ToString();

    public sealed override string ToString()
    {
        var builder = new StringBuilder();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
                builder.Append(GetPrintableCharacter(Values[x, y]));
            builder.AppendLine();
        }
        return FinalizeResultingString(builder);
    }
}
