using AdventOfCode.Utilities;

namespace AdventOfCode.Problems.Year2017;

public partial class Day16 : Problem<string>
{
    private Dance dance;

    public override string SolvePart1()
    {
        return dance.GetFinalProgramOrder();
    }
    public override string SolvePart2()
    {
        return dance.GetFinalProgramOrder(1000000000);
    }

    protected override void LoadState()
    {
        dance = Dance.Parse(FileContents);
    }
    protected override void ResetState()
    {
        dance = null;
    }

    private class Dance
    {
        private DanceMove[] moves;

        public Dance(IEnumerable<DanceMove> danceMoves)
        {
            moves = danceMoves.ToArray();
        }

        public string GetFinalProgramOrder(int danceCount = 1)
        {
            var result = new ProgramArrangement();
            var arrangements = new FlexibleDictionary<ulong, int?>();
            var roundArrangements = new FlexibleDictionary<int, ulong>();

            for (int i = 0; i < danceCount; i++)
            {
                foreach (var move in moves)
                    move.Operate(result);

                ulong arrangementCode = result.GetCurrentArrangementCode();
                if (arrangements[arrangementCode] is int firstOccurrence)
                {
                    // Skip remaining moves
                    int loopSize = firstOccurrence - i;
                    int offset = firstOccurrence - loopSize;
                    int finalLoopedIndex = (danceCount - offset) % loopSize - 1;
                    return ProgramArrangement.FromArrangementCode(roundArrangements[finalLoopedIndex]);
                }
                else
                {
                    arrangements[arrangementCode] = i;
                    roundArrangements[i] = arrangementCode;
                }
#if DEBUG
                if (i % 1000 is 0)
                    Console.WriteLine($"Dance performed {i} times");
#endif
            }

            return new(result.ConstructArray());
        }

        public static Dance Parse(string raw)
        {
            return new(raw.Split(',').Select(DanceMove.ParseMove));
        }
    }

    private class ProgramArrangement : ConstructableArray<char>
    {
        public const int ProgramCount = 16;

        public ProgramArrangement()
            : base(ProgramCount)
        {
            for (int i = 0; i < ProgramCount; i++)
                Array[i] = (char)(i + 'a');
        }

        public ulong GetCurrentArrangementCode()
        {
            ulong result = 0;
            for (int i = 0; i < Length; i++)
            {
                result <<= 4;
                result |= (uint)(Array[i] - 'a');
            }
            return result;
        }

        public string GetCurrentArrangementString() => new(ConstructArray());

        public static string FromArrangementCode(ulong arrangementCode)
        {
            var result = new char[ProgramCount];

            for (int i = 0; i < ProgramCount; i++)
            {
                result[^(i + 1)] = (char)((int)(arrangementCode & 0xF) + 'a');
                arrangementCode >>= 4;
            }

            return new(result);
        }
    }

    private sealed partial record ExchangeMove(int X, int Y) : DanceMove
    {
        private static readonly Regex exchangePattern = ExchangeRegex();

        public override void Operate(ProgramArrangement programs)
        {
            programs.SwapPosition(X, Y);
        }

        public static ExchangeMove Parse(string raw)
        {
            var match = exchangePattern.Match(raw);

            if (!match.Success)
                return null;

            var groups = match.Groups;
            int x = groups["x"].Value.ParseInt32();
            int y = groups["y"].Value.ParseInt32();
            return new ExchangeMove(x, y);
        }

        [GeneratedRegex("x(?'x'\\d*)/(?'y'\\d*)", RegexOptions.Compiled)]
        private static partial Regex ExchangeRegex();
    }
    private sealed partial record PartnerOperation(char X, char Y) : DanceMove
    {
        private static readonly Regex partnerPattern = PartnerRegex();

        public override void Operate(ProgramArrangement programs)
        {
            programs.SwapItem(X, Y);
        }

        public static PartnerOperation Parse(string raw)
        {
            var match = partnerPattern.Match(raw);

            if (!match.Success)
                return null;

            var groups = match.Groups;
            char x = groups["x"].Value[0];
            char y = groups["y"].Value[0];
            return new PartnerOperation(x, y);
        }

        [GeneratedRegex("p(?'x'\\w)/(?'y'\\w)", RegexOptions.Compiled)]
        private static partial Regex PartnerRegex();
    }
    private sealed record SpinOperation(int Rotation) : DanceMove
    {
        private static readonly Regex spinPattern = new(@"s(?'x'\d*)", RegexOptions.Compiled);

        public override void Operate(ProgramArrangement programs)
        {
            programs.Rotate(Rotation);
        }
        public override void OperateReverse(ProgramArrangement programs)
        {
            programs.Rotate(-Rotation);
        }

        public static SpinOperation Parse(string raw)
        {
            var match = spinPattern.Match(raw);

            if (!match.Success)
                return null;

            var groups = match.Groups;
            int rotation = groups["x"].Value.ParseInt32();

            return new SpinOperation(rotation);
        }
    }

    private abstract record DanceMove
    {
        public abstract void Operate(ProgramArrangement arrangement);
        public virtual void OperateReverse(ProgramArrangement arrangement) => Operate(arrangement);

        public static DanceMove ParseMove(string raw)
        {
            DanceMove result = null;

            result ??= ExchangeMove.Parse(raw);
            result ??= PartnerOperation.Parse(raw);
            result ??= SpinOperation.Parse(raw);

            return result;
        }
    }
}
