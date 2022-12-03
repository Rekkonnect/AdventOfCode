using AdventOfCode.Functions;
using AdventOfCode.Utilities.ThreeDimensions;
using AdventOfCSharp;
using Garyon.Objects.Enumerators;
using System.Collections;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2018;

public class Day23 : Problem<int>
{
    private NanobotCollection nanobots;

    public override int SolvePart1()
    {
        return nanobots.StrongestCountInRange;
    }
    [PartSolution(PartSolutionStatus.WIP)]
    public override int SolvePart2()
    {
        var signalOverlappings = nanobots.Nanobots.Select(NanobotSignalOverlapping.InitialForSingle)
                                                  .ToList();

        int largestOverlapping = 1;

        while (signalOverlappings.Any())
        {
            var nextOverlappings = new List<NanobotSignalOverlapping>();

            // This threatens to kill runtime
            var homogenousPermutation = signalOverlappings.CachedHomogenousCartesianProduct();

            foreach (var (a, b) in homogenousPermutation)
            {
                var combined = a.Combine(b);
                if (combined is null)
                    continue;

                nextOverlappings.Add(combined);
                largestOverlapping.AssignMax(combined.NanobotIDs.Count);
            }

            signalOverlappings = nextOverlappings;
        }

        return largestOverlapping;
    }

    protected override void LoadState()
    {
        nanobots = NanobotCollection.Parse(FileLines);
    }

#nullable enable

    private record NanobotCollection(ImmutableArray<Nanobot> Nanobots)
    {
        public Nanobot Strongest => Nanobots.MaxBy(n => n.Radius);

        public int StrongestCountInRange => CountInRange(Strongest);

        public int CountInRange(Nanobot selectedNanobot)
        {
            return selectedNanobot.CountInRange(Nanobots);
        }
        public int CountInRangeForOther(Nanobot selectedNanobot)
        {
            return selectedNanobot.CountInRangeForOther(Nanobots);
        }

        public static NanobotCollection Parse(string[] lines)
        {
            return new(lines.Select(Nanobot.Parse).ToImmutableArray());
        }
    }

    private record NanobotSignalOverlapping(Octahedron SignalSpace, IReadOnlySet<int> NanobotIDs)
    {
        public NanobotSignalOverlapping? Combine(NanobotSignalOverlapping other)
        {
            bool overlaps = SignalSpace.Overlaps(other.SignalSpace, out var resultOverlap);
            if (!overlaps)
                return null;

            var overlappingIDs = new HashSet<int>();
            overlappingIDs.AddRange(NanobotIDs);
            overlappingIDs.AddRange(other.NanobotIDs);
            return new(resultOverlap, overlappingIDs);
        }

        public static NanobotSignalOverlapping InitialForSingle(Nanobot nanobot)
        {
            var idSet = new SingleValueSet<int>(nanobot.ID);
            return new(nanobot.SignalOctahedron, idSet);
        }

        private sealed record SingleValueSet<T>(T Value) : IReadOnlySet<T>
            where T : notnull
        {
            public int Count => 1;

            public SingleElementCollection<T> SingleElementCollection => new(Value);

            public bool Contains(T item)
            {
                return Value.Equals(item);
            }

            public bool IsProperSubsetOf(IEnumerable<T> other)
            {
                return IsSubsetOf(other)
                    && other.Count() > 1;
            }
            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                return !other.Any();
            }
            public bool IsSubsetOf(IEnumerable<T> other)
            {
                return Overlaps(other);
            }
            public bool IsSupersetOf(IEnumerable<T> other)
            {
                return SetEquals(other);
            }
            public bool Overlaps(IEnumerable<T> other)
            {
                return other.Contains(Value);
            }
            public bool SetEquals(IEnumerable<T> other)
            {
                return other.Count() is 1
                    && other.Single().Equals(Value);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return SingleElementCollection.GetEnumerator()!;
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }

    // XR, YR, ZR are made-up names to represent the rotated version of the X, Y, Z axes
    // THE MATH BELOW IS WRONG; TODO
    /*
     * The origin of the octahedron is    ^
     * shown in the figure:              /|\
     *                         Origin-> <--->
     * The axes XR and YR travel across  \|/     |  - XR = (X, -Y, 1)
     * the X and Y axes, given a stable   v      |  - YR = (X, Y, 1)
     * Z position, whereas the ZR axis           |  - ZR = (X, 1, Z)
     * traverses the X and Z axis for a given Z  |  
     */
    private readonly record struct Octahedron(Location3D Origin, int XR, int YR, int ZR)
    {
        public static Octahedron Invalid { get; } = new(default, -1, -1, -1);

        public bool Overlaps(Octahedron other, out Octahedron overlapping)
        {
            overlapping = Invalid;

            // TODO: Implement the logic

            return false;
        }

        public static Octahedron Regular(Location3D origin, int side)
        {
            return new(origin, side, side, side);
        }

        #region Operators
        public static Location3D Offset(Location3D origin, int x, int y, int z)
        {
            // TODO: Wrong
            var ox = OffsetXR(origin, x);
            var oy = OffsetYR(ox, y);
            var oz = OffsetZR(oy, z);
            return oz;
        }

        // Perhaps right?
        public static Location3D OffsetXR(Location3D origin, int x)
        {
            return origin + (2 * x, -2 * x, 0);
        }
        public static Location3D OffsetYR(Location3D origin, int y)
        {
            return origin + (2 * y, 2 * y, 0);
        }
        public static Location3D OffsetZR(Location3D origin, int z)
        {
            return origin + (2 * z, 0, 2 * z);
        }
        #endregion
    }

    private record struct Nanobot(int ID, Location3D Position, int Radius)
    {
        private static readonly Regex nanobotDescriptionPattern = new(@"pos=<(?'position'[\-\d\,]*)>, r=(?'radius'.*)");

        public Location3D LeftmostPosition => Position - (Radius, 0, 0);
        public Octahedron SignalOctahedron => Octahedron.Regular(LeftmostPosition, Radius);

        public bool IsInRange(Nanobot other)
        {
            return Position.ManhattanDistance(other.Position) <= Radius;
        }
        public bool IsInRangeForOther(Nanobot other)
        {
            return other.IsInRange(this);
        }

        public int CountInRange(IEnumerable<Nanobot> nanobots)
        {
            return nanobots.Count(IsInRange);
        }
        public int CountInRangeForOther(IEnumerable<Nanobot> nanobots)
        {
            return nanobots.Count(IsInRangeForOther);
        }

        public static Nanobot Parse(string raw, int index)
        {
            var match = nanobotDescriptionPattern.Match(raw);
            var positionString = match.Groups["position"].Value;
            int radius = match.Groups["radius"].Value.ParseInt32();
            var positionValues = positionString.ParseInt32Array(',');
            var position = new Location3D(positionValues[0], positionValues[1], positionValues[2]);

            return new(index, position, radius);
        }

        public override int GetHashCode()
        {
            return ID;
        }
    }
}
