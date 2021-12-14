using AdventOfCode.Utilities.TwoDimensions;
using System.Numerics;

namespace AdventOfCode.Problems.Year2016;

public class Day13 : Problem<int>
{
    private int favoriteNumber;

    public override int SolvePart1()
    {
        return GetShortestPath(31, 39, out _).Count;
    }
    public override int SolvePart2()
    {
        GetShortestPath(50, 50, out var distances);
        return distances.Cast<int>().Count(d => d <= 50);
    }

    private List<Direction> GetShortestPath(int width, int height, out int[,] distances)
    {
        var maze = new Maze(width, height, favoriteNumber);
        return maze.GetShortestPath((1, 1), (width, height), out distances);
    }

    protected override void LoadState()
    {
        favoriteNumber = FileContents.ParseInt32();
    }

    private sealed class Maze : PrintableGrid2D<MazeCell>
    {
        private readonly uint number;

        public Maze(int width, int height, int favoriteNumber)
            : base(width * 2, height * 2)
        {
            number = (uint)favoriteNumber;
        }

        protected override bool IsImpassableObject(MazeCell element) => element is MazeCell.Wall;

        public override char GetPrintableCharacter(MazeCell value)
        {
            return value switch
            {
                MazeCell.Unknown => '?',
                MazeCell.OpenSpace => '.',
                MazeCell.Wall => '#',
            };
        }

        public sealed override MazeCell this[int x, int y]
        {
            get
            {
                // Override to lazily evaluate the required values without additional logic
                var value = Values[x, y];
                if (value is MazeCell.Unknown)
                {
                    // Reducing the expression
                    // x^2 + 3x + 2xy + y + y^2
                    // x(x + 3) + 2xy + y(y + 1)
                    uint expressionResult = (uint)(x * (x + 3) + 2 * x * y + y * (y + 1) + number);
                    int bitCountParity = BitOperations.PopCount(expressionResult) % 2;
                    Values[x, y] = value = bitCountParity is 0 ? MazeCell.OpenSpace : MazeCell.Wall;
                }

                return value;
            }
            set { }
        }
    }
    private enum MazeCell
    {
        Unknown,
        OpenSpace,
        Wall,
    }
}
