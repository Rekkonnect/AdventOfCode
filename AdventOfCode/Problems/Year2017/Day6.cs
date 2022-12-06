using AdventOfCode.Utilities;
using System.Data;
using AdventOfCode.Functions;

#if DEBUG
using System.Diagnostics;
#endif

namespace AdventOfCode.Problems.Year2017;

public class Day6 : Problem<int>
{
    private Memory memory;

    public override int SolvePart1()
    {
        return memory.RedistributionCyclesUntilLoop;
    }
    public override int SolvePart2()
    {
        return memory.LoopStart;
    }

    protected override void LoadState()
    {
        memory = new(FileContents.ParseInt32Array('\t'));
        memory.RedistributeUntilLoop();
    }
    protected override void ResetState()
    {
        memory = null;
    }

    private class Memory
    {
        private readonly int[] banks;

        public int BankCount => banks.Length;

        public int RedistributionCyclesUntilLoop { get; private set; }
        public int LoopStart { get; private set; }

        public Memory(IEnumerable<int> bankBlockCount)
        {
            banks = bankBlockCount.ToArray();
        }
        public Memory(Memory other)
            : this(other.banks) { }

        public void RedistributeUntilLoop()
        {
            var seenConfigurations = new IDMap<UInt128> { GetStateCode() };

            while (true)
            {
                Redistribute();

                if (!seenConfigurations.TryAdd(GetStateCode(), out int loopStart))
                {
                    RedistributionCyclesUntilLoop = seenConfigurations.Count;
                    LoopStart = RedistributionCyclesUntilLoop - loopStart;

                    return;
                }
            }
        }

        private void Redistribute()
        {
            int max = banks[0];
            int maxIndex = 0;

            for (int i = 1; i < BankCount; i++)
            {
                if (banks[i] <= max)
                    continue;

                max = banks[i];
                maxIndex = i;
            }

#if DEBUG
            int totalBlocks = TotalBlockCount();
#endif

            banks[maxIndex] = 0;

            int distribution = Math.DivRem(max, BankCount, out int remaining);

            for (int i = 0; i < BankCount; i++)
            {
                int rotatedIndex = (i + BankCount - maxIndex - 1) % BankCount;
                banks[i] += distribution + Convert.ToInt32(rotatedIndex < remaining);
            }

#if DEBUG
            Debug.Assert(totalBlocks == TotalBlockCount());
#endif
        }

        private int TotalBlockCount() => banks.Sum();

        private UInt128 GetStateCode()
        {
            UInt128 result = 0;
            for (int i = 0; i < BankCount; i++)
                result |= (UInt128)banks[i] << (i * 8);
            return result;
        }
    }
}
