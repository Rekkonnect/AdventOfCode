namespace AdventOfCode.Problems.Year2019;

public class Day22 : Problem<int, long>
{
    public override int SolvePart1() => General(Part1Returner);
    [PartSolution(PartSolutionStatus.WIP)]
    public override long SolvePart2() => General(Part2Returner);

    private int Part1Returner(CardDeck deck, DeckCommandArray commands)
    {
        commands.ApplyAll(deck);
        return deck.PositionOfCard(2019);
    }
    private long Part2Returner(CardDeck deck, DeckCommandArray commands)
    {
        // TODO: Consider the non-millenia execution path

        const long cardCount = 119315717514047;
        const long repetitions = 101741582076661;
        const long desiredPosition = 2020;

        long uniqueIterations = 0;
        long currentPosition = 2020;
        do
        {
            currentPosition = commands.FindStartingPosition(currentPosition, cardCount);
            uniqueIterations++;
        }
        while (desiredPosition != currentPosition);

        long remainingIterations = repetitions % uniqueIterations;
        for (int i = 0; i < remainingIterations; i++)
            currentPosition = commands.FindStartingPosition(currentPosition, cardCount);

        return currentPosition;
    }

    private T General<T>(Returner<T> returner)
    {
        const int cardCount = 10007;

        var deck = new CardDeck(cardCount);
        var lines = FileLines;
        var commands = new DeckCommandArray(lines.Select(l => DeckCommand.Parse(l)).ToArray());

        return returner(deck, commands);
    }

    private delegate T Returner<T>(CardDeck deck, DeckCommandArray commands);

    private class DeckCommandArray
    {
        private DeckCommand[] commands;

        public DeckCommandArray(DeckCommand[] commandArray) => commands = commandArray;

        public void ApplyAll(CardDeck deck)
        {
            foreach (var c in commands)
                c.ApplyCommand(deck);
        }
        public long FindStartingPosition(long endingPosition, long cardCount)
        {
            long currentPosition = endingPosition;
            for (int i = commands.Length - 1; i >= 0; i--)
                currentPosition = commands[i].ApplyOnTrackedPosition(currentPosition, cardCount);
            return currentPosition;
        }

        public DeckCommand this[int index] => commands[index];
    }
    public abstract class DeckCommand
    {
        public abstract void ApplyCommand(CardDeck deck);
        public abstract long ApplyOnTrackedPosition(long position, long cardCount);
        public abstract long FindStartingPosition(long endingPosition, long cardCount);

        // TODO: Use Regex
        public static DeckCommand Parse(string s)
        {
            int.TryParse(s.Split(' ').Last(), out int number);
            if (s.StartsWith("cut"))
                return new CutDeckCommand(number);
            else if (s.StartsWith("deal with increment"))
                return new DealWithIncrementDeckCommand(number);
            else if (s.StartsWith("deal into new stack"))
                return new DealIntoNewStackDeckCommand();
            throw new NotImplementedException("The requested operation is not implemented.");
        }
    }
    public abstract class ParameterizedDeckCommand : DeckCommand
    {
        public int Parameter;

        public ParameterizedDeckCommand(int parameter) => Parameter = parameter;
    }
    public sealed class CutDeckCommand : ParameterizedDeckCommand
    {
        public CutDeckCommand(int parameter)
            : base(parameter) { }

        public override void ApplyCommand(CardDeck deck) => deck.Cut(Parameter);
        public override long ApplyOnTrackedPosition(long position, long cardCount) => (position + Parameter + cardCount) % cardCount;
        public override long FindStartingPosition(long endingPosition, long cardCount) => ApplyOnTrackedPosition(endingPosition, cardCount);
    }
    public sealed class DealWithIncrementDeckCommand : ParameterizedDeckCommand
    {
        public DealWithIncrementDeckCommand(int parameter)
            : base(parameter) { }

        public override void ApplyCommand(CardDeck deck) => deck.DealWithIncrement(Parameter);
        public override long ApplyOnTrackedPosition(long position, long cardCount) => position * Parameter % cardCount;
        public override long FindStartingPosition(long endingPosition, long cardCount)
        {
            // I don't know if this expression can be simplified, but I'd rather no longer bother with this shit
            long step = Parameter;
            long integersPerLine = (cardCount - 1) / step;
            long a = endingPosition + step - 1;
            return (step - 1 - a % step) * integersPerLine + a / step;
        }
    }
    public sealed class DealIntoNewStackDeckCommand : DeckCommand
    {
        public DealIntoNewStackDeckCommand() { }

        public override void ApplyCommand(CardDeck deck) => deck.DealIntoNewStack();
        public override long ApplyOnTrackedPosition(long position, long cardCount) => cardCount - position - 1;
        public override long FindStartingPosition(long endingPosition, long cardCount) => cardCount - endingPosition - 1;
    }

    public class CardDeck
    {
        public int[] Cards { get; private set; }
        public readonly int CardCount;

        public CardDeck(int cardCount)
        {
            Cards = new int[CardCount = cardCount];
            for (int i = 0; i < cardCount; i++)
                Cards[i] = i;
        }

        public void Cut(int cards)
        {
            var newCards = new int[CardCount];
            for (int i = 0; i < CardCount; i++)
                newCards[i] = Cards[(i + cards + CardCount) % CardCount];
            Cards = newCards;
        }
        public void DealIntoNewStack() => Cards = Cards.Reverse().ToArray();
        public void DealWithIncrement(int step)
        {
            int position = 0;
            var newCards = new int[CardCount];
            for (int i = 0; i < CardCount; i++, position += step)
            {
                int p = position % CardCount;
                newCards[p] = Cards[i];
            }
            Cards = newCards;
        }

        public int PositionOfCard(int card)
        {
            if (card >= CardCount)
                return -2;

            for (int i = 0; i < CardCount; i++)
                if (card == Cards[i])
                    return i;

            return -1;
        }

        public void PrintDeck() => Console.WriteLine(Cards.Select(c => c.ToString()).Aggregate((a, b) => $"{a} {b}"));
    }
}
