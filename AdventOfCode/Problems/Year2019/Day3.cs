using AdventOfCode.Utilities.TwoDimensions;
using System.Data;

namespace AdventOfCode.Problems.Year2019;

public class Day3 : Problem<int>
{
    public override int SolvePart1() => General(Part1GeneralFunction);
    public override int SolvePart2() => General(Part2GeneralFunction);

    private void Part1GeneralFunction(ref int min, Line l0, Line l1)
    {
        Location2D? intersection;
        if ((intersection = l0.GetIntersectionWith(l1)).HasValue && intersection.Value.ManhattanDistanceFromCenter < min)
            min = intersection.Value.ManhattanDistanceFromCenter;
    }
    private void Part2GeneralFunction(ref int min, Line l0, Line l1)
    {
        Location2D? intersection;
        if ((intersection = l0.GetIntersectionWith(l1)).HasValue)
        {
            int totalSteps = l0.GetTotalStepsToLocation(intersection.Value) + l1.GetTotalStepsToLocation(intersection.Value);
            if (totalSteps < min)
                min = totalSteps;
        }
    }

    private int General(GeneralFunction generalFunction)
    {
        var paths = FileLines;

        var lines0 = InitializeLines(0);
        var lines1 = InitializeLines(1);

        int min = int.MaxValue;

        foreach (var l0 in lines0)
            foreach (var l1 in lines1)
                generalFunction(ref min, l0, l1);

        return min;

        Line[] InitializeLines(int index)
        {
            var lines = paths[index].Split(',').Select(l => Line.Parse(l)).ToArray();
            for (int i = 1; i < lines.Length; i++)
                lines[i].GetLocationFromPreviousLine(lines[i - 1]);
            return lines;
        }
    }

    private delegate void GeneralFunction(ref int min, Line l0, Line l1);
}
