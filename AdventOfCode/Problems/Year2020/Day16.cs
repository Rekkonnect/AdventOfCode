using AdventOfCode.Utilities.TwoDimensions;
using Garyon.DataStructures;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day16 : Problem<int, ulong>
    {
        // Is it just me or am I abusing LINQ a bit too much?

        private FieldRuleSystem ruleSystem = new();

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
                ruleSystem.RegisterRule(r);

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
            private FlexibleDictionary<string, ValidFieldRanges> fieldRanges = new();
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

            public void RegisterRule(string raw)
            {
                var split = raw.Split(": ");
                var fieldName = split[0];

                var validRanges = new ValidFieldRanges();
                var splitValidRanges = split[1].Split(" or ");
                foreach (var range in splitValidRanges)
                {
                    var splitRange = range.Split('-').Select(int.Parse).ToArray();
                    validRanges.Set(splitRange[0], splitRange[1]);
                }

                fieldRanges[fieldName] = validRanges;
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
        private class ValidFieldRanges
        {
            public const int Length = 1000;

            private bool[] validRanges = new bool[Length];

            public void Set(int start, int end, bool value = true)
            {
                for (int i = start; i <= end; i++)
                    validRanges[i] = value;
            }

            public void Clear()
            {
                Array.Clear(validRanges, 0, validRanges.Length);
            }

            public bool this[int index]
            {
                get => validRanges[index];
                set => validRanges[index] = value;
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
}
