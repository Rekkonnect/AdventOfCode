using AdventOfCode.Utilities;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2022;

public class Day8 : Problem<int>
{
    private TreeGrid treeGrid;

    public override int SolvePart1()
    {
        return treeGrid.GetTotalVisibleTrees();
    }
    public override int SolvePart2()
    {
        return treeGrid.GetHighestScenicScore();
    }

    protected override void LoadState()
    {
        var lines = FileLines;
        int height = lines.Length;
        int width = lines[0].Length;
        treeGrid = new(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int treeHeight = lines[y][x].GetNumericValueInteger();
                treeGrid[x, y] = new(treeHeight);
            }
        }
    }

    private sealed class TreeGrid : Grid2D<TreeCell>
    {
        private bool calculatedTreeVisibilities;

        public TreeGrid(int width, int height)
            : base(width, height) { }

        public int GetTotalVisibleTrees()
        {
            CalculateTreeVisibilities();

            var span = Values.AsSpan();
            int trees = 0;
            for (int i = 0; i < span.Length; i++)
            {
                trees += Convert.ToInt32(span[i].IsVisible);
            }
            return trees;
        }

        public int GetHighestScenicScore()
        {
            // NOTE: LinearSpan2D yielded 30% worse performance here

            int max = 0;

            for (int x = 1; x < Width - 1; x++)
            {
                for (int y = 1; y < Height - 1; y++)
                {
                    max.AssignMax(ScenicScoreFor(x, y));
                }
            }

            return max;

            int ScenicScoreFor(int x, int y)
            {
                int gridHeight = Height;
                var gridWidth = Width;

                var maxHeight = Values[x, y].Height;

                int bottom = 1;

                while (true)
                {
                    int nextY = y + bottom;
                    if (nextY >= gridHeight - 1)
                        break;

                    int nextHeight = Values[x, nextY].Height;
                    if (nextHeight >= maxHeight)
                        break;

                    bottom++;
                }

                int top = 1;

                while (true)
                {
                    int nextY = y - top;
                    if (nextY <= 0)
                        break;

                    int nextHeight = Values[x, nextY].Height;
                    if (nextHeight >= maxHeight)
                        break;

                    top++;
                }

                int right = 1;

                while (true)
                {
                    int nextX = x + right;
                    if (nextX >= gridWidth - 1)
                        break;

                    int nextHeight = Values[nextX, y].Height;
                    if (nextHeight >= maxHeight)
                        break;

                    right++;
                }

                int left = 1;

                while (true)
                {
                    int nextX = x - left;
                    if (nextX <= 0)
                        break;

                    int nextHeight = Values[nextX, y].Height;
                    if (nextHeight >= maxHeight)
                        break;

                    left++;
                }

                return top * bottom * right * left;
            }
        }

        private void CalculateTreeVisibilities()
        {
            if (calculatedTreeVisibilities)
                return;

            // Even if the directions are not mapped correctly, part 1 is solvable
            // as long as they're uniquely mapped per evaluation process
            for (int x = 0; x < Width; x++)
            {
                AddVisibleDirection(x, 0, VisibilityDirections.Top);
                AddVisibleDirection(x, ^1, VisibilityDirections.Bottom);
            }
            for (int y = 0; y < Height; y++)
            {
                AddVisibleDirection(0, y, VisibilityDirections.Left);
                AddVisibleDirection(^1, y, VisibilityDirections.Right);
            }

            for (int x = 1; x < Width - 1; x++)
            {
                int visibleHeight = this[x, 0].Height;

                for (int y = 1; y < Height - 1; y++)
                {
                    ref var cell = ref GetRef(x, y);
                    int currentHeight = cell.Height;
                    if (currentHeight <= visibleHeight)
                        continue;

                    cell.AddVisibleDirection(VisibilityDirections.Top);
                    visibleHeight = currentHeight;

                    if (currentHeight >= 9)
                        break;
                }

                visibleHeight = this[x, ^1].Height;

                for (int y = Height - 2; y > 0; y--)
                {
                    ref var cell = ref GetRef(x, y);
                    int currentHeight = cell.Height;
                    if (currentHeight <= visibleHeight)
                        continue;

                    cell.AddVisibleDirection(VisibilityDirections.Bottom);
                    visibleHeight = currentHeight;

                    if (currentHeight >= 9)
                        break;
                }
            }
            for (int y = 1; y < Height - 1; y++)
            {
                int visibleHeight = this[0, y].Height;

                for (int x = 1; x < Width - 1; x++)
                {
                    ref var cell = ref GetRef(x, y);
                    int currentHeight = cell.Height;
                    if (currentHeight <= visibleHeight)
                        continue;

                    cell.AddVisibleDirection(VisibilityDirections.Left);
                    visibleHeight = currentHeight;

                    if (currentHeight >= 9)
                        break;
                }

                visibleHeight = this[^1, y].Height;

                for (int x = Width - 2; x > 0; x--)
                {
                    ref var cell = ref GetRef(x, y);
                    int currentHeight = cell.Height;
                    if (currentHeight <= visibleHeight)
                        continue;

                    cell.AddVisibleDirection(VisibilityDirections.Right);
                    visibleHeight = currentHeight;

                    if (currentHeight >= 9)
                        break;
                }
            }

            calculatedTreeVisibilities = true;
        }
        private void AddVisibleDirection(int x, int y, VisibilityDirections direction)
        {
            ref var cell = ref GetRef(x, y);
            cell.AddVisibleDirection(direction);
        }
        private void AddVisibleDirection(Index x, Index y, VisibilityDirections direction)
        {
            ref var cell = ref GetRef(x, y);
            cell.AddVisibleDirection(direction);
        }
    }

    private struct TreeCell
    {
        private short height;

        public int Height
        {
            get => height;
            private set
            {
                height = (short)value;
            }
        }
        public VisibilityDirections VisibilityDirections { get; private set; }

        public bool IsVisible => VisibilityDirections is not 0;

        public TreeCell(int height)
        {
            Height = height;
        }

        public void AddVisibleDirection(VisibilityDirections direction)
        {
            VisibilityDirections |= direction;
        }
    }

    [Flags]
    private enum VisibilityDirections : short
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
    }
}
