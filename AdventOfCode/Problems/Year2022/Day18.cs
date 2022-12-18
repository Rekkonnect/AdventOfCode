using AdventOfCode.Utilities.ThreeDimensions;
using System.Collections.Immutable;
using System.Numerics;

namespace AdventOfCode.Problems.Year2022;

public class Day18 : Problem<int>
{
    private CubeGrid grid;

    public override int SolvePart1()
    {
        grid.IterateCubes();
        return grid.ExposedSides;
    }
    [PartSolution(PartSolutionStatus.Uninitialized)]
    public override int SolvePart2()
    {
        return -1;
    }

    protected override void LoadState()
    {
        var contentSpan = FileContents.AsSpan();
        var cubeInfos = contentSpan.Trim().SelectLines(CubeInfo.Parse);
        grid = new(cubeInfos);
    }

    private class CubeGrid
    {
        private readonly ImmutableArray<CubeInfo> cubeInfos;
        private readonly Dictionary<Location3D, CubeInfo> cubeMap;

        public int ExposedSides => cubeInfos.Sum(c => c.ExposedSides);

        public CubeGrid(ImmutableArray<CubeInfo> cubes)
        {
            cubeInfos = cubes;
            cubeMap = cubes.ToDictionary(d => d.Location);
        }

        // Naming is hard
        public void IterateCubes()
        {
            foreach (var cubeInfo in cubeInfos)
            {
                IterateCubeInfo(cubeInfo);
            }
        }
        private void IterateCubeInfo(CubeInfo cubeInfo)
        {
            var locations = ConnectedCubeInfoList.Instance.CubeDisconnectedLocations(cubeInfo);
            foreach (var location in locations)
            {
                bool exists = cubeMap.TryGetValue(location.Location, out var connectedCubeInfo);
                if (!exists)
                    continue;

                cubeInfo.AddConnection(location.Connection);
                connectedCubeInfo.AddOpposingConnection(location.Connection);
            }
        }
    }

    private sealed class ConnectedCubeInfoList
    {
        public static ConnectedCubeInfoList Instance { get; } = new();
        private ConnectedCubeInfoList() { }

        private readonly List<CubeConnectionInfo> list = new(6);

        public IEnumerable<CubeConnectionInfo> CubeDisconnectedLocations(CubeInfo cube)
        {
            list.Clear();

            AddConnected(cube, Connections.LowerX);
            AddConnected(cube, Connections.LowerY);
            AddConnected(cube, Connections.LowerZ);
            AddConnected(cube, Connections.UpperX);
            AddConnected(cube, Connections.UpperY);
            AddConnected(cube, Connections.UpperZ);

            return list;
        }
        private void AddConnected(CubeInfo cube, Connections connection)
        {
            var connectionMask = cube.Connections & connection;
            if (connectionMask is not 0)
                return;

            var locationOffset = ConnectionDirectionOffset(connection);
            var nextLocation = cube.Location + locationOffset;
            list.Add(new(nextLocation, connection));
        }
    }

    private record struct CubeConnectionInfo(Location3D Location, Connections Connection);

    private class CubeInfo
    {
        public Location3D Location { get; }
        public Connections Connections { get; set; }

        public int ConnectionCount => GetConnectionCount(Connections);
        public int ExposedSides => 6 - ConnectionCount;

        public CubeInfo(Location3D location, Connections connections)
        {
            Location = location;
            Connections = connections;
        }
        public CubeInfo(Location3D location)
            : this(location, Connections.None) { }

        public void AddConnection(Connections connection)
        {
            Connections |= connection;
        }
        public void AddOpposingConnection(Connections connection)
        {
            AddConnection(Opposite(connection));
        }

        public static CubeInfo Parse(SpanString locationSpan)
        {
            var location = CommonParsing.ParseLocation3D(locationSpan);
            return new(location);
        }
    }

    private static Location3D ConnectionDirectionOffset(Connections connection)
    {
        return connection switch
        {
            Connections.LowerX => (-1, 0, 0),
            Connections.UpperX => (1, 0, 0),
            Connections.LowerY => (0, -1, 0),
            Connections.UpperY => (0, 1, 0),
            Connections.LowerZ => (0, 0, -1),
            Connections.UpperZ => (0, 0, 1),

            _ => (0, 0, 0),
        };
    }

    private static int GetConnectionCount(Connections connections)
    {
        return BitOperations.PopCount((uint)connections);
    }

    private static Connections Opposite(Connections connections)
    {
        return connections switch
        {
            Connections.LowerX => Connections.UpperX,
            Connections.UpperX => Connections.LowerX,
            Connections.LowerY => Connections.UpperY,
            Connections.UpperY => Connections.LowerY,
            Connections.LowerZ => Connections.UpperZ,
            Connections.UpperZ => Connections.LowerZ,
            _ => Connections.None,
        };
    }

    [Flags]
    private enum Connections : uint
    {
        None = 0,

        LowerX = 1 << 0,
        UpperX = 1 << 1,
        LowerY = 1 << 2,
        UpperY = 1 << 3,
        LowerZ = 1 << 4,
        UpperZ = 1 << 5,

        All = LowerX | LowerY | LowerZ
            | UpperX | UpperY | UpperZ,
    }
}
