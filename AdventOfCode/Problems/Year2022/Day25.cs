#define ANIMATION

using System.Threading;

namespace AdventOfCode.Problems.Year2022;

public class Day25 : FinalDay<string>
{
    private ImmutableArray<SNAFU> numbers;

#if ANIMATION
    [PrintsToConsole]
#endif
    public override string SolvePart1()
    {
        var sum = SNAFU.Sum(numbers);
#if ANIMATION
        ConsoleAnimation(sum);
#endif
        return sum.ToString();
    }

    protected override void LoadState()
    {
        var span = FileContents.AsSpan();
        numbers = span.Trim().SelectLines(SNAFU.Parse);
    }

    private void ConsoleAnimation(SNAFU target)
    {
        var previousPosition = Console.GetCursorPosition();
        Console.SetCursorPosition(0, previousPosition.Top + 3);
        Console.CursorVisible = false;

        int targetvalueLength = target.ToString().Length;

        long targetValue = target.Value;

        const int frames = 148;
        for (int i = 0; i < frames; i++)
        {
            double multiplier = Math.Pow((double)i / frames, 4.8);
            long currentValue = (long)(targetValue * multiplier);
            var current = new SNAFU(currentValue);
            Console.CursorLeft = 22;
            Console.Write(current.ToString().PadLeft(targetvalueLength));

            Thread.Sleep(1);
        }

        Console.CursorVisible = true;
        Console.SetCursorPosition(previousPosition.Left, previousPosition.Top);
        Console.WriteLine();
    }

    /// <summary>It is a SNAFU, duh</summary>
    /// <remarks>
    /// <see langword="S"/>pecial
    /// <see langword="N"/>umeral-<see langword="A"/>nalogue
    /// <see langword="F"/>uel
    /// <see langword="U"/>nit
    /// </remarks>
    private record struct SNAFU(long Value)
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
            if (Value is 0)
                return "0";

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
