using Garyon.DataStructures;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems
{
    using ProblemDictionary = FlexibleInitializableValueDictionary<int, FlexibleDictionary<int, Type>>;

    public class ProblemsIndex
    {
        private readonly ProblemDictionary problemDictionary = new();

        public static ProblemsIndex Instance { get; } = new();

        private ProblemsIndex()
        {
            var regex = new Regex(@"Year(\d*)\.Day(\d*)$", RegexOptions.Compiled);

            var allClasses = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract);
            foreach (var c in allClasses)
            {
                var match = regex.Match(c.FullName);
                if (!match.Success)
                    continue;

                int year = int.Parse(match.Groups[1].Value);
                int day = int.Parse(match.Groups[2].Value);

                problemDictionary[year][day] = c;
            }
        }

        public ISet<int> GetAvailableYears() => problemDictionary.Where(kvp => kvp.Value.Any()).Select(kvp => kvp.Key).ToHashSet();
        public ISet<int> GetAvailableDays(int year) => problemDictionary[year].Where(kvp => kvp.Value is not null).Select(kvp => kvp.Key).ToHashSet();

        public Type this[int year, int day] => problemDictionary[year][day];
    }
}
