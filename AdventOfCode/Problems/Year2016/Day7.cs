using AdventOfCode.Functions;

namespace AdventOfCode.Problems.Year2016;

public partial class Day7 : Problem<int>
{
    private IPv7[] ips;

    public override int SolvePart1()
    {
        return ips.Count(ip => ip.SupportsTLS);
    }
    public override int SolvePart2()
    {
        return ips.Count(ip => ip.SupportsSSL);
    }

    protected override void ResetState()
    {
        ips = null;
    }
    protected override void LoadState()
    {
        ips = ParsedFileLines(IPv7.Parse);
    }

    private partial record IPv7(IPv7Sequence[] Sequences)
    {
        private static readonly Regex ipPattern = IPRegex();

        private bool? supportsTLS;
        private bool? supportsSSL;

        public bool SupportsTLS => supportsTLS ??= DetermineTLS();
        public bool SupportsSSL => supportsSSL ??= DetermineSSL();

        private bool DetermineTLS()
        {
            bool nonHypernetSequenceABBA = false;

            foreach (var s in Sequences)
            {
                if (s.IsHypernetSequence && s.HasABBA)
                    return false;

                nonHypernetSequenceABBA |= s.HasABBA;
            }

            return nonHypernetSequenceABBA;
        }
        private bool DetermineSSL()
        {
            Sequences.Dissect(s => s.IsHypernetSequence, out var hypernetSequences, out var supernetSequences);

            foreach (var s in supernetSequences)
            {
                for (int i = 0; i < s.Characters.Length - 2; i++)
                {
                    var substring = s.Characters[i..(i + 3)];
                    if (substring[0] == substring[1])
                        continue;
                    if (substring[0] != substring[2])
                        continue;

                    char a = substring[0];
                    char b = substring[1];
                    var match = $"{b}{a}{b}";

                    if (hypernetSequences.Any(h => h.Characters.Contains(match)))
                        return true;
                }
            }

            return false;
        }

        public static IPv7 Parse(string raw)
        {
            var resultingSequences = new List<IPv7Sequence>();
            var matches = ipPattern.Matches(raw);
            foreach (Match m in matches)
            {
                var left = m.Groups["left"].Value;
                var hypernet = m.Groups["hypernet"].Value;
                var right = m.Groups["right"].Value;

                if (left.Length > 0)
                    resultingSequences.Add(new(left, false));

                resultingSequences.Add(new(hypernet, true));

                if (right.Length > 0)
                    resultingSequences.Add(new(right, false));
            }

            return new(resultingSequences.ToArray());
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            foreach (var s in Sequences)
            {
                if (s.IsHypernetSequence)
                    result.Append('[');
                result.Append(s.Characters);
                if (s.IsHypernetSequence)
                    result.Append(']');
            }

            return result.ToString();
        }

        [GeneratedRegex("(?'left'\\w*)\\[(?'hypernet'\\w*)\\](?'right'\\w*)", RegexOptions.Compiled)]
        private static partial Regex IPRegex();
    }
    private record IPv7Sequence(string Characters, bool IsHypernetSequence)
    {
        private bool? hasABBA;

        public bool HasABBA => hasABBA ??= DetermineABBA();

        private bool DetermineABBA()
        {
            for (int i = 0; i < Characters.Length - 3; i++)
            {
                var a = Characters[i..(i + 2)];
                if (a[0] == a[1])
                    continue;

                var b = Characters[(i + 2)..(i + 4)];
                if (b.ReverseOf(a))
                    return true;
            }

            return false;
        }
    }
}
