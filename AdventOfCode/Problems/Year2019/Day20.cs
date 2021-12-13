using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2019;

public class Day20 : Problem<int>
{
    [PartSolution(PartSolutionStatus.WIP)]
    public override int SolvePart1() => General(Part1GeneralFunction);
    public override int SolvePart2() => General(Part2GeneralFunction);

    private int Part1GeneralFunction(MazeGrid maze)
    {
        return 0;
    }
    private int Part2GeneralFunction(MazeGrid maze)
    {
        return 0;
    }

    private int General(GeneralFunction generalFunction)
    {
        var lines = FileLines;

        int height = lines.Length;
        int width = lines[0].Length;

        var maze = new MazeGrid(width, height);

        return generalFunction(maze);
    }

    private static bool IsValidIndex(int value, int upperBound) => value >= 0 && value < upperBound;

    private delegate int GeneralFunction(MazeGrid maze);

    private enum MazeElementType : ushort
    {
        Empty,
        Wall,
        Open,
        Portal,
    }
    private struct MazeElement
    {
        public static MazeElement NewEmpty => new MazeElement(MazeElementType.Empty);
        public static MazeElement NewWall => new MazeElement(MazeElementType.Wall);
        public static MazeElement NewOpen => new MazeElement(MazeElementType.Open);
        public static MazeElement NewPortal => new MazeElement(MazeElementType.Portal);

        public bool IsWallOrOpen => Type == MazeElementType.Wall || Type == MazeElementType.Open;

        public MazeElementType Type { get; private set; }
        public string Label
        {
            get => Type > MazeElementType.Portal ? $"{GetLabelCharAt(0)}{GetLabelCharAt(1)}" : null;
            set
            {
                if (!Type.HasFlag(MazeElementType.Portal))
                    return;
                Type = (MazeElementType)((value == null ? default : value[0] << 7 | value[1] << 2) | (int)MazeElementType.Portal);
            }
        }

        public MazeElement(MazeElementType type, string label = null)
        {
            Type = type;
            Label = label;
        }

        private char GetLabelCharAt(int index) => (char)(((int)Type >> 7 - index * 5) + 'A');

        public static MazeElement Parse(char c)
        {
            if (char.IsLetter(c))
                return new MazeElement(MazeElementType.Portal);
            return c switch
            {
                ' ' => NewEmpty,
                '#' => NewWall,
                '.' => NewOpen,
            };
        }

        public static bool operator ==(MazeElement left, MazeElement right) => left.Type == right.Type && left.Label == right.Label;
        public static bool operator !=(MazeElement left, MazeElement right) => left.Type != right.Type || left.Label != right.Label;

        public override bool Equals(object obj) => this == (MazeElement)obj;
        public override int GetHashCode() => Type.GetHashCode() ^ Label.GetHashCode();
    }

    private sealed class MazeGrid : PrintableGrid2D<MazeElement>
    {
        public MazeGrid(int width, int height) : base(width, height) { }
        public MazeGrid(MazeGrid other) : base(other) { }

        protected override Dictionary<MazeElement, char> GetPrintableCharacters()
        {
            return new Dictionary<MazeElement, char>
                {
                    { MazeElement.NewEmpty, ' ' },
                    { MazeElement.NewOpen, '#' },
                    { MazeElement.NewWall, '.' },
                };
        }

        protected override string FinalizeResultingString(StringBuilder builder)
        {

            return builder.ToString();
        }
        public static MazeGrid Parse(string[] lines)
        {
            int width = lines[0].Length;
            int height = lines.Length;

            var grid = new MazeGrid(width, height);
            for (int y = 1; y < height - 1; y++)
                for (int x = 1; x < width - 1; x++)
                    grid[x, y] = MazeElement.Parse(lines[y][x]);

            var center = new Location2D(width, height) / 2;

            var centerHoleRectangle = new Rectangle();

            var currentLocation = center;
            while (!grid[currentLocation].IsWallOrOpen)
                currentLocation.X--;
            centerHoleRectangle.Left = currentLocation.X + 1;

            currentLocation = center;
            while (!grid[currentLocation].IsWallOrOpen)
                currentLocation.X++;
            centerHoleRectangle.Right = currentLocation.X - 1;

            currentLocation = center;
            while (!grid[currentLocation].IsWallOrOpen)
                currentLocation.Y--;
            centerHoleRectangle.Bottom = currentLocation.Y + 1;

            currentLocation = center;
            while (!grid[currentLocation].IsWallOrOpen)
                currentLocation.Y++;
            centerHoleRectangle.Top = currentLocation.Y - 1;

            // Reset potential portal elements in the grid
            for (int x = centerHoleRectangle.Left; x < centerHoleRectangle.Right; x++)
                grid[x, centerHoleRectangle.Top] = grid[x, centerHoleRectangle.Bottom] = MazeElement.NewEmpty;
            for (int y = centerHoleRectangle.Bottom; y < centerHoleRectangle.Top; y++)
                grid[centerHoleRectangle.Left, y] = grid[centerHoleRectangle.Right, y] = MazeElement.NewEmpty;

            // Parse portal labels - do it tomorrow

            return grid;
        }
    }
}
