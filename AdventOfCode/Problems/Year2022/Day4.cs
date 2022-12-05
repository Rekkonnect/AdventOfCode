using AdventOfCode.Functions;
using System.Diagnostics;

namespace AdventOfCode.Problems.Year2022;

public class Day4 : Problem<int>
{
    private ElfPair[] pairs;

    public override int SolvePart1()
    {
        return pairs.Count(p => p.FullyContained);
    }
    public override int SolvePart2()
    {
        return pairs.Count(p => p.Overlapping);
    }

    public int SolvePart1Hand()
    {
        int count = 0;
        for (int i = 0; i < pairs.Length; i++)
        {
            ref var pair = ref pairs[i];
            if (pair.FullyContained)
            {
                count++;
            }
        }
        return count;
    }
    public int SolvePart2Hand()
    {
        int count = 0;
        for (int i = 0; i < pairs.Length; i++)
        {
            ref var pair = ref pairs[i];
            if (pair.Overlapping)
            {
                count++;
            }
        }
        return count;
    }

    protected override void LoadState()
    {
        pairs = FileLines.Select(ElfPair.Parse).ToArray();
    }
    protected override void ResetState()
    {
        pairs = null;
    }

    private readonly record struct ElfPair(ElfAssignment AssignmentA, ElfAssignment AssignmentB)
    {
        public bool Overlapping => AssignmentA.Overlaps(AssignmentB);
        public bool FullyContained => AssignmentA.FullyContains(AssignmentB)
                                   || AssignmentB.FullyContains(AssignmentA);

        public static ElfPair Parse(string line)
        {
            bool delimited = line.SplitOnceSpan(',', out var left, out var right);
            Debug.Assert(delimited);

            var a = ElfAssignment.Parse(left);
            var b = ElfAssignment.Parse(right);
            return new(a, b);
        }
    }

    private readonly record struct ElfAssignment(int Start, int End)
    {
        public bool Contains(int value)
        {
            return value >= Start
                && value <= End;
        }

        public bool Overlaps(ElfAssignment other)
        {
            return Contains(other.Start)
                || Contains(other.End)
                || other.Contains(Start)
                || other.Contains(End);
        }
        public bool FullyContains(ElfAssignment other)
        {
            return other.Start >= Start
                && other.End <= End;
        }

        public static ElfAssignment Parse(ReadOnlySpan<char> raw)
        {
            bool delimited = raw.SplitOnceSpan('-', out var left, out var right);
            Debug.Assert(delimited);

            int start = left.ParseInt32();
            int end = right.ParseInt32();
            return new(start, end);
        }
    }
}
