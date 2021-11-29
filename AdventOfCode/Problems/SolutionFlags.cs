using System;

namespace AdventOfCode.Problems;

[Flags]
public enum SolutionFlags
{
    Part1WIP = 1,
    Part2WIP = 1 << 1,
    Part1Unoptimized = 1 << 2,
    Part2Unoptimized = 1 << 3,

    BothWIP = Part1WIP | Part2WIP,
    BothUnoptimized = Part1Unoptimized | Part2Unoptimized,
}
