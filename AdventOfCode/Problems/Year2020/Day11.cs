using AdventOfCode.Utilities.TwoDimensions;
using static System.Convert;

namespace AdventOfCode.Problems.Year2020;

public class Day11 : Problem<int>
{
    private SeatGrid baseGrid;

    public override int SolvePart1()
    {
        var grid = new SeatGrid(baseGrid);

        bool gridAdjusted;

        do
        {
            gridAdjusted = false;

            var tempGrid = new SeatGrid(grid);

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var cell = grid[x, y];

                    if (cell == SeatGridCell.Floor)
                        continue;

                    var adjacent = grid.GetNeighborValues(x, y, SeatGridCell.Occupied);

                    tempGrid[x, y] = (cell, adjacent) switch
                    {
                        (SeatGridCell.Occupied, >= 4) => SeatGridCell.Vacant,
                        (SeatGridCell.Vacant, 0) => SeatGridCell.Occupied,
                        _ => cell,
                    };

                    if (tempGrid[x, y] != grid[x, y])
                        gridAdjusted = true;
                }
            }

            grid = tempGrid;
        }
        while (gridAdjusted);

        return grid.ValueCounters[SeatGridCell.Occupied];
    }
    public override int SolvePart2()
    {
        var grid = new SeatGrid(baseGrid);

        bool gridAdjusted;

        do
        {
            gridAdjusted = false;

            var tempGrid = new SeatGrid(grid);

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var cell = grid[x, y];

                    if (cell == SeatGridCell.Floor)
                        continue;

                    #region Discover adjacent
                    // I hate this
                    // I fucking hate this
                    // I fucking hate problems with grids because there is no good way to write code that doesn't look shitty
                    int adjacent = 0;

                    int i;

                    // [x - i, y]
                    for (i = 1; (x - i >= 0); i++)
                        if (grid[x - i, y] != SeatGridCell.Floor)
                            break;

                    if (x - i >= 0)
                        adjacent += ToInt32(grid[x - i, y] == SeatGridCell.Occupied);

                    // [x + i, y]
                    for (i = 1; (x + i < grid.Width); i++)
                        if (grid[x + i, y] != SeatGridCell.Floor)
                            break;

                    if (x + i < grid.Width)
                        adjacent += ToInt32(grid[x + i, y] == SeatGridCell.Occupied);

                    // [x, y - i]
                    for (i = 1; (y - i >= 0); i++)
                        if (grid[x, y - i] != SeatGridCell.Floor)
                            break;

                    if (y - i >= 0)
                        adjacent += ToInt32(grid[x, y - i] == SeatGridCell.Occupied);

                    // [x, y + i]
                    for (i = 1; (y + i < grid.Height); i++)
                        if (grid[x, y + i] != SeatGridCell.Floor)
                            break;

                    if (y + i < grid.Height)
                        adjacent += ToInt32(grid[x, y + i] == SeatGridCell.Occupied);

                    // [x - i, y - i]
                    for (i = 1; (x - i >= 0) && (y - i >= 0); i++)
                        if (grid[x - i, y - i] != SeatGridCell.Floor)
                            break;

                    if ((x - i >= 0) && (y - i >= 0))
                        adjacent += ToInt32(grid[x - i, y - i] == SeatGridCell.Occupied);

                    // [x - i, y + i]
                    for (i = 1; (x - i >= 0) && (y + i < grid.Height); i++)
                        if (grid[x - i, y + i] != SeatGridCell.Floor)
                            break;

                    if ((x - i >= 0) && (y + i < grid.Height))
                        adjacent += ToInt32(grid[x - i, y + i] == SeatGridCell.Occupied);

                    // [x + i, y - i]
                    for (i = 1; (x + i < grid.Width) && (y - i >= 0); i++)
                        if (grid[x + i, y - i] != SeatGridCell.Floor)
                            break;

                    if ((x + i < grid.Width) && (y - i >= 0))
                        adjacent += ToInt32(grid[x + i, y - i] == SeatGridCell.Occupied);

                    // [x + i, y + i]
                    for (i = 1; (x + i < grid.Width) && (y + i < grid.Height); i++)
                        if (grid[x + i, y + i] != SeatGridCell.Floor)
                            break;

                    if ((x + i < grid.Width) && (y + i < grid.Height))
                        adjacent += ToInt32(grid[x + i, y + i] == SeatGridCell.Occupied);
                    #endregion

                    tempGrid[x, y] = (cell, adjacent) switch
                    {
                        (SeatGridCell.Occupied, >= 5) => SeatGridCell.Vacant,
                        (SeatGridCell.Vacant, 0) => SeatGridCell.Occupied,
                        _ => cell,
                    };

                    if (tempGrid[x, y] != grid[x, y])
                        gridAdjusted = true;
                }
            }

            grid = tempGrid;
        }
        while (gridAdjusted);

        return grid.ValueCounters[SeatGridCell.Occupied];
    }

    protected override void ResetState()
    {
        baseGrid = null;
    }
    protected override void LoadState()
    {
        var lines = FileLines;

        int width = lines[0].Length;
        int height = lines.Length;

        baseGrid = new(width, height);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                baseGrid[x, y] = GetCell(lines[y][x]);
    }

    private static SeatGridCell GetCell(char c)
    {
        return c switch
        {
            '.' => SeatGridCell.Floor,
            'L' => SeatGridCell.Vacant,
            '#' => SeatGridCell.Occupied,
        };
    }

    private class SeatGrid : Grid2D<SeatGridCell>
    {
        public SeatGrid(int width, int height)
            : base(width, height) { }
        public SeatGrid(SeatGrid other)
            : base(other) { }
    }

    private enum SeatGridCell
    {
        Floor,
        Vacant,
        Occupied
    }
}
