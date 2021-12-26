using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2021;

public class Day25 : FinalDay<int>
{
    private TrafficGrid grid;

    public override int SolvePart1()
    {
        return grid.Clone().IterationsBeforeStale();
    }

    protected override void LoadState()
    {
        grid = TrafficGrid.Parse(FileLines);
    }
    protected override void ResetState()
    {
        grid = null;
    }

#nullable enable

    private enum CellDirection
    {
        None,
        Right,
        Down,
        
        East = Right,
    }

    private sealed class TrafficGrid : PrintableGrid2D<CellDirection>
    {
        public TrafficGrid(int width, int height)
            : base(width, height) { }

        public TrafficGrid(TrafficGrid other)
            : base(other) { }

        public TrafficGrid Clone() => new(this);

        public int IterationsBeforeStale()
        {
            for (int iterations = 1; ; iterations++)
            {
                if (!Iterate())
                    return iterations;
            }
        }

        public bool Iterate()
        {
            bool moved = false;

            for (int y = 0; y < Height; y++)
                moved |= IterateEastward(y);
            
            for (int x = 0; x < Width; x++)
                moved |= IterateDownward(x);
            
            return moved;
        }

        // 'in' used for typing safety -- avoiding mistakingly locally modifying the arguments
        private bool IterateEastward(in int y)
        {
            int offset = 0;
            while (offset < Width)
            {
                if (Values[offset, y] is CellDirection.None)
                    break;

                offset++;
            }

            bool moved = false;
            
            for (int x = 0; x < Width; x++)
            {
                int currentX = x + offset;
                ref var currentCell = ref Values[currentX % Width, y];
                if (currentCell is not CellDirection.East)
                    continue;

                ref var nextCell = ref Values[(currentX + 1) % Width, y];

                if (nextCell is not CellDirection.None)
                    continue;

                moved = true;
                nextCell = CellDirection.East;
                currentCell = CellDirection.None;
                // Skip next
                x++;
            }
            return moved;
        }
        private bool IterateDownward(in int x)
        {
            int offset = 0;
            while (offset < Height)
            {
                if (Values[x, offset] is CellDirection.None)
                    break;

                offset++;
            }

            bool moved = false;
            for (int y = 0; y < Height; y++)
            {
                int currentY = y + offset;
                ref var currentCell = ref Values[x, currentY % Height];
                if (currentCell is not CellDirection.Down)
                    continue;

                ref var nextCell = ref Values[x, (currentY + 1) % Height];

                if (nextCell is not CellDirection.None)
                    continue;

                moved = true;
                nextCell = CellDirection.Down;
                currentCell = CellDirection.None;
                // Skip next
                y++;
            }
            return moved;
        }

        private static CellDirection ParseDirection(char c) => c switch
        {
            'v' => CellDirection.Down,
            '>' => CellDirection.Right,
            _ => CellDirection.None,
        };

        // :)
        // Having to write this exact parsing function with little to no variation is starting to get on my nerves
        // Presumably this is going to be the last year that I'm doing it by hand
        public static TrafficGrid Parse(string[] rawLines)
        {
            int height = rawLines.Length;
            int width = rawLines[0].Length;

            var traffic = new TrafficGrid(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    traffic[x, y] = ParseDirection(rawLines[y][x]);
                }
            }

            return traffic;
        }

        public override char GetPrintableCharacter(CellDirection value)
        {
            return value switch
            {
                CellDirection.Down => 'v',
                CellDirection.East => '>',
                CellDirection.None => '.',
            };
        }
    }
}
