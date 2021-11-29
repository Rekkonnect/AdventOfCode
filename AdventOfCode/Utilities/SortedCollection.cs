using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace AdventOfCode.Utilities;

public class SortedCollection<T> : ICollection<T>
{
    private readonly List<T> list;
    private readonly IComparer<T> comparer;

    public int Count => list.Count;
    bool ICollection<T>.IsReadOnly => false;

    #region Constructors
    private SortedCollection(List<T> items, IComparer<T>? itemComparer = null)
    {
        list = items;
        comparer = itemComparer ?? Comparer<T>.Default;
    }
    private SortedCollection(List<T> items, Comparison<T> comparison)
        : this(items, Comparer<T>.Create(comparison!)) { }

    public SortedCollection()
        : this(16) { }

    public SortedCollection(int capacity)
        : this(capacity, (IComparer<T>?)null) { }

    public SortedCollection(IComparer<T>? comparer)
        : this(16, comparer) { }

    public SortedCollection(int capacity, IComparer<T>? itemComparer)
        : this(new List<T>(capacity), itemComparer) { }

    public SortedCollection(int capacity, Comparison<T> comparison)
        : this(capacity, Comparer<T>.Create(comparison!)) { }

    public SortedCollection(ICollection<T> elements, IComparer<T>? comparer = null)
        : this(elements.Count, comparer)
    {
        foreach (var e in elements)
            Add(e);
    }

    public SortedCollection(ICollection<T> elements, Comparison<T> comparison)
        : this(elements, Comparer<T>.Create(comparison!)) { }
    #endregion

    public bool Contains(T item)
    {
        return list.BinarySearch(item) > -1;
    }

    public void Add(T item)
    {
        list.Insert(GetInsertionIndex(item), item);
    }
    public bool Remove(T item)
    {
        int index = list.BinarySearch(item);
        if (index == -1)
            return false;

        list.RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index) => list.RemoveAt(index);
    public void RemoveRange(int start, int length) => list.RemoveRange(start, length);

    public void Clear()
    {
        list.Clear();
    }

    public int GetInsertionIndex(T element)
    {
        int min = 0;
        int max = Count - 1;

        if (max < 0)
            return 0;

        int mid;
        int comparisonResult;

        do
        {
            mid = (min + max) / 2;

            comparisonResult = comparer.Compare(element, list[mid]);
            if (comparisonResult is 0)
                return mid;

            if (comparisonResult < 0)
                max = mid - 1;
            else
                min = mid + 1;
        }
        while (min <= max);

        if (comparisonResult > 0)
            mid++;
        return mid;
    }

    public T this[int index] => list[index];

    // Fuck this function already
    public void CopyTo(T[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    public static SortedCollection<T> FromSortedElements(ICollection<T> elements, IComparer<T>? comparer = null) => new(new List<T>(elements), comparer);
    public static SortedCollection<T> FromSortedElements(ICollection<T> elements, Comparison<T> comparison) => new(new List<T>(elements), comparison);

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
