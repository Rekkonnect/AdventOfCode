namespace AdventOfCode.Problems.Year2019;

public class Day16 : Problem<int>
{
    public override int SolvePart1() => General(0, false);
    [PartSolution(PartSolutionStatus.WIP)]
    public override int SolvePart2() => General(10000, true);

    private int General(int repetitions, bool applyOffset)
    {
        var contents = FileContents;

        int[] numbers = new int[contents.Length * (repetitions + 1)];
        for (int i = 0; i < contents.Length; i++)
            numbers[i] = contents[i].GetNumericValueInteger();

        for (int i = 1; i <= repetitions; i++)
            for (int j = 0; j < contents.Length; j++)
                numbers[i * contents.Length + j] = numbers[j];

        int[] basePattern = { 0, 1, 0, -1 };

        for (int i = 0; i < 100; i++)
        {
            int[] newNumbers = new int[numbers.Length];

            for (int j = 0; j < numbers.Length; j++)
            {
                int sum = 0;
                for (int k = 0; k < numbers.Length; k++)
                    sum += numbers[k] * GetPatternAtIndex(k);
                newNumbers[j] = Math.Abs(sum) % 10;

                int GetPatternAtIndex(int index) => basePattern[(index + 1) / (j + 1) % 4];
            }

            numbers = newNumbers;
        }

        int offset = applyOffset ? GetFirstDigits(numbers, 7, 0) : 0;

        return GetFirstEightDigits(numbers, offset);
    }

    private int GetFirstEightDigits(int[] numbers, int offset) => GetFirstDigits(numbers, 8, offset);
    private int GetFirstDigits(int[] numbers, int count, int offset)
    {
        int result = 0;
        int m = 1;
        for (int i = offset + count - 1; i >= offset; i--, m *= 10)
            result += m * numbers[i];
        return result;
    }
}
