using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace AdventOfCode.Functions;

public static class BitManipulations
{
    public static int DecodeIndex(ulong value)
    {
        if (value is 0)
            return -1;

        for (int index = 0; index < sizeof(ulong) * 8; index++, value >>= 1)
        {
            if (value is 1)
                return index;

            if (value is not 1 && (value & 1) is 1)
                return -1;
        }

        return -1;
    }

    // TODO: Make functions for other types as well monkaS
    /// <summary>Gets all the combinations that can be formed from the given mask. A combination is formed like this: bits in the original mask set to 1 are alternated, and bits set to 0 are always 0.</summary>
    /// <param name="mask">The mask whose combinations to get.</param>
    /// <returns>The combinations that were generated. Each combination is generated and returned using <see langword="yield return"/>.</returns>
    public static IEnumerable<ulong> GetCombinationsFromMask(ulong mask, int accountedBits = 64)
    {
        if (Bmi2.X64.IsSupported)
        {
            var maxCombination = 1UL << BitOperations.PopCount(mask);
            for (ulong combinationIndex = 0; combinationIndex < maxCombination; combinationIndex++)
                yield return Bmi2.X64.ParallelBitDeposit(combinationIndex, mask);

            yield break;
        }

        // I had written this code before discovering the PDEP instruction
        // So I'm mercying people with ARM-based CPUs by leaving this piece of code here

        var aceIndices = new List<int>(accountedBits);
        ulong currentMask = 1;
        for (int i = 0; i < accountedBits; i++, currentMask <<= 1)
            if ((mask & currentMask) > 0)
                aceIndices.Add(i);

        for (ulong combinationIndex = 0; combinationIndex < (1ul << aceIndices.Count); combinationIndex++)
        {
            ulong result = 0;

            for (int i = 0; i < aceIndices.Count; i++)
            {
                if ((combinationIndex & (1ul << i)) == 0)
                    continue;

                result |= 1ul << aceIndices[i];
            }

            yield return result;
        }
    }
}
