using AdventOfCode.Functions;
using AdventOfCode.Utilities.FourDimensions;
using AdventOfCSharp.Extensions;

namespace AdventOfCode.Problems.Year2018;

public class Day25 : FinalDay<int>
{
    private Spacetime spacetime;

    public override int SolvePart1()
    {
        return spacetime.IdentifyConstellations();
    }

    protected override void LoadState()
    {
        spacetime = Spacetime.Parse(FileLines);
    }

#nullable enable

    // Is that a bit too unoptimized?
    private class ConstellationCollection
    {
        private readonly HashSet<Constellation> constellations = new();

        public int Count => constellations.Count;

        public void Add(Location4D point)
        {
            Merge(ConnectedConstellations(point).ToList(), point);
        }
        private IEnumerable<Constellation> ConnectedConstellations(Location4D point)
        {
            return constellations.Where(constellation => constellation.CanAdd(point));
        }
        private void Merge(IList<Constellation> merged, Location4D point)
        {
            if (merged.Count is 0)
            {
                constellations.Add(new(point));
                return;
            }

            if (merged.Count is 1)
            {
                merged[0].TryAdd(point);
                return;
            }

            var mergedConstellation = new Constellation(merged);
            constellations.ExceptWith(merged);
            constellations.Add(mergedConstellation);
            mergedConstellation.TryAdd(point);
        }
    }
    private class Constellation
    {
        private readonly List<Location4D> fixedPoints = new();

        public Constellation() { }
        public Constellation(Location4D starting)
        {
            fixedPoints.Add(starting);
        }
        public Constellation(IEnumerable<Constellation> others)
        {
            fixedPoints.AddRange(others.SelectMany(other => other.fixedPoints));
        }

        public bool CanAdd(Location4D point)
        {
            return fixedPoints.Any(fixedPoint => fixedPoint.ManhattanDistance(point) <= 3);
        }
        public bool TryAdd(Location4D point)
        {
            bool added = CanAdd(point);
            if (added)
                fixedPoints.Add(point);

            return added;
        }
    }
    private class Spacetime
    {
        private readonly Location4D[] points;
        private readonly ConstellationCollection constellations = new();

        private Spacetime(Location4D[] locations)
        {
            points = locations;
        }

        public int IdentifyConstellations()
        {
            foreach (var point in points)
                constellations.Add(point);

            return constellations.Count;
        }

        public static Spacetime Parse(string[] rawPoints)
        {
            return new(rawPoints.SelectArray(ParsePoint));
        }
        private static Location4D ParsePoint(string rawPoint)
        {
            var coordinates = rawPoint.ParseInt32Array(',');
            return new(coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
        }
    }
}
