using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2020;

public class Day24 : Problem<int, int>
{
    private VerticalHexTileSetDirections[] directionsList;

    public override int SolvePart1()
    {
        var grid = new HexGrid2D(directionsList.Max(d => d.Directions.Count()) * 2);
        foreach (var d in directionsList)
            grid.FlipTileAt(d.Location);
        return grid.ValueCounters[TileColor.Black];
    }
    public override int SolvePart2()
    {
        // Shamelessly copied the logic from D17
        const int days = 100;

        var defaultGrid = new HexGrid2D(directionsList.Max(d => d.Directions.Count()) * 2);
        foreach (var d in directionsList)
            defaultGrid.FlipTileAt(d.Location);

        var grid = defaultGrid.ExpandForDays(days);

        // There really could be many more improvements, just like in the solution for D17
        // But I would rather bother with this while abstracting some of the expansion logic
        for (int i = 1; i <= days; i++)
        {
            var tempGrid = new HexGrid2D(grid);
            var currentOffset = grid.Center - defaultGrid.Center - new Location2D(i);
            var currentSpace = defaultGrid.Dimensions + new Location2D(i * 2);

            for (int x = 0; x < currentSpace.X; x++)
                for (int y = 0; y < currentSpace.Y; y++)
                {
                    var currentLocation = (x, y) + currentOffset;

                    var value = grid[currentLocation];
                    int adjacents = grid.GetAdjacentValues(currentLocation, TileColor.Black);
                    tempGrid[currentLocation] = (value, adjacents) switch
                    {
                        (TileColor.Black, 0 or > 2) => TileColor.White,
                        (TileColor.White, 2) => TileColor.Black,
                        _ => value
                    };
                }

            grid = tempGrid;
        }

        return grid.ValueCounters[TileColor.Black];
    }

    protected override void LoadState()
    {
        directionsList = ParsedFileLines(VerticalHexTileSetDirections.ParseConcatenated);
    }
    protected override void ResetState()
    {
        directionsList = null;
    }

    private class HexGrid2D : HexGrid2D<TileColor>
    {
        public HexGrid2D(int radius)
            : base(radius) { }
        public HexGrid2D(HexGrid2D other)
            : base(other) { }
        public HexGrid2D(HexGrid2D other, Location2D dimensions, Location2D offset)
            : base(other, dimensions, offset) { }

        public void FlipTileAt(HexTileLocation location) => FlipTileAt(Center + location.Location);
        private void FlipTileAt(Location2D location)
        {
            this[location] = this[location] switch
            {
                TileColor.White => TileColor.Black,
                TileColor.Black => TileColor.White,
            };
        }

        public HexGrid2D ExpandForDays(int cycles)
        {
            return new(this, Dimensions + new Location2D(2 * cycles), new Location2D(cycles));
        }
    }

    private enum TileColor
    {
        White = 0,
        Black = 1,
    }
}
