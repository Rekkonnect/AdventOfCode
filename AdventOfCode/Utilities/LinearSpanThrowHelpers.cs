namespace AdventOfCode.Utilities;

internal static class LinearSpanThrowHelpers
{
    public static void ThrowNonDivisibleDimensionality(int remainder)
    {
        if (remainder is not 0)
            ThrowNonDivisibleDimensionality();
    }
    public static void ThrowNonDivisibleDimensionality()
    {
        throw new ArgumentException("The provided span cannot be perfectly divided for the given dimension sizes.");
    }

    public static void ThrowInconsistentDimensions()
    {
        throw new ArgumentException("The dimension sizes' product must match the given span's length.");
    }
}
