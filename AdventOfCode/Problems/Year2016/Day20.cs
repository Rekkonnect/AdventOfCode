using AdventOfCode.Utilities;

namespace AdventOfCode.Problems.Year2016;

public class Day20 : Problem<uint>
{
    private BlacklistedAddresses addresses;

    public override uint SolvePart1()
    {
        return addresses.LowestUnblockedAddress;
    }
    public override uint SolvePart2()
    {
        return addresses.TotalUnblocked;
    }

    protected override void LoadState()
    {
        addresses = new(ParsedFileLines(AddressRange.Parse));
    }
    protected override void ResetState()
    {
        addresses = null;
    }

    private class BlacklistedAddresses
    {
        private readonly List<AddressRange> ranges;

        public uint LowestUnblockedAddress
        {
            get
            {
                uint result = 0;
                foreach (var r in ranges)
                {
                    if (r.Contains(result))
                        result = r.End + 1;
                    else
                        return result;
                }
                return uint.MaxValue;
            }
        }
        public uint TotalBlocked => ranges.Sum(s => s.Length);
        public uint TotalUnblocked => uint.MaxValue - TotalBlocked + 1;

        public BlacklistedAddresses(IList<AddressRange> addressRanges)
        {
            var sortedStarts = new SortedCollection<AddressRange>(addressRanges, AddressRange.AscendingStart);

            ranges = new List<AddressRange>(sortedStarts.Count);
            var merged = sortedStarts[0];

            for (int i = 1; i < sortedStarts.Count; i++)
            {
                var next = sortedStarts[i];

                if (next.Start <= merged.End)
                {
                    merged.End = Math.Max(next.End, merged.End);
                }
                else
                {
                    ranges.Add(merged);
                    merged = next;
                }
            }

            ranges.Add(merged);
        }
    }
    private struct AddressRange
    {
        private static readonly Regex rangePattern = new(@"(?'start'\d*)\-(?'end'\d*)", RegexOptions.Compiled);

        public uint Start { get; set; }
        public uint End { get; set; }

        public uint Length => End - Start + 1;

        public AddressRange(uint start, uint end)
        {
            Start = start;
            End = end;
        }

        public bool Contains(uint value) => Start <= value && value <= End;

        public static int AscendingStart(AddressRange a, AddressRange b) => a.Start.CompareTo(b.Start);

        public static AddressRange Parse(string raw)
        {
            var groups = rangePattern.Match(raw).Groups;
            uint start = groups["start"].Value.ParseUInt32();
            uint end = groups["end"].Value.ParseUInt32();
            return new(start, end);
        }

        public static AddressRange Single(uint both) => new(both, both);

        public override string ToString()
        {
            return $"{Start}-{End}";
        }
    }
}
