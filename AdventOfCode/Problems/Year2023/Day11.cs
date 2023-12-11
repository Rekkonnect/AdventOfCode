using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2023;

public class Day11 : Problem<long>
{
    private SpaceImage _space;

    public override long SolvePart1()
    {
        return _space.SumOfAllDistances(1);
    }
    public override long SolvePart2()
    {
        return _space.SumOfAllDistances(1_000_000 - 1);
    }

    protected override void LoadState()
    {
        var lines = FileLines;
        int height = lines.Length;
        int width = lines[0].Length;

        int locationEstimate = (width + height) * 2;
        var locations = ImmutableArray.CreateBuilder<Location2D>(locationEstimate);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char c = lines[y][x];
                if (c is '#')
                {
                    locations.Add((x, y));
                }
            }
        }

        _space = new(locations.ToImmutable());
    }
    protected override void ResetState()
    {
        _space = null;
    }

    private class SpaceImage
    {
        private readonly (long x, long y)[] _transformedLocations;
        private readonly ImmutableArray<Location2D> _galaxies;
        private readonly ImmutableArray<int> _emptyRows;
        private readonly ImmutableArray<int> _emptyColumns;

        public SpaceImage(ImmutableArray<Location2D> galaxies)
        {
            _galaxies = galaxies;
            _transformedLocations = new (long x, long y)[galaxies.Length];
            _emptyRows = GetEmptyRows();
            _emptyColumns = GetEmptyColumns();
        }

        public long SumOfAllDistances(int expansions)
        {
            long sum = 0;

            for (int i = 0; i < _galaxies.Length; i++)
            {
                var galaxy = _galaxies[i];
                TransformLocation(galaxy, expansions, out long x, out long y);
                _transformedLocations[i] = (x, y);
            }

            for (int i = 0; i < _galaxies.Length; i++)
            {
                for (int j = i + 1; j < _galaxies.Length; j++)
                {
                    var (ax, ay) = _transformedLocations[i];
                    var (bx, by) = _transformedLocations[j];

                    sum += ManhattanDistance(ax, ay, bx, by);
                }
            }

            return sum;
        }

        private void TransformLocation(Location2D location, int expansions, out long x, out long y)
        {
            var (x0, y0) = location;
            x = TransformX(x0, expansions);
            y = TransformY(y0, expansions);
        }

        private long TransformX(int x, int expansions)
        {
            return TransformComponent(x, expansions, _emptyColumns);
        }
        private long TransformY(int x, int expansions)
        {
            return TransformComponent(x, expansions, _emptyRows);
        }

        private static long TransformComponent(int x, int expansions, ImmutableArray<int> empty)
        {
            int preceding = GetCountOfSmallerValues(empty, x);
            return x + preceding * (long)expansions;
        }

        private static int GetCountOfSmallerValues(ImmutableArray<int> source, int value)
        {
            // assuming the array is asc sorted
            return GetIndexOfLargestSmaller(source, value) + 1;
        }
        private static int GetIndexOfLargestSmaller(ImmutableArray<int> source, int value)
        {
            // assuming the array is asc sorted

            if (source.Length is 0)
                return -1;

            if (source[0] > value)
                return -1;

            if (source[^1] < value)
                return source.Length - 1;

            int min = 0;
            int max = source.Length - 1;
            while (true)
            {
                if (min == max)
                    return min;

                // we should never reach that point
                if (min > max)
                    return max;

                int midLeft = (max + min) >> 1;
                int midRight = midLeft + 1;

                var midLeftValue = source[midLeft];
                if (midLeftValue >= value)
                {
                    max = midLeft - 1;
                }
                else if (midLeftValue < value)
                {
                    min = midLeft;
                }

                var midRightValue = source[midRight];
                if (midRightValue >= value)
                {
                    max = midRight - 1;
                }
                else if (midRightValue < value)
                {
                    min = midRight;
                }
            }
        }

        private ImmutableArray<int> GetEmptyColumns()
        {
            int maxX = _galaxies.Max(x => x.X);
            var empty = new bool[maxX + 1];
            empty.Fill(true);

            foreach (var galaxy in _galaxies)
            {
                empty[galaxy.X] = false;
            }

            var builder = ImmutableArray.CreateBuilder<int>(maxX);
            for (int i = 0; i < maxX; i++)
            {
                if (empty[i])
                    builder.Add(i);
            }
            return builder.ToImmutable();
        }

        private ImmutableArray<int> GetEmptyRows()
        {
            int maxY = _galaxies.Max(x => x.Y);
            var empty = new bool[maxY + 1];
            empty.Fill(true);

            foreach (var galaxy in _galaxies)
            {
                empty[galaxy.Y] = false;
            }

            var builder = ImmutableArray.CreateBuilder<int>(maxY);
            for (int i = 0; i < maxY; i++)
            {
                if (empty[i])
                    builder.Add(i);
            }
            return builder.ToImmutable();
        }

        private static long ManhattanDistance(long ax, long ay, long bx, long by)
        {
            return Math.Abs(ax - bx) + Math.Abs(ay - by);
        }
    }
}
