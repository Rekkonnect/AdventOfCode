namespace AdventOfCode.Problems.Year2018;

public class Day1 : Problem<int>
{
    private int[] numbers;

    public override int SolvePart1()
    {
        return numbers.Sum();
    }
    public override int SolvePart2()
    {
        var frequencies = new HashSet<int> { 0 };
        int currentSum = 0;
        while (true)
        {
            foreach (var n in numbers)
            {
                currentSum += n;
                if (!frequencies.Add(currentSum))
                    return currentSum;
            }
        }
    }

    protected override void LoadState()
    {
        numbers = FileNumbersInt32;
    }
    protected override void ResetState()
    {
        numbers = null;
    }
}
