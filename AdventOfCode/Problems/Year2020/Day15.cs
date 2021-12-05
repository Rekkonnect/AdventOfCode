using AdventOfCSharp;
using System.Buffers;
using System.Linq;

namespace AdventOfCode.Problems.Year2020;

public class Day15 : Problem<int>
{
    private int[] startingNumbers;

    public override int SolvePart1()
    {
        return RunGame(2020);
    }
    public override int SolvePart2()
    {
        return RunGame(30000000);
    }

    private int RunGame(int turns)
    {
        var game = new NumberGame(startingNumbers, turns);
        game.PlayUntilTurn(turns - 1);
        return game.LastSpokenNumber;
    }

    protected override void LoadState()
    {
        startingNumbers = FileContents.Split(',').Select(int.Parse).ToArray();
    }
    protected override void ResetState()
    {
        startingNumbers = null;
    }

    private class NumberGame
    {
        private NumberGameEntry[] spokenNumbersTable;

        public int CurrentTurn { get; private set; }
        public int LastSpokenNumber { get; private set; }

        public NumberGame(int[] startingNumbers, int expectedTurns)
        {
            spokenNumbersTable = new NumberGameEntry[expectedTurns];

            for (CurrentTurn = 0; CurrentTurn < startingNumbers.Length; CurrentTurn++)
                RegisterSpokenNumber(startingNumbers[CurrentTurn]);
        }

        public void PlayNextTurns(int nextTurns) => PlayUntilTurn(CurrentTurn + nextTurns);
        public void PlayUntilTurn(int turn)
        {
            while (CurrentTurn <= turn)
                PlayTurn();
        }
        public void PlayTurn()
        {
            var last = spokenNumbersTable[LastSpokenNumber];
            if (last?.SpokenTwice == true)
                RegisterTurn(last.LastSpokenTurn - last.PreviousSpokenTurn);
            else
                RegisterTurn(0);
        }
        private void RegisterTurn(int number)
        {
            RegisterSpokenNumber(number);
            LastSpokenNumber = number;
            CurrentTurn++;
        }
        private void RegisterSpokenNumber(int number)
        {
            spokenNumbersTable[number] ??= new();
            spokenNumbersTable[number].RegisterSpoken(CurrentTurn);
        }
    }
    private class NumberGameEntry
    {
        public int LastSpokenTurn { get; private set; } = -1;
        public int PreviousSpokenTurn { get; private set; } = -1;

        public bool SpokenTwice => PreviousSpokenTurn > -1;

        public void RegisterSpoken(int currentTurn)
        {
            PreviousSpokenTurn = LastSpokenTurn;
            LastSpokenTurn = currentTurn;
        }
    }
}
