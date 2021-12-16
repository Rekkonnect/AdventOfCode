namespace AdventOfCode.Functions;

public static class EnumerableProducts
{
    public static int Product(this IEnumerable<int> source)
    {
        int product = 1;
        foreach (int value in source)
            product *= value;

        return product;
    }
    public static uint Product(this IEnumerable<uint> source)
    {
        uint product = 1;
        foreach (uint value in source)
            product *= value;

        return product;
    }
    public static long Product(this IEnumerable<long> source)
    {
        long product = 1;
        foreach (long value in source)
            product *= value;

        return product;
    }
    public static ulong Product(this IEnumerable<ulong> source)
    {
        ulong product = 1;
        foreach (ulong value in source)
            product *= value;

        return product;
    }
}
