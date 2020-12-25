using Garyon.Extensions;
using System;
using System.Text;

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
        public static string GetBinaryRepresentation(this ulong value, int totalBits = sizeof(ulong) * 8)
        {
            totalBits = Math.Min(totalBits, sizeof(ulong) * 8);

            char[] chars = new char[totalBits];
            for (int i = 0; i < totalBits; i++)
                chars[totalBits - 1 - i] = (value & (1UL << i)) > 0 ? '1' : '0';
            return new string(chars);
        }

        public static string GetGroupedBinaryRepresentation(this ulong value, int groupLength, int totalBits = sizeof(ulong) * 8)
        {
            var representationChars = value.GetBinaryRepresentation(totalBits).ToCharArray();
            var builder = new StringBuilder(totalBits + (totalBits + groupLength - 1) % groupLength);

            for (int i = 0; i < totalBits; i += groupLength)
            {
                for (int j = 0; j < groupLength; j++)
                {
                    int index = i + j;
                    if (index >= totalBits)
                        break;

                    builder.Append(representationChars[i + j]);
                }
                builder.Append(' ');
            }

            return builder.RemoveLast().ToString();
        }
    }
}
