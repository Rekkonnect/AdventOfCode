using AdventOfCode.Problems.Year2019.Utilities;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2019;

public class Day11 : Problem<int, IGlyphGrid>
{
    private const int gridSize = 149;
    private readonly Direction[] orderedDirections =
    {
        Direction.Up,
        Direction.Right,
        Direction.Down,
        Direction.Left,
    };

    public override int SolvePart1() => General(Part1GeneralFunction, Part1Returner);
    public override IGlyphGrid SolvePart2() => General(Part2GeneralFunction, Part2Returner);

    private void Part1GeneralFunction(PanelGrid grid, Location2D startingLocation) { }
    private void Part2GeneralFunction(PanelGrid grid, Location2D startingLocation)
    {
        var (x, y) = startingLocation;
        grid[x, y] = PanelColor.White;
    }

    private int Part1Returner(int paintedPanels, PanelGrid grid) => paintedPanels;
    private IGlyphGrid Part2Returner(int paintedPanels, PanelGrid grid) => grid.Trim();

    private T General<T>(GeneralFunction beforeOperation, Returner<T> returner)
    {
        var grid = new PanelGrid(gridSize);
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                grid[x, y] = PanelColor.Untouched;

        var currentLocation = new Location2D(gridSize / 2, gridSize / 2);
        var currentDirection = Direction.Up;
        int currentDirectionIndex = 0;
        int paintedPanels = 0;
        bool givenFirstOutput = false;

        beforeOperation(grid, currentLocation);

        var computer = new IntcodeComputer(FileContents);
        computer.InputRequested += InputRequested;
        computer.OutputWritten += OutputWritten;
        computer.RunToHalt();

        return returner(paintedPanels, grid);

        long InputRequested()
        {
            givenFirstOutput = false;
            var (x, y) = currentLocation;
            return (int)grid[x, y] & 1;
        }
        void OutputWritten(long output)
        {
            if (givenFirstOutput)
            {
                AddDirectionIndex(GetDirectionIndexOffset((int)output));
                currentLocation.Forward(currentDirection, true, true);
            }
            else
                PaintPanel(currentLocation, (PanelColor)(int)output);

            givenFirstOutput = true;
        }

        int GetDirectionIndexOffset(int output) => output == 0 ? 1 : -1;
        void AddDirectionIndex(int offset)
        {
            currentDirectionIndex = (currentDirectionIndex + offset + 4) % 4;
            currentDirection = orderedDirections[currentDirectionIndex];
        }
        void PaintPanel(Location2D location, PanelColor color)
        {
            var (x, y) = location;
            if (grid[x, y].HasFlag(PanelColor.Untouched))
                paintedPanels++;
            grid[x, y] = color;
        }
    }

    private enum PanelColor : byte
    {
        Black = 0,
        White = 1,
        Untouched = 1 << 2,
    }

    private delegate void GeneralFunction(PanelGrid grid, Location2D startingLocation);
    private delegate T Returner<T>(int paintedPanels, PanelGrid grid);

    private sealed class PanelGrid : PrintableGlyphGrid2D<PanelColor>
    {
        public PanelGrid(int both)
            : base(both) { }
        public PanelGrid(int width, int height)
            : base(width, height) { }

        private PanelGrid(Location2D dimensions)
            : base(dimensions) { }

        public PanelGrid Trim()
        {
            int minX = 0;
            int maxX = Width - 1;

            int minY = 0;
            int maxY = Height - 1;

            while (true)
            {
                if (!IsYEmpty(minX))
                    break;

                minX++;
            }
            while (true)
            {
                if (!IsYEmpty(maxX))
                    break;

                maxX--;
            }

            while (true)
            {
                if (!IsXEmpty(minY, minX, maxX))
                    break;

                minY++;
            }
            while (true)
            {
                if (!IsXEmpty(maxY, minX, maxX))
                    break;

                maxY--;
            }

            Location2D minLocation = (minX, minY);
            Location2D maxLocation = (maxX, maxY);
            var dimensions = maxLocation - minLocation + (1, 1);
            var result = new PanelGrid(dimensions);

            for (int x = 0; x < dimensions.X; x++)
                for (int y = 0; y < dimensions.Y; y++)
                    result[x, y] = this[(x, y) + minLocation];

            return result;
        }

        private bool IsXEmpty(int y) => IsXEmpty(y, 0, Width - 1);
        private bool IsXEmpty(int y, int minX, int maxX)
        {
            for (int x = minX; x <= maxX; x++)
                if (Values[x, y] is PanelColor.White)
                    return false;

            return true;
        }

        private bool IsYEmpty(int x) => IsYEmpty(x, 0, Height - 1);
        private bool IsYEmpty(int x, int minY, int maxY)
        {
            for (int y = minY; y <= maxY; y++)
                if (Values[x, y] is PanelColor.White)
                    return false;

            return true;
        }

        protected override bool IsDrawnPixel(PanelColor value) => value is PanelColor.White;
    }
}
