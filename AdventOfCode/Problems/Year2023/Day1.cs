using System.Buffers;

namespace AdventOfCode.Problems.Year2023;

public class Day1 : Problem<int>
{
    private const string _digitString = "0123456789";
    private readonly SearchValues<char> _digitSearchValues = SearchValues.Create(_digitString);

    private string[] _lines;

    public override int SolvePart1()
    {
        return _lines
            .Select(CalibrationValue)
            .Sum();
    }
    public override int SolvePart2()
    {
        return _lines
            .Select(RealCalibrationValue)
            .Sum();
    }

    protected override void LoadState()
    {
        _lines = FileLines;
    }
    protected override void ResetState()
    {
        _lines = null;
    }

    private int CalibrationValue(string line)
    {
        var span = line.AsSpan();
        int first = span.IndexOfAny(_digitSearchValues);
        int last = span.LastIndexOfAny(_digitSearchValues);

        if (first is -1)
            return 0;

        int firstDigit = span[first].GetNumericalValue();
        int lastDigit = span[last].GetNumericalValue();

        return firstDigit * 10 + lastDigit;
    }
    private int RealCalibrationValue(string line)
    {
        int firstDigit = GetFirstDigit(line);
        int lastDigit = GetLastDigit(line);

        return firstDigit * 10 + lastDigit;
    }

    // This version is slower by <1ms compared to copy-pasting if statements
    // From ~1.4ms to ~1.7ms
    private static readonly string[] _spelledOutDigits =
        ["one", "two", "three", "four", "five", "six", "seven", "eight", "nine"];

    private static int GetFirstDigit(string line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            int digit = GetDigitLeft(line, i);
            if (digit > -1)
                return digit;
        }

        return -1;
    }
    private static int GetDigitLeft(string line, int index)
    {
        if (line[index].IsDigit())
        {
            return line[index].GetNumericalValue();
        }

        var span = line.AsSpan();
        var subSpan = span[index..];

        for (int i = 0; i < _spelledOutDigits.Length; i++)
        {
            if (subSpan.StartsWith(_spelledOutDigits[i]))
                return i + 1;
        }

        return -1;
    }

    private static int GetLastDigit(string line)
    {
        for (int i = line.Length - 1; i >= 0; i--)
        {
            int digit = GetDigitRight(line, i);
            if (digit > -1)
                return digit;
        }

        return -1;
    }
    private static int GetDigitRight(string line, int index)
    {
        if (line[index].IsDigit())
        {
            return line[index].GetNumericalValue();
        }

        var span = line.AsSpan();
        var subSpan = span[..(index + 1)];

        for (int i = 0; i < _spelledOutDigits.Length; i++)
        {
            if (subSpan.EndsWith(_spelledOutDigits[i]))
                return i + 1;
        }

        return -1;
    }
}
