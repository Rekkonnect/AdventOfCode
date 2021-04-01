using AdventOfCode.Utilities;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day6 : Problem<int>
    {
        private string[] groups;

        public override int SolvePart1()
        {
            return SolveProblem(Predicate);

            bool Predicate(KeyValuePair<char, int> counter, int peopleCount)
            {
                return counter.Value > 0;
            }
        }
        public override int SolvePart2()
        {
            return SolveProblem(Predicate);

            bool Predicate(KeyValuePair<char, int> counter, int peopleCount)
            {
                return counter.Value == peopleCount;
            }
        }

        protected override void LoadState()
        {
            groups = NormalizedFileContents.Split("\n\n");
        }
        protected override void ResetState()
        {
            groups = null;
        }

        private int SolveProblem(SumCalculationPredicate sumCalculationPredicate)
        {
            int sum = 0;
            var dictionary = new ValueCounterDictionary<char>();
            foreach (var group in groups)
            {
                var people = group.GetLines();

                foreach (var person in people)
                    foreach (var question in person)
                        dictionary.Add(question);

                sum += dictionary.Count(kvp => sumCalculationPredicate(kvp, people.Length));
                dictionary.Clear();
            }

            return sum;
        }

        private delegate bool SumCalculationPredicate(KeyValuePair<char, int> counter, int peopleCount);
    }
}
