using Garyon.Extensions.ArrayExtensions;
using System.Linq;

namespace AdventOfCode.Problems.Year2016
{
    public class Day18 : Problem<int>
    {
        private TileType[] firstTileRow;

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
            firstTileRow = ParseFirstTileRow(FileContents);
        }
        protected override void ResetState()
        {
            firstTileRow = null;
        }

        private int GetSafeTileCount(int rows)
        {
            var grid = new TrapGrid(firstTileRow);
            grid.AnalyzeRows(rows);
            return grid.SafeTiles;
        }

        private static TileType[] ParseFirstTileRow(string contents) => contents.Select(ParseTileType).ToArray();
        private static TileType ParseTileType(char c) => c switch
        {
            '^' => TileType.Trap,
            _ => TileType.Safe,
        };

        private enum TileType
        {
            Safe,
            Trap,
        }

        private class TrapGrid
        {
            private bool alternateRow;

            private TileType[] rowA;
            private TileType[] rowB;

            public int SafeTiles { get; private set; }

            public TileType[] PreviousRow => alternateRow ? rowB : rowA;
            public TileType[] CurrentRow => alternateRow ? rowA : rowB;

            public int Length => rowA.Length;

            public TrapGrid(TileType[] firstTileRow)
            {
                rowA = firstTileRow.CopyArray();
                rowB = new TileType[rowA.Length];

                SafeTiles += rowA.Count(tile => tile is TileType.Safe);
            }

            public void AnalyzeRows(int count)
            {
                for (int i = 1; i < count; i++)
                    AnalyzeRow();
            }

            private void AnalyzeRow()
            {
                var currentRow = CurrentRow;
                var previousRow = PreviousRow;

                for (int x = 0; x < Length; x++)
                {
                    if ((currentRow[x] = GetTileType(previousRow, x)) is TileType.Safe)
                        SafeTiles++;
                }

                alternateRow = !alternateRow;
            }

            private static TileType GetTileType(TileType[] previousRow, int x)
            {
                bool trapLeft  =
                    x > 0 &&
                    previousRow[x - 1] is TileType.Trap;

                bool trapRight =
                    x + 1 < previousRow.Length &&
                    previousRow[x + 1] is TileType.Trap;

                return (trapLeft ^ trapRight) ? TileType.Trap : TileType.Safe;
            }
        }
    }
}
