﻿using AdventOfCode.Utilities;
using AdventOfCode.Utilities.TwoDimensions;
using AdventOfCSharp.Extensions;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2018;

public partial class Day6 : Problem<int>
{
    private ImmutableArray<Location2D> importantLocations;

    public override int SolvePart1()
    {
        var grid = new DangerousLocationGrid(importantLocations);
        grid.CalculateClosestLocations();
        return grid.GetLargestNonInfiniteArea();
    }
    public override int SolvePart2()
    {
        var grid = new SafeLocationGrid(importantLocations);
        return grid.GetMaxDistanceRegion(10000);
    }

    protected override void LoadState()
    {
        importantLocations = FileContents.SelectLines(ParseLocation);
    }
    protected override void ResetState()
    {
        importantLocations = default;
    }

    private static Location2D ParseLocation(SpanString span)
    {
        span.SplitOnce(", ", out var xSpan, out var ySpan);
        int x = xSpan.ParseInt32();
        int y = ySpan.ParseInt32();
        return (x, y);
    }

    private class SafeLocationGrid : LocationGridBase<bool>
    {
        public SafeLocationGrid(ImmutableArray<Location2D> importantLocations)
            : base(importantLocations) { }

        public int GetMaxDistanceRegion(int maxDistance)
        {
            foreach (var cellLocation in EnumerateWholeGridLocations())
            {
                int totalDistance = 0;

                foreach (var importantLocation in ImportantLocations)
                {
                    totalDistance += cellLocation.ManhattanDistance(importantLocation);
                    if (totalDistance > maxDistance)
                        goto abort;
                }

                this[cellLocation] = true;

            abort:;
            }

            return ValueCounters[true];
        }
    }

    private class DangerousLocationGrid : LocationGridBase<DangerousLocationGridCell>
    {
        private readonly IDMap<Location2D> locationIDs;

        public DangerousLocationGrid(ImmutableArray<Location2D> importantLocations)
            : base(importantLocations)
        {
            locationIDs = new(ImportantLocations);
        }

        public int GetLargestNonInfiniteArea()
        {
            var excludedLocationIDs = new HashSet<int>();

            AddExcluded(GetXLine(0));
            AddExcluded(GetXLine(^1));
            AddExcluded(GetYLine(0));
            AddExcluded(GetYLine(^1));

            int max = 0;
            foreach (var locationID in locationIDs.ValuesByID)
            {
                int id = locationID.Key;
                var location = locationID.Value;

                if (excludedLocationIDs.Contains(id))
                    continue;

                int area = ValueCounters[new(id)];
                if (area > max)
                    max = area;
            }

            return max;

            void AddExcluded(DangerousLocationGridCell[] line)
            {
                excludedLocationIDs.AddRange(line.Select(c => c.ClosestLocation));
            }
        }

        public void CalculateClosestLocations()
        {
            foreach (var location in EnumerateWholeGridLocations())
            {
                var closest = ImportantLocations.MinSource(l => l.ManhattanDistance(location));
                this[location] = closest switch
                {
                    (0, 0) => DangerousLocationGridCell.NoClosestLocation,
                    _ => new DangerousLocationGridCell(locationIDs.GetID(closest)),
                };
            }
        }
    }

    private struct DangerousLocationGridCell
    {
        private const int noClosestLocation = -1;

        public static DangerousLocationGridCell NoClosestLocation => new(noClosestLocation);

        public int ClosestLocation { get; }
        public bool HasClosestLocation => ClosestLocation != noClosestLocation;

        public DangerousLocationGridCell(int c) => ClosestLocation = c;
    }

    private abstract class LocationGridBase<T> : Grid2D<T>
    {
        protected readonly ImmutableArray<Location2D> ImportantLocations;

        protected LocationGridBase(ImmutableArray<Location2D> importantLocations)
            : base(importantLocations.MaxSource(l => l.ValueSum) + (1, 1))
        {
            ImportantLocations = importantLocations;
        }
    }
}
