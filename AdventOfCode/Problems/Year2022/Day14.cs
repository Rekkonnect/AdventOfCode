using AdventOfCode.Utilities.TwoDimensions;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2022;

public class Day14 : Problem<int>
{
    private ImmutableArray<DrawnLines> drawnLines;

#if DEBUG
    [PrintsToConsole]
#endif
    public override int SolvePart1()
    {
        return SolvePart(Cave.FromLines);
    }
    public override int SolvePart2()
    {
        // This could be more quickly calculated by taking
        // advantage of the "shadows" below the floors of the
        // structures
        // If this turns out too expensive, I will consider
        // rewriting this using the aforementioned algorithm
        return SolvePart(Cave.FromLinesWithFloor);
    }

    private int SolvePart(Func<IReadOnlyList<DrawnLines>, Cave> caveFactory)
    {
        var cave = caveFactory(drawnLines);
        return cave.DropSandUntilFilled();
    }

    protected override void LoadState()
    {
        drawnLines = FileContents.AsSpan().Trim().SelectLines(DrawnLines.Parse);
    }

    private class Cave : PrintableGrid2D<CellType>
    {
        private readonly int xOffset;

        public int SandUnits { get; private set; }

        private Cave(int width, int height, int xOffset)
            : base(width, height)
        {
            this.xOffset = xOffset;
        }

        private void DrawLines(IEnumerable<DrawnLines> linesCollection)
        {
            foreach (var lines in linesCollection)
            {
                DrawLines(lines);
            }
        }
        private void DrawLines(DrawnLines lines)
        {
            foreach (var line in lines.Lines)
            {
                DrawLine(line);
            }
        }
        private void DrawLine(DrawnLine line)
        {
            var difference = line.Difference;
            int steps = Math.Abs(difference.ValueSum);
            var unitLocation = Unit(difference);

            var current = line.Start;
            current.X -= xOffset;

            for (int i = 0; i <= steps; i++)
            {
                this[current] = CellType.Rock;
                current += unitLocation;
            }
        }

        private static Location2D Unit(Location2D location)
        {
            var (x, y) = location;
            int unitX = Unit(x);
            int unitY = Unit(y);
            return (unitX, unitY);
        }
        private static int Unit(int value)
        {
            return value switch
            {
                0 => 0,
                < 0 => -1,
                _ => 1,
            };
        }

        public int DropSandUntilFilled()
        {
            while (true)
            {
                bool placed = DropSand();
                if (!placed)
                    break;
            }
#if DEBUG
            PrintGrid();
#endif
            return SandUnits;
        }

        public bool DropSand()
        {
            var current = StartingSandLocation();

            if (this[current] is not CellType.Air)
            {
                // Our sand cannot even enter the cave; the cave is filled
                return false;
            }

            while (true)
            {
                // Kinda copy-paste but it is what it is
                var down = current + (0, 1);
                if (!IsValidLocation(down))
                {
                    // Preemptively evaluate that we have reached the void
                    return false;
                }

                switch (this[down])
                {
                    case CellType.Air:
                        current = down;
                        continue;
                }
                
                var downLeft = current + (-1, 1);
                if (IsValidLocation(downLeft))
                {
                    switch (this[downLeft])
                    {
                        case CellType.Air:
                            current = downLeft;
                            continue;
                    }
                }

                var downRight = current + (1, 1);
                if (IsValidLocation(downRight))
                {
                    switch (this[downRight])
                    {
                        case CellType.Air:
                            current = downRight;
                            continue;
                    }
                }

                // The sand will come to rest here
                this[current] = CellType.Sand;
                break;
            }

            SandUnits++;
            return true;
        }

        private Location2D StartingSandLocation()
        {
            return (StartingSandX(), 0);
        }
        private int StartingSandX()
        {
            const int realX = 500;
            return realX - xOffset;
        }

        public static Cave FromLines(IReadOnlyList<DrawnLines> lines)
        {
            var bounds = DrawnBounds(lines);
            return FromBounds(bounds, lines);
        }
        public static Cave FromLinesWithFloor(IReadOnlyList<DrawnLines> lines)
        {
            var bounds = DrawnBounds(lines);

            int floorY = bounds.MaxY + 1;
            bounds.Top += 2;

            // Greatly overshoot the width because laziness
            int previousWidth = bounds.Width;
            bounds.Left -= floorY;
            bounds.Right += floorY;

            Location2D floorStart = (bounds.MinX, floorY);
            Location2D floorEnd = (bounds.MaxX, floorY);
            var floorLine = new DrawnLine(floorStart, floorEnd);

            var cave = FromBounds(bounds, lines);
            cave.DrawLine(floorLine);
            return cave;
        }

        private static Cave FromBounds(Rectangle bounds, IEnumerable<DrawnLines> lines)
        {
            int xOffset = bounds.MinX;

            var cave = new Cave(bounds.Width, bounds.MaxY, xOffset);
            cave.DrawLines(lines);
            return cave;
        }

        private static Rectangle DrawnBounds(IReadOnlyList<DrawnLines> lines)
        {
            var bounds = lines[0].DrawnBounds;
            for (int i = 1; i < lines.Count; i++)
            {
                var drawnLines = lines[i];
                bounds = Rectangle.FromRectangles(bounds, drawnLines.DrawnBounds);
            }

            return bounds;
        }

        public override char GetPrintableCharacter(CellType value)
        {
            return value switch
            {
                CellType.Rock => '#',
                CellType.Sand => 'o',
                _ => '.',
            };
        }
    }

    private class DrawnLines
    {
        public ImmutableArray<DrawnLine> Lines { get; }

        public Rectangle DrawnBounds
        {
            get
            {
                int minX = int.MaxValue;
                int maxX = int.MinValue;
                int maxY = int.MinValue;

                foreach (var line in Lines)
                {
                    AssignExtremums(line.Start);
                }
                AssignExtremums(Lines.Last().End);

                var min = new Location2D(minX, 0) - (1, 0);
                var max = new Location2D(maxX, maxY) + (1, 1);

                return Rectangle.FromBounds(min, max);

                void AssignExtremums(Location2D location)
                {
                    minX.AssignMin(location.X);
                    maxX.AssignMax(location.X);
                    maxY.AssignMax(location.Y);
                }
            }
        }

        private DrawnLines(ImmutableArray<DrawnLine> lines)
        {
            Lines = lines;
        }

        public static DrawnLines Parse(SpanString rawSequenceLine)
        {
            var builder = new DrawnLineCollectionBuilder();

            while (true)
            {
                bool hasNext = rawSequenceLine.SplitOnceSpan(" -> ", out var startSpan, out var nextSpan);

                if (!hasNext)
                {
                    ParseAddLocation(builder, rawSequenceLine);
                    break;
                }

                ParseAddLocation(builder, startSpan);
                rawSequenceLine = nextSpan;
            }

            return builder.Finalize();

            static void ParseAddLocation(DrawnLineCollectionBuilder builder, SpanString startSpan)
            {
                var lastLocation = CommonParsing.ParseLocation2D(startSpan);
                builder.Add(lastLocation);
            }
        }

        private class DrawnLineCollectionBuilder
        {
            private readonly ImmutableArray<DrawnLine>.Builder builder;
            private Location2D? lastLocation;

            public DrawnLineCollectionBuilder()
            {
                builder = ImmutableArray.CreateBuilder<DrawnLine>();
            }

            public DrawnLines Finalize()
            {
                return new(builder.ToImmutable());
            }

            public void Add(Location2D location)
            {
                if (lastLocation is null)
                {
                    lastLocation = location;
                    return;
                }

                ConditionallyAddLine(location);
                lastLocation = location;
            }

            private void ConditionallyAddLine(Location2D nextLocation)
            {
                if (ContainsSimilarOppositeLine(nextLocation))
                    return;

                var start = NewLineStartLocation();
                var end = nextLocation;
                var line = new DrawnLine(start, end);
                builder.Add(line);
            }

            private Location2D NewLineStartLocation()
            {
                return lastLocation.GetValueOrDefault();
            }

            private bool ContainsSimilarOppositeLine(Location2D nextLocation)
            {
                if (builder.Count <= 0)
                    return false;

                return builder.Last().Start == nextLocation;
            }
        }
    }

    private readonly record struct DrawnLine(Location2D Start, Location2D End)
    {
        public Location2D Difference => End - Start;
    }

    private enum CellType
    {
        Air,
        Rock,
        Sand,
    }
}
