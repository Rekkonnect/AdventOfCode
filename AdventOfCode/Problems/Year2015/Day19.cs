using AdventOfCode.Utilities;
using AdventOfCSharp;
using AdventOfCSharp.Extensions;
using Garyon.DataStructures;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2015;

public class Day19 : Problem<int>
{
    private string calibrationMolecule;
    private ReplacementMachine replacementMachine;

    public override int SolvePart1()
    {
        return replacementMachine.GenerateReplacedMolecules(calibrationMolecule);
    }
    public override int SolvePart2()
    {
        return replacementMachine.FindShortestModuleReplacement(calibrationMolecule);
    }

    protected override void ResetState()
    {
        calibrationMolecule = null;
        replacementMachine = null;
    }
    protected override void LoadState()
    {
        var contents = NormalizedFileContents;
        var sections = contents.Split("\n\n");
        var rules = sections[0];
        calibrationMolecule = sections[1];
        var moleculeRules = rules.GetLines(false).Select(RawReplacementRule.Parse).ToArray();
        replacementMachine = new(moleculeRules);
    }

    private class ReplacementMachine
    {
        private readonly IDMap<string> moleculeIDs = new() { "e" };
        private readonly ReplacementRuleSystem replacementRuleSystem;

        public ReplacementMachine(IEnumerable<RawReplacementRule> rules)
        {
            rules.ForEach(RegisterMolecules);
            var compiledRules = rules.Select(GetCompiledReplacementRule);
            replacementRuleSystem = new(compiledRules);
        }

        public int FindShortestModuleReplacement(string targetMolecule)
        {
            // Credits to u/askalski for the discovery
            var targetMoleculeString = GetMoleculeString(targetMolecule);
            int length = targetMoleculeString.Length;
            var counters = targetMoleculeString.Counters;
            int rn = counters[moleculeIDs.GetID("Rn")];
            int ar = counters[moleculeIDs.GetID("Ar")];
            int y = counters[moleculeIDs.GetID("Y")];
            return length - rn - ar - 2 * y - 1;
        }

        // Have fun, supercomputers
        private int FindShortestModuleReplacementBruteForce(string targetMolecule)
        {
            int leastSteps = int.MaxValue;

            var targetMoleculeString = GetMoleculeString(targetMolecule);

            Iterate(targetMoleculeString, 0, 0);

            return leastSteps;

            void Iterate(MoleculeString currentMoleculeString, int steps, int start)
            {
                if (steps >= leastSteps)
                    return;

                if (currentMoleculeString.Length == 1) // target "e"
                {
                    leastSteps = steps;
                    return;
                }

                int end = currentMoleculeString.Length;
                for (int i = start; i < end; i++)
                {
                    for (int j = 1; j <= replacementRuleSystem.LargestReplacementRule; j++)
                    {
                        if (i + j > currentMoleculeString.Length)
                            break;

                        var substring = currentMoleculeString.Substring(i, j);
                        int replacement = replacementRuleSystem.GetRuleParent(substring);
                        if (replacement == -1)
                            continue;

                        var replaced = currentMoleculeString.Replace(replacement, i, j);
                        Iterate(replaced, steps + 1, i + replacementRuleSystem.LargestReplacementRule);
                    }
                }
            }
        }

        public int GenerateReplacedMolecules(string originalMolecule)
        {
            return GenerateReplacedMoleculesSet(originalMolecule).Count;
        }

        private HashSet<MoleculeString> GenerateReplacedMoleculesSet(string originalMolecule)
        {
            var originalMoleculeString = GetMoleculeString(originalMolecule);
            var distinctReplacedMolecules = new HashSet<MoleculeString>(originalMoleculeString.Length * 16);

            for (int i = 0; i < originalMoleculeString.Length; i++)
            {
                var currentMolecule = originalMoleculeString[i];

                foreach (var replacement in replacementRuleSystem.GetReplacements(currentMolecule))
                {
                    var replaced = originalMoleculeString.Replace(replacement, i);

                    distinctReplacedMolecules.Add(replaced);
                }
            }

            return distinctReplacedMolecules;
        }

        private MoleculeString GetMoleculeString(string raw)
        {
            var splitMolecules = SplitMolecules(raw).ToArray();
            return new(splitMolecules.Select(m => moleculeIDs.GetID(m)).ToArray());
        }

        private CompiledReplacementRule GetCompiledReplacementRule(RawReplacementRule replacementRule)
        {
            int moleculeID = moleculeIDs.GetID(replacementRule.Molecule);
            var replacementString = GetMoleculeString(replacementRule.Replacement);
            return new(moleculeID, replacementString);
        }

        private void RegisterMolecules(RawReplacementRule rule)
        {
            RegisterMolecules(rule.Molecule);
            RegisterMolecules(rule.Replacement);
        }
        private void RegisterMolecules(string s)
        {
            var split = SplitMolecules(s);
            foreach (var molecule in split)
                moleculeIDs.Add(molecule);
        }
        private static IEnumerable<string> SplitMolecules(string s)
        {
            int uppercaseIndex = 0;
            for (int i = 1; i < s.Length; i++)
            {
                if (s[i].IsUpper())
                {
                    yield return s[uppercaseIndex..i];
                    uppercaseIndex = i;
                }
            }
            yield return s[uppercaseIndex..];
        }
    }

    private class MoleculeString : IEquatable<MoleculeString>
    {
        private int? hashCode;
        private readonly int[] moleculeIDs;

        public bool IsEmpty => moleculeIDs.Length == 1 && moleculeIDs[0] == 0;
        public int Length => moleculeIDs.Length;

        public ValueCounterDictionary<int> Counters => new(moleculeIDs);

        public MoleculeString(int[] ids)
        {
            moleculeIDs = ids;
        }

        public MoleculeString Substring(int start, int length)
        {
            return this[start..(start + length)];
        }

        public bool SubstringEquals(MoleculeString match, int start)
        {
            if (start + match.Length > Length)
                return false;

            for (int i = 0; i < match.Length; i++)
                if (moleculeIDs[start + i] != match[i])
                    return false;

            return true;
        }

        public MoleculeString Replace(int replacement, int start, int length = 1)
        {
            return new(moleculeIDs.Replace(replacement, start, length));
        }
        public MoleculeString Replace(MoleculeString replacement, int start, int length = 1)
        {
            return Replace(replacement.moleculeIDs, start, length);
        }
        public MoleculeString Replace(int[] replacement, int start, int length = 1)
        {
            return new(moleculeIDs.Replace(replacement, start, length));
        }

        public bool Equals(MoleculeString other)
        {
            if (moleculeIDs.Length != other.Length)
                return false;

            for (int i = 0; i < moleculeIDs.Length; i++)
                if (moleculeIDs[i] != other[i])
                    return false;

            return true;
        }

        public int this[int index] => moleculeIDs[index];
        public MoleculeString this[Range range] => new(moleculeIDs[range]);

        public override int GetHashCode()
        {
            if (hashCode is null)
                CalculateHashCode();
            return hashCode.Value;
        }
        public override bool Equals(object obj)
        {
            return obj is MoleculeString moleculeString && Equals(moleculeString);
        }

        public override string ToString() => string.Concat(moleculeIDs.Select(id => (char)('a' + id)));

        // Imagine spending time trying to generate a hash code generation algorithm
        // NOTE: An implementation of a hash code generator existed but was discarded in favor of the built-in string hash code generator
        private void CalculateHashCode() => hashCode = ToString().GetHashCode();
    }

    private class ReplacementRuleSystem
    {
        // This class is being privately used, there is no reason to defensively copy the returned data
        private readonly HashSet<CompiledReplacementRule> rules = new();

        private readonly FlexibleListDictionary<int, CompiledReplacementRule> rulesByMolecule = new();
        private readonly FlexibleListDictionary<int, MoleculeString> ruleReplacementsByMolecule = new();
        private readonly Dictionary<MoleculeString, int> ruleReplacementParents = new();
        private readonly FlexibleListDictionary<int, CompiledReplacementRule> matchingReplacementStartRules = new();
        private readonly FlexibleListDictionary<int, int> matchingReplacementStartIDs = new();

        public int LargestReplacementRule { get; private init; }

        public IEnumerable<CompiledReplacementRule> Rules => rules;

        public ReplacementRuleSystem(IEnumerable<CompiledReplacementRule> compiledRules)
        {
            foreach (var r in compiledRules)
            {
                rules.Add(r);

                rulesByMolecule[r.Molecule].Add(r);
                ruleReplacementsByMolecule[r.Molecule].Add(r.Replacement);
                ruleReplacementParents.Add(r.Replacement, r.Molecule);

                if (r.Replacement.Length > LargestReplacementRule)
                    LargestReplacementRule = r.Replacement.Length;
            }

            // Discover the replacement starts
            var recursionStackSet = new HashSet<int>();
            foreach (var r in compiledRules)
            {
                int replacementStart = r.Replacement[0];
                recursionStackSet.Add(replacementStart);
                FindMatchingReplacementStarts(replacementStart, recursionStackSet);
            }
        }

        private void FindMatchingReplacementStarts(int id, HashSet<int> recursionStackSet)
        {
            // The replacement starts have been previously identified
            if (matchingReplacementStartIDs.ContainsKey(id))
                return;

            // Avoid infinite recursion
            if (recursionStackSet.Contains(id))
                return;

            var replacementStartRules = matchingReplacementStartRules[id];
            var replacementStartIDs = matchingReplacementStartIDs[id];
            foreach (var r in rulesByMolecule[id])
            {
                int ruleStart = r.Replacement[0];

                // Recursively identify the rules for the rule's starting molecule
                recursionStackSet.Add(ruleStart);
                FindMatchingReplacementStarts(ruleStart, recursionStackSet);
                recursionStackSet.Remove(ruleStart);

                // Add the rule's starting molecule's replacement start rules and itself
                replacementStartRules.AddRange(matchingReplacementStartRules[ruleStart]);
                replacementStartRules.Add(r);

                replacementStartIDs.AddRange(matchingReplacementStartIDs[ruleStart]);
                replacementStartIDs.Add(ruleStart);
            }
        }

        public int GetRuleParent(MoleculeString moleculeString)
        {
            if (ruleReplacementParents.TryGetValue(moleculeString, out int value))
                return value;
            return -1;
        }
        public IEnumerable<MoleculeString> GetReplacements(int moleculeID)
        {
            return ruleReplacementsByMolecule[moleculeID];
        }
        public IEnumerable<CompiledReplacementRule> GetMatchingReplacementStartRules(int moleculeID)
        {
            return matchingReplacementStartRules[moleculeID];
        }
        public IEnumerable<int> GetMatchingReplacementStartIDs(int moleculeID)
        {
            return matchingReplacementStartIDs[moleculeID];
        }
    }

    private record RawReplacementRule(string Molecule, string Replacement)
    {
        private static readonly Regex rulePattern = new(@"(?'molecule'\w*) =\> (?'replacement'\w*)", RegexOptions.Compiled);

        public static RawReplacementRule Parse(string s)
        {
            var groups = rulePattern.Match(s).Groups;
            var molecule = groups["molecule"].Value;
            var replacement = groups["replacement"].Value;
            return new(molecule, replacement);
        }
    }
    private record CompiledReplacementRule(int Molecule, MoleculeString Replacement);
}
