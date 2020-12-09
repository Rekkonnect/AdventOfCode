using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day6 : Problem2<int>
    {
        public override int SolvePart1()
        {
            var groups = NormalizedFileContents.Split("\n\n");

            int sum = 0;
            var dictionary = new BoolDictionary('a', 'z');
            foreach (var group in groups)
            {
                foreach (var person in group.GetLines())
                    foreach (var question in person)
                        dictionary.Set(question);
                
                sum += dictionary.Count;
                dictionary.Reset();
            }

            return sum;
        }
        public override int SolvePart2()
        {
            var groups = NormalizedFileContents.Split("\n\n");

            int sum = 0;
            var dictionary = new ValueCounterDictionary<char>();
            foreach (var group in groups)
            {
                var people = group.GetLines();

                foreach (var person in people)
                    foreach (var question in person)
                        dictionary.Add(question);

                sum += dictionary.Count(kvp => kvp.Value == people.Length);
                dictionary.Clear();
            }

            return sum;
        }
    }
}
