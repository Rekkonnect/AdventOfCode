using Garyon.Functions;

namespace AdventOfCode.Problems.Year2017;

public class Day2 : Problem<int>
{
    private Spreadsheet spreadsheet;

    public override int SolvePart1()
    {
        return spreadsheet.Checksum;
    }
    public override int SolvePart2()
    {
        return spreadsheet.ResultSum;
    }

    protected override void LoadState()
    {
        spreadsheet = Spreadsheet.Parse(FileContents);
    }
    protected override void ResetState()
    {
        spreadsheet = null;
    }

    private class Spreadsheet
    {
        private readonly SpreadsheetRow[] rows;

        public int Checksum => rows.Sum(r => r.ExtremumDifference);
        public int ResultSum => rows.Sum(r => r.Result);

        public Spreadsheet(IEnumerable<SpreadsheetRow> spreadsheetRows)
        {
            rows = spreadsheetRows.ToArray();
        }

        public static Spreadsheet Parse(string raw) => new(raw.GetLines().Select(SpreadsheetRow.Parse));
    }

    private record SpreadsheetRow(int[] Values)
    {
        public int Length => Values.Length;

        public int ExtremumDifference
        {
            get
            {
                var minMax = Values.MinMax();
                return minMax.Max - minMax.Min;
            }
        }

        public int Result
        {
            get
            {
                for (int i = 0; i < Length; i++)
                    for (int j = i + 1; j < Length; j++)
                    {
                        int max = Values[i];
                        int min = Values[j];
                        if (max < min)
                            Misc.Swap(ref min, ref max);

                        int result = Math.DivRem(max, min, out int remainder);
                        if (remainder is 0)
                            return result;
                    }

                return -1;
            }
        }

        public SpreadsheetRow(IEnumerable<int> values)
            : this(values.ToArray()) { }

        public static SpreadsheetRow Parse(string s) => new(s.Split('\t').Select(int.Parse).ToArray());
    }
}
