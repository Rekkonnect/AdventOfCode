#if DEBUG
using AdventOfCode.Functions;
#endif

namespace AdventOfCode.Problems.Year2018;

public partial class Day12 : Problem<int, long>
{
    private PlantIterator plantIterator;

    public override int SolvePart1()
    {
        var iterator = new PlantIterator(plantIterator);
        iterator.Iterate(20);
        return iterator.State.PotIndexSum;
    }
    public override long SolvePart2()
    {
        var iterator = new PlantIterator(plantIterator);
        int iterations = iterator.IterateUntilContinuous();
        var state = iterator.State;
        long remainingIterations = 50_000_000_000 - iterations;

        return state.PotIndexSum + remainingIterations * state.PotCount;
    }

    protected override void LoadState()
    {
        plantIterator = PlantIterator.Parse(NormalizedFileContents);
    }
    protected override void ResetState()
    {
        plantIterator = null;
    }

    private struct NeighboringPots
    {
        public int Code { get; private set; }

        public NeighboringPots(IEnumerable<bool> pots)
        {
            Code = 0;
            foreach (var pot in pots)
            {
                Code <<= 1;

                if (pot)
                    Code |= 1;
            }
        }

        public NeighboringPots(LinkedListNode<Pot> centralPot)
        {
            Code = 0;

            const int centerMask = 0b00100;

            if (centralPot.Value.AlivePlant)
                Code |= centerMask;

            int leftMask = centerMask;
            var leftCurrent = centralPot;

            int rightMask = centerMask;
            var rightCurrent = centralPot;

            for (int i = 0; i < 2; i++)
            {
                // Left
                leftCurrent = leftCurrent?.Previous;
                leftMask <<= 1;

                if (leftCurrent?.Value.AlivePlant ?? false)
                    Code |= leftMask;

                // Right
                rightCurrent = rightCurrent?.Next;
                rightMask >>= 1;

                if (rightCurrent?.Value.AlivePlant ?? false)
                    Code |= rightMask;
            }
        }

        public override int GetHashCode() => Code;
    }

    private partial record PlantRule(NeighboringPots InitialPlants, bool ResultPlant)
    {
        private static readonly Regex rulePattern = RuleRegex();

        public static PlantRule Parse(string rule)
        {
            var groups = rulePattern.Match(rule).Groups;
            var initialPlants = new NeighboringPots(groups["initial"].Value.Select(ParsePotCharacter));
            bool result = ParsePotCharacter(groups["result"].Value[0]);
            return new(initialPlants, result);
        }

        private static bool ParsePotCharacter(char c) => c is '#';
        [GeneratedRegex("(?'initial'[.#]*) => (?'result'[.#])", RegexOptions.Compiled)]
        private static partial Regex RuleRegex();
    }

    private class PlantIterator
    {
        private readonly PlantRuleDictionary ruleDictionary;

        public PotRow State { get; }

        private PlantIterator(PotRow initialState, PlantRuleDictionary rules)
        {
            State = initialState;
            ruleDictionary = rules;
        }
        public PlantIterator(PotRow initialState, IEnumerable<PlantRule> rules)
            : this(initialState, new PlantRuleDictionary(rules)) { }
        public PlantIterator(PlantIterator iterator)
            : this(new(iterator.State), iterator.ruleDictionary) { }

        public int IterateUntilContinuous()
        {
            return State.IterateUntilContinuous(ruleDictionary);
        }
        public void Iterate(int times)
        {
            State.Iterate(ruleDictionary, times);
        }

        public static PlantIterator Parse(string raw)
        {
            var sections = raw.Split("\n\n");
            var initialState = PotRow.Parse(sections[0]);
            var rules = sections[1].GetLines(false).Select(PlantRule.Parse);
            return new(initialState, rules);
        }
    }

    private partial class PotRow
    {
        private static readonly Regex initialStatePattern = InitialStateRegex();

        private PotLinkedList pots;

        public int PotIndexSum => pots.GetAlivePlantIndices().Sum();
        public int PotCount => pots.GetAlivePlantIndices().Count;

        public bool IsContinuous
        {
            get
            {
                var current = pots.First;

                while (!current.Value.AlivePlant)
                {
                    current = current.Next;

                    if (current is null)
                        return false;
                }

                // Has found the first alive plant
                while (current.Value.AlivePlant)
                {
                    current = current.Next;

                    if (current is null)
                        return true;
                }

                // Has found all the consecutive alive plants
                while (!current.Value.AlivePlant)
                {
                    current = current.Next;

                    if (current is null)
                        return true;
                }

                // Has found a non-consecutive alive plant
                return false;
            }
        }

        private PotRow(IEnumerable<bool> initialPlants)
        {
            pots = new(initialPlants.WithIndex().Select(value => new Pot(value.Index, value.Current)));
        }
        public PotRow(PotRow row)
        {
            pots = new(row.pots);
        }

        // TODO: Solve the problem below
        // This does not account for other infinitely repeating patterns
        public int IterateUntilContinuous(PlantRuleDictionary rules)
        {
            int iterations = 0;

            while (!IsContinuous)
            {
                Iterate(rules);
                iterations++;
            }

            return iterations;
        }
        public void Iterate(PlantRuleDictionary rules, int times)
        {
#if DEBUG
            int min = 0 - times / 2;
            int max = pots.Count + times;
            int timesDigits = times.GetDigitCount();
#endif

            for (int i = 0; i < times; i++)
            {
#if DEBUG
                Console.WriteLine($"{i.ToString().PadLeft(timesDigits)}: {ToString(min, max)}");
#endif
                Iterate(rules);
            }

#if DEBUG
            Console.WriteLine($"{times.ToString().PadLeft(timesDigits)}: {ToString(min, max)}");
#endif
        }
        public void Iterate(PlantRuleDictionary rules)
        {
            pots.EnsureEmptyPots();

            var next = new PotLinkedList(pots);

            var currentPotsNode = pots.First;
            var currentNextPotsNode = next.First;

            while (currentPotsNode is not null)
            {
                var currentOldPots = new NeighboringPots(currentPotsNode);
                var result = rules[currentOldPots.Code]?.ResultPlant ?? false;

                currentNextPotsNode.Value = currentNextPotsNode.Value.WithPlant(result);

                currentPotsNode = currentPotsNode.Next;
                currentNextPotsNode = currentNextPotsNode.Next;
            }

            pots = next;
        }

        public string ToString(int min, int max)
        {
            int length = max - min + 1;
            char[] result = new char[length];
            result.Fill('.');

            var currentNode = pots.First;
            while (currentNode.Value.Index < min)
                currentNode = currentNode.Next;

            for (int i = currentNode.Value.Index; i <= max; i++)
            {
                if (currentNode.Value.AlivePlant)
                    result[i - min] = '#';

                if ((currentNode = currentNode.Next) is null)
                    break;
            }

            return new(result);
        }

        public static PotRow Parse(string rawInitialState)
        {
            var plantsString = initialStatePattern.Match(rawInitialState).Groups["initialState"].Value;
            var plants = plantsString.Select(p => p == '#').ToArray();
            return new(plants);
        }

        [GeneratedRegex("initial state: (?'initialState'.*)")]
        private static partial Regex InitialStateRegex();
    }

    // TODO: Migrate this into a HashedItemDictionary
    private class PlantRuleDictionary : FlexibleDictionary<int, PlantRule>
    {
        public PlantRuleDictionary()
            : base() { }
        public PlantRuleDictionary(IEnumerable<PlantRule> rules)
            : base()
        {
            AddRange(rules);
        }

        public void Add(PlantRule rule)
        {
            Add(rule.InitialPlants.Code, rule);
        }
        public void AddRange(IEnumerable<PlantRule> rules)
        {
            foreach (var rule in rules)
                Add(rule);
        }
    }

    private class PotLinkedList : LinkedList<Pot>
    {
        public PotLinkedList()
            : base() { }
        public PotLinkedList(IEnumerable<Pot> pots)
            : base(pots) { }
        public PotLinkedList(PotLinkedList other)
            : base(other) { }

        public List<int> GetAlivePlantIndices()
        {
            var indices = new List<int>(Count);

            foreach (var node in this)
            {
                if (!node.AlivePlant)
                    continue;

                indices.Add(node.Index);
            }

            return indices;
        }

        public void EnsureEmptyPots()
        {
            EnsureEmptyLeftPots();
            EnsureEmptyRightPots();
        }

        public void EnsureEmptyLeftPots()
        {
            EnsureEmptyPots(First.Next, c => c.Previous, AddLeftPots);
        }
        public void EnsureEmptyRightPots()
        {
            EnsureEmptyPots(Last.Previous, c => c.Next, AddRightPots);
        }

        private static void EnsureEmptyPots(LinkedListNode<Pot> first, CurrentPotSelector currentPotSelector, Action<int> potAdder)
        {
            int consecutive = 0;
            var current = first;

            for (int i = 0; i < 2; i++)
            {
                if (!current.Value.AlivePlant)
                    consecutive++;
                else
                    consecutive = 0;

                current = currentPotSelector(current);
            }

            potAdder(2 - consecutive);
        }

        public void AddLeftPots(int count)
        {
            for (int i = 0; i < count; i++)
                AddLeftPot();
        }
        public void AddLeftPot()
        {
            AddBefore(First, First.Value.NewPrevious());
        }

        public void AddRightPots(int count)
        {
            for (int i = 0; i < count; i++)
                AddRightPot();
        }
        public void AddRightPot()
        {
            AddAfter(Last, Last.Value.NewNext());
        }

        private delegate LinkedListNode<Pot> CurrentPotSelector(LinkedListNode<Pot> pot);
    }

    private struct Pot
    {
        public int Index { get; }
        public bool AlivePlant { get; }

        public Pot(int index, bool alivePlant)
        {
            Index = index;
            AlivePlant = alivePlant;
        }

        public Pot WithPlant(bool alivePlant) => new(Index, alivePlant);

        public Pot NewPrevious() => new(Index - 1, default);
        public Pot NewNext() => new(Index + 1, default);
    }
}
