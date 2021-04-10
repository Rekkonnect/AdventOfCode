using System;

namespace AdventOfCode.Functions
{
    public static class RangeExtensions
    {
        public static void GetStartAndEnd(this Range range, int length, out int start, out int end)
        {
            start = range.Start.GetOffset(length);
            end = range.End.GetOffset(length);
        }
    }
}
