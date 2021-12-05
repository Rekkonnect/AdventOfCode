using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using AdventOfCSharp;
using Garyon.Extensions;
using System.Linq;

namespace AdventOfCode.Problems.Year2015;

public class Day20 : Problem<int>
{
    private int targetPresents;

    // I'm so sad by the fact that I don't know enough arithmetic analysis to optimally solve this
    // Take the 20+ seconds and see me another day

    [PartSolution(PartSolutionStatus.Unoptimized)]
    public override int SolvePart1()
    {
        return SolvePart(10, null);
    }
    [PartSolution(PartSolutionStatus.Unoptimized)]
    public override int SolvePart2()
    {
        return SolvePart(11, (n, divisor) => n / divisor <= 50);
    }

    private int SolvePart(int targetSumDivisor, DivisorPredicate divisorPredicate)
    {
        int targetSum = targetPresents / targetSumDivisor;
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
