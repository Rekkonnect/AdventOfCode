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
}
