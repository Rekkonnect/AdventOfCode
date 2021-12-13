namespace AdventOfCode.Problems.Year2020;

public class Day9 : Problem<long>
{
    private long[] numbers;

    public override long SolvePart1()
    {
        return GetFirstInvalidNumber(out _);
    }
    public override long SolvePart2()
    {
        var invalid = GetFirstInvalidNumber(out int index);

        for (int i = 0; i < index; i++)
        {
            long currentSum = numbers[i];

            for (int j = i + 1; j < index; j++)
            {
                currentSum += numbers[j];

                if (currentSum > invalid)
                    break;

                if (currentSum == invalid)
                {
                    var slice = numbers[i..(j + 1)];
                    long min = slice.Min();
                    long max = slice.Max();
                    return min + max;
                }
            }
        }

        return -1;
    }

    protected override void ResetState()
    {
        numbers = null;
    }
    protected override void LoadState()
    {
        numbers = FileNumbersInt64;
    }

    private long GetFirstInvalidNumber(out int index)
    {
        for (int offset = 0; offset < numbers.Length - 26; offset++)
        {
            bool hasValidNext = false;

            for (int i = 0; i < 25; i++)
            {
                long x = numbers[offset + i];

                for (int j = 24; j > i; j--)
                {
                    long y = numbers[offset + j];

                    if (x + y == numbers[offset + 25])
                    {
                        hasValidNext = true;
                        break;
                    }
                }

                if (hasValidNext)
                    break;
            }

            if (!hasValidNext)
            {
                return numbers[index = offset + 25];
            }
        }

        index = -1;
        return -1;
    }
}
