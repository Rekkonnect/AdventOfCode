using Garyon.Objects;

namespace AdventOfCode.Utilities;

public static class MinMaxExtensions
{
    public static ulong Difference(this MinMaxResult<ulong> minmax) => minmax.Max - minmax.Min;
    public static long Difference(this MinMaxResult<long> minmax) => minmax.Max - minmax.Min;
    public static uint Difference(this MinMaxResult<uint> minmax) => minmax.Max - minmax.Min;
    public static int Difference(this MinMaxResult<int> minmax) => minmax.Max - minmax.Min;
}
