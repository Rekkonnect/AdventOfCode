using AdventOfCode.Functions;
using Garyon.DataStructures;
using Garyon.Extensions;
using Garyon.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2015
{
    public class Day13 : Problem<int>
    {
        private DinnerTable dinnerTable;

        public override int SolvePart1()
        {
            return dinnerTable.GetHappiestArrangement();
        }
        public override int SolvePart2()
        {
            dinnerTable.AddSelfArrangementRules();
            return dinnerTable.GetHappiestArrangement();
        }

        protected override void ResetState()
        {
            dinnerTable = null;
        }
        protected override void LoadState()
        {
            dinnerTable = new(FileLines.Select(SittingArrangementRule.Parse));
        }

        private class DinnerTable
        {
            private readonly PersonDictionary people = new();

            public DinnerTable(IEnumerable<SittingArrangementRule> rules)
            {
                foreach (var r in rules)
                    RegisterArrangementRule(r);
            }

            public void AddSelfArrangementRules()
            {
                var self = new Person("Self");
                foreach (var person in people.Values)
                {
                    self.RegisterArrangementRule(new(self.Name, person.Name, 0));
                    person.RegisterArrangementRule(new(person.Name, self.Name, 0));
                }
                people[self.Name] = self;
            }

            public int GetHappiestArrangement()
            {
                return GetDesiredArrangement(ComparisonResult.Greater, int.MinValue);
            }
            private int GetDesiredArrangement(ComparisonResult desiredComparison, int startingHappiness)
            {
                // Shamelessly copied from D9
                var availablePeople = people.Values.ToAvailabilityDictionary();
                int desiredHappiness = startingHappiness;

                Person initialPerson;
                foreach (var person in people.Values)
                {
                    Iterate(initialPerson = person, 0, people.Count - 1);
                }

                return desiredHappiness;

                void Iterate(Person currentPerson, int currentHappiness, int remaining)
                {
                    if (remaining == 0)
                    {
                        currentHappiness += people[initialPerson.Name].GetTotalHappinessAdjustmentWith(currentPerson);

                        var result = currentHappiness.GetComparisonResult(desiredHappiness);
                        if (result == desiredComparison)
                            desiredHappiness = currentHappiness;

                        return;
                    }

                    availablePeople[currentPerson] = false;
                    foreach (var availability in availablePeople)
                    {
                        if (!availability.Value)
                            continue;

                        var targetPerson = availability.Key;
                        int adjustment = currentPerson.GetTotalHappinessAdjustmentWith(targetPerson);
                        Iterate(targetPerson, currentHappiness + adjustment, remaining - 1);
                    }
                    availablePeople[currentPerson] = true;
                }
            }

            private void RegisterArrangementRule(SittingArrangementRule rule)
            {
                people[rule.Passive].RegisterArrangementRule(rule);
            }
        }

        private class Person
        {
            private FlexibleDictionary<string, SittingArrangementRule> arrangements = new();

            public string Name { get; }

            public Person(string name)
            {
                Name = name;
            }

            public int GetHappinessAdjustmentFrom(Person person) => GetHappinessAdjustmentFrom(person.Name);
            public int GetHappinessAdjustmentFrom(string person) => arrangements[person].HappinessAdjustment;

            public int GetTotalHappinessAdjustmentWith(Person person)
            {
                return GetHappinessAdjustmentFrom(person) + person.GetHappinessAdjustmentFrom(this);
            }

            public void RegisterArrangementRule(SittingArrangementRule rule)
            {
                arrangements[rule.Active] = rule;
            }
        }
        private record SittingArrangementRule(string Passive, string Active, int HappinessAdjustment)
        {
            private static readonly Regex rulePattern = new(@"(?'passive'\w*) would have (?'adjustment'[+-]\d*) happiness units by sitting next to (?'active'\w*)\.", RegexOptions.Compiled);

            public static SittingArrangementRule Parse(string s)
            {
                s = s.Replace("gain ", "have +").Replace("lose ", "have -");
                var groups = rulePattern.Match(s).Groups;
                var passive = groups["passive"].Value;
                var active = groups["active"].Value;
                int adjustment = groups["adjustment"].Value.ParseInt32();
                return new(passive, active, adjustment);
            }
        }

        private class PersonDictionary : FlexibleDictionary<string, Person>
        {
            public override Person this[string key]
            {
                get
                {
                    if (!Dictionary.ContainsKey(key))
                        Dictionary.Add(key, new(key));
                    return Dictionary[key];
                }
            }
        }
    }
}
