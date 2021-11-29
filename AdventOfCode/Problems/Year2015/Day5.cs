using AdventOfCode.Functions;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2015;

public class Day5 : Problem<int>
{
    private string[] strings;

    public override int SolvePart1()
    {
        return ComputeNiceStrings<StringEvaluatorPart1>();
    }
    public override int SolvePart2()
    {
        return ComputeNiceStrings<StringEvaluatorPart2>();
    }

    private int ComputeNiceStrings<T>()
        where T : StringEvaluator, new()
    {
        return strings.Count(new T().IsNice);
    }

    protected override void ResetState()
    {
        strings = null;
    }
    protected override void LoadState()
    {
        strings = FileLines;
    }

    private class StringEvaluatorPart2 : StringEvaluator
    {
        public override bool IsNice(string s)
        {
            if (!HasTwoLetterPair(s))
                return false;

            if (!HasSingleIntermediateLetterPair(s))
                return false;

            return true;
        }

        private static bool HasTwoLetterPair(string s)
        {
            var pairs = new Dictionary<string, int>();
            for (int i = 0; i < s.Length - 1; i++)
            {
                var sequence = s.Substring(i, 2);
                if (pairs.ContainsKey(sequence))
                {
                    if (i - pairs[sequence] > 1)
                        return true;
                }
                else
                {
                    pairs.Add(sequence, i);
                }
            }

            return false;
        }
        private static bool HasSingleIntermediateLetterPair(string s)
        {
            for (int i = 0; i < s.Length - 2; i++)
            {
                char c = s[i];
                if (s[i + 2] == c)
                    return true;
            }

            return false;
        }

        private static bool IsVowel(char c)
        {
            return c switch
            {
                'a' or 'e' or 'i' or 'o' or 'u' => true,
                _ => false,
            };
        }
    }

    private class StringEvaluatorPart1 : StringEvaluator
    {
        private static readonly string[] naughtySubstrings = { "ab", "cd", "pq", "xy" };

        public override bool IsNice(string s)
        {
            if (!HasVowels(s))
                return false;

            if (!HasConsecutiveLetter(s))
                return false;

            if (HasNaughtySubstring(s))
                return false;

            return true;
        }

        private static bool HasVowels(string s) => s.CountAtLeast(IsVowel, 3);
        private static bool HasConsecutiveLetter(string s)
        {
            char previous = s[0];
            for (int i = 1; i < s.Length; i++)
            {
                char c = s[i];
                if (c == previous)
                    return true;
                previous = c;
            }

            return false;
        }
        private static bool HasNaughtySubstring(string s)
        {
            foreach (var naughty in naughtySubstrings)
            {
                if (s.Contains(naughty))
                    return true;
            }

            return false;
        }

        private static bool IsVowel(char c)
        {
            return c switch
            {
                'a' or 'e' or 'i' or 'o' or 'u' => true,
                _ => false,
            };
        }
    }

    private abstract class StringEvaluator
    {
        public abstract bool IsNice(string s);
    }
}
