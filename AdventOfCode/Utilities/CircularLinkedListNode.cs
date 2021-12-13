using Garyon.Exceptions;

namespace AdventOfCode.Utilities;

public class CircularLinkedListNode<T>
{
    private CircularLinkedListNode<T> next;
    private CircularLinkedListNode<T> previous;

    public T Value { get; set; }

    public CircularLinkedListNode<T> Next
    {
        get => next;
        set
        {
            if (value == this)
                ThrowHelper.Throw<ArgumentException>("The node cannot point to itself as its next node.");

            if (IsSelfLoopingHead)
            {
                previous = value;
                previous.next = this;
            }

            next = value;
            next.previous = this;
        }
    }
    public CircularLinkedListNode<T> Previous
    {
        get => previous;
        set
        {
            if (value == this)
                ThrowHelper.Throw<ArgumentException>("The node cannot point to itself as its previous node.");

            if (IsSelfLoopingHead)
            {
                next = value;
                next.previous = this;
            }

            previous = value;
            previous.next = this;
        }
    }

    public bool IsSelfLoopingHead => (next == this) && (previous == this);

    public CircularLinkedListNode() { }
    public CircularLinkedListNode(T value)
    {
        Value = value;
    }
    public CircularLinkedListNode(T value, CircularLinkedListNode<T> previous, CircularLinkedListNode<T> next)
        : this(value)
    {
        Previous = previous;
        Next = next;
    }

    public void SetSelfLoopingHead()
    {
        previous = next = this;
    }

    public CircularLinkedListNode<T> GetPrevious(int steps)
    {
        var current = this;
        for (int i = 0; i < steps; i++)
            current = current.previous;
        return current;
    }
    public CircularLinkedListNode<T> GetNext(int steps)
    {
        var current = this;
        for (int i = 0; i < steps; i++)
            current = current.next;
        return current;
    }

    public override string ToString() => Value.ToString();
}
