using AdventOfCode.Utilities;
using AdventOfCode.Utilities.TwoDimensions;
using System.IO.Compression;

namespace AdventOfCode.Problems.Year2022;

public class Day23 : Problem<int>
{
    private ImmutableArray<Location2D> locations;

    public override int SolvePart1()
    {
        var grid = new ElfGrid(locations);
        grid.Iterate(10);
        return grid.MBREmptySpace();
    }
    public override int SolvePart2()
    {
        return -1;
        var grid = new ElfGrid(locations);
        grid.IterateToEnd();
    }

    protected override void LoadState()
    {
        var lines = FileLines;
        int height = lines.Length;
        int width = lines[0].Length;
        var builder = ImmutableArray.CreateBuilder<Location2D>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char c = lines[y][x];
                if (c is not '#')
                    continue;

                builder.Add((x, y));
            }
        }

        locations = builder.ToImmutable();
    }

    private sealed class ElfGrid
    {
        private readonly ImmutableArray<Elf> elves;
        private readonly Dictionary<Location2D, Elf> elfDictionary = new();

        private readonly HashSet<Elf> aliveElves = new();

        private readonly ElfMovementIterator movementIterator = new();

        public int ElfCount => elves.Length;

        public ElfGrid(ImmutableArray<Location2D> locations)
        {
            elves = locations.Select(l => new Elf(l))
                             .ToImmutableArray();

            RegisterElves();
            aliveElves.AddRange(elves);
        }

        private void RegisterElves()
        {
            RegisterElves(elves);
        }
        private void RegisterElves(IEnumerable<Elf> elves)
        {
            foreach (var elf in elves)
            {
                RegisterElf(elf);
            }
        }

        private void RegisterElf(Elf elf)
        {
            elfDictionary[elf.Location] = elf;
        }
        private void DiscardElf(Elf elf)
        {
            elfDictionary.Remove(elf.Location);
        }
        private void RefreshElf(Elf elf)
        {
            DiscardElf(elf);
            RegisterElf(elf);
        }

        public bool Iterate()
        {
            var iteratedElves = aliveElves.ToArray();

            foreach (var elf in iteratedElves)
            {
                AdjustElf(elf);

                // The elf will become alive again as soon as its neighbors approach
                if (elf.IsImmobile)
                    aliveElves.Remove(elf);
            }

            var targetLocationDictionary = new FlexibleListDictionary<Location2D, Elf>();
            foreach (var elf in iteratedElves)
            {
                var targetLocation = elf.TargetLocation();
                targetLocationDictionary[targetLocation].Add(elf);
            }

            var movingElves = targetLocationDictionary.Values.PickSingles().ToList();
            foreach (var movingElf in movingElves)
            {
                if (movingElf.IsImmobile)
                    continue;

                DiscardElf(movingElf);
                movingElf.MoveToTargetLocation();
                RegisterAliveElf(movingElf);
            }

            RegisterElves(movingElves);

            movementIterator.AdvanceRound();
            return aliveElves.Count > 0;
        }
        public void Iterate(int count)
        {
            for (int i = 0; i < count; i++)
            {
                bool alive = Iterate();
                if (!alive)
                    break;
            }
        }
        public void IterateToEnd()
        {
            Iterate(int.MaxValue);
        }

        private void RegisterAliveElf(Elf elf)
        {
            RegisterAliveRegion(elf.Location);
        }
        private void RegisterAliveRegion(Location2D location)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if ((x, y) is (0, 0))
                        continue;

                    var nextLocation = location + (x, y);
                    var elf = ElfAt(nextLocation);
                    if (elf is null)
                        continue;

                    bool added = aliveElves.Add(elf);
                    if (!added)
                        continue;

                    RegisterAliveRegion(nextLocation);
                }
            }
        }

        private void AdjustElf(Elf elf)
        {
            for (int i = 0; i < 4; i++)
            {
                var direction = movementIterator.ForStep(i);
                bool evaluation = EvaluateElfProposal(elf, direction);
                if (evaluation)
                {
                    elf.DesiredDirection = direction;
                    return;
                }
            }

            elf.IsImmobile = true;
        }
        private bool EvaluateElfProposal(Elf elf, ManhattanDirection direction)
        {
            var elfLocation = elf.Location;
            switch (direction)
            {
                case ManhattanDirection.North:
                    return IterateLocations(ManhattanDirection.North,
                                            ManhattanDirection.NorthWest,
                                            ManhattanDirection.NorthEast);

                case ManhattanDirection.South:
                    return IterateLocations(ManhattanDirection.South,
                                            ManhattanDirection.SouthWest,
                                            ManhattanDirection.SouthEast);

                case ManhattanDirection.East:
                    return IterateLocations(ManhattanDirection.East,
                                            ManhattanDirection.NorthEast,
                                            ManhattanDirection.SouthEast);

                case ManhattanDirection.West:
                    return IterateLocations(ManhattanDirection.West,
                                            ManhattanDirection.NorthWest,
                                            ManhattanDirection.SouthWest);
            }
            return false;

            bool IterateLocations(ManhattanDirection a, ManhattanDirection b, ManhattanDirection c)
            {
                var elfA = ElfAt(elfLocation, a);
                var elfB = ElfAt(elfLocation, b);
                var elfC = ElfAt(elfLocation, c);
                return NullGuards.AnyNonNull(elfA, elfB, elfC);
            }
        }

        private Elf ElfAt(Location2D location, ManhattanDirection directionOffset)
        {
            location = ApplyDirection(location, directionOffset);
            return ElfAt(location);
        }
        private Elf ElfAt(Location2D location)
        {
            return elfDictionary.ValueOrDefault(location);
        }

        public Rectangle MBR()
        {
            var locations = elves.Select(e => e.Location).ToList();
            return Rectangle.MBR(locations);
        }

        public int MBREmptySpace()
        {
            var mbr = MBR();
            return mbr.Area - ElfCount;
        }

        private sealed class Elf : IEquatable<Elf>
        {
            public Location2D StartingLocation { get; }
            public Location2D Location { get; private set; }
            public ManhattanDirection DesiredDirection { get; set; }

            public bool IsImmobile { get; set; }

            public Elf(Location2D startingLocation)
            {
                StartingLocation = startingLocation;
                Location = startingLocation;
            }

            public Location2D TargetLocation()
            {
                if (IsImmobile)
                    return Location;

                return ApplyDirection(Location, DesiredDirection);
            }
            public void MoveToTargetLocation()
            {
                if (IsImmobile)
                    return;

                Location = TargetLocation();
            }

            public override int GetHashCode()
            {
                return StartingLocation.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                return obj is Elf elf
                    && Equals(elf);
            }
            public bool Equals(Elf elf)
            {
                return StartingLocation == elf.StartingLocation;
            }
        }

        private sealed class ElfMovementIterator
        {
            private const int directionCount = 4;

            private readonly ManhattanDirection[] directions =
            {
                ManhattanDirection.North,
                ManhattanDirection.South,
                ManhattanDirection.West,
                ManhattanDirection.East,
            };

            private int round = 0;

            private int StartingIndex => round % directionCount;

            public void AdvanceRound()
            {
                round++;
            }
            public ManhattanDirection ForStep(int step)
            {
                int index = (StartingIndex + step) % directionCount;
                return directions[index];
            }
        }
    }

    private static Location2D ApplyDirection(Location2D location, ManhattanDirection direction)
    {
        return location + OffsetForDirection(direction);
    }

    private static Location2D OffsetForDirection(ManhattanDirection direction)
    {
        return direction switch
        {
            ManhattanDirection.North => (0, -1),
            ManhattanDirection.South => (0, 1),
            ManhattanDirection.West => (-1, 0),
            ManhattanDirection.East => (1, 0),
            ManhattanDirection.NorthWest => (-1, -1),
            ManhattanDirection.NorthEast => (1, -1),
            ManhattanDirection.SouthWest => (-1, 1),
            ManhattanDirection.SouthEast => (1, 1),

            _ => throw null,
        };
    }

    private enum ManhattanDirection
    {
        North,
        South,
        West,
        East,
        NorthWest,
        NorthEast,
        SouthWest,
        SouthEast,
    }
}
