namespace AdventOfCode.Problems.Year2021;

public class Day1 : Problem<int>
{
    private int[] numbers;

    public override int SolvePart1() => CountIncreasedMeasurements(1);
    public override int SolvePart2() => CountIncreasedMeasurements(3);

    private int CountIncreasedMeasurements(int indexDifference)
    {
        int count = 0;
        for (int i = indexDifference; i < numbers.Length; i++)
            if (numbers[i] > numbers[i - indexDifference])
                count++;

        return count;
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
