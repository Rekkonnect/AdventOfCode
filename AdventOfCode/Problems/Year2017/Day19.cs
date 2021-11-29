using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2017;

public class Day19 : Problem<string, int>
{
    private PathDiagram diagram;
    private string pathString;
    private int steps;

    public override string SolvePart1()
    {
        return pathString;
    }
    public override int SolvePart2()
    {
        return steps;
    }

    protected override void LoadState()
    {
        diagram = PathDiagram.Parse(FileLines);
        pathString = diagram.GetPathString(out steps);
    }
    protected override void ResetState()
    {
        diagram = null;
    }

    private class PathDiagram : Grid2D<DiagramCell>
    {
        public PathDiagram(int width, int height)
            : base(width, height, default, initializeValueCounters: false) { }

        public string GetPathString(out int steps)
        {
            // Find the starting line
            int x = 0;
            for (; ; x++)
            {
                if (Values[x, 0].Type is DiagramCellType.Pathable)
                    break;
            }

            string result = "";
            steps = 1;

            Location2D location = (x, 0);
            var direction = new DirectionalLocation(Direction.Down, invertY: true);

            while (true)
            {
                // Attempt going forward, then attempt turning left, then turning right from original
                // If neither works; we've found the end
                Location2D next;
                if (!AttemptNextLocation())
                {
                    direction.TurnLeft();
                    if (!AttemptNextLocation())
                    {
                        direction.TurnRight(2);
                        if (!AttemptNextLocation())
                        {
                            return result;
                        }
                    }
                }

                steps++;
                location = next;
                char letter = this[location].Letter;
                if (letter != default)
                    result += letter;

                bool AttemptNextLocation()
                {
                    next = location + direction.LocationOffset;
                    return IsValidLocation(next) && this[next].Type is DiagramCellType.Pathable;
                }
            }
        }

        public static PathDiagram Parse(string[] lines)
        {
            int height = lines.Length;
            int width = lines[0].Length;

            var result = new PathDiagram(width, height);

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    result[x, y] = DiagramCell.Parse(lines[y][x]);

            return result;
        }
    }

    private struct DiagramCell
    {
        public DiagramCellType Type { get; }
        public char Letter { get; }

        public DiagramCell(DiagramCellType type)
            : this(type, default) { }
        public DiagramCell(DiagramCellType type, char letter)
        {
            Type = type;
            Letter = letter;
        }

        public static DiagramCell Parse(char c)
        {
            return c switch
            {
                ' ' => new(DiagramCellType.Empty),
                '|' or '-' or '+' => new(DiagramCellType.Pathable),
                _ => new(DiagramCellType.Pathable, c),
            };
        }

        public override string ToString()
        {
            return $"{Type} {Letter}";
        }
    }

    private enum DiagramCellType
    {
        Empty,
        Pathable,
    }
}
