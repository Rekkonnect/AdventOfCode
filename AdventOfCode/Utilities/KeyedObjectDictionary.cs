using System.Collections;
using System.Collections.Generic;

namespace AdventOfCode.Utilities;

public class KeyedObjectDictionary<TKey, TObject> : IEnumerable<TObject>
    where TObject : IKeyedObject<TKey>
{
    private readonly Dictionary<TKey, TObject> d;

    public int Count => d.Count;

    public ICollection<TKey> Keys => d.Keys;
    public ICollection<TObject> Values => d.Values;

    public KeyedObjectDictionary()
    {
        d = new();
    }
    public KeyedObjectDictionary(int capacity)
    {
        d = new(capacity);
    }
    public KeyedObjectDictionary(IEnumerable<TObject> objects)
        : this()
    {
        AddRange(objects);
    }
    public KeyedObjectDictionary(KeyedObjectDictionary<TKey, TObject> other)
    {
        d = new(other.d);
    }

    public bool TryAddPreserve(TObject value) => TryAddPreserve(value, out _);
    public bool TryAddPreserve(TObject value, out TObject resultingValue)
    {
        var key = value.Key;
        if (ContainsKey(key))
        {
            resultingValue = d[key];
            return false;
        }

        d.Add(key, resultingValue = value);
        return true;
    }

    public void Add(TObject value) => d.Add(value.Key, value);
    public bool Remove(TKey key) => d.Remove(key);
    public void Clear() => d.Clear();

    public void AddRange(IEnumerable<TObject> objects)
    {
        foreach (var o in objects)
            Add(o);
    }

    public bool Contains(TObject item) => d.ContainsKey(item.Key);
    public bool ContainsKey(TKey key) => d.ContainsKey(key);

    public bool TryGetValue(TKey key, out TObject value) => d.TryGetValue(key, out value);

    public IEnumerator<TObject> GetEnumerator() => d.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public TObject this[TKey key]
    {
        get => d[key];
        set => d[key] = value;
    }
}
