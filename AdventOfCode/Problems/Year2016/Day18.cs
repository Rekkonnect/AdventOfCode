using AdventOfCode.Functions;
using AdventOfCSharp;
using System;
using UltimateOrb;

namespace AdventOfCode.Problems.Year2016;

public class Day18 : Problem<int>
{
    private TileRow firstTileRow;

    public override int SolvePart1()
    {
        return GetSafeTileCount(40);
    }
    public override int SolvePart2()
    {
        return GetSafeTileCount(400000);
    }

    protected override void LoadState()
    {
        firstTileRow = TileRow.Parse(FileContents);
    }

    private int GetSafeTileCount(int rows)
    {
        var grid = new TrapGrid(firstTileRow);
        grid.AnalyzeRows(rows);
        return grid.SafeTiles;
    }

    private struct TileRow
    {
        private UInt128 bits;
        public int Length { get; }

        public int TrapCount => bits.PopCount();
        public int SafeCount => Length - TrapCount;

        private TileRow(int length, UInt128 rowBits)
        {
            Length = length;
            bits = rowBits;
        }

        public TileRow GetNext()
        {
            var mask = GetMask(Length);

            var left = (bits << 1) & mask;
            var right = bits >> 1;

            return new(Length, left ^ right);
        }

        public static TileRow Parse(string contents)
        {
            int length = contents.Length;
            UInt128 bits = 0;
            UInt128 bitMask = 1;

            for (int i = contents.Length - 1; i >= 0; i--, bitMask <<= 1)
            {
                if (contents[i] is '^')
                    bits |= bitMask;
            }

            return new(length, bits);
        }

        private static UInt128 GetMask(int leftLength) => ~(UInt128.MaxValue << leftLength);
    }

    private class TrapGrid
    {
        private TileRow currentRow;

        public int SafeTiles { get; private set; }

        public int Length => currentRow.Length;

        public TrapGrid(TileRow firstRow)
        {
            currentRow = firstRow;

            SafeTiles += firstRow.SafeCount;
        }

        public void AnalyzeRows(int count)
        {
            for (int i = 1; i < count; i++)
                AnalyzeRow();
        }

        private void AnalyzeRow()
        {
            currentRow = currentRow.GetNext();
            SafeTiles += currentRow.SafeCount;
        }
    }
}
