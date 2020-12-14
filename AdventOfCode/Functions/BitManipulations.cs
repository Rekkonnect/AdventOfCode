using System.Collections.Generic;

namespace AdventOfCode.Functions
{
    public static class BitManipulations
    {
        // TODO: Make functions for other types as well monkaS
        /// <summary>Gets all the combinations that can be formed from the given mask. A combination is formed like this: bits in the original mask set to 1 are alternated, and bits set to 0 are always 0.</summary>
        /// <param name="mask">The mask whose combinations to get.</param>
        /// <returns>The combinations that were generated. Each combination is generated and returned using <see langword="yield return"/>.</returns>
        public static IEnumerable<ulong> GetCombinationsFromMask(ulong mask, int accountedBits = 64)
        {
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
}
