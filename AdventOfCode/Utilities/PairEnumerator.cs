using System.Collections;
using System.Collections.Generic;

namespace AdventOfCode.Utilities;

public struct PairEnumerator<T> : IEnumerator<T>
{
    private readonly T a, b;
    private int index;

    public T Current => index switch
    {
        0 => a,
        1 => b,
        _ => default,
    };
    object IEnumerator.Current => Current;

    public PairEnumerator(T first, T second)
    {
        a = first;
        b = second;
        index = -1;
    }

    public bool MoveNext()
    {
        index++;
        return index < 2;
    }
    public void Reset()
    {
        index = -1;
    }

    public void Dispose() { }
}
