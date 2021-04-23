namespace AdventOfCode.Utilities.TwoDimensions
{
    // TODO: Implement a horizontal variant
    public abstract class HexGrid2D<T> : Grid2D<T>
    {
        private static readonly Location2D[] adjacentOffsets = new Location2D[]
        {
            (2, 0),
            (1, -1),
            (-1, -1),
            (-2, 0),
            (-1, 1),
            (1, 1),
        };

        protected HexGrid2D(int radius)
            : base(radius * 2, radius) { }
        protected HexGrid2D(HexGrid2D<T> other)
            : base(other) { }
        protected HexGrid2D(HexGrid2D<T> other, Location2D dimensions, Location2D offset)
            : base(other, dimensions, offset) { }

        public override int GetAdjacentValues(int x, int y, T value)
        {
            int count = 0;
            foreach (var o in adjacentOffsets)
            {
                var location = (x, y) + o;
                if (!IsValidLocation(location))
                    continue;

                if (this[location].Equals(value))
                    count++;
            }
            return count;
        }

        public T this[HexTileLocation location]
        {
            get => this[location.Location];
            set => this[location.Location] = value;
        }
    }
}
