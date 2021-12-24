namespace AdventOfCode.Functions;

public static class IComparableExtensions
{
    public static void AssignMin<T>(this ref T min, T other)
        where T : struct, IComparable<T>
    {
        if (other.CompareTo(min) < 0)
            min = other;
    }
    public static void AssignMax<T>(this ref T max, T other)
        where T : struct, IComparable<T>
    {
        if (other.CompareTo(max) > 0)
            max = other;
    }
}
