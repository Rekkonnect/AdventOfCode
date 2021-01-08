using AdventOfCode.Functions;
using Garyon.Extensions;

namespace AdventOfCode.Problems.Year2020
{
    public class Day25 : Problem2<ulong>
    {
        private PublicKey cardKey;
        private PublicKey doorKey;

        public override ulong SolvePart1()
        {
            int loopSize = cardKey.GetLoopSize();
            var transformer = new SubjectNumberTransformer(doorKey.Key);
            transformer.Transform(loopSize);
            return transformer.CurrentValue;
        }
        public override ulong SolvePart2()
        {
            // D20, D23
            return default;
        }

        protected override void LoadState()
        {
            var lines = FileLines;
            // The ordering is not specified, but it does not matter either
            // Due to the symmetric properties of the encryption method
            cardKey = PublicKey.Parse(lines[0]);
            doorKey = PublicKey.Parse(lines[1]);
        }

        private record PublicKey(ulong Key)
        {
            public int GetLoopSize()
            {
                return new SubjectNumberTransformer(7).TransformUntil(Key);
            }

            public static PublicKey Parse(string key) => new(key.ParseUInt64());
        }
        private class SubjectNumberTransformer
        {
            private const ulong dividend = 20201227;

            public ulong SubjectNumber { get; }
            public ulong CurrentValue { get; set; } = 1;

            public SubjectNumberTransformer(ulong subjectNumber)
            {
                SubjectNumber = subjectNumber;
            }

            public void Transform()
            {
                CurrentValue *= SubjectNumber;
                CurrentValue %= dividend;
            }
            public void Transform(int times)
            {
                for (int i = 0; i < times; i++)
                    Transform();
            }

            public int TransformUntil(ulong target)
            {
                int count = 0;
                while (CurrentValue != target)
                {
                    Transform();
                    count++;
                }
                return count;
            }
        }
    }
}
