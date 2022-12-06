namespace AdventOfCode.Problems.Year2020;

public class Day19 : Problem<int>
{
    private RuleSystem ruleSystem;
    private string[] messages;

    public override int SolvePart1()
    {
        return GetValidatedMessagesCount();
    }
    public override int SolvePart2()
    {
        // Because that's the correct problem
        if (ruleSystem.Count < 12)
            return 0;

        var baseLeft = ruleSystem[42].RegexExpressionString;
        var baseRight = ruleSystem[31].RegexExpressionString;

        ruleSystem[8].RegexExpressionString = $"({baseLeft})+";
        ruleSystem[11].RegexExpressionString = new StringBuilder().Append("((?'open'").Append(baseLeft).Append($")+(?'close-open'").Append(baseRight).Append($")+)").ToString();
        ruleSystem[0].RecompileRegexExpression(ruleSystem);

        return GetValidatedMessagesCount();
    }

    protected override void LoadState()
    {
        var groups = NormalizedFileContents.Split("\n\n");

        ruleSystem = new RuleSystem(groups[0].GetLines(false).Select(Rule.Parse));
        messages = groups[1].GetLines(false);
    }
    protected override void ResetState()
    {
        ruleSystem = null;
        messages = null;
    }

    private int GetValidatedMessagesCount()
    {
        return messages.Count(ruleSystem[0].ExactRegexExpression.IsMatch);
    }

    private class RuleSystem
    {
        private readonly Rule[] ruleArray;

        public int Count => ruleArray.Length;

        public RuleSystem(IEnumerable<Rule> rules)
        {
            ruleArray = new Rule[rules.Max(r => r.RuleID) + 1];
            foreach (var r in rules)
                ruleArray[r.RuleID] = r;

            foreach (var r in ruleArray)
                r?.CompileRegexExpression(this);
        }

        public Rule this[int index] => ruleArray[index];
    }
    private class Rule
    {
        private Regex regexExpression;

        public int RuleID { get; }
        public string RawRuleExpression { get; }

        public Regex RegexExpression
        {
            get => regexExpression;
            set => ExactRegexExpression = new Regex($"^{regexExpression = value}$", RegexOptions.Compiled);
        }
        public Regex ExactRegexExpression { get; private set; }

        public string RegexExpressionString
        {
            get => RegexExpression?.ToString();
            set => RegexExpression = new Regex(value, RegexOptions.Compiled);
        }
        public string ExactRegexExpressionString => ExactRegexExpression?.ToString();

        public Rule(int ruleID, string rawRuleExpression)
        {
            RuleID = ruleID;
            RawRuleExpression = rawRuleExpression;
            GetRawStringRegex();
        }

        private void GetRawStringRegex()
        {
            if (RawRuleExpression.StartsWith('"'))
                RegexExpression = new(RawRuleExpression[1..^1]);
        }

        public void CompileRegexExpression(RuleSystem system)
        {
            if (RegexExpressionString != null)
                return;

            RecompileRegexExpression(system);
        }
        public void RecompileRegexExpression(RuleSystem system)
        {
            var builder = new StringBuilder();
            var split = RawRuleExpression.Split(" | ");

            builder.Append('(');
            foreach (var pattern in split)
            {
                var splitPatternIDs = pattern.Split(' ');
                foreach (var patternID in splitPatternIDs)
                {
                    var rule = system[patternID.ParseInt32()];
                    // Ensure that the rule has a compiled regex expresion
                    rule.CompileRegexExpression(system);
                    builder.Append(rule.RegexExpressionString);
                }
                builder.Append('|');
            }
            builder[^1] = ')';

            RegexExpressionString = builder.ToString();
        }

        public static Rule Parse(string raw)
        {
            var split = raw.Split(": ");
            int id = split[0].ParseInt32();
            var rawExpression = split[1];
            return new(id, rawExpression);
        }
    }
}
