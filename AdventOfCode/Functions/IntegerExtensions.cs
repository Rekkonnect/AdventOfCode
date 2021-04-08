namespace AdventOfCode.Functions
{
    public static class IntegerExtensions
    {
        public static char ToHexChar(this int value)
        {
            if (value < 10)
                return (char)(value + '0');
            return (char)(value - 10 + 'A');
        }
    }
}
