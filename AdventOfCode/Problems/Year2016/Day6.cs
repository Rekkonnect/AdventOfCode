using AdventOfCode.Utilities;
using Garyon.Objects;

namespace AdventOfCode.Problems.Year2016;

public class Day6 : Problem<string>
{
    private string[] messages;
    private NextValueCounterDictionary<char>[] columnCounters;
    private int messageLength;

    public override string SolvePart1()
    {
        return CorrectMessage(ComparisonResult.Greater);
    }
    public override string SolvePart2()
    {
        return CorrectMessage(ComparisonResult.Less);
    }

    private string CorrectMessage(ComparisonResult characterComparison)
    {
        char[] chars = new char[messageLength];
        for (int i = 0; i < messageLength; i++)
            chars[i] = columnCounters[i].Best(characterComparison).Key;

        return new(chars);
    }

    protected override void ResetState()
    {
        messages = null;
        columnCounters = null;
    }
    protected override void LoadState()
    {
        messages = FileLines;
        messageLength = messages[0].Length;
        columnCounters = new NextValueCounterDictionary<char>[messageLength];

        for (int i = 0; i < messageLength; i++)
        {
            var counters = columnCounters[i] = new NextValueCounterDictionary<char>();
            foreach (var m in messages)
                counters.Add(m[i]);
        }
    }
}
