using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using Garyon.Extensions;
using System.Linq;

namespace AdventOfCode.Problems.Year2015;

[SolutionInfo(SolutionFlags.BothUnoptimized)]
public class Day20 : Problem<int>
{
    private int targetPresents;

    // I'm so sad by the fact that I don't know enough arithmetic analysis to optimally solve this
    // Take the 20+ seconds and see me another day

    public override int SolvePart1()
    {
        int targetSum = targetPresents / 10;
        return SolvePart(targetSum, null);
    }
    public override int SolvePart2()
    {
        int targetSum = targetPresents / 11;
        return SolvePart(targetSum, (n, divisor) => n / divisor <= 50);
    }

    private int SolvePart(int targetSum, DivisorPredicate divisorPredicate)
    {
        long lcm = 1;
        for (int n = 2; lcm < targetSum / 5; n++)
            lcm = MathFunctions.LCM(lcm, n);

        for (int i = (int)lcm; ; i++)
        {
            var divisors = PrimeContainer.Current.Factorize(i).GetAllDivisors();
            if (divisorPredicate is not null)
                divisors = divisors.Where(divisor => divisorPredicate(i, divisor));

            int sum = divisors.Sum();
            if (sum >= targetSum)
                return i;
        }
    }

    protected override void LoadState()
    {
        targetPresents = FileContents.ParseInt32();
    }

    private delegate bool DivisorPredicate(int number, int divisor);
}
