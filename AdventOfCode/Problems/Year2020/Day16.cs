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
            return new(raw.AsSpan().SplitSelect(',', SpanStringExtensions.ParseInt32));
        }

        public override string ToString() => string.Join(", ", Fields);
    }
    private record ValidFieldRanges(string Name = null) : IKeyedObject<string>
    {
        public const int Length = 1000;

        private readonly bool[] validRanges = new bool[Length];

        string IKeyedObject<string>.Key => Name;

        public void Set(FieldRange range, bool value = true)
        {
            Set(range.Start, range.End, value);
        }
        public void Set(int start, int end, bool value = true)
        {
            int count = end - start + 1;
            Array.Fill(validRanges, value, start, count);
        }

        public bool this[int index]
        {
            get => validRanges[index];
            set => validRanges[index] = value;
        }

        public static ValidFieldRanges Parse(string raw)
        {
            var rawSpan = raw.AsSpan();
            rawSpan.SplitOnceSpan(": ", out var fieldNameSpan, out var fieldRanges);
            var fieldName = fieldNameSpan.ToString();
            var result = new ValidFieldRanges(fieldName);

            var ranges = fieldRanges.SplitSelect(" or ", FieldRange.Parse);

            foreach (var range in ranges)
            {
                result.Set(range);
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

    private record struct FieldRange(int Start, int End)
    {
        public static FieldRange Parse(SpanString spanString)
        {
            spanString.SplitOnceSpan('-', out var left, out var right);
            int start = left.ParseInt32();
            int end = right.ParseInt32();
            return new(start, end);
        }
    }
}
