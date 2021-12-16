using AdventOfCSharp.Utilities;
using Garyon.Exceptions;

namespace AdventOfCode.Utilities.TwoDimensions;

public class SquareGrid2D<T> : Grid2D<T>
{
    public int Size => Width;

    #region Constructors
    protected SquareGrid2D(int size, T defaultValue, NextValueCounterDictionary<T> valueCounters)
        : base(size, size, defaultValue, valueCounters) { }

    // TODO: More constructors might be needed
    public SquareGrid2D(int size, T defaultValue = default)
        : base(size, defaultValue) { }
    public SquareGrid2D(SquareGrid2D<T> other)
        : base(other) { }
    public SquareGrid2D(Grid2D<T> other)
        : base(other)
    {
        if (other.Width != other.Height)
            ThrowHelper.Throw<InvalidOperationException>("The provided grid is not a square.");
    }
    public SquareGrid2D(SquareGrid2D<T> other, Location2D dimensions, Location2D offset)
        : base(other, dimensions, offset) { }
    #endregion

    protected override SquareGrid2D<T> InitializeClone()
    {
        return new(Size, default, ValueCounters);
    }

    // "peak" keyword please
    public override SquareGrid2D<T> RotateClockwise(int turns = 1) => base.RotateClockwise(turns) as SquareGrid2D<T>;
    public override SquareGrid2D<T> RotateCounterClockwise(int turns = 1) => base.RotateCounterClockwise(turns) as SquareGrid2D<T>;
    public override SquareGrid2D<T> FlipHorizontally() => base.FlipHorizontally() as SquareGrid2D<T>;
    public override SquareGrid2D<T> FlipVertically() => base.FlipVertically() as SquareGrid2D<T>;
}
