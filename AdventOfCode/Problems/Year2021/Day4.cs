#nullable enable

using AdventOfCSharp;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2021;

public class Day4 : Problem<uint>
{
    private BingoGame? game;

    public override uint SolvePart1()
    {
        return game!.GetFirstWinningBoard().Score;
    }
    public override uint SolvePart2()
    {
        return game!.GetLastWinningBoard().Score;
    }

    protected override void LoadState()
    {
        game = BingoGame.Parse(FileContents);
    }
    protected override void ResetState()
    {
        game = null;
    }

    private static uint ParseBinary(string binary)
    {
        uint result = 0;
        for (int i = 0; i < binary.Length; i++)
            result |= (uint)(binary[^(i + 1)] - '0') << i;
        
        return result;
    }

    private class BingoBoard
    {
        private readonly uint[,] numbers;

        public int SideLength => numbers.GetLength(0);

        public BingoBoard(uint[,] values)
        {
            numbers = values;
        }

        public IEnumerable<uint> FilteredValues(BoardValuePredicate predicate)
        {
            for (int row = 0; row < SideLength; row++)
            {
                for (int column = 0; column < SideLength; column++)
                {
                    if (predicate(row, column))
                        yield return numbers[row, column];
                }
            }
        }

        public bool TryGetIndexOf(uint number, out int row, out int column)
        {
            // Unfortunately mandatory
            column = 0;

            for (row = 0; row < numbers.GetLength(0); row++)
            {
                for (column = 0; column < numbers.GetLength(1); column++)
                {
                    if (numbers[row, column] == number)
                        return true;
                }
            }

            return false;
        }

        public uint this[int row, int column] => numbers[row, column];

        public static BingoBoard Parse(string input)
        {
            input = input.Trim();
            var lines = input.GetLines();
            int sideLength = lines.Length;
            uint[,] numbers = new uint[sideLength, sideLength];

            for (int row = 0; row < sideLength; row++)
            {
                var line = lines[row];
                var rowNumbers = line.Split(' ').RemoveEmptyElements().Select(uint.Parse).ToArray();
                
                for (int column = 0; column < sideLength; column++)
                    numbers[row, column] = rowNumbers[column];
            }

            return new(numbers);
        }
    }

    private delegate bool BoardValuePredicate(int row, int column);

    private record MarkedBingoBoard(BingoBoard Board)
    {
        private BoardMarks marks;

        // By the time a board is fully marked, it will have been considered as winning
        public uint Score { get; private set; }
        public bool HasWon => Score > 0;

        public void ResetMarks() => marks.Reset();

        public bool MarkDrawn(uint number)
        {
            if (HasWon)
                return false;

            if (!Board.TryGetIndexOf(number, out int row, out int column))
                return false;

            marks.Mark(row, column);
            bool wins = marks.HasAnyComplete;
            if (wins)
                Score = number * GetUnmarkedNumberSum();

            return wins;
        }

        private uint GetUnmarkedNumberSum() => Board.FilteredValues(marks.IsUnmarked).Sum();
    }

    private struct BoardMarks
    {
        private const uint columnMask = 0b00001_00001_00001_00001_00001;
        private const uint rowMask = 0b11111;

        private uint bits;

        public void Mark(int row, int column)
        {
            bits |= GetBitMask(row, column);
        }

        public bool IsMarked(int row, int column)
        {
            return (bits & GetBitMask(row, column)) != 0;
        }
        public bool IsUnmarked(int row, int column) => !IsMarked(row, column);

        public void Reset() => bits = 0;

        private static int GetBitIndex(int row, int column) => row * 5 + column;
        private static uint GetBitMask(int row, int column) => 1U << GetBitIndex(row, column);

        public bool HasAnyComplete => HasAnyCompleteRow || HasAnyCompleteColumn;

        private bool HasAnyCompleteRow => HasComplete(rowMask, 5);
        private bool HasAnyCompleteColumn => HasComplete(columnMask, 1);

        private bool HasComplete(uint mask, int shiftMultiplier)
        {
            for (int i = 0; i < 5; i++)
                if (HasComplete(mask, i, shiftMultiplier))
                    return true;

            return false;
        }

        private bool HasComplete(uint mask, int index, int shiftMultiplier)
        {
            uint indexedMask = mask << (index * shiftMultiplier);
            return (bits & indexedMask) == indexedMask;
        }
    }

    private class BingoGame
    {
        private readonly uint[] drawings;
        private int currentDrawIndex;

        private readonly MarkedBingoBoard[] markedBingoBoards;

        public BingoGame(IEnumerable<uint> drawSequence, IEnumerable<BingoBoard> bingoBoards)
        {
            drawings = drawSequence.ToArray();
            markedBingoBoards = bingoBoards.Select(board => new MarkedBingoBoard(board)).ToArray();
        }

        public MarkedBingoBoard GetFirstWinningBoard()
        {
            Reset();
            while (CanDrawNext())
            {
                var winning = DrawNext();
                if (winning is not null)
                    return winning;
            }
            // There will always be one winning board, hence the !
            return null!;
        }
        public MarkedBingoBoard GetLastWinningBoard()
        {
            Reset();
            MarkedBingoBoard lastWinner = null!;
            while (CanDrawNext())
            {
                var winning = DrawNext();
                if (winning is not null)
                    lastWinner = winning;
            }
            return lastWinner;
        }

        private MarkedBingoBoard? DrawNext()
        {
            var drawn = drawings[currentDrawIndex];

            MarkedBingoBoard? winner = null;
            foreach (var board in markedBingoBoards)
            {
                if (board.MarkDrawn(drawn))
                    winner = board;
            }

            currentDrawIndex++;
            return winner;
        }

        private bool CanDrawNext()
        {
            return currentDrawIndex < drawings.Length;
        }

        public void Reset()
        {
            currentDrawIndex = 0;
            foreach (var board in markedBingoBoards)
                board.ResetMarks();
        }

        public static BingoGame Parse(string fileContents)
        {
            var sections = fileContents.Split("\n\n");
            var drawings = sections[0].Split(',').Select(uint.Parse);
            var bingoBoards = sections.Skip(1).Select(BingoBoard.Parse);

            return new(drawings, bingoBoards);
        }
    }
}
