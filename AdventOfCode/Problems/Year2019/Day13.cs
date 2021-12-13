using AdventOfCode.Problems.Year2019.Utilities;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2019;

public class Day13 : Problem<int>
{
    private IntcodeComputer computer;

    public override int SolvePart1() => General(Part1GeneralFunction, Part1Returner);
    public override int SolvePart2() => General(Part2GeneralFunction, Part2Returner);

    protected override void LoadState()
    {
        computer = new IntcodeComputer(FileContents);
    }
    protected override void ResetState()
    {
        computer = null;
    }

    private void Part1GeneralFunction() { }
    private void Part2GeneralFunction() => computer.SetMemoryAt(0, 2);

    private int Part1Returner(ValueCounterDictionary<TileType> tileCounts, int score) => tileCounts[TileType.Block];
    private int Part2Returner(ValueCounterDictionary<TileType> tileCounts, int score) => score;

    private T General<T>(GeneralFunction beforeOperation, Returner<T> returner)
    {
        int startingRow = Console.CursorTop;

        var grid = new GameGrid();
        int outputs = 0;
        bool gameRunning = false;

        var currentLocation = new Location2D();
        int score = 0;

        var currentBallLocation = new Location2D();
        var currentBallVelocity = new Location2D(1);
        var currentPaddleLocation = new Location2D();

        computer.Reset();
        computer.ResetEvents();
        computer.InputRequested += InputRequested;
        computer.OutputWritten += OutputWritten;

        beforeOperation();

        computer.RunToHalt();

        return returner(grid.ValueCounters, score);

#if DEBUG
        void PrintGrid()
        {
            Console.SetCursorPosition(0, startingRow);
            grid.PrintGrid();
        }
#endif

        long InputRequested()
        {
            gameRunning = true;
#if DEBUG
            PrintGrid();
#endif
            var newBallLocation = currentBallLocation + currentBallVelocity;
            return Math.Sign(currentBallLocation.X - currentPaddleLocation.X);

        }
        void OutputWritten(long output)
        {
            int intput = (int)output;
            switch (outputs % 3)
            {
                case 0:
                    currentLocation.X = intput;
                    break;
                case 1:
                    currentLocation.Y = intput;
                    break;
                case 2:
                    if (currentLocation == (-1, 0))
                    {
                        score = intput;
                        break;
                    }

                    var (x, y) = currentLocation;
                    switch (grid[x, y] = (TileType)intput)
                    {
                        case TileType.Ball:
                            if (gameRunning)
                                currentBallVelocity = currentLocation - currentBallLocation;
                            currentBallLocation = currentLocation;
                            break;
                        case TileType.HorizontalPaddle:
                            currentPaddleLocation = currentLocation;
                            break;
                    }
                    break;
            }
            outputs++;
        }
    }

    private enum TileType : byte
    {
        Empty,
        Wall,
        Block,
        HorizontalPaddle,
        Ball
    }

    private delegate void GeneralFunction();
    private delegate T Returner<T>(ValueCounterDictionary<TileType> tileCounts, int score);

    private sealed class GameGrid : PrintableGrid2D<TileType>
    {
        public GameGrid()
            : base(50, 25) { }
        public GameGrid(int width, int height)
            : base(width, height) { }

        protected override Dictionary<TileType, char> GetPrintableCharacters()
        {
            return new Dictionary<TileType, char>
            {
                { TileType.Empty , ' ' },
                { TileType.Wall , '#' },
                { TileType.Block , '+' },
                { TileType.HorizontalPaddle , '_' },
                { TileType.Ball , 'O' },
            };
        }
    }
}
