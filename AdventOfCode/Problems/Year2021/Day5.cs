#nullable enable

using AdventOfCSharp;
using Garyon.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using static System.Math;

namespace AdventOfCode.Problems.Year2021;

public class Day5 : Problem<int>
{
    private Lines? lines;

    public override int SolvePart1()
    {
        return SolvePart(lines!.HorizontalOrVerticalLines);
    }
    public override int SolvePart2()
    {
        return SolvePart(lines!);
    }

    protected override void LoadState()
    {
        lines = Lines.Parse(FileLines);
    }
    protected override void ResetState()
    {
        lines = null;
    }

    private int SolvePart(IEnumerable<Line> drawnLines)
    {
        var arrangement = new VentArrangement(lines!);
        arrangement.DrawLines(drawnLines);
        return arrangement.OverlapCount;
    }

    private class VentArrangement
    {
        private readonly int[,] vents;
        private readonly int offsetX, offsetY;

        public int OverlapCount { get; private set; }

        public VentArrangement(Lines lines)
        {
            vents = new int[lines.Width, lines.Height];
            offsetX = lines.MinX;
            offsetY = lines.MinY;
        }

        public void DrawLines(IEnumerable<Line> lines)
        {
            foreach (var line in lines)
                DrawLine(line);
        }

        private void DrawLine(Line line)
        {
            if (line.IsHorizontal)
                DrawHorizontalLine(line);
            else if (line.IsVertical)
                DrawVerticalLine(line);
            else
                DrawDiagonalLine(line);
        }
        private void DrawHorizontalLine(Line line)
        {
            for (int x = line.MinX; x <= line.MaxX; x++)
                PlaceAt(x, line.StartY);
        }
        private void DrawVerticalLine(Line line)
        {
            for (int y = line.MinY; y <= line.MaxY; y++)
                PlaceAt(line.StartX, y);
        }
        private void DrawDiagonalLine(Line line)
        {
            int steps = line.MaxX - line.MinX;

            for (int i = 0; i <= steps; i++)
                PlaceAt(line.XAt(i), line.YAt(i));
        }

        private void PlaceAt(int x, int y)
        {
            ref int vent = ref vents[x - offsetX, y - offsetY];
            if (vent >= 2)
                return;

            if (vent is 1)
                OverlapCount++;
            vent++;
        }
    }

    private class Lines : IEnumerable<Line>
    {
        private readonly Line[] lines;

        public int MinX { get; }
        public int MinY { get; }
        public int MaxX { get; }
        public int MaxY { get; }

        public int Width => MaxX - MinX + 1;
        public int Height => MaxY - MinY + 1;

        public IEnumerable<Line> HorizontalOrVerticalLines => lines.Where(line => line.IsHorizontalOrVertical);

        public Lines(IEnumerable<Line> lineCollection)
        {
            lines = lineCollection.ToArray();

            MinX = lines.Min(line => line.MinX);
            MinY = lines.Min(line => line.MinY);
            MaxX = lines.Max(line => line.MaxX);
            MaxY = lines.Max(line => line.MaxY);
        }

        public Line this[int index] => lines[index];

        public IEnumerator<Line> GetEnumerator() => lines.Cast<Line>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static Lines Parse(string[] lines)
        {
            return new(lines.Select(Line.Parse));
        }
    }

    private record Line(int StartX, int StartY, int EndX, int EndY)
    {
        private static readonly Regex linePattern = new(@"(?'startX'\d*),(?'startY'\d*) -> (?'endX'\d*),(?'endY'\d*)");
        
        public int MinX { get; } = Min(StartX, EndX);
        public int MinY { get; } = Min(StartY, EndY);
        public int MaxX { get; } = Max(StartX, EndX);
        public int MaxY { get; } = Max(StartY, EndY);

        public int StepX { get; } = Sign(EndX - StartX);
        public int StepY { get; } = Sign(EndY - StartY);

        public bool IsHorizontal => StartY == EndY;
        public bool IsVertical => StartX == EndX;
        public bool IsHorizontalOrVertical => IsHorizontal || IsVertical;
        public bool IsDiagonal => !IsHorizontalOrVertical;

        public int XAt(int stepIndex) => StartX + stepIndex * StepX;
        public int YAt(int stepIndex) => StartY + stepIndex * StepY;

        public static Line Parse(string line)
        {
            var match = linePattern.Match(line);
            int startX = match.Groups["startX"].Value.ParseInt32();
            int startY = match.Groups["startY"].Value.ParseInt32();
            int endX = match.Groups["endX"].Value.ParseInt32();
            int endY = match.Groups["endY"].Value.ParseInt32();

            return new(startX, startY, endX, endY);
        }
    }
}
