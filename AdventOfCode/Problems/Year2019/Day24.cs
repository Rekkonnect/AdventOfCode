using AdventOfCode.Utilities.TwoDimensions;
using System.Collections;
using System.Threading;
using static System.Convert;

namespace AdventOfCode.Problems.Year2019;

public class Day24 : Problem<int>
{
    public override int SolvePart1() => General(Part1GeneralFunction);
    [PartSolution(PartSolutionStatus.WIP)]
    public override int SolvePart2() => General(Part2GeneralFunction);

    private int Part1GeneralFunction(BugGrid grid)
    {
        var knownGridCounts = new BitArray(1 << 25);
        var minutesAtGrid = new int[1 << 25];
        for (int i = 0; i < 1 << 25; i++)
            minutesAtGrid[i] = -1;
        var currentGrid = grid;
        var rating = currentGrid.BiodiversityRating;
        int minutes = 0;

        int startingCursorPosition = Console.CursorTop;

        while (!knownGridCounts[rating])
        {
            knownGridCounts[rating] = true;
            minutesAtGrid[rating] = minutes;
            minutes++;
            currentGrid = currentGrid.GetGridInNextMinute();
            rating = currentGrid.BiodiversityRating;
        }

        return rating;

        void DisplayCurrent()
        {
            Console.SetCursorPosition(0, startingCursorPosition);
            Console.WriteLine($"After {minutes} minutes");
            currentGrid.PrintGrid();
            Thread.Sleep(1000 / 60);
        }
    }
    private int Part2GeneralFunction(BugGrid grid)
    {
        var manager = new RecursiveBugGridManager(new RecursiveBugGrid(grid));
        manager.AdvanceTimeBy(200, CurrentTestCase * 11 - 1);
        return manager.TotalBugCount;
    }

    private int General(GeneralFunction generalFunction)
    {
        var lines = FileLines;

        var grid = BugGrid.Parse(lines);

        return generalFunction(grid);
    }

    private static GridElement ParseGridElement(char c)
    {
        return c switch
        {
            '#' => GridElement.Bug,
            '.' => GridElement.Empty,
            '?' => GridElement.Grid,
        };
    }

    private delegate int GeneralFunction(BugGrid grid);

    private enum GridElement : byte
    {
        Empty,
        Bug,
        Grid,
    }

    private class RecursiveBugGridManager
    {
        private List<RecursiveBugGrid> innerGrids;
        private List<RecursiveBugGrid> outerGrids;

        public RecursiveBugGrid StartingGrid { get; private set; }

        public RecursiveBugGrid InnermostGrid => this[MaxDepth];
        public RecursiveBugGrid OutermostGrid => this[MinDepth];

        public int MinDepth => -outerGrids.Count;
        public int MaxDepth => innerGrids.Count;

        public int TotalBugCount
        {
            get
            {
                int result = 0;
                for (int i = MinDepth; i <= MaxDepth; i++)
                    result += this[i].BugCount;
                return result;
            }
        }

        public RecursiveBugGridManager(RecursiveBugGrid grid, int capacity = 200)
        {
            innerGrids = new List<RecursiveBugGrid>(capacity);
            outerGrids = new List<RecursiveBugGrid>(capacity);
            StartingGrid = grid;
        }
        public RecursiveBugGridManager(RecursiveBugGridManager other)
        {
            CopyFrom(other);
        }

        public void AdvanceToNextMinute()
        {
            var newInnermostGrid = new RecursiveBugGrid { OuterGrid = InnermostGrid };
            var newOutermostGrid = new RecursiveBugGrid { InnerGrid = OutermostGrid };

            var temporaryManager = new RecursiveBugGridManager(this);

            for (int i = MinDepth; i <= MaxDepth; i++)
                temporaryManager[i] = (RecursiveBugGrid)this[i].GetGridInNextMinute();

            newInnermostGrid = (RecursiveBugGrid)newInnermostGrid.GetGridInNextMinute();
            newOutermostGrid = (RecursiveBugGrid)newOutermostGrid.GetGridInNextMinute();

            CopyFrom(temporaryManager);

            InnermostGrid.InnerGrid = newInnermostGrid;
            OutermostGrid.OuterGrid = newOutermostGrid;

            if (newInnermostGrid.BugCount > 0)
                innerGrids.Add(newInnermostGrid);
            if (newOutermostGrid.BugCount > 0)
                outerGrids.Add(newOutermostGrid);
        }
        public void AdvanceTimeBy(int minutes, int printAtMinute = -1)
        {
            int startingCursorPosition = Console.CursorTop;

            for (int i = 0; i < minutes; i++)
            {
                Console.SetCursorPosition(0, startingCursorPosition);
                PrintManager();
                Console.ReadKey(true);

                AdvanceToNextMinute();
            }
        }

        public void CopyTo(RecursiveBugGridManager other) => other.CopyFrom(this);
        public void CopyFrom(RecursiveBugGridManager other)
        {
            innerGrids = new List<RecursiveBugGrid>(other.innerGrids);
            outerGrids = new List<RecursiveBugGrid>(other.outerGrids);
            StartingGrid = other.StartingGrid;
        }

        public void PrintManager()
        {
            for (int i = MinDepth; i <= MaxDepth; i++)
                Console.WriteLine($"{$"{i,3}",-5}\n{this[i]}\n");
        }

        public RecursiveBugGrid this[int depth]
        {
            get
            {
                if (depth > 0)
                    return innerGrids[depth - 1];
                if (depth < 0)
                    return outerGrids[-depth - 1];
                return StartingGrid;
            }
            private set
            {
                if (depth > 0)
                    innerGrids[depth - 1] = value;
                else if (depth < 0)
                    outerGrids[-depth - 1] = value;
                else
                    StartingGrid = value;
            }
        }
    }

    private sealed class RecursiveBugGrid : BugGrid
    {
        private RecursiveBugGrid innerGrid, outerGrid;

        public RecursiveBugGrid InnerGrid
        {
            get => innerGrid;
            set => value.outerGrid = innerGrid = value;
        }
        public RecursiveBugGrid OuterGrid
        {
            get => outerGrid;
            set => value.innerGrid = outerGrid = value;
        }

        public RecursiveBugGrid()
            : base() => InitializeCentralGridTile();
        public RecursiveBugGrid(BugGrid grid)
            : base(grid) => InitializeCentralGridTile();
        public RecursiveBugGrid(RecursiveBugGrid grid)
            : this(grid as BugGrid)
        {
            innerGrid = grid.innerGrid;
            outerGrid = grid.outerGrid;
        }

        public override int GetAdjacentValues(int x, int y, GridElement value)
        {
            int result = base.GetAdjacentValues(x, y, value);

            var distance = (x, y) - Center;

            if (distance.ManhattanDistanceFromCenter == 1) // these should have 8 adjacent cells
            {
                if (InnerGrid != null)
                {
                    if (distance.Y == 0)
                    {
                        var x0 = (distance.X + 1) * 2;
                        for (int y0 = 0; y0 < GridSize; y0++)
                            result += ToInt32(value.Equals(InnerGrid[x0, y0]));
                    }
                    else if (distance.X == 0)
                    {
                        var y0 = (distance.Y + 1) * 2;
                        for (int x0 = 0; x0 < GridSize; x0++)
                            result += ToInt32(value.Equals(InnerGrid[x0, y0]));
                    }
                }
            }
            else if (OuterGrid != null)
            {
                if (x == 0)
                    result += ToInt32(value.Equals(OuterGrid[GridCenter - 1, GridCenter]));
                else if (x == Width - 1)
                    result += ToInt32(value.Equals(OuterGrid[GridCenter + 1, GridCenter]));
                if (y == 0)
                    result += ToInt32(value.Equals(OuterGrid[GridCenter, GridCenter - 1]));
                else if (y == Height - 1)
                    result += ToInt32(value.Equals(OuterGrid[GridCenter, GridCenter + 1]));
            }

            return result;
        }

        protected override BugGrid InitializeNewGrid() => new RecursiveBugGrid(this);

        private void InitializeCentralGridTile() => Values[GridCenter, GridCenter] = GridElement.Grid;

        public override GridElement this[int x, int y]
        {
            get => base[x, y];
            set => base[x, y] = (x, y) == Center ? GridElement.Grid : value;
        }
    }

    private class BugGrid : PrintableGrid2D<GridElement>
    {
        public const int GridSize = 5;
        public const int GridCenter = 2;

        public int BiodiversityRating => GetHashCode();
        public int BugCount => ValueCounters[GridElement.Bug];

        public BugGrid()
            : base(GridSize) { }
        public BugGrid(BugGrid other)
            : base(other) { }

        public override char GetPrintableCharacter(GridElement value)
        {
            return value switch
            {
                GridElement.Empty => '.',
                GridElement.Bug => '#',
                GridElement.Grid => '?',
            };
        }

        public virtual BugGrid GetGridInNextMinute() => ApplyNewGridTransformation(InitializeNewGrid());

        private BugGrid ApplyNewGridTransformation(BugGrid grid)
        {
            for (int x = 0; x < GridSize; x++)
                for (int y = 0; y < GridSize; y++)
                {
                    int adjacentBugs = GetAdjacentValues(x, y, GridElement.Bug);
                    grid[x, y] = adjacentBugs switch
                    {
                        1 => GridElement.Bug,
                        2 => Values[x, y] ^ GridElement.Bug,
                        _ => GridElement.Empty,
                    };
                }

            return grid;
        }
        protected virtual BugGrid InitializeNewGrid() => new BugGrid();

        public static BugGrid Parse(string[] lines)
        {
            var grid = new BugGrid();

            for (int x = 0; x < GridSize; x++)
                for (int y = 0; y < GridSize; y++)
                    grid[x, y] = ParseGridElement(lines[y][x]);

            return grid;
        }

        public override int GetHashCode()
        {
            int result = 0;
            int value = 1;
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++, value <<= 1)
                    if (Values[x, y] == GridElement.Bug)
                        result |= value;
            return result;
        }
    }
}
