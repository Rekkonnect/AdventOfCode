namespace AdventOfCode.Functions
{
    public static class CharExtensions
    {
        public static int ToInt(this char c) => c - '0';

        public static bool IsValidHexCharacter(this char c) => char.IsDigit(c) || (c >= 'a' && c <= 'f');
    }
}
