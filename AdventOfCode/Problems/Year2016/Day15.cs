using AdventOfCode.Utilities;

namespace AdventOfCode.Problems.Year2016;

public class Day15 : Problem<int>
{
    private DiscCollection discs;

    public override int SolvePart1()
    {
        return discs.GetFirstButtonPressTime();
    }
    public override int SolvePart2()
    {
        discs.Add(new(discs.Count + 1, 11, 0));
        return discs.GetFirstButtonPressTime();
    }

    protected override void LoadState()
    {
        discs = new(ParsedFileLines(Disc.Parse));
    }
    protected override void ResetState()
    {
        discs = null;
    }

    private class DiscCollection
    {
        private readonly SortedCollection<Disc> sortedDiscs;

        public int Count => sortedDiscs.Count;

        public DiscCollection(ICollection<Disc> discs)
        {
            sortedDiscs = new(discs, Disc.DescendingPositionCounts);
        }

        public void Add(Disc disc) => sortedDiscs.Add(disc);

        public int GetFirstButtonPressTime()
        {
            int currentTime = 0;
            int step = 1;

            for (int lockedDiscs = 0; lockedDiscs < Count; lockedDiscs++)
            {
                currentTime += sortedDiscs[lockedDiscs].PositionsAwayFromZero(currentTime) * step;

                while (true)
                {
                    for (int i = 0; i <= lockedDiscs; i++)
                        if (sortedDiscs[i].ButtonPressedPositionAt(currentTime) is not 0)
                            goto elapse;

                    break;

                elapse:
                    currentTime += step;
                }

                // Since all the discs' position counts are primes, avoid computing the LCM explicitly
                step *= sortedDiscs[lockedDiscs].PositionCount;
            }

            return currentTime % step;
        }
    }

    private record Disc(int ID, int PositionCount, int StartingPosition)
    {
        private static readonly Regex discPattern = new(@"Disc #(?'id'\d*) has (?'positions'\d*) positions; at time=0, it is at position (?'start'\d*)\.", RegexOptions.Compiled);

        public int PositionsAwayFromZero(int time) => (PositionCount - ButtonPressedPositionAt(time)) % PositionCount;
        public int ButtonPressedPositionAt(int time) => (time + ID + StartingPosition) % PositionCount;

        public static Disc Parse(string raw)
        {
            var groups = discPattern.Match(raw).Groups;
            int id = groups["id"].Value.ParseInt32();
            int positions = groups["positions"].Value.ParseInt32();
            int start = groups["start"].Value.ParseInt32();
            return new(id, positions, start);
        }

        public static int DescendingPositionCounts(Disc left, Disc right) => right.PositionCount.CompareTo(left.PositionCount);
    }
}
