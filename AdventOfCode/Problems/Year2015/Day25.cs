using Garyon.Extensions;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2015
{
    public class Day25 : Problem<ulong, string>
    {
        private CodeGridLocation gridLocation;

        public override ulong SolvePart1()
        {
            int codeIndex = gridLocation.CodeIndex;

            ulong resultingCode = 20151125;
            for (int i = 1; i < codeIndex; i++)
                resultingCode = resultingCode * 252533 % 33554393;

            return resultingCode;
        }
        public override string SolvePart2()
        {
            return "Congratulations on completing all of AoC 2015!";
        }

        protected override void LoadState()
        {
            gridLocation = CodeGridLocation.Parse(FileContents);
        }

        private struct CodeGridLocation
        {
            private static readonly Regex manualLocationPattern = new(@"row (?'row'\d*), column (?'column'\d*)", RegexOptions.Compiled);

            public int Row { get; }
            public int Column { get; }

            public int CodeIndex
            {
                get
                {
                    int sumIndex = Row + Column - 1;
                    int sum = Sum(sumIndex);
                    return sum - (sumIndex - Column);
                }
            }

            public CodeGridLocation(int row, int column) => (Row, Column) = (row, column);

            public static CodeGridLocation Parse(string raw)
            {
                var groups = manualLocationPattern.Match(raw).Groups;
                int row = groups["row"].Value.ParseInt32();
                int column = groups["column"].Value.ParseInt32();
                return new(row, column);
            }

            private static int Sum(int n) => (n + 1) * n / 2;
        }
    }
}
