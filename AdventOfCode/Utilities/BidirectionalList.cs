namespace AdventOfCode.Utilities;

public class BidirectionalList<T>
{
    private readonly FlexibleList<T> left;
    private readonly FlexibleList<T> right;

    public int Count => left.Count + right.Count;

    public int MinIndex => -left.Count;
    public int MaxIndex => right.Count - 1;

    public BidirectionalList()
    {
        left = new();
        right = new();
    }
    public BidirectionalList(IEnumerable<T> elements)
    {
        left = new();
        right = new(elements);
    }

    public BidirectionalList(BidirectionalList<T> other)
    {
        left = new(other.left);
        right = new(other.right);
    }

    public void AddLeft(T value) => left.Add(value);
    public void AddRight(T value) => right.Add(value);

    public void RemoveAt(int index)
    {
        if (index < 0)
            left.RemoveAt(-index - 1);
        else
            right.RemoveAt(index);
    }

    public T this[int index]
    {
        get => index < 0 ? left[-index - 1] : right[index];
        set
        {
            if (index < 0)
                left[-index - 1] = value;
            else
                right[index] = value;
        }
    }
}
