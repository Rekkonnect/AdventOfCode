namespace AdventOfCode.Utilities;

public sealed class FlexStack<T>
{
    private readonly Stack<T> stack;

    public int Count => stack.Count;

    public T Bottommost { get; private set; }

    public FlexStack()
    {
        stack = new();
    }
    public FlexStack(int capacity)
    {
        stack = new(capacity);
    }
    public FlexStack(FlexStack<T> other)
    {
        stack = new(other.stack.Reverse());
        Bottommost = other.Bottommost;
    }

    public FlexStack<T> Clone() => new(this);

    public T Peek() => stack.Peek();

    public T Pop()
    {
        if (Count is 1)
            Bottommost = default;

        return stack.Pop();
    }

    public void Push(T crate)
    {
        if (Count is 0)
            Bottommost = crate;

        stack.Push(crate);
    }

    public IEnumerable<T> PopRange(int count)
    {
        if (Count == count)
        {
            Bottommost = default;
        }

        return stack.PopRange(count);
    }

    public void PushRange(ICollection<T> values)
    {
        if (values.Count is 0)
            return;

        if (Count is 0)
        {
            Bottommost = values.First();
        }

        stack.PushRange(values);
    }
    public void PushRangeReversed(ICollection<T> values)
    {
        if (values.Count is 0)
            return;

        if (Count is 0)
        {
            Bottommost = values.Last();
        }

        stack.PushRange(values.Reverse());
    }
}
