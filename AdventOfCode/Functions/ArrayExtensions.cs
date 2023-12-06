namespace AdventOfCode.Functions;

public static class ArrayExtensions
{
    public static T AtOrDefault<T>(this T[] array, int index)
    {
        return index < array.Length ? array[index] : default;
    }
}
