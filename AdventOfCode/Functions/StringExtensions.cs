using Garyon.Exceptions;
using Garyon.Extensions;
using System;
using System.Linq;

namespace AdventOfCode.Functions
{
    public static class StringExtensions
    {
        public static int GetCharacterOccurences(this string s, char match)
        {
            int count = 0;
            foreach (var c in s)
                if (c == match)
                    count++;
            return count;
        }
        public static bool IsValidHexString(this string hex)
        {
            return !hex.Any(c => !c.IsValidHexCharacter());
        }

        // TODO: Migrate to Garyon with documentation
        public static int ParseInt32(this string s) => int.Parse(s);
        public static uint ParseUInt32(this string s) => uint.Parse(s);
        public static long ParseInt64(this string s) => long.Parse(s);
        public static ulong ParseUInt64(this string s) => ulong.Parse(s);

        public static bool TryParseInt32(this string s, out int value) => int.TryParse(s, out value);
        public static bool TryParseUInt32(this string s, out uint value) => uint.TryParse(s, out value);
        public static bool TryParseInt64(this string s, out long value) => long.TryParse(s, out value);
        public static bool TryParseUInt64(this string s, out ulong value) => ulong.TryParse(s, out value);

        public static int ParseFirstInt32(this string s, int startingIndex, out int endIndex)
        {
            if (s.TryParseFirstInt32(startingIndex, out int value, out endIndex))
                return value;

            ThrowHelper.Throw<ArgumentException>("The number could not be parsed from that index.");
            return -1;
        }
        public static bool TryParseFirstInt32(this string s, int startingIndex, out int value, out int endIndex)
        {
            for (endIndex = startingIndex; endIndex < s.Length; endIndex++)
                if (!s[endIndex].IsDigit())
                    break;
            
            return s[startingIndex..endIndex].TryParseInt32(out value);
        }
    }
}
