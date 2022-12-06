using AdventOfCode.Utilities;
using AdventOfCSharp.Utilities;

namespace AdventOfCode.Problems.Year2021;

public partial class Day14 : Problem<ulong>
{
    // If you wanna burn your computer, use this
    private Polymer templatePolymer;

    private CompactPolymer compactTemplatePolymer;
    private InsertionRules rules;

    public override ulong SolvePart1()
    {
        return SolvePart(10);
    }
    public override ulong SolvePart2()
    {
        return SolvePart(40);
    }

    private ulong SolvePartThroughFireAndFlames(int iterations)
    {
        var polymer = templatePolymer.Clone();
        var result = polymer.BurnComputer(rules, iterations);
        var minmax = result.ElementCounters.Values.MinMax();
        return (ulong)minmax.Difference();
    }
    private ulong SolvePart(int iterations)
    {
        var polymer = new CompactPolymer(compactTemplatePolymer);
        polymer.IterateInsertion(iterations);
        var minmax = polymer.GetElementCounters().Values.MinMax();
        return minmax.Difference();
    }

    protected override void LoadState()
    {
        var sections = NormalizedFileContents.Trim().Split("\n\n");
        rules = InsertionRules.Parse(sections[1].GetLines(false));
        compactTemplatePolymer = new(sections[0], rules);
        templatePolymer = new(sections[0]);
    }
    protected override void ResetState()
    {
        compactTemplatePolymer = null;
        rules = null;
    }

    private class InsertionRules
    {
        private readonly RuleTable rules = new();
        private readonly List<ElementAdjacency> adjacencies;

        public IReadOnlyCollection<ElementAdjacency> Adjacencies => adjacencies;

        public InsertionRules(IEnumerable<InsertionRule> insertionRules)
        {
            insertionRules.ForEach(rules.AddRule);
            adjacencies = insertionRules.Select(rule => rule.Adjacency).ToList();
        }

        public InsertionRule this[ElementAdjacency adjacency]
        {
            get => rules[adjacency];
            set => rules[adjacency] = value;
        }

        public static InsertionRules Parse(string[] rawRules)
        {
            return new(rawRules.Select(InsertionRule.Parse));
        }
    }

    private abstract class ElementAdjacencyTable<T> : LookupTable<T>
    {
        public ElementAdjacencyTable()
            : base(0, 26 * 26) { }
        public ElementAdjacencyTable(ElementAdjacencyTable<T> other)
            : base(other) { }

        public T this[ElementAdjacency adjacency]
        {
            get => this[adjacency.Code];
            set => this[adjacency.Code] = value;
        }
    }
    private sealed class AdjacencyCounterTable : ElementAdjacencyTable<ulong>
    {
        public AdjacencyCounterTable() { }
        public AdjacencyCounterTable(AdjacencyCounterTable other)
            : base(other) { }
    }
    private sealed class RuleTable : ElementAdjacencyTable<InsertionRule>
    {
        public void AddRule(InsertionRule rule)
        {
            this[rule.Adjacency.Code] = rule;
        }
    }

    private partial record InsertionRule(ElementAdjacency Adjacency, char Inserted)
    {
        private static readonly Regex pattern = RuleRegex();

        [GeneratedRegex("(?'adjacency'\\w{2}) -> (?'inserted'\\w)")]
        private static partial Regex RuleRegex();

        public ElementAdjacency LeftProducedAdjacency = new(Adjacency.Left, Inserted);
        public ElementAdjacency RightProducedAdjacency = new(Inserted, Adjacency.Right);

        public static InsertionRule Parse(string raw)
        {
            var groups = pattern.Match(raw).Groups;
            var adjacency = ElementAdjacency.Parse(groups["adjacency"].Value);
            char inserted = groups["inserted"].Value[0];
            return new(adjacency, inserted);
        }
    }

    private record struct ElementAdjacency(char Left, char Right)
    {
        public static ElementAdjacency Parse(string raw) => new(raw[0], raw[1]);

        public int Code => (Left - 'A') * 26 + (Right - 'A');

        public override int GetHashCode() => Code;
    }

    private sealed class CompactPolymer
    {
        private readonly AdjacencyCounterTable counters = new();
        private readonly InsertionRules rules;
        private readonly string initial;

        public CompactPolymer(string initialString, InsertionRules insertionRules)
        {
            initial = initialString;
            rules = insertionRules;
            for (int i = 0; i < initialString.Length - 1; i++)
            {
                var adjacency = new ElementAdjacency(initialString[i], initialString[i + 1]);
                counters[adjacency]++;
            }
        }
        public CompactPolymer(CompactPolymer other)
        {
            counters = new(other.counters);
            rules = other.rules;
            initial = other.initial;
        }

        public void IterateInsertion(int steps)
        {
            for (int i = 0; i < steps; i++)
                IterateInsertion();
        }
        public void IterateInsertion()
        {
            var adjacencyCounters = MapAdjacencyCounters();
            foreach (var counter in adjacencyCounters)
            {
                var rule = rules[counter.Key];
                counters[rule.Adjacency] -= counter.Value;
                counters[rule.LeftProducedAdjacency] += counter.Value;
                counters[rule.RightProducedAdjacency] += counter.Value;
            }
        }
        private IEnumerable<KeyValuePair<ElementAdjacency, ulong>> MapAdjacencyCounters()
        {
            var result = new List<KeyValuePair<ElementAdjacency, ulong>>(rules.Adjacencies.Count);
            foreach (var adjacency in rules.Adjacencies)
            {
                ulong count = counters[adjacency];
                if (count is 0)
                    continue;

                result.Add(new(adjacency, count));
            }

            return result;
        }
        public FlexibleDictionary<char, ulong> GetElementCounters()
        {
            var result = new FlexibleDictionary<char, ulong>();
            foreach (var adjacency in rules.Adjacencies)
            {
                ulong count = counters[adjacency];
                result[adjacency.Left] += count;
                result[adjacency.Right] += count;
            }

            // Adjacent chars will appear duplicated
            foreach (var c in result.Keys)
                result[c] /= 2;

            // Except for the first and the last that are not adjacent to any other
            result[initial.First()]++;
            result[initial.Last()]++;

            return result;
        }
    }

    private sealed class Polymer
    {
        private readonly char[] elements;

        public ValueCounterDictionary<char> ElementCounters => new(elements);

        private Polymer(char[] elementArray)
        {
            elements = elementArray;
        }
        public Polymer(string elementString)
            : this(elementString.ToCharArray()) { }

        public Polymer Clone() => new(elements.ToArray());

        // *cough* Phrasing *cough*
        private char[] GapeString()
        {
            char[] newElements = new char[elements.Length * 2 - 1];
            for (int i = 0; i < elements.Length; i++)
                newElements[i * 2] = elements[i];
            return newElements;
        }
        public Polymer BurnComputer(InsertionRules rules)
        {
            var newElements = GapeString();

            for (int i = 0; i < elements.Length - 1; i++)
            {
                var adjacency = new ElementAdjacency(elements[i], elements[i + 1]);
                newElements[i * 2 + 1] = rules[adjacency].Inserted;
            }

            return new(newElements);
        }

        public Polymer BurnComputer(InsertionRules rules, int iterations)
        {
            var current = this;
            for (int i = 0; i < iterations; i++)
                current = current.BurnComputer(rules);
            return current;
        }
    }
}
