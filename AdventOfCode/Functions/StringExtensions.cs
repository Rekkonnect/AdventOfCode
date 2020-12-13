using System.Linq;
using System.Runtime.CompilerServices;

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
    }
}
