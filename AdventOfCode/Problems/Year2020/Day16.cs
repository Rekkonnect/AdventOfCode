using AdventOfCode.Utilities;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2020;

public partial class Day16 : Problem<int, ulong>
{
    // Is it just me or am I abusing LINQ a bit too much?

    private readonly FieldRuleSystem ruleSystem = new();

    private Ticket personalTicket;
    private List<Ticket> nearbyTickets;

    public override int SolvePart1()
    {
        return nearbyTickets.Select(t => t.GetTicketScanningErrorRate(ruleSystem.AllRanges)).Sum();
    }
    public override ulong SolvePart2()
    {
        var validTickets = nearbyTickets.Where(t => t.IsValid(ruleSystem.AllRanges));
        ruleSystem.DiscoverColumns(validTickets);

        var columns = ruleSystem.FieldColumnNames;
        ulong product = 1;
        for (int i = 0; i < columns.Length; i++)
        {
            if (!columns[i].StartsWith("departure"))
                continue;

            product *= (ulong)personalTicket.Fields[i];
        }
        return product;
    }

    protected override void LoadState()
    {
        var fileGroups = NormalizedFileContents.Split("\n\n");

        var ranges = fileGroups[0].GetLines(false);
        foreach (var r in ranges)
            ruleSystem.RegisterRule(ValidFieldRanges.Parse(r));

        personalTicket = Ticket.Parse(fileGroups[1].GetLines(false)[1]);

        nearbyTickets = fileGroups[2].GetLines(false).Skip(1).Select(Ticket.Parse).ToList();
    }
    protected override void ResetState()
    {
        ruleSystem.Clear();
        personalTicket = null;
        nearbyTickets = null;
    }

    private class FieldRuleSystem
    {
        private readonly KeyedObjectDictionary<string, ValidFieldRanges> fieldRanges = new();
        private ValidFieldRanges cachedAllRanges;

        private string[] availableNames;
        private BoolLatinSquare candidateSquare;

        public ValidFieldRanges AllRanges => cachedAllRanges ??= ValidFieldRanges.Union(fieldRanges.Values);

        public string[] FieldColumnNames
        {
            get
            {
                string[] result = new string[availableNames.Length];
                for (int i = 0; i < availableNames.Length; i++)
                {
                    int index = candidateSquare.GetFirstIndexInX(i);
                    if (index > -1)
                        result[i] = availableNames[index];
                }
                return result;
            }
        }

        public void DiscoverColumns(IEnumerable<Ticket> validTickets)
        {
            if (candidateSquare is null)
                InitializeColumnDiscovery();

            foreach (var ticket in validTickets)
            {
                for (int i = 0; i < ticket.Fields.Length; i++)
                {
                    int value = ticket.Fields[i];

                    for (int j = 0; j < candidateSquare.Size; j++)
                    {
                        if (!candidateSquare[i, j])
                            continue;

                        candidateSquare[i, j] = fieldRanges[availableNames[j]][value];
                    }
                }
            }
        }

        private void InitializeColumnDiscovery()
        {
            availableNames = fieldRanges.Keys.ToArray();
            candidateSquare = new(availableNames.Length, true);
        }

        public void RegisterRule(ValidFieldRanges validRanges)
        {
            fieldRanges.Add(validRanges);
        }
        public void Clear()
        {
            fieldRanges.Clear();
            cachedAllRanges = null;
            candidateSquare = null;
        }
    }

    private class Ticket
    {
        public int[] Fields;

        public Ticket(IEnumerable<int> fields)
        {
            Fields = fields.ToArray();
        }

        public bool IsValid(ValidFieldRanges allRanges) => !GetInvalidFields(allRanges).Any();
        public int GetTicketScanningErrorRate(ValidFieldRanges allRanges)
        {
            return GetInvalidFields(allRanges).Sum();
        }

        private IEnumerable<int> GetInvalidFields(ValidFieldRanges allRanges) => Fields.Where(f => !allRanges[f]);

        public static Ticket Parse(string raw)
        {
            return new(raw.Split(',').Select(int.Parse));
        }

        public override string ToString() => Fields.Select(f => f.ToString()).Aggregate((a, b) => $"{a}, {b}");
    }
    private partial class ValidFieldRanges : IKeyedObject<string>
    {
        public const int Length = 1000;

        private static readonly Regex rulePattern = RuleRegex();
        private static readonly Regex rangePattern = RangeRegex();

        [GeneratedRegex("(?'name'[\\w ]*)\\: (?'ranges'.*)", RegexOptions.Compiled)]
        private static partial Regex RuleRegex();
        [GeneratedRegex("(?'start'\\d*)\\-(?'end'\\d*)", RegexOptions.Compiled)]
        private static partial Regex RangeRegex();

        private readonly bool[] validRanges = new bool[Length];

        public string Name { get; }

        string IKeyedObject<string>.Key => Name;

        public ValidFieldRanges(string name = null) => Name = name;

        public void Set(int start, int end, bool value = true)
        {
            for (int i = start; i <= end; i++)
                validRanges[i] = value;
        }

        public bool this[int index]
        {
            get => validRanges[index];
            set => validRanges[index] = value;
        }

        public static ValidFieldRanges Parse(string raw)
        {
            var groups = rulePattern.Match(raw).Groups;
            var fieldName = groups["name"].Value;
            var result = new ValidFieldRanges(fieldName);

            var ranges = groups["ranges"].Value;

            foreach (Match range in rangePattern.Matches(ranges))
            {
                var rangeGroups = range.Groups;
                int start = rangeGroups["start"].Value.ParseInt32();
                int end = rangeGroups["end"].Value.ParseInt32();
                result.Set(start, end);
            }

            return result;
        }

        public static ValidFieldRanges Union(IEnumerable<ValidFieldRanges> ranges)
        {
            var result = new ValidFieldRanges();

            foreach (var range in ranges)
                for (int i = 0; i < Length; i++)
                    if (range[i])
                        result[i] = true;

            return result;
        }
    }
}
