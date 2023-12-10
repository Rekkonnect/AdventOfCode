using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2023;

public class Day10 : Problem<int>
{
    private PipeGrid _grid;

    public override int SolvePart1()
    {
        var loopGrid = _grid.CalculateLoopFromStartPosition();
        return loopGrid.LoopSize / 2;
    }
    [PartSolution(PartSolutionStatus.WIP)]
    public override int SolvePart2()
    {
        return _grid.CalculateLoopFromStartPosition()
            .CalculateEnclosedTileCount();
    }

    protected override void LoadState()
    {
        _grid = ParseGrid(FileLines);
    }
    protected override void ResetState()
    {
        _grid = null;
    }

#nullable enable

    private static PipeGrid ParseGrid(string[] lines)
    {
        int width = lines[0].Length;
        int height = lines.Length;

        var grid = new PipeGrid(width, height);

        for (int y = 0; y < lines.Length; y++)
        {
            for (int x = 0; x < lines[y].Length; x++)
            {
                var c = lines[y][x];
                grid[x, y] = ParseCellType(c);
                if (c is 'S')
                {
                    grid.StartPosition = (x, y);
                }
            }
        }

        grid.ImplyConnectionAtStartPosition();
        grid.CalculateLoopFromStartPosition();

        return grid;
    }

    private static PipeGridCellType ParseCellType(char c)
    {
        return c switch
        {
            '|' => PipeGridCellType.TopBottom,
            '-' => PipeGridCellType.LeftRight,
            'L' => PipeGridCellType.TopRight,
            'J' => PipeGridCellType.TopLeft,
            '7' => PipeGridCellType.BottomLeft,
            'F' => PipeGridCellType.BottomRight,
            _ => PipeGridCellType.Empty,
        };
    }

    private class PipeGrid(int width, int height)
        : Grid2D<PipeGridCellType>(width, height)
    {
        public Location2D StartPosition { get; set; }

        private LoopGrid? _loopGrid;

        public LoopGrid CalculateLoopFromStartPosition()
        {
            if (_loopGrid is not null)
                return _loopGrid;

            var result = new LoopGrid(Width, Height);

            var previousDirection = PipeGridCellType.Empty;
            var start = StartPosition;
            var currentLocation = start;
            int loopSize = 0;
            while (true)
            {
                loopSize++;
                result[currentLocation] = LoopGridCellType.Pipe;

                var cell = this[currentLocation];

                if (CanMoveTo(PipeGridCellType.Left))
                {
                    var next = currentLocation + (-1, 0);
                    if (next == start)
                    {
                        break;
                    }
                    currentLocation = next;
                    previousDirection = PipeGridCellType.Right;
                    continue;
                }
                if (CanMoveTo(PipeGridCellType.Right))
                {
                    var next = currentLocation + (1, 0);
                    if (next == start)
                    {
                        break;
                    }
                    currentLocation = next;
                    previousDirection = PipeGridCellType.Left;
                    continue;
                }
                if (CanMoveTo(PipeGridCellType.Top))
                {
                    var next = currentLocation + (0, -1);
                    if (next == start)
                    {
                        break;
                    }
                    currentLocation = next;
                    previousDirection = PipeGridCellType.Bottom;
                    continue;
                }
                if (CanMoveTo(PipeGridCellType.Bottom))
                {
                    var next = currentLocation + (0, 1);
                    if (next == start)
                    {
                        break;
                    }
                    currentLocation = next;
                    previousDirection = PipeGridCellType.Top;
                    continue;
                }

                break;

                bool CanMoveTo(PipeGridCellType targetDirection)
                {
                    return previousDirection != targetDirection
                        && cell.HasFlag(targetDirection);
                }
            }

            result.LoopSize = loopSize;
            _loopGrid = result;
            return result;
        }

        public void ImplyConnectionAtStartPosition()
        {
            ImplyConnectionAt(StartPosition);
        }

        public void ImplyConnectionAt(Location2D location)
        {
            var (x, y) = location;
            ImplyConnectionAt(x, y);
        }
        public void ImplyConnectionAt(int x, int y)
        {
            if (!IsValidLocation(x, y))
                return;

            var cellType = PipeGridCellType.Empty;

            var right = AccessibleValueOrDefault(x + 1, y);
            if (right.HasFlag(PipeGridCellType.Left))
            {
                cellType |= PipeGridCellType.Right;
            }

            var left = AccessibleValueOrDefault(x - 1, y);
            if (left.HasFlag(PipeGridCellType.Right))
            {
                cellType |= PipeGridCellType.Left;
            }

            var bottom = AccessibleValueOrDefault(x, y + 1);
            if (bottom.HasFlag(PipeGridCellType.Top))
            {
                cellType |= PipeGridCellType.Bottom;
            }

            var top = AccessibleValueOrDefault(x, y - 1);
            if (top.HasFlag(PipeGridCellType.Bottom))
            {
                cellType |= PipeGridCellType.Top;
            }

            this[x, y] = cellType;
        }
    }

    [Flags]
    private enum PipeGridCellType
    {
        Empty,

        Left = 1 << 0,
        Right = 1 << 1,
        Top = 1 << 2,
        Bottom = 1 << 3,

        LeftRight = Left | Right,
        TopBottom = Top | Bottom,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right,
        TopRight = Top | Right,
        TopLeft = Top | Left,
    }

    private class LoopGrid(int width, int height)
        : Grid2D<LoopGridCellType>(width, height)
    {
        private readonly bool[,] _visits = new bool[width, height];

        public int LoopSize { get; set; }

        public int CalculateEnclosedTileCount()
        {
            int outers = 0;
            _visits.Clear();

            for (int x = 0; x < Width; x++)
            {
                Visit(x, 0);
                Visit(x, Height - 1);
            }

            for (int y = 0; y < Height; y++)
            {
                Visit(0, y);
                Visit(Width - 1, y);
            }

            // This algorithm does not support squeezing for the time being

            return TotalElements - LoopSize - outers;

            void Visit(int x, int y)
            {
                if (_visits[x, y])
                    return;

                _visits[x, y] = true;

                var cell = this[x, y];
                if (cell is not LoopGridCellType.Empty)
                    return;

                outers++;

                VisitIfValid(x - 1, y);
                VisitIfValid(x + 1, y);
                VisitIfValid(x, y - 1);
                VisitIfValid(x, y + 1);
            }
            void VisitIfValid(int x, int y)
            {
                if (IsValidLocation(x, y))
                    Visit(x, y);
            }
        }
    }

    private enum LoopGridCellType
    {
        Empty,
        Pipe,
    }
}
