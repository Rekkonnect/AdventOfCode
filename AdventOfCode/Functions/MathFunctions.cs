using Garyon.Functions;
using System.Numerics;
using static System.Math;

namespace AdventOfCode.Functions;

public static class MathFunctions
{
    #region Divisions
    public static T GCD<T>(T a, T b)
        where T : IBinaryNumber<T>
    {
        a = T.Abs(a);
        b = T.Abs(b);

        while (true)
        {
            if (a == T.Zero || b == T.Zero)
                break;

            if (a > b)
                a %= b;
            else
                b %= a;
        }

        return a | b;
    }

    public static T LCM<T>(T a, T b)
        where T : IBinaryNumber<T>
    {
        return a * b
             / GCD(a, b);
    }

    public static T LCM<T>(IEnumerable<T> values)
        where T : IBinaryNumber<T>
    {
        var lcm = T.One;

        foreach (var value in values)
        {
            if (value == T.One)
            {
                lcm = value;
            }
            else
            {
                lcm = LCM(lcm, value);
            }
        }

        return lcm;
    }

    public static void SimplifyFraction<T>(ref T nominator, ref T denominator)
        where T : IBinaryNumber<T>
    {
        var gcd = GCD(nominator, denominator);
        nominator /= gcd;
        denominator /= gcd;
    }
    #endregion

    #region Trigonometry
    public const double FullCircleDegrees = 360;
    public const double FullCircleRadians = 2 * PI;
    public const double HalfCircleDegrees = 180;
    public const double HalfCircleRadians = PI;

    public static double ToDegrees(double rad) => rad / PI * 180;
    public static double ToRadians(double deg) => deg / 180 * PI;
    public static double AddDegrees(double deg, double add)
    {
        double result = (deg + add) % FullCircleDegrees;
        if (result < 0)
            result += FullCircleDegrees;
        return result;
    }
    public static double InvertDegrees(double deg) => (FullCircleDegrees - deg) % FullCircleDegrees;
    public static double AddRadians(double rad, double add)
    {
        double result = (rad + add) % FullCircleRadians;
        if (result < 0)
            result += FullCircleRadians;
        return result;
    }
    public static double InvertRadians(double rad) => (FullCircleRadians - rad) % FullCircleRadians;
    #endregion

    #region Tens
    public static long MultipleOfTen(int exponent)
    {
        switch (exponent)
        {
            case < 0:
                return 0;
            case 0:
                return 1;
            case 1:
                return 10;
            case 2:
                return 100;
            case 3:
                return 1000;
            case 4:
                return 10000;
        }

        return 10000 * MultipleOfTen(exponent - 4);
    }
    #endregion

    #region Comparisons
    public static bool BetweenInclusive<T>(T value, T a, T b)
        where T : IBinaryNumber<T>, IShiftOperators<T, T, T>
    {
        EnsureOrdered(ref a, ref b);
        return a <= value && value <= b;
    }

    public static void Order<T>(in T a, in T b, out T min, out T max)
        where T : IComparable<T>
    {
        min = a;
        max = b;
        EnsureOrdered(ref min, ref max);
    }
    public static void EnsureOrdered<T>(ref T min, ref T max)
        where T : IComparable<T>
    {
        if (min.CompareTo(max) > 0)
            Misc.Swap(ref min, ref max);
    }
    #endregion

    #region Sequences
    public static T HalveInteger<T>(this T number)
        where T : IBinaryNumber<T>, IShiftOperators<T, T, T>
    {
        if (T.IsNegative(number))
        {
            number++;
        }
        return number >> T.One;
    }

    public static T Sum<T>(T max)
        where T : IBinaryNumber<T>, IShiftOperators<T, T, T>
    {
        return HalveInteger(max * (max + T.One));
    }
    public static T Sum<T>(T start, T end)
        where T : IBinaryNumber<T>, IShiftOperators<T, T, T>
    {
        return Sum(end) - Sum(start);
    }
    #endregion
}
