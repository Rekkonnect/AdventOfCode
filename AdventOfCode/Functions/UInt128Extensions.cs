using System.Numerics;
using UltimateOrb;
using UInt128 = UltimateOrb.UInt128;

namespace AdventOfCode.Functions;

public static class UInt128Extensions
{
    // Next time I should develop my own numeric structure
    public static unsafe int PopCount(this UInt128 value)
    {
        var valuePtr = (ulong*)&value;
        return BitOperations.PopCount(valuePtr[0]) + BitOperations.PopCount(valuePtr[1]);
    }
    public static string GetBinaryRepresentation(this UInt128 value) => $"{value.HiInt64Bits.GetBinaryRepresentation()}{value.LoInt64Bits.GetBinaryRepresentation()}";
}
