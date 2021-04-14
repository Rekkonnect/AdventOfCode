using Garyon.Exceptions;
using Garyon.Extensions;
using System;

namespace AdventOfCode.Functions
{
    public static class StringExtensions
    {
        // TODO: Migrate to Garyon with documentation
        public static int ParseFirstInt32(this string s, int startingIndex, out int endIndex)
        {
            if (s.TryParseFirstInt32(startingIndex, out int value, out endIndex))
                return value;

            ThrowHelper.Throw<ArgumentException>("The number could not be parsed from that index.");
            return -1;
        }
        public static bool TryParseFirstInt32(this string s, int startingIndex, out int value, out int endIndex)
        {
            endIndex = startingIndex;
            if (s[endIndex] is '+' or '-')
                endIndex++;

            for (; endIndex < s.Length; endIndex++)
                if (!s[endIndex].IsDigit())
                    break;

            return s[startingIndex..endIndex].TryParseInt32(out value);
        }

        // Too implementation-specific
        public static bool ExtractInt32AndFirstChar(this string s, out int value, out char firstChar)
        {
            firstChar = s[0];
            return s.TryParseInt32(out value);
        }

        public static bool ReverseOf(this string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[^(i + 1)])
                    return false;

            return true;
        }
    }
}
