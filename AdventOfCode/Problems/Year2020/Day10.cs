using AdventOfCode.Utilities;
using Garyon.DataStructures;
using System;

namespace AdventOfCode.Problems.Year2020
{
    public class Day10 : Problem<int, long>
    {
        private static FlexibleDictionary<int, long> combinations = new();

        private int[] numbers;

        public override int SolvePart1()
        {
            int previousJoltage = 0;
            var differences = new ValueCounterDictionary<int>();
            foreach (int n in numbers)
            {
                differences.Add(n - previousJoltage);
                previousJoltage = n;
            }
            differences.Add(3);

            return differences[1] * differences[3];
        }
        public override long SolvePart2()
        {
            // Registering segments
            var segmentList = new SegmentList();

            int previousJoltage = 0;
            for (int i = 0; i < numbers.Length; i++)
            {
                int difference = numbers[i] - previousJoltage;
                if (difference == 3)
                    segmentList.AddSegmentStart(i + 1);

                previousJoltage = numbers[i];
            }
            segmentList.AddSegmentStart(numbers.Length + 1);

            // Analyzing segments
            var segmentLengths = segmentList.GetSegmentLengths();
            long result = 1;

            foreach (int segmentLength in segmentLengths)
            {
                int omittable = segmentLength - 2;
                if (omittable < 1)
                    continue;

                // TODO: Improve efficiency using combinatorics? (Is that even possible?)
                long segmentCombinations = GetCombinationsForOmittable();

                result *= segmentCombinations;

                long GetCombinationsForOmittable()
                {
                    if (combinations[omittable] == default)
                        combinations[omittable] = GetCombinations(0, 1, true) + GetCombinations(0, 0, false);
                    return combinations[omittable];
                }
                long GetCombinations(int index, int currentConsecutiveAces, bool currentValue)
                {
                    if (index >= omittable - 1)
                        return 1;

                    long result = GetCombinations(index + 1, 0, false);

                    if (currentConsecutiveAces < 2)
                        result += GetCombinations(index + 1, currentConsecutiveAces + 1, true);

                    return result;
                }
            }

            return result;
        }

        protected override void ResetState()
        {
            numbers = null;
        }
        protected override void LoadState()
        {
            numbers = FileNumbersInt32;
            Array.Sort(numbers);
        }
    }
}
