namespace AdventOfCode.Problems.Year2020;

public class Day1 : Problem<int>
{
    private int[] numbers;

    public override int SolvePart1()
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            int a = numbers[i];
            int b = 2020 - a;
            int index = Array.BinarySearch(numbers, b);
            if (index > -1)
                return a * b;
        }
        return -1;
    }
    public override int SolvePart2()
    {
        // THIS IS O(n^2 * logn) AND IT'S ACTUALLY THE BEST I COULD DO
        // SO MGOSTIH DOES NOT CRY

        for (int i = 0; i < numbers.Length; i++)
        {
            int a = numbers[i];

            for (int j = numbers.Length - 1; j > i; j--)
            {
                int b = numbers[j];
                if (a + b > 2020)
                    continue;

                int c = 2020 - a - b;
                int index = Array.BinarySearch(numbers, c);
                if (index > -1)
                    return a * b * c;
            }
        }
        return -1;
    }

    protected override void LoadState()
    {
        numbers = FileNumbersInt32;
        Array.Sort(numbers);
    }
    protected override void ResetState()
    {
        numbers = null;
    }
}
