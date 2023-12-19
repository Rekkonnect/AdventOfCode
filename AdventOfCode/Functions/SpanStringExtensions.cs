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

    public static short ParseInt16(this SpanString spanString)
    {
        return short.Parse(spanString);
    }
    public static ushort ParseUInt16(this SpanString spanString)
    {
        return ushort.Parse(spanString);
    }
}
