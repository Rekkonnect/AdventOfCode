using AdventOfCode.Functions;
using AdventOfCode.Utilities.TwoDimensions;
using Garyon.Functions;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2018;

public class Day22 : Problem<int>
{
    private Cave cave;

    public override int SolvePart1()
    {
        return cave.TotalRiskLevelForTargetRectangle();
    }
    [PartSolution(PartSolutionStatus.WIP)]
    public override int SolvePart2()
    {
        return -1;
    }

    protected override void LoadState()
    {
        cave = Cave.Parse(FileLines);
    }

#nullable enable

    private class RescueAdventure
    {
        private static Equipment GetCommonEquipment(RegionType a, RegionType b)
        {
            if (a > b)
            {
                Misc.Swap(ref a, ref b);
            }
            return (a, b) switch
            {
                (RegionType.Rocky, RegionType.Narrow) => Equipment.ClimbingGear,
                (RegionType.Rocky, RegionType.Wet) => Equipment.Torch,
                (RegionType.Narrow, RegionType.Wet) => Equipment.None,

                _ => throw null!, // Unreachable
            };
        }

        private static bool AllowsEquipment(RegionType type, Equipment equipment)
        {
            return type switch
            {
                RegionType.Rocky  => equipment is Equipment.Torch
                                               or Equipment.ClimbingGear,

                RegionType.Narrow => equipment is Equipment.None
                                               or Equipment.Torch,

                RegionType.Wet    => equipment is Equipment.None
                                               or Equipment.ClimbingGear,

                _ => false,
            };
        }
    }

    // For this problem, int32 will suffice
    // No values can cause any overflow, unless the input is too massive
    private record Cave(int Depth, Location2D Target)
    {
        private readonly Grid2D<CaveRegion> regions = new(Target + (1, 1), CaveRegion.Invalid);

        private CaveRegion GetRegion(Location2D location)
        {
            return GetRegion(location.X, location.Y);
        }
        private CaveRegion GetRegion(int x, int y)
        {
            ref var target = ref regions.GetUnsafeRef(x, y);
            if (target.IsInvalid)
            {
                int geologicIndex = CalculateGeologicIndex(x, y);
                target = CaveRegion.ForCave(this, geologicIndex);
            }

            return target;
        }
        private int CalculateGeologicIndex(int x, int y)
        {
            // Sorted as described in the problem
            if ((x, y) is (0, 0))
                return 0;

            if ((x, y) == Target)
                return 0;

            if (y is 0)
                return x * 16807;

            if (x is 0)
                return y * 48271;

            int erosionA = GetRegion(x - 1, y).ErosionLevel;
            int erosionB = GetRegion(x, y - 1).ErosionLevel;
            return erosionA * erosionB;
        }

        public int TotalRiskLevelForTargetRectangle()
        {
            int totalRiskLevel = 0;

            var locations = regions.EnumerateWholeGridLocations();
            foreach (var location in locations)
            {
                var region = GetRegion(location);
                int riskLevel = CalculateRiskLevel(region.RegionType);
                totalRiskLevel += riskLevel;
            }
            return totalRiskLevel;
        }

        private static int CalculateRiskLevel(RegionType regionType)
        {
            return regionType switch
            {
                RegionType.Rocky => 0,
                RegionType.Wet => 1,
                RegionType.Narrow => 2,

                RegionType.Invalid => 0,
            };
        }

        public static Cave Parse(string[] lines)
        {
            // At least it's efficient when running
            const string delimiter = ": ";

            int depth = lines[0].SubstringSpanAfter(delimiter).ParseInt32();
            var locationSpan = lines[1].SubstringSpanAfter(delimiter);
            int locationDelimiter = locationSpan.IndexOf(',');
            int x = locationSpan[..locationDelimiter].ParseInt32();
            int y = locationSpan[(locationDelimiter + 1)..].ParseInt32();
            return new(depth, (x, y));
        }

        private readonly record struct CaveRegion(int GeologicIndex, int ErosionLevel)
        {
            public static CaveRegion Invalid { get; } = new(-1, -1);

            public bool IsInvalid => GeologicIndex < 0 || ErosionLevel < 0;

            public RegionType RegionType => (ErosionLevel % 3) switch
            {
                0 => RegionType.Rocky,
                1 => RegionType.Wet,
                2 => RegionType.Narrow,

                _ => RegionType.Invalid, // should be unreachable
            };

            public static CaveRegion ForCave(Cave cave, int geologicIndex)
            {
                int erosionLevel = (geologicIndex + cave.Depth) % 20183;
                return new(geologicIndex, erosionLevel);
            }
        }
    }
    private enum RegionType
    {
        Invalid,

        Rocky,
        Wet,
        Narrow,
    }
    private enum Equipment
    {
        None,
        Torch,
        ClimbingGear,
    }
}
