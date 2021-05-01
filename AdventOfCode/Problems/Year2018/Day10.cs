using AdventOfCode.Utilities.TwoDimensions;
using Garyon.Extensions;
using Garyon.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2018
{
    public class Day10 : Problem<Day10.StarGrid, int>
    {
        private StarGrid message;
        private int formationTime;

        public override StarGrid SolvePart1()
        {
            return message;
        }
        public override int SolvePart2()
        {
            return formationTime;
        }

        protected override void LoadState()
        {
            var points = new PointCollection(ParsedFileLines(Point.Parse));
            message = points.GetFormedStarGrid(out formationTime);
        }
        protected override void ResetState()
        {
            message = null;
        }

        private record Point(Location2D StartingPosition, Location2D Velocity)
        {
            private static readonly Regex configurationPattern = new(@"position=(?'position'.*) velocity=(?'velocity'.*)");
            private static readonly Regex locationPattern = new(@"<\s*(?'x'[-\d]*),\s*(?'y'[-\d]*)>", RegexOptions.Compiled);

            public Location2D PositionAt(int time) => StartingPosition + Velocity * time;

            public static Point Parse(string raw)
            {
                var groups = configurationPattern.Match(raw).Groups;
                var position = ParseLocation(groups["position"].Value);
                var velocity = ParseLocation(groups["velocity"].Value);
                return new(position, velocity);
            }
            private static Location2D ParseLocation(string location)
            {
                var groups = locationPattern.Match(location).Groups;
                int x = groups["x"].Value.ParseInt32();
                int y = groups["y"].Value.ParseInt32();
                return (x, y);
            }
        }

        private class PointCollection
        {
            private readonly Point[] points;

            public PointCollection(Point[] points)
            {
                this.points = points;
            }

            public StarGrid GetFormedStarGrid(out int time)
            {
                int t = time = GetFormedMessageTime();
                var dimensions = GetPointSpaceDimensionsAt(time, out var offset);
                return new(dimensions, points.Select(p => p.PositionAt(t) - offset).ToArray());
            }

            public int GetFormedMessageTime()
            {
                int currentTime = 1;

                int minSpaceTime = 1;

                long minSpace = EvaluateSpace(time => time * 2);

                currentTime /= 2;
                minSpace = Math.Min(minSpace, EvaluateSpace(time => time + 1));

                return minSpaceTime;

                long EvaluateSpace(Func<int, int> nextTimeEvaluator)
                {
                    long minSpace = long.MaxValue;

                    while (true)
                    {
                        long currentSpace = GetPointSpaceAt(currentTime);

                        if (currentSpace >= minSpace)
                            return minSpace;

                        minSpace = currentSpace;
                        minSpaceTime = currentTime;

                        currentTime = nextTimeEvaluator(currentTime);
                    }
                }
            }

            private long GetPointSpaceAt(int time) => GetPointSpaceDimensionsAt(time, out _).ValueProduct64;
            private Location2D GetPointSpaceDimensionsAt(int time, out Location2D offset) => Location2D.GetLocationSpace(GetPositionsAt(time), out offset);
            private IEnumerable<Location2D> GetPositionsAt(int time) => points.Select(p => p.PositionAt(time));
        }

        public class StarGrid : PrintableGrid2D<bool>
        {
            public StarGrid(Location2D dimensions, Location2D[] values)
                : base(dimensions)
            {
                foreach (var v in values)
                    this[v] = true;
            }

            protected override Dictionary<bool, char> GetPrintableCharacters()
            {
                return new()
                {
                    [true] = '#',
                    [false] = '.',
                };
            }
        }
    }
}
