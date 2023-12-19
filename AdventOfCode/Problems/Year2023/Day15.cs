namespace AdventOfCode.Problems.Year2023;

public class Day15 : Problem<long>
{
    private string _values;

    public override long SolvePart1()
    {
        var valuesSpan = _values.AsSpan()
            .Trim();
        return valuesSpan.SplitSelect(',', Hash)
            .Sum();
    }
    public override long SolvePart2()
    {
        return -1;
    }

    protected override void LoadState()
    {
        _values = FileContents;
    }
    protected override void ResetState()
    {
        _values = null;
    }

    private static long Hash(SpanString s)
    {
        long sum = 0;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            sum += c;
            sum *= 17;
            sum &= 0xFF;
        }
        return sum;
    }
}
