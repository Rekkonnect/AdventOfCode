namespace AdventOfCode.Utilities;

public class SegmentList
{
    private readonly List<int> startIndices = new List<int> { 0 };

    public void AddSegmentStart(int start)
    {
        startIndices.Add(start);
    }
    public void RemoveSegmentStart(int start)
    {
        startIndices.Remove(start);
    }
    public void Clear()
    {
        startIndices.Clear();
        startIndices.Add(0);
    }

    public int[] GetSegmentLengths()
    {
        var result = new int[startIndices.Count - 1];

        for (int i = 1; i < startIndices.Count; i++)
            result[i - 1] = startIndices[i] - startIndices[i - 1];

        return result;
    }
    // This was proven to be useless in D10, but it could turn out useful in a future problem
    public ArraySegment<T>[] GetArraySegments<T>(T[] array)
    {
        var result = new ArraySegment<T>[startIndices.Count];

        startIndices.Add(array.Length);

        for (int i = 0; i < startIndices.Count - 1; i++)
            result[i] = new ArraySegment<T>(array, startIndices[i], startIndices[i + 1]);

        startIndices.RemoveAt(startIndices.Count - 1);

        return result;
    }
}
