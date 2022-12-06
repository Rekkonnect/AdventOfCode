using AdventOfCode.Utilities.ThreeDimensions;
using AdventOfCSharp.Extensions;
using Garyon.Objects;

namespace AdventOfCode.Problems.Year2021;

public partial class Day22 : Problem<ulong>
{
    private CommandList commands;

    public override ulong SolvePart1()
    {
        return SolvePart(commands.LimitedCommands);
    }
    [PartSolution(PartSolutionStatus.Unoptimized)]
    public override ulong SolvePart2()
    {
        return SolvePart(commands.Commands);
    }

    private static ulong SolvePart(IReadOnlyCollection<Command> commands)
    {
        var grid = CompactStateGrid.ForCommands(commands);
        grid.ApplyCommands(commands);
        return grid.TotalState(LightState.On);
    }

    protected override void LoadState()
    {
        commands = CommandList.Parse(FileLines);
    }
    protected override void ResetState()
    {
        commands = null;
    }

#nullable enable

    private enum LightState
    {
        Off,
        On
    }

    // The optimal solution would involve using B/R-trees
    // which is the target for this solution

    #region Compressed grid - kinda slow
    private sealed class CompactStateGrid : Grid3D<LightState>
    {
        private readonly CompressedCoordinateMapper mapper;

        private CompactStateGrid(CompressedCoordinateMapper coordinateMapper)
            : base(coordinateMapper.CompressedDimensions)
        {
            mapper = coordinateMapper;
        }

        public unsafe ulong TotalState(LightState state)
        {
            ulong sum = 0;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        if (Values[x, y, z] != state)
                            continue;

                        sum += mapper.CompressedVolume((x, y, z));
                    }
                }
            }

            return sum;
        }

        private unsafe void SetRectangle(Location3D start, Location3D end, LightState state)
        {
            for (int x = start.X; x < end.X; x++)
            {
                for (int y = start.Y; y < end.Y; y++)
                {
                    for (int z = start.Z; z < end.Z; z++)
                    {
                        Values[x, y, z] = state;
                    }
                }
            }
        }

        private void ApplyCommand(Command command)
        {
            var compressedCommand = mapper.Compress(command);
            var rectangle = compressedCommand.Rectangle;
            SetRectangle(rectangle.Start, rectangle.End, command.State);
        }
        public void ApplyCommands(IReadOnlyCollection<Command> commads)
        {
            foreach (var command in commads)
                ApplyCommand(command);
        }

        public static CompactStateGrid ForCommands(IReadOnlyCollection<Command> commands)
        {
            var mapper = new CompressedCoordinateMapper(commands.Select(command => command.Rectangle));
            return new(mapper);
        }
    }

    private sealed class CompressedCoordinateMapper
    {
        private readonly Dictionary<int, int> compressedX = new();
        private readonly Dictionary<int, int> compressedY = new();
        private readonly Dictionary<int, int> compressedZ = new();

        private readonly CompressedRegion[] compressedRegionsX, compressedRegionsY, compressedRegionsZ;

        public Location3D CompressedDimensions => (compressedRegionsX.Length, compressedRegionsY.Length, compressedRegionsZ.Length);

        public CompressedCoordinateMapper(IEnumerable<Rectangle3D> rectangles)
        {
            var foundX = new HashSet<int>();
            var foundY = new HashSet<int>();
            var foundZ = new HashSet<int>();

            foreach (var rectangle in rectangles)
            {
                RegisterFoundLocation(rectangle.Start);
                RegisterFoundLocation(rectangle.End);

                void RegisterFoundLocation(Location3D location)
                {
                    AddNearby(foundX, location.X);
                    AddNearby(foundY, location.Y);
                    AddNearby(foundZ, location.Z);
                }
                static void AddNearby(HashSet<int> foundValues, int index)
                {
                    foundValues.Add(index);
                }
            }

            MapDiscoveredCoordinates(foundX, compressedX, ref compressedRegionsX);
            MapDiscoveredCoordinates(foundY, compressedY, ref compressedRegionsY);
            MapDiscoveredCoordinates(foundZ, compressedZ, ref compressedRegionsZ);
        }

        public ulong CompressedVolume(Location3D compressedLocation)
        {
            return (ulong)compressedRegionsX[compressedLocation.X].Size
                 * (ulong)compressedRegionsY[compressedLocation.Y].Size
                 * (ulong)compressedRegionsZ[compressedLocation.Z].Size;
        }

        public Command Compress(Command original)
        {
            return original with { Rectangle = Compress(original.Rectangle) };
        }
        public Rectangle3D Compress(Rectangle3D original)
        {
            return new(Compress(original.Start), Compress(original.End));
        }
        public Location3D Compress(Location3D original)
        {
            return (compressedX[original.X], compressedY[original.Y], compressedZ[original.Z]);
        }

        private static void MapDiscoveredCoordinates(IReadOnlyCollection<int> discovered, Dictionary<int, int> compressedIndices, ref CompressedRegion[] compressedRegions)
        {
            var sorted = discovered.ToArray().Sort();

            compressedRegions = new CompressedRegion[sorted.Length];

            for (int i = 0; i < sorted.Length; i++)
            {
                int mappedIndex = i;
                compressedIndices.Add(sorted[i], mappedIndex);

                var mappedRegion = new CompressedRegion(sorted[i]);
                if (i + 1 < sorted.Length)
                    mappedRegion.End = sorted[i + 1];
                compressedRegions[mappedIndex] = mappedRegion;
            }
        }

        private record struct CompressedRegion(int Start, int End)
        {
            public int Size => End - Start;

            public CompressedRegion(int both)
                : this(both, both) { }
        }
    }
    #endregion

    private sealed record Rectangle3D(Location3D Start, Location3D End)
    {
        public Location3D Dimensions => End - Start;
        public ulong Volume => (ulong)Dimensions.ValueProduct64;

        public bool Contains(Location3D location)
        {
            return Start.SatisfiesComparisonPerCoordinate(location, ComparisonKinds.LessOrEqual)
                && End.SatisfiesComparisonPerCoordinate(location, ComparisonKinds.Greater);
        }
        public bool OverlapsWith(Rectangle3D other)
        {
            return (ContainsX(other.Start.X) || ContainsX(other.End.X))
                && (ContainsY(other.Start.Y) || ContainsY(other.End.Y))
                && (ContainsZ(other.Start.Z) || ContainsZ(other.End.Z));
        }
        
        private bool ContainsX(int x) => Start.X <= x && x <= End.X;
        private bool ContainsY(int y) => Start.Y <= y && y <= End.Y;
        private bool ContainsZ(int z) => Start.Z <= z && z <= End.Z;

        public bool FullyContains(Rectangle3D other)
        {
            return Start.SatisfiesComparisonPerCoordinate(other.Start, ComparisonKinds.LessOrEqual)
                && End.SatisfiesComparisonPerCoordinate(other.End, ComparisonKinds.GreaterOrEqual);
        }

        public static Rectangle3D Min(Rectangle3D left, Rectangle3D right)
        {
            return new(Location3D.Min(left.Start, right.Start), Location3D.Min(left.End, right.End));
        }
        public static Rectangle3D Max(Rectangle3D left, Rectangle3D right)
        {
            return new(Location3D.Max(left.Start, right.Start), Location3D.Max(left.End, right.End));
        }
    }
    private class CommandList
    {
        private static readonly Rectangle3D smallRectangle = new(new(-50), new(51));

        private readonly Command[] commands;
        private readonly Command[] limitedCommands;

        public IReadOnlyCollection<Command> Commands => commands;
        public IReadOnlyCollection<Command> LimitedCommands => limitedCommands;

        public CommandList(IEnumerable<Command> commandCollection)
        {
            commands = commandCollection.ToArray();
            limitedCommands = commandCollection.Where(command => smallRectangle.FullyContains(command.Rectangle)).ToArray();
        }

        public static CommandList Parse(string[] rawCommands)
        {
            return new(rawCommands.SelectArray(Command.Parse));
        }
    }

    private sealed partial record Command(LightState State, Rectangle3D Rectangle)
    {
        // It would help if the same pattern could be abstracted away
        private static readonly Regex commandPattern = ComandRegex();

        [GeneratedRegex("(?'state'\\w*) x=(?'startX'-?\\d*)\\.\\.(?'endX'-?\\d*),y=(?'startY'-?\\d*)\\.\\.(?'endY'-?\\d*),z=(?'startZ'-?\\d*)\\.\\.(?'endZ'-?\\d*)")]
        private static partial Regex ComandRegex();

        public static Command Parse(string raw)
        {
            var groups = commandPattern.Match(raw).Groups;
            var state = ParseState(groups["state"].Value);
            var startX = groups["startX"].Value.ParseInt32();
            var startY = groups["startY"].Value.ParseInt32();
            var startZ = groups["startZ"].Value.ParseInt32();
            var endX = groups["endX"].Value.ParseInt32() + 1;
            var endY = groups["endY"].Value.ParseInt32() + 1;
            var endZ = groups["endZ"].Value.ParseInt32() + 1;

            return new(state, new((startX, startY, startZ), (endX, endY, endZ)));
        }

        private static LightState ParseState(string state) => state switch
        {
            "off" => LightState.Off,
            "on" => LightState.On,
        };
    }
}
