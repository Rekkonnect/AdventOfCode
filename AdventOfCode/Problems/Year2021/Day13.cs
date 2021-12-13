#nullable enable

using AdventOfCode.Utilities.TwoDimensions;
using AdventOfCSharp;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2021;

public class Day13 : Problem<int, IGlyphGrid>
{
    private TransparentPaper? paper;
    private FoldingInstruction[]? foldingInstructions;

    public override int SolvePart1()
    {
        var clonedPaper = paper!.Clone();
        foldingInstructions![0].FoldPaper(clonedPaper);
        return clonedPaper.DotCount;
    }
    public override IGlyphGrid SolvePart2()
    {
        var clonedPaper = paper!.Clone();
        foreach (var instruction in foldingInstructions!)
            instruction.FoldPaper(clonedPaper);
        return clonedPaper.ConvertToGrid();
    }

    protected override void LoadState()
    {
        var sections = NormalizedFileContents.Trim().Split("\n\n");
        paper = TransparentPaper.Parse(sections[0]);
        foldingInstructions = sections[1].GetLines().Select(FoldingInstruction.Parse).ToArray();
    }
    protected override void ResetState()
    {
        paper = null;
        foldingInstructions = null;
    }

    private sealed record class FoldAlongXInstruction(int AxisIndex)
        : FoldingInstruction(AxisIndex)
    {
        public override void FoldPaper(TransparentPaper paper) => paper.FoldAlongX(AxisIndex);
    }
    private sealed record class FoldAlongYInstruction(int AxisIndex)
        : FoldingInstruction(AxisIndex)
    {
        public override void FoldPaper(TransparentPaper paper) => paper.FoldAlongY(AxisIndex);
    }
    private abstract record class FoldingInstruction(int AxisIndex)
    {
        private static readonly Regex instructionPattern = new(@"fold along (?'axis'\w)=(?'index'\d*)");

        public abstract void FoldPaper(TransparentPaper paper);

        public static FoldingInstruction Parse(string instruction)
        {
            var groups = instructionPattern.Match(instruction).Groups;
            char axis = groups["axis"].Value[0];
            int index = groups["index"].Value.ParseInt32();
            return axis switch
            {
                'x' => new FoldAlongXInstruction(index),
                'y' => new FoldAlongYInstruction(index),
            };
        }
    }

    private sealed class PaperGrid : PrintableGrid2D<bool>, IGlyphGrid
    {
        public PaperGrid(IEnumerable<Location2D> dotLocations)
            : base(Location2D.GetLocationSpace(dotLocations, out var offset))
        {
            foreach (var location in dotLocations)
                this[location + offset] = true;
        }

        protected override IDictionary<bool, char> GetPrintableCharacters()
        {
            return new Dictionary<bool, char>
            {
                [false] = '.',
                [true] = '#',
            };
        }
    }

    private sealed class TransparentPaper
    {
        private readonly HashSet<Location2D> locations;

        public int DotCount => locations.Count;

        private TransparentPaper(IEnumerable<Location2D> dotLocations)
        {
            locations = new(dotLocations);
        }

        public PaperGrid ConvertToGrid()
        {
            return new(locations.ToArray());
        }

        public TransparentPaper Clone() => new(locations);

        // Kinda copypasted, better avoid too much abstraction overhead
        public void FoldAlongX(int index)
        {
            var foldedLeft = locations.Where(location => location.X > index).ToArray();
            foreach (var location in foldedLeft)
            {
                int distance = location.X - index;
                locations.Remove(location);
                locations.Add(location - (2 * distance, 0));
            }
        }
        public void FoldAlongY(int index)
        {
            var foldedUp = locations.Where(location => location.Y > index).ToArray();
            foreach (var location in foldedUp)
            {
                int distance = location.Y - index;
                locations.Remove(location);
                locations.Add(location - (0, 2 * distance));
            }
        }

        private static Location2D ParseLocation(string location)
        {
            var split = location.Split(',');
            int x = split[0].ParseInt32();
            int y = split[1].ParseInt32();
            return new(x, y);
        }

        public static TransparentPaper Parse(string rawPaper)
        {
            return new(rawPaper.GetLines().Select(ParseLocation));
        }
    }
}
