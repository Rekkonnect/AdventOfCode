using AdventOfCode.Functions;
using AdventOfCode.Problems.Year2017.Utilities;
using AdventOfCode.Utilities.TwoDimensions;
using UltimateOrb;

namespace AdventOfCode.Problems.Year2017;

public class Day14 : Problem<int>
{
    private Disk disk;

    public override int SolvePart1()
    {
        return disk.UsedSquares;
    }
    public override int SolvePart2()
    {
        disk.ToGrid().GetRegionMap(DiskState.Used, out int regionCount);
        return regionCount;
    }

    protected override void LoadState()
    {
        disk = new(FileContents);
    }
    protected override void ResetState()
    {
        disk = null;
    }

    private class DiskGrid : Grid2D<DiskState>
    {
        public DiskGrid(UInt128[] rows)
            : base(128, rows.Length, default, initializeValueCounters: false)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Values[x, y] = (DiskState)UInt128.TestBit(rows[y], x);
                }
            }
        }
    }

    private class Disk
    {
        public const int RowCount = 128;

        private readonly string key;
        private UInt128[] rows = new UInt128[RowCount];

        public int UsedSquares => rows.Sum(r => r.PopCount());

        public Disk(string keyString)
        {
            key = keyString;
            AnalyzeRows();
        }

        private void AnalyzeRows()
        {
            for (int i = 0; i < RowCount; i++)
            {
                var hasher = KnotHasher.FromString($"{key}-{i}");
                var value = rows[i] = hasher.GetKnotHash128();
#if DEBUG
                if (i > 8)
                    continue;
                Console.WriteLine(value.GetBinaryRepresentation()[0..8]);
#endif
            }
        }

        public DiskGrid ToGrid()
        {
            return new(rows);
        }
    }

    private enum DiskState
    {
        Free,
        Used
    }
}
