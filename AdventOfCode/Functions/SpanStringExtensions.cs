namespace AdventOfCode.Functions;

using System;

// For convenience when typing the type out
using SpanString = ReadOnlySpan<char>;

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
    public static int IndexOf(this SpanString s, char delimiter, out int nextIndex)
    {
        int index = s.IndexOf(delimiter);
        nextIndex = -1;
        if (index > -1)
        {
            nextIndex = index + 1;
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
}
