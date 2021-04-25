using Garyon.Extensions;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2017
{
    public class Day15 : Problem<int>
    {
        private Duel duel;

        public override int SolvePart1()
        {
            return duel.FindMatchCount(40000000);
        }
        public override int SolvePart2()
        {
            return duel.FindStrictMatchCount(5000000);
        }

        protected override void LoadState()
        {
            duel = Duel.Parse(FileLines);
        }
        protected override void ResetState()
        {
            duel = null;
        }

        private class GeneratorA : Generator
        {
            public override ulong Factor => 16807;
            public override ulong StrictMultiple => 4;

            public GeneratorA(ulong startingValue)
                : base(startingValue) { }
        }
        private class GeneratorB : Generator
        {
            public override ulong Factor => 48271;
            public override ulong StrictMultiple => 8;

            public GeneratorB(ulong startingValue)
                : base(startingValue) { }
        }

        private abstract class Generator
        {
            public ulong CurrentValue { get; protected set; }

            public ulong StartingValue { get; }
            public abstract ulong Factor { get; }

            public abstract ulong StrictMultiple { get; }

            public Generator(ulong startingValue)
            {
                StartingValue = startingValue;
                CurrentValue = startingValue;
            }

            public ulong Next()
            {
                CurrentValue *= Factor;
                CurrentValue %= int.MaxValue;
                return CurrentValue;
            }
            public ulong NextStrict()
            {
                do
                {
                    CurrentValue *= Factor;
                    CurrentValue %= int.MaxValue;
                }
                while (CurrentValue % StrictMultiple is not 0);
                return CurrentValue;
            }

            public void Reset()
            {
                CurrentValue = StartingValue;
            }
        }

        private class Duel
        {
            private static readonly Regex generatorPattern = new(@"Generator (?'name'\w) starts with (?'startingValue'\d*)", RegexOptions.Compiled);

            public GeneratorA A { get; }
            public GeneratorB B { get; }

            public Duel(GeneratorA a, GeneratorB b)
            {
                A = a;
                B = b;
            }

            public int FindMatchCount(int pairCount)
            {
                A.Reset();
                B.Reset();

                int count = 0;
                for (int i = 0; i < pairCount; i++)
                {
                    ulong a = A.Next();
                    ulong b = B.Next();

                    if (Match(a, b))
                        count++;
                }
                return count;
            }
            public int FindStrictMatchCount(int pairCount)
            {
                A.Reset();
                B.Reset();

                int count = 0;
                for (int i = 0; i < pairCount; i++)
                {
                    ulong a = A.NextStrict();
                    ulong b = B.NextStrict();

                    if (Match(a, b))
                        count++;
                }
                return count;
            }

            private static bool Match(ulong a, ulong b) => (a & 0xFFFF) == (b & 0xFFFF);

            public static Duel Parse(string[] generatorLines)
            {
                GeneratorA a = null;
                GeneratorB b = null;

                // This really sucks
                for (int i = 0; i < generatorLines.Length; i++)
                {
                    var groups = generatorPattern.Match(generatorLines[i]).Groups;
                    var name = groups["name"].Value;
                    ulong startingValue = groups["startingValue"].Value.ParseUInt64();

                    if (name is "A")
                        a = new(startingValue);
                    else
                        b = new(startingValue);
                }

                return new(a, b);
            }
        }
    }
}
