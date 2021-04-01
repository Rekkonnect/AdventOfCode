using AdventOfCode.Utilities.FourDimensions;
using AdventOfCode.Utilities.ThreeDimensions;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2020
{
    public class Day17 : Problem<int>
    {
        private Grid2D<CubeState> defaultGrid;

        public override int SolvePart1()
        {
            const int cycles = 6;

            var defaultGrid3D = new CubeGrid3D(defaultGrid.As3D);
            var grid = defaultGrid3D.ExpandForCycles(cycles);

            for (int i = 1; i <= cycles; i++)
            {
                var tempGrid = new CubeGrid3D(grid);
                var currentOffset = grid.Center - defaultGrid3D.Center - new Location3D(i);
                var currentSpace = defaultGrid3D.Dimensions + new Location3D(i * 2);

                for (int x = 0; x < currentSpace.X; x++)
                    for (int y = 0; y < currentSpace.Y; y++)
                        for (int z = 0; z < currentSpace.Z; z++)
                        {
                            var currentLocation = (x, y, z) + currentOffset;

                            var value = grid[currentLocation];
                            int neighbors = grid.GetNeighborValues(currentLocation, CubeState.Active);
                            // Hot pattern matching
                            tempGrid[currentLocation] = (value, neighbors) switch
                            {
                                (CubeState.Active, 2 or 3) => CubeState.Active,
                                (CubeState.Inactive, 3) => CubeState.Active,
                                _ => CubeState.Inactive
                            };
                        }

                grid = tempGrid;
            }

            return grid.ValueCounters[CubeState.Active];
        }
        public override int SolvePart2()
        {
            // This is awfully copy-pasted
            const int cycles = 6;

            var defaultGrid4D = new CubeGrid4D(defaultGrid.As4D);
            var grid = defaultGrid4D.ExpandForCycles(cycles);

            for (int i = 1; i <= cycles; i++)
            {
                var tempGrid = new CubeGrid4D(grid);
                var currentOffset = grid.Center - defaultGrid4D.Center - new Location4D(i);
                var currentSpace = defaultGrid4D.Dimensions + new Location4D(i * 2);

                for (int x = 0; x < currentSpace.X; x++)
                    for (int y = 0; y < currentSpace.Y; y++)
                        for (int z = 0; z < currentSpace.Z; z++)
                            for (int w = 0; w < currentSpace.W; w++)
                            {
                                var currentLocation = (x, y, z, w) + currentOffset;

                                var value = grid[currentLocation];
                                int neighbors = grid.GetNeighborValues(currentLocation, CubeState.Active);
                                // Hot pattern matching
                                tempGrid[currentLocation] = (value, neighbors) switch
                                {
                                    (CubeState.Active, 2 or 3) => CubeState.Active,
                                    (CubeState.Inactive, 3) => CubeState.Active,
                                    _ => CubeState.Inactive
                                };
                            }

                grid = tempGrid;
            }

            return grid.ValueCounters[CubeState.Active];
        }

        protected override void LoadState()
        {
            var lines = FileLines;
            int width = lines[0].Length;
            int height = lines.Length;
            defaultGrid = new(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    defaultGrid[x, y] = ParseState(lines[y][x]);
            }
        }
        protected override void ResetState()
        {
            defaultGrid = null;
        }

        private static CubeState ParseState(char c) => c switch
        {
            '#' => CubeState.Active,
            _ => CubeState.Inactive,
        };

        private class CubeGrid4D : Grid4D<CubeState>
        {
            // Constructors go brrr
            public CubeGrid4D(int all)
                : base(all, all, all, default) { }
            public CubeGrid4D(int width, int height, int depth)
                : base(width, height, depth, default) { }
            public CubeGrid4D(Location4D dimensions)
                : base(dimensions.X, dimensions.Y, dimensions.Z, default) { }
            public CubeGrid4D(Grid4D<CubeState> other)
                : base(other, other.Dimensions, Location4D.Zero) { }
            public CubeGrid4D(Grid4D<CubeState> other, Location4D dimensions, Location4D offset)
                : base(other, dimensions, offset) { }

            public CubeGrid4D ExpandForCycles(int cycles)
            {
                return new(this, Dimensions + new Location4D(2 * cycles), new Location4D(cycles));
            }
        }
        private class CubeGrid3D : Grid3D<CubeState>
        {
            // Constructors go brrr
            public CubeGrid3D(int all)
                : base(all, all, all, default) { }
            public CubeGrid3D(int width, int height, int depth)
                : base(width, height, depth, default) { }
            public CubeGrid3D(Location3D dimensions)
                : base(dimensions.X, dimensions.Y, dimensions.Z, default) { }
            public CubeGrid3D(Grid3D<CubeState> other)
                : base(other, other.Dimensions, Location3D.Zero) { }
            public CubeGrid3D(Grid3D<CubeState> other, Location3D dimensions, Location3D offset)
                : base(other, dimensions, offset) { }

            public CubeGrid3D ExpandForCycles(int cycles)
            {
                return new(this, Dimensions + new Location3D(2 * cycles), new Location3D(cycles));
            }
        }

        private enum CubeState
        {
            Inactive,
            Active
        }
    }
}
