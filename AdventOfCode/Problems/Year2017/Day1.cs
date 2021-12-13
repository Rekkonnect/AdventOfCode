namespace AdventOfCode.Problems.Year2017;

public class Day1 : Problem<int>
{
    private string digits;

    public override int SolvePart1()
    {
        int sum = 0;
        for (int i = 0; i < digits.Length; i++)
        {
            if (digits[i] == digits[(i + 1) % digits.Length])
                sum += digits[i].GetNumericValueInteger();
        }
        return sum;
    }
    public override int SolvePart2()
    {
        int sum = 0;
        int half = digits.Length / 2;
        for (int i = 0; i < half; i++)
        {
            if (digits[i] == digits[i + half])
                sum += digits[i].GetNumericValueInteger() * 2;
        }
        return sum;
    }

    protected override void LoadState()
    {
        digits = FileContents;
    }
    protected override void ResetState()
    {
        digits = null;
    }
}
