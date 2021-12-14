//#define ARCADE
//#define REALTIME

using AdventOfCode.Problems.Year2019.Utilities;
using AdventOfCode.Utilities.TwoDimensions;
#if ARCADE
using Garyon.Functions;
#if REALTIME
using System.Threading;
#endif
#endif

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
        int initialBlocks = 0;

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

#if ARCADE
        if (gameRunning)
            PrintScreen();
#endif

        return returner(grid.ValueCounters, score);

#if ARCADE
        void PrintScreen()
        {
            Console.SetCursorPosition(0, startingRow + 1);
            ConsoleUtilities.WriteWithColor("SCORE ", ConsoleColor.Cyan, false);
            ConsoleUtilities.WriteLineWithColor($"{score,6}", GetProgressColor(), true);
            grid.PrintGrid();
#if REALTIME
            Thread.Sleep(1);
#endif
        }

        ConsoleColor GetProgressColor()
        {
            double remainingBlocks = grid.ValueCounters[TileType.Block];
            double percentage = remainingBlocks * 100 / initialBlocks;
            return percentage switch
            {
                0 => ConsoleColor.Green,
                < 20 => ConsoleColor.DarkGreen,
                < 40 => ConsoleColor.Magenta,
                < 60 => ConsoleColor.DarkMagenta,
                < 80 => ConsoleColor.DarkRed,
                _ => ConsoleColor.Red,
            };
        }
#endif

        long InputRequested()
        {
            if (!gameRunning)
            {
                gameRunning = true;
                initialBlocks = grid.ValueCounters[TileType.Block];
            }
#if ARCADE
            PrintScreen();
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

        public override char GetPrintableCharacter(TileType value)
        {
            return value switch
            {
                TileType.Empty => ' ',
                TileType.Wall => '#',
                TileType.Block => '+',
                TileType.HorizontalPaddle => '_',
                TileType.Ball => 'O',
            };
        }
    }
}
