using System;

namespace AdventOfCode.Functions
{
    public static class IntegerExtensions
    {
        public static string GetBinaryRepresentation(this int value, int totalBits = sizeof(int) * 8)
        {
            totalBits = Math.Min(totalBits, sizeof(int) * 8);

            char[] chars = new char[totalBits];
            for (int i = 0; i < totalBits; i++)
                chars[totalBits - 1 - i] = (value & (1 << i)) > 0 ? '1' : '0';
            return new string(chars);
        }
    }
}
