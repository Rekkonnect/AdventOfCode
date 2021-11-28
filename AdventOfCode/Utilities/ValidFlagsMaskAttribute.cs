using System;

namespace AdventOfCode.Utilities
{
    // TODO: Use this in the following API:
    // T Garyon.Extensions.EnumExtensions.PreserveValidFlags<T>(this T enum)

    // Attributes cannot be generic :pepega:
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public class ValidFlagsMaskAttribute : Attribute
    {
        public Enum ValidFlagsMask { get; }

        public ValidFlagsMaskAttribute(Enum validFlagsMask)
        {
            ValidFlagsMask = validFlagsMask;
        }
    }
}
