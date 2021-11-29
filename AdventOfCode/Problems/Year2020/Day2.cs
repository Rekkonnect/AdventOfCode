using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2020;

public class Day2 : Problem<int>
{
    private PasswordPolicyRule[] passwords;

    public override int SolvePart1()
    {
        return GetValidPasswords(passwords.Select(p => new PasswordPolicyPart1(p)));
    }
    public override int SolvePart2()
    {
        return GetValidPasswords(passwords.Select(p => new PasswordPolicyPart2(p)));
    }

    private int GetValidPasswords(IEnumerable<PasswordPolicyBase> policies)
    {
        return policies.Count(p => p.IsValid);
    }

    protected override void ResetState()
    {
        passwords = null;
    }
    protected override void LoadState()
    {
        passwords = ParsedFileLines(PasswordPolicyRule.Parse);
    }

    private class PasswordPolicyPart2 : PasswordPolicyBase
    {
        public override bool IsValid => (Password[Start - 1] == MatchingCharacter) ^ (Password[End - 1] == MatchingCharacter);

        public PasswordPolicyPart2(PasswordPolicyRule rule)
            : base(rule) { }
    }
    private class PasswordPolicyPart1 : PasswordPolicyBase
    {
        public override bool IsValid
        {
            get
            {
                int occurrences = Password.GetCharacterOccurences(MatchingCharacter);
                return Start <= occurrences && occurrences <= End;
            }
        }

        public PasswordPolicyPart1(PasswordPolicyRule rule)
            : base(rule) { }
    }
    private abstract class PasswordPolicyBase
    {
        public PasswordPolicyRule Rule { get; }

        public int Start => Rule.Start;
        public int End => Rule.End;
        public char MatchingCharacter => Rule.MatchingCharacter;
        public string Password => Rule.Password;

        public abstract bool IsValid { get; }

        protected PasswordPolicyBase(PasswordPolicyRule rule)
        {
            Rule = rule;
        }
    }
    private record PasswordPolicyRule(int Start, int End, char MatchingCharacter, string Password)
    {
        private readonly static Regex policyPattern = new(@"(?'start'\d*)\-(?'end'\d*) (?'match'\w)\: (?'password'\w*)", RegexOptions.Compiled);

        public static PasswordPolicyRule Parse(string s)
        {
            var groups = policyPattern.Match(s).Groups;
            int start = groups["start"].Value.ParseInt32();
            int end = groups["end"].Value.ParseInt32();
            char matchingCharacter = groups["match"].Value[0];
            string password = groups["password"].Value;
            return new(start, end, matchingCharacter, password);
        }
    }
}
