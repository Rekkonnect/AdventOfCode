using static System.Math;

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

        public static int GetDigitCount(this int value)
        {
            if (value is 0)
                return 1;

            return (int)Round(Floor(Log10(value))) + 1;
        }
    }
}
