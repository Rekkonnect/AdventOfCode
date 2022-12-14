using Garyon.Exceptions;
using System.Collections.Immutable;
using System.Globalization;
using static AdventOfCode.Functions.SpanStringExtensions;

namespace AdventOfCode.Functions;

public static class SpanStringExtensions
{
    // And of course more overloads
    public static int ParseInt32(this SpanString spanString)
    {
        return int.Parse(spanString);
    }
    public static long ParseInt64(this SpanString spanString)
    {
        return long.Parse(spanString);
    }
    public static uint ParseUInt32(this SpanString spanString)
    {
        return uint.Parse(spanString);
    }
    public static ulong ParseUInt64(this SpanString spanString)
    {
        return ulong.Parse(spanString);
    }

    // Bloating the library
    public static bool TryParseInt32(this SpanString spanString, out int value)
    {
        return int.TryParse(spanString, out value);
    }
    public static bool TryParseInt64(this SpanString spanString, out long value)
    {
        return long.TryParse(spanString, out value);
    }
    public static bool TryParseUInt32(this SpanString spanString, out uint value)
    {
        return uint.TryParse(spanString, out value);
    }
    public static bool TryParseUInt64(this SpanString spanString, out ulong value)
    {
        return ulong.TryParse(spanString, out value);
    }

    public static bool TryParseInt32(this SpanString spanString, NumberStyles numberStyles, IFormatProvider? formatProvider, out int value)
    {
        return int.TryParse(spanString, numberStyles, formatProvider, out value);
    }
    public static bool TryParseInt64(this SpanString spanString, NumberStyles numberStyles, IFormatProvider? formatProvider, out long value)
    {
        return long.TryParse(spanString, numberStyles, formatProvider, out value);
    }
    public static bool TryParseUInt32(this SpanString spanString, NumberStyles numberStyles, IFormatProvider? formatProvider, out uint value)
    {
        return uint.TryParse(spanString, numberStyles, formatProvider, out value);
    }
    public static bool TryParseUInt64(this SpanString spanString, NumberStyles numberStyles, IFormatProvider? formatProvider, out ulong value)
    {
        return ulong.TryParse(spanString, numberStyles, formatProvider, out value);
    }

    public static int ParseFirstInt32(this SpanString spanString, int startingIndex, out int endIndex)
    {
        if (spanString.TryParseFirstInt32(startingIndex, out int value, out endIndex))
            return value;

        ThrowHelper.Throw<ArgumentException>("The number could not be parsed from that index.");
        return -1;
    }
    public static bool TryParseFirstInt32(this SpanString spanString, int startingIndex, out int value, out int endIndex)
    {
        endIndex = startingIndex;
        if (spanString[endIndex] is '+' or '-')
            endIndex++;

        for (; endIndex < spanString.Length; endIndex++)
            if (!spanString[endIndex].IsDigit())
                break;

        return spanString[startingIndex..endIndex].TryParseInt32(out value);
    }

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
    public static int ParseLastInt32(this SpanString spanString)
    {
        int startIndex = LastNumberStartIndex(spanString);
        return spanString[startIndex..].ParseInt32();
    }
    public static long ParseLastInt64(this SpanString spanString)
    {
        int startIndex = LastNumberStartIndex(spanString);
        return spanString[startIndex..].ParseInt64();
    }
    public static uint ParseLastUInt32(this SpanString spanString)
    {
        int startIndex = LastNumberStartIndex(spanString);
        return spanString[startIndex..].ParseUInt32();
    }
    public static ulong ParseLastUInt64(this SpanString spanString)
    {
        int startIndex = LastNumberStartIndex(spanString);
        return spanString[startIndex..].ParseUInt64();
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

    /// <summary>
    /// Splits the given <seealso cref="SpanString"/> based on the first
    /// occurrence of the delimiter, returning the left and right slices.
    /// </summary>
    /// <param name="spanString">The string to delimit.</param>
    /// <param name="delimiter">The delimiter to find in the string.</param>
    /// <param name="left">
    /// The left segment of the string up until before the delimiter.
    /// If the delimiter is not found, this will equal the entire string.
    /// </param>
    /// <param name="right">
    /// The right segment of the string starting from the next character
    /// after the first occurrence of the delimiter. If the delimiter is
    /// not found, this will equal <see langword="default"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the delimiter was found once, otherwise
    /// <see langword="false"/>.
    /// </returns>
    public static bool SplitOnceSpan(this SpanString spanString, string delimiter, out SpanString left, out SpanString right)
    {
        left = spanString;
        right = default;

        int index = spanString.IndexOf(delimiter, out int nextIndex);
        if (index < 0)
            return false;

        left = spanString[..index];
        right = spanString[nextIndex..];
        return true;
    }
    /// <inheritdoc cref="SplitOnceSpan(SpanString, string, out SpanString, out SpanString)"/>
    public static bool SplitOnceSpan(this SpanString spanString, char delimiter, out SpanString left, out SpanString right)
    {
        left = spanString;
        right = default;

        int index = spanString.IndexOf(delimiter, out int nextIndex);
        if (index < 0)
            return false;

        left = spanString[..index];
        right = spanString[nextIndex..];
        return true;
    }

    public static SpanString SliceAfter(this SpanString spanString, char delimiter)
    {
        spanString.IndexOf(delimiter, out int startIndex);
        if (startIndex < 0)
            return spanString;

        return spanString[startIndex..];
    }
    public static SpanString SliceBefore(this SpanString spanString, char delimiter)
    {
        int endIndex = spanString.IndexOf(delimiter);
        if (endIndex < 0)
            return spanString;

        return spanString[..endIndex];
    }

    public static SpanString SliceBetween(this SpanString spanString, char delimiterStart, char delimiterEnd)
    {
        spanString = SliceAfter(spanString, delimiterStart);
        return SliceBefore(spanString, delimiterEnd);
    }

    public static SpanString SliceAfter(this SpanString spanString, string delimiter)
    {
        spanString.IndexOf(delimiter, out int startIndex);
        if (startIndex < 0)
            return spanString;

        return spanString[startIndex..];
    }
    public static SpanString SliceBefore(this SpanString spanString, string delimiter)
    {
        int endIndex = spanString.IndexOf(delimiter);
        if (endIndex < 0)
            return spanString;

        return spanString[..endIndex];
    }

    public static SpanString SliceBetween(this SpanString spanString, string delimiterStart, string delimiterEnd)
    {
        spanString = SliceAfter(spanString, delimiterStart);
        return SliceBefore(spanString, delimiterEnd);
    }

    public static IReadOnlyList<string> SplitToStrings(this SpanString spanString, string delimiter)
    {
        return SplitSelect(spanString, delimiter, span => new string(span));
    }
    public static IReadOnlyList<T> SplitSelect<T>(this SpanString spanString, string delimiter, SpanStringSelector<T> selector)
    {
        var results = new List<T>();

        var remainingSpan = spanString;
        while (true)
        {
            int delimiterIndex = remainingSpan.IndexOf(delimiter, out int nextIndex);
            if (delimiterIndex < 0)
                break;

            var delimitedSlice = remainingSpan[..delimiterIndex];
            AddResult(delimitedSlice);
            remainingSpan = remainingSpan[nextIndex..];
        }

        AddResult(remainingSpan);

        return results;

        void AddResult(SpanString span)
        {
            results.Add(selector(span));
        }
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

    public static IReadOnlyList<string> SplitToStrings(this SpanString spanString, char delimiter)
    {
        return SplitSelect(spanString, delimiter, span => new string(span));
    }
    public static IReadOnlyList<T> SplitSelect<T>(this SpanString spanString, char delimiter, SpanStringSelector<T> selector)
    {
        var results = new List<T>();

        var remainingSpan = spanString;
        while (true)
        {
            int delimiterIndex = remainingSpan.IndexOf(delimiter, out int nextIndex);
            if (delimiterIndex < 0)
                break;

            var delimitedSlice = remainingSpan[..delimiterIndex];
            AddResult(delimitedSlice);
            remainingSpan = remainingSpan[nextIndex..];
        }

        AddResult(remainingSpan);

        return results;

        void AddResult(SpanString span)
        {
            results.Add(selector(span));
        }
    }
}

public delegate T SpanStringSelector<T>(SpanString spanString);
