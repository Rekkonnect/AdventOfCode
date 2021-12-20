using AdventOfCode.Utilities;
using AdventOfCode.Utilities.ThreeDimensions;
using AdventOfCSharp.Extensions;
using Garyon.Functions;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2021;

public class Day19 : Problem<int>
{
    private Space space;
    private ScannerComplex scannerComplex;

    // Unoptimized, incorrect, incapable
    [PartSolution(PartSolutionStatus.WIP)]
    public override int SolvePart1()
    {
        scannerComplex.InitializeScanners();
        var absoluteSpace = new AbsoluteSpace(scannerComplex);
        return absoluteSpace.BeaconsCount;
    }
    public override int SolvePart2()
    {
        return -1;
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
        }
    }

    private sealed class ScannerComplex
    {
        private readonly RelativeScannerData[] orderedRelativeScanners;
        private readonly Scanner?[] scanners;

        public IEnumerable<Scanner> Scanners => scanners.Where(Predicates.NotNull) as IEnumerable<Scanner>;

        public ScannerComplex(RelativeScannerData[] relativeScanners)
        {
            orderedRelativeScanners = new RelativeScannerData[relativeScanners.Length];
            foreach (var scanner in relativeScanners)
                orderedRelativeScanners[scanner.ID] = scanner;

            scanners = new Scanner[orderedRelativeScanners.Length];
            // Initialize the first scanner, which is 0
            scanners[0] = new(orderedRelativeScanners[0], Location3D.Zero, Orientation.AllPositiveXYZ);
        }

        public void InitializeScanners()
        {
            bool alive;
            do
            {
                alive = false;

                for (int i = 1; i < scanners.Length; i++)
                {
                    if (scanners[i] is not null)
                        continue;

                    alive = true;
                    InitializeScanner(i);
                }
            }
            while (alive);
        }

        private void InitializeScanner(int scannerIndex)
        {
            for (int absoluteScannerIndex = 0; absoluteScannerIndex < scanners.Length; absoluteScannerIndex++)
            {
                var scanner = scanners[absoluteScannerIndex];
                if (scanner is null)
                    continue;

                if (!scanner.CanBeReferenceForScannerID(scannerIndex))
                    continue;

                if (TryInitializeScanner(scannerIndex, absoluteScannerIndex))
                    return;

                scanner.RegisterFailedReferenceForScannerID(scannerIndex);
            }
        }
        private bool TryInitializeScanner(int targetScannerIndex, int referenceScannerIndex)
        {
            var relativeTargetScanner = orderedRelativeScanners[targetScannerIndex];
            var relativeReferenceScanner = orderedRelativeScanners[referenceScannerIndex];
            var absoluteReferenceScanner = scanners[referenceScannerIndex]!;

            var mapper = new BeaconMapper();

            foreach (var orientation in GetScannerOrientations())
            {
                mapper.Clear();

                foreach (var relativeTargetBeacon in relativeTargetScanner.RelativeBeacons)
                {
                    foreach (var relativeTargetBeaconDistance in relativeTargetBeacon.Distances)
                    {
                        var normalizedRelativeTargetBeaconDistance = relativeTargetBeaconDistance.Distance.Shuffle(orientation);
                        var relativeReferenceBeaconDistance = relativeReferenceScanner.Distances.GetDistance(normalizedRelativeTargetBeaconDistance);
                        if (relativeReferenceBeaconDistance is null)
                            continue;

                        mapper.RegisterEquality(new(relativeReferenceBeaconDistance, relativeTargetBeaconDistance));
                    }
                }

                if (mapper.MappedBeaconCount < 12)
                {
                    continue;
                }

                var targetOrientation = absoluteReferenceScanner.Orientation.InnerProduct(orientation);

                var sampleMapped = mapper.GetSampleMapping();

                var referenceLocation = sampleMapped.Key.Location.Shuffle(absoluteReferenceScanner.Orientation);
                var targetLocation = sampleMapped.Value.Location.Shuffle(targetOrientation);

                var absoluteReferenceLocation = referenceLocation + absoluteReferenceScanner.Offset;
                var offset = absoluteReferenceLocation - targetLocation;

                var resultingTargetScanner = new Scanner(relativeTargetScanner, offset, targetOrientation);
                scanners[targetScannerIndex] = resultingTargetScanner;

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

        // TODO: Won't be useful most likely
        private readonly Dictionary<Beacon, Beacon> mapping = new();

        public int MappedBeaconCount => referenceBeacons.Count;
        public int EqualityCount => equalities.Count;

        public IReadOnlyDictionary<Beacon, Beacon> MappedBeacons => mapping;

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
            mapping.Clear();
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

        public Scanner(RelativeScannerData data, Location3D offset, Orientation orientation)
        {
            Offset = offset;
            Orientation = orientation;

            var absoluteBeaconBuilder = ImmutableArray.CreateBuilder<AbsoluteBeacon>(data.RelativeBeacons.Length);
            foreach (var beacon in data.RelativeBeacons)
                absoluteBeaconBuilder.Add(beacon.ToAbsolute(offset, orientation));
            AbsoluteBeacons = absoluteBeaconBuilder.ToImmutable();
        }

        public bool CanBeReferenceForScannerID(int id) => !referenceScannerFailures[id];
        public bool RegisterFailedReferenceForScannerID(int id) => referenceScannerFailures[id] = true;
    }

    private sealed class BeaconDistanceStorage
    {
        private readonly Dictionary<Location3D, BeaconDistance> distanceDictionary = new();

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

        public BeaconDistance Transform(Orientation orientation) => this with { Distance = Distance.Shuffle(orientation) };

        // TODO: Consider removing
        public bool MatchesBeaconIndices(BeaconDistance other) => Start.Index == other.Start.Index && End.Index == other.End.Index;

        public bool Equals(BeaconDistance? other) => Distance == other?.Distance;

        public override int GetHashCode() => Distance.GetHashCode();
    }

    private abstract record class Beacon(Location3D Location)
    {
        private readonly List<BeaconDistance> distances = new();

        public IEnumerable<BeaconDistance> Distances => distances;
        public int Index => distances.Count;

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
        // This might need the distances from other aboslute beacons

        public override int GetHashCode() => Location.GetHashCode();
    }
    private sealed record class RelativeBeacon(Location3D Location)
        : Beacon(Location)
    {
        public AbsoluteBeacon ToAbsolute(Location3D offset, Orientation orientation)
        {
            return new((Location).Shuffle(orientation) + offset);
        }
    }

    private sealed class RelativeScannerData
    {
        private static readonly Regex scannerDataPattern = new(@"--- scanner (?'id'\d*) ---");

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
            for (int i = 0; i < RelativeBeacons.Length; i++)
            {
                for (int j = i + 1; j < RelativeBeacons.Length; j++)
                {
                    RelativeBeacons[i].LinkDistance(RelativeBeacons[j]);
                }
            }

            foreach (var beacon in RelativeBeacons)
                Distances.AddRange(beacon.Distances);
        }

        public static RelativeScannerData Parse(string[] lines)
        {
            int scannerID = scannerDataPattern.Match(lines[0]).Groups["id"].Value.ParseInt32();
            return new(scannerID, lines.Skip(1).Select(ParseLocation).ToArray());
        }
        private static Location3D ParseLocation(string raw)
        {
            var values = raw.Split(',').SelectArray(int.Parse);
            return new(values[0], values[1], values[2]);
        }
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
