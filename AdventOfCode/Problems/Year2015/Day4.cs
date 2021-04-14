using AdventOfCode.Problems.Utilities;

namespace AdventOfCode.Problems.Year2015
{
    public class Day4 : Problem<int>
    {
        private string secretKey;

        public override int SolvePart1()
        {
            return MineAdventCoins<AdventCoinMinerPart1>();
        }
        public override int SolvePart2()
        {
            return MineAdventCoins<AdventCoinMinerPart2>();
        }

        private int MineAdventCoins<T>()
            where T : AdventCoinMiner, new()
        {
            return new T().FindSuitableHash(secretKey);
        }

        protected override void ResetState()
        {
            secretKey = null;
        }
        protected override void LoadState()
        {
            secretKey = FileContents;
        }

        private sealed class AdventCoinMinerPart2 : AdventCoinMiner
        {
            protected override bool DetermineHashValidity(byte[] hash)
            {
                for (int i = 0; i < 3; i++)
                    if (hash[i] > 0)
                        return false;

                return true;
            }
        }

        private sealed class AdventCoinMinerPart1 : AdventCoinMiner
        {
            protected override bool DetermineHashValidity(byte[] hash)
            {
                for (int i = 0; i < 2; i++)
                    if (hash[i] > 0)
                        return false;

                return hash[2] < 0x10;
            }
        }

        private abstract class AdventCoinMiner : MD5HashBruteForcer
        {
            public int FindSuitableHash(string secretKey) => FindSuitableHash(secretKey, out _);
        }
    }
}
