using AdventOfCode.Utilities;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day15 : Problem<int, long>
    {
        private int[] startingNumbers;

        public override int SolvePart1()
        {
            var game = new NumberGame(startingNumbers);
            game.PlayUntilTurn(2019);
            return game.LastSpokenNumber;
        }
        public override long SolvePart2()
        {
            var game = new NumberGame(startingNumbers);
            // Brute force because fuck it
            game.PlayUntilTurn(30000000 - 1);
            return game.LastSpokenNumber;
        }

        protected override void ResetState()
        {
            startingNumbers = null;
        }
        protected override void LoadState()
        {
            startingNumbers = FileContents.Split(',').Select(int.Parse).ToArray();
        }

        private class NumberGame
        {
            private FlexibleDictionary<int, NumberGameEntry> spokenNumbers = new();

            public int CurrentTurn { get; private set; }
            public int LastSpokenNumber { get; private set; }

            public NumberGame(int[] startingNumbers)
            {
                for (CurrentTurn = 0; CurrentTurn < startingNumbers.Length; CurrentTurn++)
                    RegisterSpokenNumber(startingNumbers[CurrentTurn]);
            }

            public void PlayNextTurns(int nextTurns) => PlayUntilTurn(CurrentTurn + nextTurns);
            public void PlayUntilTurn(int turn)
            {
                for (; CurrentTurn <= turn; )
                    PlayTurn();
            }
            public void PlayTurn()
            {
                var last = spokenNumbers[LastSpokenNumber];
                if ((last?.SpokenTimes ?? 0) <= 1)
                    RegisterTurn(0);
                else
                    RegisterTurn(last.LastSpokenTurn - last.PreviousSpokenTurn);
            }
            private void RegisterTurn(int number)
            {
                RegisterSpokenNumber(number);
                LastSpokenNumber = number;
                CurrentTurn++;
            }
            private void RegisterSpokenNumber(int number)
            {
                spokenNumbers[number] ??= new NumberGameEntry(number);
                spokenNumbers[number].RegisterSpoken(CurrentTurn);
            }
        }
        private class NumberGameEntry
        {
            public int Number { get; init; }
            public int SpokenTimes { get; private set; }
            public int LastSpokenTurn { get; private set; }
            public int PreviousSpokenTurn { get; private set; }

            public NumberGameEntry(int number)
            {
                Number = number;
            }

            public void RegisterSpoken(int currentTurn)
            {
                SpokenTimes++;
                PreviousSpokenTurn = LastSpokenTurn;
                LastSpokenTurn = currentTurn;
            }
        }
    }
}
