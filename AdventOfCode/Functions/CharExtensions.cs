namespace AdventOfCode.Functions
{
    public static class CharExtensions
    {
        public static bool IsValidHexCharacter(this char c) => char.IsDigit(c) || (c >= 'a' && c <= 'f');
    }
}
