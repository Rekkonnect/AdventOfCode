using AdventOfCode.Utilities.TwoDimensions;
using System.Collections;

namespace AdventOfCode.Problems.Year2018;

public class Day18 : Problem<int>
{
    private AreaIterator iterator;

    public override int SolvePart1()
    {
        return iterator.IterateToMinute(10).ResourceValue;
    }
    public override int SolvePart2()
    {
        return iterator.IterateToMinute(1_000_000_000).ResourceValue;
    }

    protected override void LoadState()
    {
        iterator = new AreaIterator(Area.Parse(FileLines));
    }
    protected override void ResetState()
    {
        iterator = null;
    }

    private class PeriodicCollection<T> : IEnumerable<T>
        where T : IEquatable<T>
    {
        private readonly List<T> encountered = new();

        public int EncounteredCount => encountered.Count;

        public int Period => EncounteredCount - PeriodStart;
        public int PeriodStart { get; private set; } = -1;

        public T Last => encountered.Last();

        public T this[int index]
        {
            get
            {
                int encounteredIndex = index;

                if (PeriodStart > -1)
                {
                    int periodIndex = (index - PeriodStart) / Period;
                    int localPeriodStart = Period * periodIndex + PeriodStart;
                    encounteredIndex = PeriodStart + index - localPeriodStart;
                }

                return encountered[encounteredIndex];
            }
        }

        /// <summary>Adds a value to the encountered collection, and attempts to detect a period's end. If a period has already been discovered, nothing happens.</summary>
        /// <param name="value">The value to add to the encountered collection.</param>
        /// <returns><see langword="true"/> if a period has been discovered, otherwise <see langword="false"/>.</returns>
        public bool Add(T value)
        {
            if (PeriodStart > -1)
                return true;

            for (int i = 0; i < EncounteredCount; i++)
            {
                if (encountered[i].Equals(value))
                {
                    PeriodStart = i;
                    return true;
                }
            }

            encountered.Add(value);
            return false;
        }

        public IEnumerator<T> GetEnumerator() => encountered.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class AreaIterator
    {
        private readonly PeriodicCollection<Area> encounteredAreas = new();

        private Area Current => encounteredAreas.Last;

        public AreaIterator(Area startingArea)
        {
            encounteredAreas.Add(startingArea);
        }

        public Area IterateToMinute(int minutes)
        {
            if (minutes < encounteredAreas.EncounteredCount)
                return encounteredAreas[minutes];

            for (int i = encounteredAreas.EncounteredCount; i <= minutes; i++)
                if (SetIteratedCurrentArea())
                    break;

            return encounteredAreas[minutes];
        }
        private Area IterateMinute()
        {
            var result = new Area(Current.Height);

            for (int x = 0; x < result.Width; x++)
            {
                for (int y = 0; y < result.Height; y++)
                {
                    result[x, y] = GetNextMinuteType(x, y);
                }
            }

            return result;
        }
        private bool SetIteratedCurrentArea()
        {
            return SetCurrentArea(IterateMinute());
        }
        private bool SetCurrentArea(Area next)
        {
            return encounteredAreas.Add(next);
        }

        private AcreType GetNextMinuteType(int x, int y)
        {
            var currentType = Current[x, y];
            switch (currentType)
            {
                case AcreType.Open:
                    return DetermineAcreTypeEvolution(x, y, currentType, AcreType.Trees);

                case AcreType.Trees:
                    return DetermineAcreTypeEvolution(x, y, currentType, AcreType.Lumbderyard);

                case AcreType.Lumbderyard:
                    bool hasAdjacentTrees = Current.HasNeighborValues(x, y, AcreType.Trees);
                    bool hasAdjacentLumberyards = Current.HasNeighborValues(x, y, AcreType.Lumbderyard);
                    return hasAdjacentLumberyards && hasAdjacentTrees ? AcreType.Lumbderyard : AcreType.Open;
            }
            return currentType;
        }
        private AcreType DetermineAcreTypeEvolution(int x, int y, AcreType currentType, AcreType evolvedType)
        {
            int neighbors = Current.GetNeighborValues(x, y, evolvedType);
            return neighbors >= 3 ? evolvedType : currentType;
        }
    }

    private class Area : PrintableGrid2D<AcreType>, IEquatable<Area>
    {
        public int ResourceValue => ValueCounters[AcreType.Trees] * ValueCounters[AcreType.Lumbderyard];

        public Area(int size)
            : base(size) { }

        public override char GetPrintableCharacter(AcreType value)
        {
            return value switch
            {
                AcreType.Open => '.',
                AcreType.Trees => '|',
                AcreType.Lumbderyard => '#',
            };
        }
        private static AcreType ParseAcreType(char typeChar)
        {
            return typeChar switch
            {
                '.' => AcreType.Open,
                '|' => AcreType.Trees,
                '#' => AcreType.Lumbderyard,
            };
        }

        public static Area Parse(string[] rawAreaRows)
        {
            int size = rawAreaRows.Length;
            var result = new Area(size);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    result[x, y] = ParseAcreType(rawAreaRows[y][x]);
                }
            }
            return result;
        }

        // I'm starting to notice the need for self-type interfaces even more
        public bool Equals(Area other)
        {
            return Equals((Grid2D<AcreType>)other);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Area);
        }
    }

    private enum AcreType
    {
        Open,
        Trees,
        Lumbderyard
    }
}
