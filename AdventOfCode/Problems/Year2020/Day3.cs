using AdventOfCode.Utilities;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2020
{
    public class Day3 : Problem2<long>
    {
        public override long SolvePart1()
        {
            long trees = 0;

            var grid = ParseGrid();
            for (int i = 0; i < grid.Height; i++)
            {
                if (grid[i * 3, i] == TreeGridCellType.Tree)
                    trees++;
            }

            return trees;
        }
        public override long SolvePart2()
        {
            var locationOffsets = new Location2D[] { (1, 1), (3, 1), (5, 1), (7, 1), (1, 2) };
            var slopes = new ValueCounterDictionary<Location2D>(locationOffsets, 0);

            var grid = ParseGrid();
            for (int i = 0; i < grid.Height; i++)
            {
                foreach (var o in locationOffsets)
                {
                    var location = o * i;
                    if (location.Y >= grid.Height)
                        continue;

                    if (grid[location] == TreeGridCellType.Tree)
                        slopes.Add(o);
                }
            }

            long product = 1;
            foreach (var s in slopes)
                product *= s.Value;
            return product;
        }

        private TreeGrid ParseGrid()
        {
            var lines = FileLines;

            int width = lines[0].Length;
            int height = lines.Length;

            var grid = new TreeGrid(width, height);

            for (int y = 0; y < lines.Length; y++)
                for (int x = 0; x < lines[y].Length; x++)
                    grid[x, y] = ParseCellType(lines[y][x]);
            
            return grid;
        }

        private static TreeGridCellType ParseCellType(char c)
        {
            return c switch
            {
                '#' => TreeGridCellType.Tree,
                _ => TreeGridCellType.Empty
            };
        }

        private class TreeGrid : RepeatableGrid<TreeGridCellType>
        {
            public TreeGrid(int width, int height)
                : base(width, height) { }
        }

        private enum TreeGridCellType
        {
            Empty = 0,
            Tree = 1,
        }
    }
}
