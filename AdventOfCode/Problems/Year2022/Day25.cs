namespace AdventOfCode.Problems.Year2022;

public class Day25 : FinalDay<string>
{
    private ImmutableArray<SNAFU> numbers;

    public override string SolvePart1()
    {
        return SNAFU.Sum(numbers).ToString();
    }

    protected override void LoadState()
    {
        var span = FileContents.AsSpan();
        numbers = span.Trim().SelectLines(SNAFU.Parse);
    }

    /// <summary>It is a SNAFU, duh</summary>
    /// <remarks>
    /// <see langword="S"/>pecial
    /// <see langword="N"/>umeral-<see langword="A"/>nalogue
    /// <see langword="F"/>uel
    /// <see langword="U"/>nit
    /// </remarks>
    private record SNAFU(long Value)
    {
        private const int radix = 5;

        public static SNAFU Sum(IEnumerable<SNAFU> numbers)
        {
            long sum = numbers.Sum(n => n.Value);
            return new(sum);
        }

        public static SNAFU Parse(SpanString spanString)
        {
            long multiplier = 1;
            long sum = 0;

            for (int i = 1; i <= spanString.Length; i++)
            {
                char digit = spanString[^i];
                int value = Parse(digit);
                sum += value * multiplier;

                multiplier *= radix;
            }
            return new(sum);
        }

        private static int Parse(char digit)
        {
            return digit switch
            {
                '=' => -2,
                '-' => -1,
                '0' => 0,
                '1' => 1,
                '2' => 2,
            };
        }
        private static char GetDigit(int value)
        {
            return value switch
            {
                -2 => '=',
                -1 => '-',
                0 => '0',
                1 => '1',
                2 => '2',
            };
        }

        public override string ToString()
        {
            long leftover = Value;
            var stringBuilder = new StringBuilder();

            while (leftover > 0)
            {
                leftover = Math.DivRem(leftover, radix, out long digitValue);
                if (digitValue >= 3)
                {
                    digitValue -= radix;
                    leftover++;
                }

                char digit = GetDigit((int)digitValue);
                stringBuilder.Insert(0, digit);
            }

            return stringBuilder.ToString();
        }
    }
}
