namespace AdventOfCode.Problems.Year2021;

public class Day3 : Problem<uint>
{
    private DiagnosticReport report;

    public override uint SolvePart1()
    {
        return report.PowerConsumption;
    }
    public override uint SolvePart2()
    {
        return report.LifeSupportRating;
    }

    protected override void LoadState()
    {
        report = new(FileLines);
    }
    protected override void ResetState()
    {
        report = null;
    }

    private static uint ParseBinary(string binary)
    {
        uint result = 0;
        for (int i = 0; i < binary.Length; i++)
            result |= (uint)(binary[^(i + 1)] - '0') << i;
        
        return result;
    }

    private record DiagnosticReport(string[] Numbers)
    {
        private uint gammaRate = 0;

        public int NumberBits = Numbers[0].Length;

        public uint GammaRate
        {
            get
            {
                if (gammaRate is not 0)
                    return gammaRate;

                for (int i = 0; i < NumberBits; i++)
                    gammaRate |= new BitCounts(Numbers, ^(i + 1)).MostCommonBit << i;

                return gammaRate;
            }
        }
        public uint EpsilonRate => ~GammaRate & ~(uint.MaxValue << NumberBits);

        public uint PowerConsumption => GammaRate * EpsilonRate;

        public uint OxygenGeneratorRating => GetUInt32Rating(ValueRarity.MostCommon);
        public uint CO2ScrubberRating => GetUInt32Rating(ValueRarity.LeastCommon);
        public uint LifeSupportRating => OxygenGeneratorRating * CO2ScrubberRating;

        private uint GetUInt32Rating(ValueRarity targetRarity) => ParseBinary(GetRating(targetRarity));
        private string GetRating(ValueRarity targetRarity)
        {
            uint tiebreaker = (uint)targetRarity;

            var availableNumbers = new List<string>(Numbers);
            int currentBit = 0;
            while (availableNumbers.Count > 1)
            {
                var bitCounts = new BitCounts(availableNumbers, currentBit);

                uint keptBit = bitCounts.BitWithRarity(targetRarity, tiebreaker);
                char keptBitChar = (char)(keptBit + '0');

                availableNumbers.RemoveAll(ShouldRemove);
                currentBit++;

                bool ShouldRemove(string number) => number[currentBit] != keptBitChar;
            }

            return availableNumbers.First();
        }
    }

    private enum ValueRarity : uint
    {
        LeastCommon = 0,
        MostCommon = 1,
    }

    private ref struct BitCounts
    {
        private readonly uint aceCount;

        public uint NumberCount { get; }

        public bool HaveEqualCount => aceCount * 2 == NumberCount;

        public uint AceCount => aceCount;
        public uint ZeroCount => NumberCount - aceCount;

        public uint MostCommonBit => Convert.ToUInt32(AceCount > ZeroCount);
        public uint LeastCommonBit => MostCommonBit ^ 1;

        public BitCounts(ICollection<string> numbers, Index bitIndex)
            : this(numbers, bitIndex.GetOffset(numbers.First().Length)) { }

        public BitCounts(ICollection<string> numbers, int bitIndex)
        {
            aceCount = 0;
            NumberCount = (uint)numbers.Count;
            foreach (string number in numbers)
                aceCount += (uint)(number[bitIndex] - '0');
        }

        public uint BitWithRarity(ValueRarity rarity) => rarity switch
        {
            ValueRarity.MostCommon => MostCommonBit,
            ValueRarity.LeastCommon => LeastCommonBit,
        };
        public uint BitWithRarity(ValueRarity rarity, uint tiebreaker)
        {
            return HaveEqualCount ? tiebreaker : BitWithRarity(rarity);
        }
    }
}
