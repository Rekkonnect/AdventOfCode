namespace AdventOfCode.Functions;

public static class SpanStringExtensionsEx
{
    public static int LastNumberStartIndex(this SpanString spanString)
    {
        int startIndex = spanString.Length - 1;

        if (!spanString[startIndex].IsDigit())
            return -1;

        while (startIndex > 0)
        {
            int next = startIndex - 1;
            if (!spanString[next].IsDigit())
                break;

            startIndex = next;
        }

        return startIndex;
    }

    public static int IndexOf(this SpanString s, string delimiter, out int nextIndex)
    {
        int index = s.IndexOf(delimiter);
        nextIndex = -1;
        if (index > -1)
        {
            nextIndex = index + delimiter.Length;
        }

        return index;
    }

    public static ImmutableArray<TResult> SelectLines<TResult>(this SpanString spanString, SpanStringSelector<TResult> selector)
    {
        var arrayBuilder = ImmutableArray.CreateBuilder<TResult>();

        var lineEnumerator = spanString.EnumerateLines();
        foreach (var line in lineEnumerator)
        {
            var selected = selector(line);
            arrayBuilder.Add(selected);
        }
        return arrayBuilder.ToImmutable();
    }
}
