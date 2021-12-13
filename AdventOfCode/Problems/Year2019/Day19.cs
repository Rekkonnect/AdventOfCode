using AdventOfCode.Problems.Year2019.Utilities;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2019;

public class Day19 : Problem<int>
{
    public override int SolvePart1() => General(50, Part1Returner);
    public override int SolvePart2() => General(1300, Part2Returner);

    public int Part1Returner(BeamGrid grid, int squareSize = 100) => grid.BeamPoints;
    public int Part2Returner(BeamGrid grid, int squareSize = 100)
    {
        for (int y = 0; y < grid.Height; y++)
        {
            int x = grid.GetMedianXOfFirstRegion(y, PointType.Beam);

            bool result = ValidateSquare(x, y);
            if (result)
            {
                Location2D closestLocation = (x, y);
                for (int i = 0; i < x; i++)
                    for (int j = 0; j < y; j++)
                    {
                        if (closestLocation.ManhattanDistanceFromCenter < new Location2D((x - i, y - j)).ManhattanDistanceFromCenter)
                            continue;
                        if (ValidateSquare(x - i, y - j))
                            closestLocation = (x - i, y - j);
                    }
                return closestLocation.X * 10000 + closestLocation.Y;
            }
        }
        return -1;

        bool ValidateSquare(int x, int y)
        {
            if (x == -1)
                return false;

            bool isValid = true;
            for (int x0 = x; isValid && x0 < x + squareSize; x0++)
                isValid = grid[x0, y] == PointType.Beam;

            if (!isValid)
                return false;

            for (int y0 = y; isValid && y0 < y + squareSize; y0++)
                isValid = grid[x, y0] == PointType.Beam;

            return isValid;
        }
    }

    private int General(int gridSize, Returner returner)
    {
        var grid = new BeamGrid(gridSize);

        var computer = new IntcodeComputer(FileContents);
        int previousFirstY = 0;
        int previousLastY = 0;

        for (int x = 0; x < gridSize; x++)
        {
            int firstY = -1;
            int lastY = -1;

            for (int y = previousFirstY; y < gridSize; y++)
            {
                int output = (int)computer.RunToHalt(null, x, y);
                grid[x, y] = (PointType)output;
                computer.Reset();
                if (grid[x, y] == PointType.Beam)
                {
                    firstY = y;
                    if (previousLastY < firstY)
                        previousLastY = firstY;
                    for (int y0 = firstY; y0 < previousLastY; y0++) // cover gaps
                        grid[x, y0] = PointType.Beam;
                    break;
                }
            }
            for (int y = previousLastY; y < gridSize; y++)
            {
                int output = (int)computer.RunToHalt(null, x, y);
                grid[x, y] = (PointType)output;
                computer.Reset();
                if (grid[x, y] == PointType.Air)
                {
                    lastY = y;
                    break;
                }
            }

            if (firstY > -1)
                previousFirstY = firstY;
            if (lastY > -1)
                previousLastY = lastY;
        }

        return returner(grid);
    }

    private static PointType ParsePointType(char c)
    {
        return c switch
        {
            '.' => PointType.Air,
            '#' => PointType.Beam,
        };
    }

    public enum PointType : byte
    {
        Air,
        Beam,
    }

    private delegate int Returner(BeamGrid grid, int squareSize = 100);

    public sealed class BeamGrid : PrintableGrid2D<PointType>
    {
        public int BeamPoints => ValueCounters[PointType.Beam];

        public BeamGrid(int both) : base(both) { }
        public BeamGrid(int width, int height) : base(width, height) { }

        protected override Dictionary<PointType, char> GetPrintableCharacters()
        {
            return new Dictionary<PointType, char>
                {
                    { PointType.Air , '.' },
                    { PointType.Beam , '#' },
                };
        }

        public static BeamGrid Parse(string[] s)
        {
            int width = s[0].Length;
            int height = s.Length;
            var result = new BeamGrid(width, height);

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    result[x, y] = ParsePointType(s[y][x]);

            return result;
        }
    }
}
