namespace AdventOfCode.Functions
{
    public static class StringExtensions
    {
        public static string NormalizeLineEndings(this string s) => s.Replace("\r\n", "\n").Replace('\r', '\n');

        public static string[] GetLines(this string s) => s.NormalizeLineEndings().Split('\n');

        public static int OccurrencesOf(this string s, char match)
        {
            int count = 0;
            foreach (var c in s)
                if (c == match)
                    count++;
            return count;
        }
    }
}
