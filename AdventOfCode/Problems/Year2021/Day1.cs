namespace AdventOfCode.Problems.Year2021;

public class Day1 : Problem<int>
{
    private int[] numbers;

    public override int SolvePart1()
    {
        int increasedMeasurements = 0;
        for (int i = 1; i < numbers.Length; i++)
            if (numbers[i] > numbers[i - 1])
                increasedMeasurements++;

        return increasedMeasurements;
    }
    public override int SolvePart2()
    {
        int increasedMeasurements = 0;
        int currentMeasurementSum = numbers[0] + numbers[1];
        int previousMeasurementSum = int.MaxValue;
        for (int i = 2; i < numbers.Length; i++)
        {
            currentMeasurementSum += numbers[i];

            if (currentMeasurementSum > previousMeasurementSum)
                increasedMeasurements++;

            previousMeasurementSum = currentMeasurementSum;
            currentMeasurementSum -= numbers[i - 2];
        }

        return increasedMeasurements;
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
