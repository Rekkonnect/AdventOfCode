using AdventOfCode.Utilities.TwoDimensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day24 : Problem<int, int>
    {
        private HexTileSetDirections[] directionsList;

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
            // But I would rather bother wth this while abstracting some of the expansion logic
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
            directionsList = FileLines.Select(HexTileSetDirections.Parse).ToArray();
        }
        protected override void ResetState()
        {
            directionsList = null;
        }

        private static HexSide ParseHexSide(string originalString, ref int index)
        {
            char c0 = originalString[index];
            if (c0 is 's' or 'n')
            {
                index++;
                char c1 = originalString[index];
                return (c0, c1) switch
                {
                    ('s', 'e') => HexSide.SouthEast,
                    ('s', 'w') => HexSide.SouthWest,
                    ('n', 'e') => HexSide.NorthEast,
                    ('n', 'w') => HexSide.NorthWest,
                };
            }
            return c0 switch
            {
                'e' => HexSide.East,
                'w' => HexSide.West,
            };
        }

        private class HexTileLocation : IEquatable<HexTileLocation>
        {
            private Location2D location;

            public Location2D Location => location;

            public HexTileLocation() { }
            public HexTileLocation(IEnumerable<HexSide> directions)
            {
                foreach (var s in directions)
                    ApplyDirection(s);
            }

            public void ApplyDirection(HexSide side)
            {
                location += side switch
                {
                    HexSide.East => (2, 0),
                    HexSide.SouthEast => (1, -1),
                    HexSide.SouthWest => (-1, -1),
                    HexSide.West => (-2, 0),
                    HexSide.NorthWest => (-1, 1),
                    HexSide.NorthEast => (1, 1),
                };
            }

            public bool Equals(HexTileLocation other) => location == other.location;
            public override bool Equals(object obj) => obj is HexTileLocation location && Equals(location);
            public override int GetHashCode() => location.GetHashCode();
            public override string ToString() => location.ToString();
        }

        private class HexTileSetDirections
        {
            private List<HexSide> directions;

            public HexTileLocation Location { get; private init; }
            public IEnumerable<HexSide> Directions => directions;

            public HexTileSetDirections(IEnumerable<HexSide> hexTileSetDirections)
            {
                directions = new(hexTileSetDirections);
                Location = new(hexTileSetDirections);
            }

            public override string ToString() => Location.ToString();

            public static HexTileSetDirections Parse(string rawDirections)
            {
                var directions = new List<HexSide>();
                for (int i = 0; i < rawDirections.Length; i++)
                    directions.Add(ParseHexSide(rawDirections, ref i));
                return new(directions);
            }
        }

        private class HexGrid2D : Grid2D<TileColor>
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

            public HexGrid2D(int radius)
                : base(radius * 2, radius) { }
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

            public override int GetAdjacentValues(int x, int y, TileColor value)
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

            public TileColor this[HexTileLocation location]
            {
                get => this[location.Location];
                set => this[location.Location] = value;
            }
        }

        private enum HexSide
        {
            East,
            SouthEast,
            SouthWest,
            West,
            NorthWest,
            NorthEast,
        }
        public enum TileColor
        {
            White = 0,
            Black = 1,
        }
    }
}
