using Garyon.DataStructures;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AdventOfCode.Problems.Year2020;

public class Day7 : Problem<int>
{
    private RuleSystem system;

    public override int SolvePart1()
    {
        return system.GetAllowedContainers("shiny gold").Count;
    }
    public override int SolvePart2()
    {
        return system.GetTotalContainedBags("shiny gold");
    }

    protected override void LoadState()
    {
        var rules = ParsedFileLines(Rule.Parse);
        system = new RuleSystem(rules);
    }
    protected override void ResetState()
    {
        system = null;
    }

    private class RuleSystem
    {
        private readonly FlexibleInitializableValueDictionary<string, HashSet<string>> directContainability = new();
        private readonly FlexibleDictionary<string, Rule> ruleDictionary = new();

        public RuleSystem(IEnumerable<Rule> rules)
        {
            ruleDictionary = new FlexibleDictionary<string, Rule>(rules, r => r.Container);
            AnalyzeContainability();
        }

        public HashSet<string> GetAllowedContainers(string contained)
        {
            var set = new HashSet<string>();
            foreach (var c in directContainability[contained])
            {
                set.Add(c);
                set.UnionWith(GetAllowedContainers(c));
            }
            return set;
        }
        public int GetTotalContainedBags(string container, int initialSum = 0)
        {
            int sum = initialSum;

            foreach (var contained in this[container].Contained)
                sum += contained.Count * GetTotalContainedBags(contained.Color, 1);

            return sum;
        }

        private void AnalyzeContainability()
        {
            foreach (var r in ruleDictionary.Values)
                foreach (var contained in r.Contained)
                    directContainability[contained.Color].Add(r.Container);
        }

        public Rule this[string container] => ruleDictionary[container];
    }

    private class Rule
    {
        private int? totalContainedBags;

        public string Container { get; init; }
        public ImmutableList<ContainedBag> Contained { get; init; }

        public int TotalContainedBags
        {
            get
            {
                if (totalContainedBags == null)
                {
                    totalContainedBags = 0;
                    foreach (var c in Contained)
                        totalContainedBags += c.Count;
                }

                return totalContainedBags.Value;
            }
        }

        public Rule(string container, IEnumerable<ContainedBag> contained)
        {
            Container = container;
            Contained = contained.ToImmutableList();
        }

        public static Rule Parse(string rawRule)
        {
            var split = rawRule.Split(" contain ");
            var container = split[0][..^(" bags".Length)];
            var contained = split[1].Split(", ");

            var containedBags = new List<ContainedBag>();
            if (contained.Length > 1 || !contained[0].StartsWith("no other"))
            {
                foreach (var c in contained)
                {
                    int lastSpace = c.LastIndexOf(' ');
                    var rawContainedBag = c[..lastSpace];
                    containedBags.Add(ContainedBag.Parse(rawContainedBag));
                }
            }
            return new Rule(container, containedBags);
        }
    }

    private class ContainedBag
    {
        public int Count { get; init; }
        public string Color { get; init; }

        public ContainedBag() { }
        public ContainedBag(int count, string color)
        {
            Count = count;
            Color = color;
        }

        public static ContainedBag Parse(string raw)
        {
            int firstSpaceIndex = raw.IndexOf(' ');
            int count = int.Parse(raw[..firstSpaceIndex]);
            var color = raw[(firstSpaceIndex + 1)..];
            return new ContainedBag(count, color);
        }
    }
}
