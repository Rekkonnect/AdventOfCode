namespace AdventOfCode.Functions;

public static class ArrayExtensions
{
    public static T[] SortBy<T>(this T[] array, IComparer<T> comparer)
    {
        Array.Sort(array, comparer);
        return array;
    }
    public static T[] SortBy<T>(this T[] array, Comparison<T> comparison)
    {
        Array.Sort(array, comparison);
        return array;
    }
}
