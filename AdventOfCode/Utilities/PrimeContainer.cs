using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Utilities;

public class PrimeContainer
{
    public static PrimeContainer Current { get; } = new();

    private List<int> primes = new() { 2, 3 };

    public bool IsPrime(int number)
    {
        if (number < 2)
            return false;

        FindUntil(number);

        return primes.BinarySearch(number) >= 0;
    }

    public FactorizationResult Factorize(int number)
    {
        FindUntil(number);

        var result = new FactorizationResult();
        foreach (int prime in primes)
        {
            int exponent = 0;
            while (number % prime == 0)
            {
                exponent++;
                number /= prime;
            }

            result.AddFactor(prime, exponent);

            if (number == 1)
                break;
        }

        return result;
    }
    public int GetFactorCount(int number)
    {
        return Factorize(number).FactorCount;
    }

    private void FindUntil(int bound)
    {
        int last = primes.Last();
        if (last > (bound - 2))
            return;

        for (int number = last + 2; number <= bound; number += 2)
        {
            int sqrt = Sqrt(number);

            bool isPrime = true;
            foreach (int prime in primes)
            {
                isPrime = number % prime != 0;
                if (!isPrime)
                    break;

                if (number > sqrt)
                    break;
            }

            if (isPrime)
                primes.Add(number);
        }
    }

    private static int Sqrt(int number) => (int)Math.Round(Math.Sqrt(number));
}
