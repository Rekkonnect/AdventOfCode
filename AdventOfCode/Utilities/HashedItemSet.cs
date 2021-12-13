namespace AdventOfCode.Utilities;

public class HashedItemSet<T> : HashSet<int>
{
    public HashedItemSet()
        : base() { }
    public HashedItemSet(int capacity)
        : base(capacity) { }
    public HashedItemSet(HashedItemSet<T> other)
        : base(other) { }
    public HashedItemSet(IEnumerable<T> other)
        : this()
    {
        AddRange(other);
    }

    public void AddRange(IEnumerable<T> elements)
    {
        foreach (var e in elements)
            Add(e);
    }
    public bool Add(T item) => Add(item.GetHashCode());
    public bool Contains(T item) => Contains(item.GetHashCode());
    public bool Remove(T item) => Remove(item.GetHashCode());
}
