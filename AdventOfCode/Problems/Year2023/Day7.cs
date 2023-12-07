using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AdventOfCode.Problems.Year2023;

public class Day7 : Problem<long>
{
    private HandSet _hands;

    public override long SolvePart1()
    {
        return SolvePart(HandBid.ComparerRegular.Instance);
    }
    public override long SolvePart2()
    {
        return SolvePart(HandBid.ComparerJoker.Instance);
    }

    private long SolvePart(IComparer<HandBid> comparer)
    {
        return _hands.TotalWinnings(comparer);
    }

    protected override void LoadState()
    {
        var bids = ParsedFileLines((SpanStringSelector<HandBid>)ParseHandBid);
        _hands = new(bids.ToImmutableArray());
    }
    protected override void ResetState()
    {
        _hands = null;
    }

    private static HandBid ParseHandBid(SpanString span)
    {
        span.SplitOnce(' ', out var handSpan, out var bidSpan);
        var hand = ParseHand(handSpan);
        var bid = bidSpan.ParseInt32();
        return new(hand, bid);
    }

    private static Hand ParseHand(SpanString span)
    {
        var a = ParseCard(span[0]);
        var b = ParseCard(span[1]);
        var c = ParseCard(span[2]);
        var d = ParseCard(span[3]);
        var e = ParseCard(span[4]);
        return new(a, b, c, d, e);
    }

    private static Card ParseCard(char c)
    {
        return c switch
        {
            >= '2' and <= '9' => new(c.GetNumericValueInteger()),
            'T' => Card.Ten,
            'J' => Card.Jack,
            'Q' => Card.Queen,
            'K' => Card.King,
            'A' => Card.Ace,
        };
    }

    private record HandSet(ImmutableArray<HandBid> HandBids)
    {
        public long TotalWinnings(IComparer<HandBid> comparer)
        {
            var sortedHands = HandBids.ToArray()
                .SortBy(comparer);

            long total = 0;
            for (int i = 0; i < sortedHands.Length; i++)
            {
                int rank = i + 1;
                var hand = sortedHands[i];
                long winning = hand.Bid * rank;
                total += winning;
            }
            return total;
        }
    }

    private readonly record struct HandBid(Hand Hand, int Bid)
    {
        public sealed class ComparerRegular : IComparer<HandBid>
        {
            public static ComparerRegular Instance { get; } = new();

            int IComparer<HandBid>.Compare(HandBid x, HandBid y)
            {
                return Hand.ComparerRegular.Instance.Compare(x.Hand, y.Hand);
            }
        }
        public sealed class ComparerJoker : IComparer<HandBid>
        {
            public static ComparerJoker Instance { get; } = new();

            int IComparer<HandBid>.Compare(HandBid x, HandBid y)
            {
                return Hand.ComparerJoker.Instance.Compare(x.Hand, y.Hand);
            }
        }

        public override string ToString()
        {
            return $"{Hand} {Bid}";
        }
    }

    private readonly struct Hand
    {
        private readonly Card a, b, c, d, e;

        public HandType Type { get; }
        public HandType JokerType { get; }

        public Card A => a;
        public Card B => b;
        public Card C => c;
        public Card D => d;
        public Card E => e;

        public Hand(Card a, Card b, Card c, Card d, Card e)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;

            Type = CalculateType();
            JokerType = CalculateJokerType();
        }

        public void Deconstruct(out Card a, out Card b, out Card c, out Card d, out Card e)
        {
            a = this.a;
            b = this.b;
            c = this.c;
            d = this.d;
            e = this.e;
        }

        private HandType CalculateType()
        {
            return HandTypeCalculation.Instance.Calculate(this);
        }
        private HandType CalculateJokerType()
        {
            return HandTypeCalculation.Instance.CalculateJoker(Type);
        }

        public override string ToString()
        {
            return $"{a}{b}{c}{d}{e}";
        }

        public sealed class ComparerRegular : ComparerBase, IComparer<Hand>
        {
            public static ComparerRegular Instance { get; } = new();

            public int Compare(Hand x, Hand y)
            {
                int type = x.Type.CompareTo(y.Type);
                if (type is not 0)
                    return type;

                return CompareCards(x, y, Card.ComparerRegular.Instance);
            }
        }
        public sealed class ComparerJoker : ComparerBase, IComparer<Hand>
        {
            public static ComparerJoker Instance { get; } = new();

            public int Compare(Hand x, Hand y)
            {
                int type = x.JokerType.CompareTo(y.JokerType);
                if (type is not 0)
                    return type;

                return CompareCards(x, y, Card.ComparerJoker.Instance);
            }
        }
        public abstract class ComparerBase
        {
            protected static int CompareCards(Hand x, Hand y, IComparer<Card> cardComparer)
            {
                int a = cardComparer.Compare(x.a, y.a);
                if (a is not 0)
                    return a;
                int b = cardComparer.Compare(x.b, y.b);
                if (b is not 0)
                    return b;
                int c = cardComparer.Compare(x.c, y.c);
                if (c is not 0)
                    return c;
                int d = cardComparer.Compare(x.d, y.d);
                if (d is not 0)
                    return d;
                int e = cardComparer.Compare(x.e, y.e);
                if (e is not 0)
                    return e;

                return 0;
            }
        }
    }

    private sealed class HandTypeCalculation
    {
        public static HandTypeCalculation Instance { get; } = new();

        private const int LowestValue = 2;
        private const int TotalCardCount = (9 - LowestValue + 1) + 5;

        private readonly int[] _counters = new int[TotalCardCount];

        public HandType Calculate(Hand hand)
        {
            Reset();

            var (a, b, c, d, e) = hand;
            AddCounter(a);
            AddCounter(b);
            AddCounter(c);
            AddCounter(d);
            AddCounter(e);

            int twoPairValue = 0;
            var counterFlags = CounterFlags.None;
            var result = HandType.None;

            AnalyzeCounter(a);
            if (result is not HandType.None)
                return result;

            AnalyzeCounter(b);
            if (result is not HandType.None)
                return result;

            AnalyzeCounter(c);
            if (result is not HandType.None)
                return result;

            AnalyzeCounter(d);
            if (result is not HandType.None)
                return result;

            AnalyzeCounter(e);
            if (result is not HandType.None)
                return result;

            if (counterFlags is CounterFlags.OnePair)
                return HandType.OnePair;

            return HandType.HighCard;

            void AnalyzeCounter(Card card)
            {
                var counter = GetCounter(card);
                switch (counter)
                {
                    case 1:
                        counterFlags |= CounterFlags.One;
                        break;
                    case 2:
                        if (twoPairValue > 0 && twoPairValue != card.Value)
                        {
                            result = HandType.TwoPair;
                            return;
                        }
                        counterFlags |= CounterFlags.Two;
                        twoPairValue = card.Value;
                        break;
                    case 3:
                        counterFlags |= CounterFlags.Three;
                        break;
                    case 4:
                        result = HandType.FourOfAKind;
                        return;
                    case 5:
                        result = HandType.FiveOfAKind;
                        return;
                }

                switch (counterFlags)
                {
                    case CounterFlags.FullHouse:
                        result = HandType.FullHouse;
                        return;
                    case CounterFlags.ThreeOfAKind:
                        result = HandType.ThreeOfAKind;
                        return;
                }
            }
        }

        /// <exception cref="UnreachableException"></exception>
        /// <remarks>
        /// Ensure that this is called right after a calculation has been performed;
        /// otherwise the state may have been overwritten.
        /// </remarks>
        public HandType CalculateJoker(HandType firstType)
        {
            int jokers = GetJokerCount();

            if (jokers is 0)
                return firstType;

            switch (firstType)
            {
                case HandType.ThreeOfAKind:
                    // AAABJ
                    // JJJAB
                    if (jokers is 1 or 3)
                        return HandType.FourOfAKind;

                    return HandType.None;

                case HandType.FullHouse:
                    // AAAJJ
                    // JJJAA
                    if (jokers is 2 or 3)
                        return HandType.FiveOfAKind;

                    return HandType.None;

                case HandType.FourOfAKind:
                    // AAAAJ
                    // JJJJA
                    if (jokers is 1 or 4)
                        return HandType.FiveOfAKind;

                    return HandType.None;

                case HandType.TwoPair:
                    // AABBJ
                    if (jokers is 1)
                        return HandType.FullHouse;

                    // AAJJB
                    if (jokers is 2)
                        return HandType.FourOfAKind;

                    return HandType.None;

                case HandType.OnePair:
                    // AABCJ
                    if (jokers is 1)
                        return HandType.ThreeOfAKind;

                    // JJABC
                    if (jokers is 2)
                        return HandType.ThreeOfAKind;

                    return HandType.None;

                case HandType.HighCard:
                    // ABCDJ
                    if (jokers is 1)
                        return HandType.OnePair;

                    return HandType.None;

                case HandType.FiveOfAKind:
                    return HandType.FiveOfAKind;
            }

            return HandType.None;
        }

        private int GetCounter(Card card)
        {
            int index = GetCardIndex(card);
            return _counters[index];
        }

        private void AddCounter(Card card)
        {
            int index = GetCardIndex(card);
            _counters[index]++;
        }

        private static int GetCardIndex(Card card)
        {
            return GetValueIndex(card.Value);
        }
        private static int GetValueIndex(int value)
        {
            return value - LowestValue;
        }

        private int GetJokerCount()
        {
            return _counters[GetValueIndex(Card.JackValue)];
        }

        private void Reset()
        {
            _counters.Clear();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 1)]
    private readonly struct Card
    {
        public const int
            TenValue = 10,
            JackValue = 11,
            QueenValue = 12,
            KingValue = 13,
            AceValue = 14,

            JokerValue = 1
            ;

        private readonly byte _value;

        public static Card Ten = new(TenValue);
        public static Card Jack = new(JackValue);
        public static Card Queen = new(QueenValue);
        public static Card King = new(KingValue);
        public static Card Ace = new(AceValue);

        public int Value => _value;

        public Card(byte value)
        {
            _value = value;
        }
        public Card(int value)
            : this((byte)value) { }

        public override string ToString()
        {
            return _value switch
            {
                >= 2 and <= 9 => _value.ToString(),
                TenValue => "T",
                JackValue => "J",
                QueenValue => "Q",
                KingValue => "K",
                AceValue => "A",

                _ => string.Empty
            };
        }

        public sealed class ComparerRegular : IComparer<Card>
        {
            public static ComparerRegular Instance { get; } = new();

            public int Compare(Card x, Card y)
            {
                return x._value.CompareTo(y._value);
            }
        }
        public sealed class ComparerJoker : IComparer<Card>
        {
            public static ComparerJoker Instance { get; } = new();

            public int Compare(Card x, Card y)
            {
                int xvalue = ConvertedValue(x._value);
                int yvalue = ConvertedValue(y._value);
                return xvalue.CompareTo(yvalue);
            }

            private static int ConvertedValue(int value)
            {
                if (value is JackValue)
                    return JokerValue;

                return value;
            }
        }
    }

    private enum HandType : byte
    {
        None,

        HighCard,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        FullHouse,
        FourOfAKind,
        FiveOfAKind,
    }

    private enum CounterFlags
    {
        None = 0,

        One = 1 << 0,
        Two = 1 << 1,
        Three = 1 << 2,
        Four = 1 << 3,
        Five = 1 << 4,

        OnePair = Two | One,
        ThreeOfAKind = Three | One,
        FullHouse = Three | Two,
        FourOfAKind = Four | One,
        FiveOfAKind = Five,
    }
}
