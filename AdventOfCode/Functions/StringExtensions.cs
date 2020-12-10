namespace AdventOfCode.Functions
{
    public static class StringExtensions
    {
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
