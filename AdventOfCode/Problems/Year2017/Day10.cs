using AdventOfCode.Utilities;
using System.Linq;

namespace AdventOfCode.Problems.Year2017
{
    public class Day10 : Problem<int, string>
    {
        private string input;

        public override int SolvePart1()
        {
            var knotHash = new KnotHasher(input.Split(',').Select(int.Parse).ToArray());
            knotHash.IterateOnce();
            return knotHash[0] * knotHash[1];
        }
        public override string SolvePart2()
        {
            return new KnotHasher(input.Select(c => (int)c).Concat(new[] { 17, 31, 73, 47, 23 }).ToArray()).GetKnotHash();
        }

        protected override void LoadState()
        {
            input = FileContents;
        }
        protected override void ResetState()
        {
            input = null;
        }

        private class KnotHasher
        {
            public const int ElementCount = 256;

            private readonly ConstructableArray<int> array;
            private int[] lengths;
            private int currentSkipSize;

            public KnotHasher(int[] inputLengths, int elementCount = ElementCount)
            {
                lengths = inputLengths;
                array = new(elementCount);
                Reset();
            }

            public void Reset()
            {
                currentSkipSize = 0;
                for (int i = 0; i < array.Length; i++)
                    array[i] = i;
            }

            public void IterateOnce()
            {
                Iterate();
                array.ResetRotation();
            }

            public string GetKnotHash()
            {
                for (int i = 0; i < 64; i++)
                    Iterate();

                array.ResetRotation();
                var resultHash = new int[16];
                for (int i = 0; i < array.Length; i++)
                {
                    resultHash[i / 16] ^= array[i];
                }
                return string.Concat(resultHash.Select(h => h.ToString("x2")));
            }

            private void Iterate()
            {
                foreach (var length in lengths)
                {
                    array.ReverseOrder(0, length - 1);
                    array.Rotate(-(length + currentSkipSize));
                    currentSkipSize++;
                }
            }

            public int this[int index] => array[index];
        }
    }
}
