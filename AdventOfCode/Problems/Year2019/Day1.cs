namespace AdventOfCode.Problems.Year2019;

public class Day1 : Problem<int>
{
    public override int SolvePart1() => General(Part1Summer);
    public override int SolvePart2() => General(Part2Summer);

    private int Part1Summer(int fuel) => fuel / 3 - 2;
    private int Part2Summer(int fuel)
    {
        int sum = 0;
        while (fuel > 5)
            sum += fuel = fuel / 3 - 2;
        return sum;
    }
    private int General(Func<int, int> summer)
    {
        int sum = 0;
        foreach (var line in FileLines)
            sum += summer(Convert.ToInt32(line));
        return sum;
    }
}
