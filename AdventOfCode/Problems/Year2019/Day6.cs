namespace AdventOfCode.Problems.Year2019;

public class Day6 : Problem<int>
{
    public override int SolvePart1() => General(Part1CountCalculator);
    public override int SolvePart2() => General(Part2CountCalculator);

    private void Part1CountCalculator(ref int count, SortedDictionary<string, OrbitalObject> orbitals)
    {
        foreach (var o in orbitals.Values)
            count += o.TotalOrbitCount;
    }
    private void Part2CountCalculator(ref int count, SortedDictionary<string, OrbitalObject> orbitals)
    {
        count = orbitals["YOU"].CalculateDistanceFrom(orbitals["SAN"]) - 2;
    }

    private int General(CountCalculator countCalculator)
    {
        var lines = FileLines;
        var orbitals = new SortedDictionary<string, OrbitalObject>();
        foreach (var l in lines)
        {
            var split = l.Split(')');
            var orbiteeName = split[0];
            var orbiterName = split[1];
            if (!orbitals.TryGetValue(orbiteeName, out var orbitee))
                orbitals.Add(orbiteeName, orbitee = new OrbitalObject(orbiteeName));
            if (!orbitals.TryGetValue(orbiterName, out var orbiter))
                orbitals.Add(orbiterName, orbiter = new OrbitalObject(orbiterName));
            orbiter.OrbitAround(orbitee);
        }
        int count = 0;
        countCalculator(ref count, orbitals);
        return count;
    }

    public delegate void CountCalculator(ref int count, SortedDictionary<string, OrbitalObject> orbitals);

    public class OrbitalObject
    {
        private OrbitalObject directOrbit;
        private readonly HashSet<OrbitalObject> directOrbiters = new HashSet<OrbitalObject>();

        private int distance = int.MaxValue;

        public readonly string Name;
        public IReadOnlyCollection<OrbitalObject> Orbits
        {
            get
            {
                var result = new HashSet<OrbitalObject>();
                if (directOrbit != null)
                {
                    result.Add(directOrbit);
                    foreach (var indirectOrbit in directOrbit.Orbits)
                        result.Add(indirectOrbit);
                }
                return result;
            }
        }
        public int TotalOrbitCount => Orbits.Count;

        public OrbitalObject(string name) => Name = name;

        public void OrbitAround(OrbitalObject other)
        {
            directOrbit?.directOrbiters.Remove(this);
            directOrbit = other;
            other.directOrbiters.Add(this);
        }
        public int CalculateDistanceFrom(OrbitalObject other)
        {
            distance = 0;
            directOrbit?.CalculateDistanceFrom(other, 1);
            foreach (var o in directOrbiters)
                o.CalculateDistanceFrom(other, 1);
            return other.distance;
        }

        private void CalculateDistanceFrom(OrbitalObject other, int depth)
        {
            if (distance <= depth)
                return;

            distance = depth;

            if (this == other)
                return;

            directOrbit?.CalculateDistanceFrom(other, depth + 1);
            foreach (var o in directOrbiters)
                o.CalculateDistanceFrom(other, depth + 1);
        }

        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name;
    }
}
