using AdventOfCode.Utilities.TwoDimensions;
using Garyon.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2018;

public class Day17 : Problem<int>
{
    private WaterSystem system;

    [PartSolution(PartSolutionStatus.Uninitialized)]
    public override int SolvePart1()
    {
        return -1;
    }
    [PartSolution(PartSolutionStatus.Uninitialized)]
    public override int SolvePart2()
    {
        return -1;
    }

    protected override void LoadState()
    {
        system = WaterSystem.FromVeins(ParsedFileLines(ClayVein.Parse));
    }
    protected override void ResetState()
    {
        system = null;
    }

    private class WaterSystem : Grid2D<GroundType>
    {
        public int MinY { get; }

        public WaterSystem(int width, int height, int minY)
            : base(width, height, default, true)
        {
            MinY = minY;
        }

        public static WaterSystem FromVeins(ClayVein[] veins)
        {
            int width = veins.Max(vein => vein.X.End.Value) + 1;
            int height = veins.Max(vein => vein.Y.End.Value);
            int minY = veins.Max(vein => vein.Y.Start.Value);

            var result = new WaterSystem(width, height, minY);

            foreach (var vein in veins)
                result[vein.X, vein.Y] = GroundType.Clay;

            return result;
        }
    }

    private record ClayVein(Range X, Range Y)
    {
        private static readonly Regex veinPattern = new(@"(?'a'[xy])=(?'aValue'\d*), (?'b'[xy])=(?'bStart'\d*)\.\.(?'bEnd'\d*)", RegexOptions.Compiled);

        public static ClayVein Parse(string raw)
        {
            var groups = veinPattern.Match(raw).Groups;

            Range x = default;
            Range y = default;

            char a = groups["a"].Value[0];
            int aStart = groups["aValue"].Value.ParseInt32();
            var aRange = aStart..(aStart + 1);
            AssignRange(a, aRange, ref x, ref y);

            char b = groups["b"].Value[0];
            int bStart = groups["bStart"].Value.ParseInt32();
            int bEnd = groups["bEnd"].Value.ParseInt32();
            var bRange = bStart..(bEnd + 1);
            AssignRange(b, bRange, ref x, ref y);

            return new(x, y);
        }

        private static void AssignRange(char rangeName, Range value, ref Range x, ref Range y)
        {
            switch (rangeName)
            {
                case 'x':
                    x = value;
                    break;
                case 'y':
                    y = value;
                    break;
            }
        }
    }

    private enum GroundType
    {
        Sand,
        Clay,
        FixedWater,
        FlowingWater,
    }
}
