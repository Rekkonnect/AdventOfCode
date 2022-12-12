using AdventOfCode.Utilities;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2022;

public class Day12 : Problem<int>
{
    private HeightMap heightMap;

    public override int SolvePart1()
    {
        return heightMap.GetShortestPath();
    }
    public override int SolvePart2()
    {
        return heightMap.GetBestShortestPath();
    }

    protected override void LoadState()
    {
        heightMap = HeightMap.Parse(NormalizedFileContents);
    }

    private class HeightMap : Grid2D<int>
    {
        private Location2D start;
        private Location2D end;

        public HeightMap(int width, int height)
            : base(width, height) { }

        private bool CanMoveTo(Location2D current, Location2D next)
        {
            int heightDifference = this[next] - this[current];
            return heightDifference <= 1;
        }

        public int GetBestShortestPath()
        {
            int minDistance = int.MaxValue;

            var grid = new int[Width, Height];
            grid.AsSpan().Fill(int.MaxValue);

            AnalyzeGridDepth(end, Location2D.Zero, 0);

            return minDistance;

            void AnalyzeGridDepth(Location2D previous, Location2D offset, int depth)
            {
                var location = previous + offset;

                if (!IsValidLocation(location))
                    return;

                if (depth >= minDistance)
                    return;

                if (!CanMoveTo(location, previous))
                    return;

                var (x1, y1) = location;
                if (grid[x1, y1] <= depth)
                    return;

                grid[x1, y1] = depth;

                if (this[location] is 0)
                {
                    // We have already assessed that the depth is
                    // below the current min distance, so we overwrite it
                    minDistance = depth;
                }

                var currentDirection = new DirectionalLocation(Direction.Right);
                for (int i = 0; i < 4; i++)
                {
                    AnalyzeGridDepth(location, currentDirection.LocationOffset, depth + 1);

                    currentDirection.TurnRight();
                }
            }
        }
        public int GetShortestPath()
        {
            var grid = new int[Width, Height];
            grid.AsSpan().Fill(int.MaxValue);

            AnalyzeGridDepth(start, Location2D.Zero, 0);

            return grid[end.X, end.Y];

            void AnalyzeGridDepth(Location2D previous, Location2D offset, int depth)
            {
                var location = previous + offset;

                if (!IsValidLocation(location))
                    return;

                if (depth >= grid[end.X, end.Y])
                    return;

                if (!CanMoveTo(previous, location))
                    return;

                var (x1, y1) = location;
                if (grid[x1, y1] <= depth)
                    return;

                grid[x1, y1] = depth;

                if (location == end)
                    return;

                var currentDirection = new DirectionalLocation(Direction.Right);
                for (int i = 0; i < 4; i++)
                {
                    AnalyzeGridDepth(location, currentDirection.LocationOffset, depth + 1);

                    currentDirection.TurnRight();
                }
            }
        }

        public static HeightMap Parse(string normalizedMultiline)
        {
            var lines = normalizedMultiline.AsSpan().Trim().SplitToStrings('\n');
            int width = lines[0].Length;
            int height = lines.Count;

            var result = new HeightMap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char c = lines[y][x];
                    char heightChar = c;
                    switch (c)
                    {
                        case 'S':
                            result.start = (x, y);
                            heightChar = 'a';
                            break;

                        case 'E':
                            result.end = (x, y);
                            heightChar = 'z';
                            break;
                    }

                    int localHeight = heightChar - 'a';
                    result[x, y] = localHeight;
                }
            }

            return result;
        }
    }
}
