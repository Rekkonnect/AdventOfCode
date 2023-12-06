using System.Buffers;
using SearchChars = System.Buffers.SearchValues<char>;

namespace AdventOfCode.Functions;

public static class Parsing
{
    public static readonly SearchChars DigitsWithMinus
        = SearchValues.Create("0123456789-");
    public static readonly SearchChars Digits
        = SearchValues.Create("0123456789");
    public static readonly SearchChars LowerEnglishLetters
        = SearchValues.Create("abcdefghijklmnopqrstuvwxyz");
    public static readonly SearchChars UpperEnglishLetters
        = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
    public static readonly SearchChars EnglishLetters
        = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
    public static readonly SearchChars EnglishLettersDigits
        = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");

    public static SpanString LastNumberSlice(SpanString s, bool supportMinus = false)
    {
        var digits = DigitsWithOptionalMinus(supportMinus);
        int index = s.LastIndexOfAnyExcept(digits);
        if (index < 0)
            return s;

        return s[(index + 1)..];
    }

    public static short ParseLastInt16(SpanString s, bool supportMinus = false)
    {
        return LastNumberSlice(s, supportMinus).ParseInt16();
    }
    public static int ParseLastInt32(SpanString s, bool supportMinus = false)
    {
        return LastNumberSlice(s, supportMinus).ParseInt32();
    }
    public static long ParseLastInt64(SpanString s, bool supportMinus = false)
    {
        return LastNumberSlice(s, supportMinus).ParseInt64();
    }
    public static ushort ParseLastUInt16(SpanString s)
    {
        return LastNumberSlice(s, false).ParseUInt16();
    }
    public static uint ParseLastUInt32(SpanString s)
    {
        return LastNumberSlice(s, false).ParseUInt32();
    }
    public static ulong ParseLastUInt64(SpanString s)
    {
        return LastNumberSlice(s, false).ParseUInt64();
    }

    public static ImmutableArray<short> ParseAllInt16(SpanString s, bool supportMinus = false)
    {
        var digits = DigitsWithOptionalMinus(supportMinus);
        return ParseAll(s, digits, SpanStringExtensionsEx.ParseInt16);
    }
    public static ImmutableArray<int> ParseAllInt32(SpanString s, bool supportMinus = false)
    {
        var digits = DigitsWithOptionalMinus(supportMinus);
        return ParseAll(s, digits, SpanStringExtensions.ParseInt32);
    }
    public static ImmutableArray<long> ParseAllInt64(SpanString s, bool supportMinus = false)
    {
        var digits = DigitsWithOptionalMinus(supportMinus);
        return ParseAll(s, digits, SpanStringExtensions.ParseInt64);
    }

    public static ImmutableArray<ushort> ParseAllUInt16(SpanString s)
    {
        return ParseAll(s, Digits, SpanStringExtensionsEx.ParseUInt16);
    }
    public static ImmutableArray<uint> ParseAllUInt32(SpanString s)
    {
        return ParseAll(s, Digits, SpanStringExtensions.ParseUInt32);
    }
    public static ImmutableArray<ulong> ParseAllUInt64(SpanString s)
    {
        return ParseAll(s, Digits, SpanStringExtensions.ParseUInt64);
    }

    public static SearchChars DigitsWithOptionalMinus(bool includeMinus)
    {
        return includeMinus ? DigitsWithMinus : Digits;
    }

    public static ImmutableArray<T> ParseAll<T>(
        SpanString s, SearchChars searchChars, SpanStringParser<T> parser)
    {
        var result = ImmutableArray.CreateBuilder<T>();

        while (true)
        {
            if (s.Length <= 0)
                break;

            int nextStart = s.IndexOfAny(searchChars);
            if (nextStart < 0)
                break;

            s.AdvanceSliceRef(nextStart);
            int nextStop = s.IndexOfAnyExcept(searchChars);
            if (nextStop < 0)
            {
                nextStop = s.Length;
            }

            var slice = s[..nextStop];
            var parsed = parser(slice);

            result.Add(parsed);

            s.AdvanceSliceRef(nextStop);
        }

        return result.ToImmutable();
    }
}

public delegate T SpanStringParser<T>(SpanString parser);
