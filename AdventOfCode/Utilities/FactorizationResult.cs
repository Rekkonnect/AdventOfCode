using Garyon.Mathematics;
using System.Collections.Generic;

namespace AdventOfCode.Utilities;

public class FactorizationResult
{
    private readonly List<Factor> factors = new();

    public int FactorCount
    {
        get
        {
            int count = 1;
            foreach (var f in factors)
                count *= f.Exponent + 1;
            return count;
        }
    }

    public void AddFactor(int prime, int exponent)
    {
        if (exponent == 0)
            return;

        factors.Add(new(prime, exponent));
    }

    public IEnumerable<int> GetAllDivisors()
    {
        yield return 1;

        int[] exponents = new int[factors.Count];
        exponents[0] = 1;

        bool alive = true;
        while (alive)
        {
            // Calculate and yield result
            int result = 1;
            for (int i = 0; i < exponents.Length; i++)
                result *= GeneralMath.Power(factors[i].Prime, exponents[i]);

            yield return result;

            // Advance exponents
            int index = 0;
            while (true)
            {
                exponents[index]++;

                if (exponents[index] <= factors[index].Exponent)
                    break;

                if (index == exponents.Length - 1)
                {
                    alive = false;
                    break;
                }

                exponents[index] = 0;
                index++;
            }
        }
    }

    private struct Factor
    {
        public int Prime { get; }
        public int Exponent { get; }

        public Factor(int prime, int exponent) => (Prime, Exponent) = (prime, exponent);
    }
}
