using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using AdventOfCode.Utilities.ThreeDimensions;
using AdventOfCSharp.Extensions;
using Garyon.Functions;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2021;

public partial class Day19 : Problem<int>
{
    private Space space;
    private ScannerComplex scannerComplex;

    public override int SolvePart1()
    {
        scannerComplex.InitializeScanners();
        var absoluteSpace = new AbsoluteSpace(scannerComplex);
        return absoluteSpace.BeaconsCount;
    }
    public override int SolvePart2()
    {
        scannerComplex.InitializeScanners();
        return scannerComplex.Scanners.CachedCartesianProductExcludingSame().Max(ScannerDistanceComparer);

        static int ScannerDistanceComparer((Scanner A, Scanner B) scanners)
        {
            return scanners.A.Offset.ManhattanDistance(scanners.B.Offset);
        }
    }

    protected override void LoadState()
    {
        space = Space.Parse(NormalizedFileContents.Trim());
        scannerComplex = space.ToScannerComplex();
    }
    protected override void ResetState()
    {
        space = null;
    }

#nullable enable

    private sealed class AbsoluteSpace
    {
        private readonly HashSet<Location3D> beacons = new();

        public ScannerComplex ScannerComplex { get; }

        public int BeaconsCount => beacons.Count;

        public AbsoluteSpace(ScannerComplex complex)
        {
            ScannerComplex = complex;

            foreach (var scanner in ScannerComplex.Scanners)
                beacons.UnionWith(scanner.AbsoluteBeacons.Select(beacon => beacon.Location));

#if DEBUG
            var beaconArray = beacons.ToArray();
            Array.Sort(beaconArray, (a, b) => a.X.CompareTo(b.X));
            foreach (var beacon in beaconArray)
            {
                Console.WriteLine(beacon);
            }
#endif
        }
    }

    private sealed class ScannerComplex
    {
        private readonly RelativeScannerData[] relativeScanners;
        private readonly Scanner?[] absoluteScanners;

        public IReadOnlyList<Scanner> Scanners => absoluteScanners.Where(Predicates.NotNull).ToArray() as IReadOnlyList<Scanner>;

        public ScannerComplex(RelativeScannerData[] relativeScannerData)
        {
            relativeScanners = new RelativeScannerData[relativeScannerData.Length];
            foreach (var scanner in relativeScannerData)
                relativeScanners[scanner.ID] = scanner;

            absoluteScanners = new Scanner[relativeScanners.Length];
            // Initialize the first scanner, which is 0
            absoluteScanners[0] = new(relativeScanners[0], Location3D.Zero, Orientation.AllPositiveXYZ);
        }

        public void InitializeScanners()
        {
            bool alive = true;
            while (alive)
            {
                alive = false;

                for (int i = 1; i < absoluteScanners.Length; i++)
                {
                    if (absoluteScanners[i] is not null)
                        continue;

                    alive |= InitializeScanner(i);
                }
            }
        }

        private bool InitializeScanner(int relativeScannerIndex)
        {
            for (int absoluteScannerIndex = 0; absoluteScannerIndex < absoluteScanners.Length; absoluteScannerIndex++)
            {
                var scanner = absoluteScanners[absoluteScannerIndex];
                if (scanner is null)
                    continue;

                if (!scanner.CanBeReferenceForScannerID(relativeScannerIndex))
                    continue;

                if (TryInitializeScanner(relativeScannerIndex, scanner))
                    return true;

                scanner.RegisterFailedReferenceForScannerID(relativeScannerIndex);
            }

            return false;
        }
        private bool TryInitializeScanner(int targetScannerIndex, Scanner absoluteReferenceScanner)
        {
            var relativeTargetScanner = relativeScanners[targetScannerIndex];

            var mapper = new BeaconMapper();

            foreach (var orientation in GetScannerOrientations())
            {
                mapper.Clear();

                foreach (var relativeTargetBeacon in relativeTargetScanner.RelativeBeacons)
                {
                    foreach (var relativeTargetBeaconDistance in relativeTargetBeacon.Distances)
                    {
                        var shuffledRelativeTargetBeaconDistance = relativeTargetBeaconDistance.Distance.Shuffle(orientation);
                        var absoluteReferenceBeaconDistance = absoluteReferenceScanner.Distances.GetDistance(shuffledRelativeTargetBeaconDistance);
                        if (absoluteReferenceBeaconDistance is null)
                            continue;

                        mapper.RegisterEquality(new(absoluteReferenceBeaconDistance, relativeTargetBeaconDistance));
                    }
                }

                if (mapper.MappedBeaconCount < 12)
                {
                    continue;
                }

                var sampleMapped = mapper.GetSampleMapping();

                var referenceLocation = sampleMapped.Key.Location;
                var targetLocation = sampleMapped.Value.Location.Shuffle(orientation);

                var offset = targetLocation - referenceLocation;

                var resultingTargetScanner = new Scanner(relativeTargetScanner, offset, orientation);
                absoluteScanners[targetScannerIndex] = resultingTargetScanner;

                return true;
            }

            return false;
        }

        // Credits: https://www.reddit.com/r/adventofcode/comments/rjwhdv/comment/hp65cya/?utm_source=share&utm_medium=web2x&context=3
        private static Orientation[] GetScannerOrientations()
        {
            /*
             * Order does not matter, so we adjust it to our will
             * 
             * ABC
             * +++
             * +--
             * -+-
             * --+
             * 
             * ACB
             * ---
             * -++
             * +-+
             * ++-
             */
            var result = new Orientation[24];
            CopyInto(result, 0, AxesOrder.XYZ, AxesOrder.XZY);
            CopyInto(result, 8, AxesOrder.YZX, AxesOrder.YXZ);
            CopyInto(result, 16, AxesOrder.ZXY, AxesOrder.ZYX);
            return result;

            static void CopyInto(Orientation[] result, int index, AxesOrder abc, AxesOrder acb)
            {
                var abcOrientation = new Orientation(abc);
                var acbOrientation = new Orientation(acb);

                Span<Orientation> abcSpan = stackalloc Orientation[]
                {
                    abcOrientation with { Signs = (+1, +1, +1) },
                    abcOrientation with { Signs = (+1, -1, -1) },
                    abcOrientation with { Signs = (-1, +1, -1) },
                    abcOrientation with { Signs = (-1, -1, +1) },
                };

                Span<Orientation> acbSpan = stackalloc Orientation[4];
                for (int i = 0; i < 4; i++)
                {
                    acbSpan[i] = abcSpan[i] with
                    {
                        AxesOrder = acb,
                        Signs = -abcSpan[i].Signs
                    };
                }

                abcSpan.CopyTo(new(result, index, 4));
                acbSpan.CopyTo(new(result, index + 4, 4));
            }
        }
    }

    private class BeaconMapper
    {
        private readonly List<BeaconDistanceEquality> equalities = new();
        private readonly HashSet<Beacon> referenceBeacons = new();
        private readonly HashSet<Beacon> targetBeacons = new();

        public int MappedBeaconCount => referenceBeacons.Count;
        public int EqualityCount => equalities.Count;

        public IEnumerable<BeaconDistance> ReferenceDistances => equalities.Select(equality => equality.Reference);
        public IEnumerable<BeaconDistance> TargetDistances => equalities.Select(equality => equality.Target);

        public void RegisterEquality(BeaconDistanceEquality equality)
        {
            equalities.Add(equality);

            referenceBeacons.Add(equality.Reference.Start);
            referenceBeacons.Add(equality.Reference.End);

            targetBeacons.Add(equality.Target.Start);
            targetBeacons.Add(equality.Target.End);
        }

        public KeyValuePair<Beacon, Beacon> GetSampleMapping()
        {
            var referenceDistances = equalities.Select(equality => equality.Reference);
            var targetDistances = equalities.Select(equality => equality.Target);

            var referenceManhattanDistances = MapBeaconsByTotalManhattanDistance(referenceDistances);
            var targetManhattanDistances = MapBeaconsByTotalManhattanDistance(targetDistances);

            var referenceEntry = referenceManhattanDistances.Where(kvp => kvp.Value.Count is 1).First();
            int distance = referenceEntry.Key;
            var referenceBeacon = referenceEntry.Value.First();
            var targetBeacon = targetManhattanDistances[distance].First();
            return new(referenceBeacon, targetBeacon);
        }
        private static FlexibleListDictionary<int, Beacon> MapBeaconsByTotalManhattanDistance(IEnumerable<BeaconDistance> distances)
        {
            var totalManhattanDistances = new FlexibleDictionary<Beacon, int>();

            foreach (var distance in distances)
            {
                totalManhattanDistances[distance.Start] += distance.Distance.ManhattanDistanceFromCenter;
                totalManhattanDistances[distance.End] += distance.Distance.ManhattanDistanceFromCenter;
            }

            var result = new FlexibleListDictionary<int, Beacon>();
            
            foreach (var total in totalManhattanDistances)
            {
                result[total.Value].Add(total.Key);
            }

            return result;
        }

        public void Clear()
        {
            equalities.Clear();
            referenceBeacons.Clear();
            targetBeacons.Clear();
        }
    }

    private sealed record BeaconDistanceEquality(BeaconDistance Reference, BeaconDistance Target)
    {
        public Location3D Distance => Reference.Distance;
    }

    private sealed class Scanner
    {
        private readonly FlexibleList<bool> referenceScannerFailures = new();

        public Location3D Offset { get; }
        public Orientation Orientation { get; }

        public ImmutableArray<AbsoluteBeacon> AbsoluteBeacons { get; }
        public BeaconDistanceStorage Distances { get; } = new(); 

        public Scanner(RelativeScannerData data, Location3D offset, Orientation orientation)
        {
            Offset = offset;
            Orientation = orientation;

            var absoluteBeaconBuilder = ImmutableArray.CreateBuilder<AbsoluteBeacon>(data.RelativeBeacons.Length);
            foreach (var beacon in data.RelativeBeacons)
                absoluteBeaconBuilder.Add(beacon.ToAbsolute(offset, orientation));
            AbsoluteBeacons = absoluteBeaconBuilder.ToImmutable();

            foreach (var (start, end) in AbsoluteBeacons.CachedCartesianProductExcludingSame())
                Distances.Add(new(start, end));
        }

        public bool CanBeReferenceForScannerID(int id) => !referenceScannerFailures[id];
        public bool RegisterFailedReferenceForScannerID(int id) => referenceScannerFailures[id] = true;
    }

    private sealed class BeaconDistanceStorage
    {
        private readonly Dictionary<Location3D, BeaconDistance> distanceDictionary = new();

        public IEnumerable<BeaconDistance> Values => distanceDictionary.Values;

        public BeaconDistance? GetDistance(Location3D distance) => distanceDictionary.ValueOrDefault(distance);

        public bool Contains(BeaconDistance distance) => distanceDictionary.ContainsKey(distance.Distance);
        public void Add(BeaconDistance distance) => distanceDictionary.TryAdd(distance.Distance, distance);

        public void AddRange(IEnumerable<BeaconDistance> distances)
        {
            foreach (var distance in distances)
                Add(distance);
        }
    }

    private sealed record BeaconDistance(Beacon Start, Beacon End)
    {
        public Location3D Distance { get; init; } = End.Location - Start.Location;

        public bool Equals(BeaconDistance? other) => Distance == other?.Distance;

        public override int GetHashCode() => Distance.GetHashCode();
    }

    private abstract record class Beacon(Location3D Location)
    {
        private readonly List<BeaconDistance> distances = new();

        public IEnumerable<BeaconDistance> Distances => distances;

        public void LinkDistance(RelativeBeacon other)
        {
            var distance = new BeaconDistance(this, other);
            distances.Add(distance);
        }

        public sealed override string ToString() => Location.ToString();
    }
    private sealed record class AbsoluteBeacon(Location3D Location)
        : Beacon(Location)
    {
        public override int GetHashCode() => Location.GetHashCode();
    }
    private sealed record class RelativeBeacon(Location3D Location)
        : Beacon(Location)
    {
        public AbsoluteBeacon ToAbsolute(Location3D offset, Orientation orientation)
        {
            return new(Location.Shuffle(orientation) - offset);
        }
    }

    private sealed partial class RelativeScannerData
    {
        private static readonly Regex scannerDataPattern = ScannerDataRegex();

        private readonly Location3D[] positions;

        public int ID { get; }
        public IEnumerable<Location3D> Positions => positions;

        public readonly ImmutableArray<RelativeBeacon> RelativeBeacons;
        public readonly BeaconDistanceStorage Distances = new();

        private RelativeScannerData(int id, Location3D[] positionArray)
        {
            ID = id;
            positions = positionArray;

            var beaconArrayBuilder = ImmutableArray.CreateBuilder<RelativeBeacon>();
            foreach (var position in positionArray)
                beaconArrayBuilder.Add(new RelativeBeacon(position));
            
            RelativeBeacons = beaconArrayBuilder.ToImmutable();

            foreach (var (start, end) in RelativeBeacons.CachedCartesianProductExcludingSame())
                start.LinkDistance(end);
            
            foreach (var beacon in RelativeBeacons)
                Distances.AddRange(beacon.Distances);
        }

        public static RelativeScannerData Empty(int id = -1) => new(id, Array.Empty<Location3D>());

        public static RelativeScannerData Parse(string[] lines)
        {
            int scannerID = scannerDataPattern.Match(lines[0]).Groups["id"].Value.ParseInt32();
            return new(scannerID, lines.Skip(1).Select(ParseLocation).ToArray());
        }
        private static Location3D ParseLocation(string raw)
        {
            var values = raw.ParseInt32Array(',');
            return new(values[0], values[1], values[2]);
        }

        [GeneratedRegex("--- scanner (?'id'\\d*) ---")]
        private static partial Regex ScannerDataRegex();
    }

    private sealed class Space
    {
        private readonly RelativeScannerData[] scanners;

        private Space(RelativeScannerData[] scanners)
        {
            this.scanners = scanners;
        }

        public ScannerComplex ToScannerComplex() => new(scanners);

        public static Space Parse(string rawScanners)
        {
            return new(rawScanners.Split("\n\n").SelectArray(section => RelativeScannerData.Parse(section.GetLines(false))));
        }
    }
}
