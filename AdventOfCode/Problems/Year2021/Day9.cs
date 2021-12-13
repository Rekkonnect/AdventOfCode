#nullable enable

using AdventOfCode.Functions;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2021;

public class Day9 : Problem<int>
{
    private HeightMap? heightMap;

    public override int SolvePart1()
    {
        return heightMap!.RiskLevelSum;
    }
    public override int SolvePart2()
    {
        return heightMap!.GetLargestRegions(3).Product();
    }

    protected override void LoadState()
    {
        heightMap = HeightMap.Parse(FileLines);
    }
    protected override void ResetState()
    {
        heightMap = null;
    }

    private class HeightMap
    {
        private readonly Grid2D<char> heights;
        private Grid2D<bool>? regioned;

        // Ugly solutions for ugly problems
        public int RiskLevelSum
        {
            get
            {
                int sum = 0;
                var adjacentSlots = new AdjacentValueSlots<char>();
                for (int x = 0; x < heights.Width; x++)
                {
                    for (int y = 0; y < heights.Height; y++)
                    {
                        char centralHeight = heights[x, y];
                        heights.GetAdjacentValueSlots(x, y, adjacentSlots);

                        if (DetermineLowestHeight())
                            sum += GetRiskLevel(centralHeight);

                        adjacentSlots.Reset();

                        bool DetermineLowestHeight()
                        {
                            // Iteration order matters for tiny speed improvement
                            return DetermineValid(Direction.Down)
                                && DetermineValid(Direction.Right)
                                && DetermineValid(Direction.Up)
                                && DetermineValid(Direction.Left);
                        }
                        bool DetermineValid(Direction direction)
                        {
                            char adjacentHeight = adjacentSlots!.ValueOnSlot(direction, out bool hasValue);
                            if (!hasValue)
                                return true;

                            return adjacentHeight > centralHeight;
                        }
                    }
                }
                return sum;
            }
        }

        private HeightMap(Grid2D<char> heightChars)
        {
            heights = new(heightChars);
        }

        public IEnumerable<int> GetLargestRegions(int count)
        {
            if (regioned is null)
                regioned = GetRegionedMap();

            var map = regioned.GetRegionMap(true, out _);
            var counters = new ValueCounterDictionary<int>(map.Cast<int>());
            counters.RemoveKeys(-1, 0);
            return counters.Values.ToArray().Sort().TakeLast(count);
        }

        private Grid2D<bool> GetRegionedMap()
        {
            var result = new Grid2D<bool>(heights.Dimensions);

            for (int x = 0; x < heights.Width; x++)
            {
                for (int y = 0; y < heights.Height; y++)
                {
                    result[x, y] = heights[x, y] is not '9';
                }
            }

            return result;
        }

        private static int GetRiskLevel(char height) => height - '0' + 1;

        public static HeightMap Parse(string[] rawLines)
        {
            int width = rawLines[0].Length;
            int height = rawLines.Length;
            var heights = new Grid2D<char>(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    heights[x, y] = rawLines[y][x];
                }
            }

            return new(heights);
        }
    }
}
