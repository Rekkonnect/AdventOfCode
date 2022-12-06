using AdventOfCode.Utilities.TwoDimensions;
using System.Data;

namespace AdventOfCode.Problems.Year2018;

public partial class Day10 : Problem<IGlyphGrid, int>
{
    private StarGrid message;
    private int formationTime;

    public override IGlyphGrid SolvePart1()
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

    private partial record Point(Location2D StartingPosition, Location2D Velocity)
    {
        private static readonly Regex configurationPattern = ConfigurationRegex();
        private static readonly Regex locationPattern = LocationRegex();

        [GeneratedRegex("position=(?'position'.*) velocity=(?'velocity'.*)")]
        private static partial Regex ConfigurationRegex();
        [GeneratedRegex("<\\s*(?'x'[-\\d]*),\\s*(?'y'[-\\d]*)>", RegexOptions.Compiled)]
        private static partial Regex LocationRegex();

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

    private class StarGrid : PrintableGlyphGrid2D<bool>
    {
        public StarGrid(Location2D dimensions, Location2D[] values)
            : base(dimensions)
        {
            foreach (var v in values)
                this[v] = true;
        }

        protected override bool IsDrawnPixel(bool value) => value;
    }
}
