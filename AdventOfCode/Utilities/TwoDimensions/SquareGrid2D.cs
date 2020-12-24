using Garyon.Exceptions;
using System;

namespace AdventOfCode.Utilities.TwoDimensions
{
    public class SquareGrid2D<T> : Grid2D<T>
    {
        public int Size => Width;

        #region Constructors
        protected SquareGrid2D(int size, T defaultValue, ValueCounterDictionary<T> valueCounters)
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

        public override SquareGrid2D<T> RotateClockwise(int turns) => new(base.RotateClockwise(turns));
        public override SquareGrid2D<T> RotateCounterClockwise(int turns) => new(base.RotateCounterClockwise(turns));
    }
}
