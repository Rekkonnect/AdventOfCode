using System.Numerics;
using UltimateOrb;

namespace AdventOfCode.Functions
{
    public static class UInt128Extensions
    {
        // Next time I should develop my own numeric structure
        public static unsafe int PopCount(this UInt128 value)
        {
            long hi = value.HiInt64Bits;
            long lo = value.LoInt64Bits;
            ulong hiu = *(ulong*)&hi;
            ulong lou = *(ulong*)&lo;

            return BitOperations.PopCount(hiu) + BitOperations.PopCount(lou);
        }
    }
}
