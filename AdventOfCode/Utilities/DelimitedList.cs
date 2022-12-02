namespace AdventOfCode.Utilities;

public class DelimitedList<T> : List<T>
{
    public string Delimiter { get; }

    public DelimitedList(string delimiter)
    {
        Delimiter = delimiter;
    }
    public DelimitedList(int capacity, string delimiter)
        : base(capacity)
    {
        Delimiter = delimiter;
    }
    public DelimitedList(IEnumerable<T> collection, string delimiter)
        : base(collection)
    {
        Delimiter = delimiter;
    }

    public override string ToString()
    {
        return this.Select(ToStringSelector).Combine(Delimiter);

        static string ToStringSelector(T other) => other!.ToString()!;
    }
}

public static class DelimitedListExtensions
{
    public static DelimitedList<T> ToDelimitedList<T>(this IEnumerable<T> source, string delimiter) => new(source, delimiter);
}