namespace AdventOfCode.Problems.Year2023;

public class Day4 : Problem<int>
{
    private Card[] _cards;

    public override int SolvePart1()
    {
        int total = 0;

        for (int i = 0; i < _cards.Length; i++)
        {
            var card = _cards[i];
            int matching = card.MatchingNumbers();
            if (matching is 0)
                continue;

            int points = 1 << (matching - 1);
            total += points;
        }

        return total;
    }
    public override int SolvePart2()
    {
        int[] copies = new int[_cards.Length];
        copies.Fill(1);

        for (int i = 0; i < copies.Length; i++)
        {
            int multiplier = copies[i];
            var card = _cards[i];
            int matches = card.MatchingNumbers();
            for (int n = 1; n <= matches; n++)
            {
                int nextIndex = i + n;
                if (nextIndex >= copies.Length)
                    break;

                copies[nextIndex] += multiplier;
            }
        }

        return copies.Sum();
    }

    protected override void LoadState()
    {
        // What the fuck?
        _cards = ParsedFileLines((SpanStringSelector<Card>)ParseCard);
    }
    protected override void ResetState()
    {
        _cards = null;
    }

#nullable enable

    private static Card ParseCard(SpanString span)
    {
        span.SplitOnce(':', out var cardDeclaration, out var numbers);

        var id = Parsing.ParseLastInt32(cardDeclaration);

        numbers.SplitOnce('|', out var winning, out var played);

        var winningNumbers = Parsing.ParseAllUInt16(winning);
        var playedNumbers = Parsing.ParseAllUInt16(played);

        return new(id, winningNumbers, playedNumbers);
    }

    private record Card(
        int ID, ImmutableArray<ushort> Winning, ImmutableArray<ushort> Played)
    {
        public int MatchingNumbers()
        {
            var winning = Winning;
            int count = 0;
            for (int i = 0; i < Played.Length; i++)
            {
                if (winning.Contains(Played[i]))
                {
                    count++;
                }
            }
            return count;
        }
    }
}
