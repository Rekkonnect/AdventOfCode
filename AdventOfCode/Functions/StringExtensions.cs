using AdventOfCSharp.Extensions;

namespace AdventOfCode.Functions;

public static class StringExtensions
{
    // TODO: Migrate to Garyon with documentation
    public static int ParseFirstInt32(this string s, int startingIndex, out int endIndex)
    {
        return s.AsSpan().ParseFirstInt32(startingIndex, out endIndex);
    }
    public static bool TryParseFirstInt32(this string s, int startingIndex, out int value, out int endIndex)
    {
        return s.AsSpan().TryParseFirstInt32(startingIndex, out value, out endIndex);
    }

    // Too implementation-specific
    public static bool ExtractInt32AndFirstChar(this string s, out int value, out char firstChar)
    {
        firstChar = s[0];
        return s.TryParseInt32(out value);
    }
    public static bool ExtractInt64AndFirstChar(this string s, out long value, out char firstChar)
    {
        firstChar = s[0];
        return s.TryParseInt64(out value);
    }

    public static bool ReverseOf(this string a, string b)
    {
        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[^(i + 1)])
                return false;

        return true;
    }

    public static int[] ParseInt32Array(this string s, char delimiter)
    {
        return s.Split(delimiter).SelectArray(int.Parse);
    }
    public static int[] ParseInt32Array(this string s, string delimiter)
    {
        return s.Split(delimiter).SelectArray(int.Parse);
    }

    public static string SubstringAfter(this string s, string match)
    {
        int index = s.IndexOfAfter(match);
        if (index < 0)
            return s;

        return s[index..];
    }
    public static SpanString SubstringSpanAfter(this string s, string match)
    {
        int index = s.IndexOfAfter(match);
        var result = s.AsSpan();
        if (index < 0)
            return result;

        return result[index..];
    }
    public static SpanString SubstringSpanAfter(this string s, char match)
    {
        s.IndexOf(match, out int index);
        var result = s.AsSpan();
        if (index < 0)
            return result;

        return result[index..];
    }

    public static int IndexOf(this string s, string delimiter, out int nextIndex)
    {
        int index = s.IndexOf(delimiter);
        nextIndex = -1;
        if (index > -1)
        {
            nextIndex = index + delimiter.Length;
        }

        return index;
    }
    public static int IndexOf(this string s, char delimiter, out int nextIndex)
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
    /// Splits the given string based on the first occurrence of the delimiter,
    /// returning the left and right span slices of the string.
    /// </summary>
    /// <param name="s">The string to delimit.</param>
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
    public static bool SplitOnceSpan(this string s, string delimiter, out SpanString left, out SpanString right)
    {
        return s.AsSpan().SplitOnce(delimiter, out left, out right);
    }
    /// <inheritdoc cref="SplitOnceSpan(string, string, out SpanString, out SpanString)"/>
    public static bool SplitOnceSpan(this string s, char delimiter, out SpanString left, out SpanString right)
    {
        return s.AsSpan().SplitOnce(delimiter, out left, out right);
    }

    public static ImmutableArray<TResult> SelectLines<TResult>(this string s, SpanStringSelector<TResult> selector)
    {
        return s.AsSpan().SelectLines(selector);
    }
}
