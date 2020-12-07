using static System.Math;

namespace AdventOfCode.Functions
{
    public static class MathExtensions
    {
        public const double FullCircleDegrees = 360;
        public const double FullCircleRadians = 2 * PI;
        public const double HalfCircleDegrees = 180;
        public const double HalfCircleRadians = PI;

        public static int GCD(int a, int b)
        {
            a = Abs(a);
            b = Abs(b);

            while (a != 0 && b != 0)
                if (a > b)
                    a %= b;
                else
                    b %= a;

            return a == 0 ? b : a;
        }
        public static void SimplifyFraction(ref int a, ref int b)
        {
            var gcd = GCD(a, b);
            a /= gcd;
            b /= gcd;
        }

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
    }
}
