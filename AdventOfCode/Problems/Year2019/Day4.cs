using static System.Convert;

namespace AdventOfCode.Problems.Year2019;

public class Day4 : Problem<int>
{
    private Range range;
    
    // Not really proud of this code, consider improving it

    public override int SolvePart1() => General(Part1Criteria);
    public override int SolvePart2() => General(Part2Criteria);

    private bool Part1Criteria(int n) => CriteriaCalculator(n, Part1GeneralCriteria);
    private bool Part2Criteria(int n) => CriteriaCalculator(n, Part2GeneralCriteria);
    private bool CriteriaCalculator(int n, Criteria criteria)
    {
        var s = n.ToString();
        int multipleDigits = 1;
        bool hasDouble = false;
        for (int i = 1; i < s.Length; i++)
            if (!criteria(s[i], s[i - 1], ref multipleDigits, ref hasDouble))
                return false;
        return hasDouble |= multipleDigits == 2;
    }
    private bool Part1GeneralCriteria(char currentChar, char previousChar, ref int multipleDigits, ref bool hasDouble)
    {
        if (previousChar > currentChar)
            return false;
        hasDouble |= currentChar == previousChar;
        return true;
    }
    private bool Part2GeneralCriteria(char currentChar, char previousChar, ref int multipleDigits, ref bool hasDouble)
    {
        if (previousChar > currentChar)
            return false;
        if (previousChar == currentChar)
            multipleDigits++;
        else
        {
            if (multipleDigits == 2)
                hasDouble = true;
            multipleDigits = 1;
        }
        return true;
    }

    protected override void LoadState()
    {
        var values = FileContents.Split('-');
        int start = ToInt32(values[0]);
        int end = ToInt32(values[1]);

        range = start..end;
    }

    private int General(MeetsCriteriaFunction meetsCriteria)
    {
        int count = 0;

        for (int i = range.Start.Value; i <= range.End.Value; i++)
            if (meetsCriteria(i))
                count++;

        return count;
    }

    private delegate bool MeetsCriteriaFunction(int n);
    private delegate bool Criteria(char currentChar, char previousChar, ref int multipleDigits, ref bool hasDouble);
}
