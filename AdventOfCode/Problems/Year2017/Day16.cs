using AdventOfCode.Utilities;
using System.Security.Cryptography.X509Certificates;

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
        private readonly DanceMove[] moves;

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

        public static Dance Parse(SpanString raw)
        {
            return new(raw.SplitSelect(',', DanceMove.ParseMove));
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
        public override void Operate(ProgramArrangement programs)
        {
            programs.SwapPosition(X, Y);
        }

        public static ExchangeMove Parse(SpanString raw)
        {
            if (raw[0] is not 'x')
                return null;

            var pattern = raw[1..];
            pattern.SplitOnce('/', out var left, out var right);
            int x = left.ParseInt32();
            int y = right.ParseInt32();

            return new ExchangeMove(x, y);
        }
    }
    private sealed partial record PartnerOperation(char X, char Y) : DanceMove
    {
        public override void Operate(ProgramArrangement programs)
        {
            programs.SwapItem(X, Y);
        }

        public static PartnerOperation Parse(SpanString raw)
        {
            if (raw[0] is not 'p')
                return null;

            char x = raw[1];
            char y = raw[3];
            return new PartnerOperation(x, y);
        }
    }
    private sealed record SpinOperation(int Rotation) : DanceMove
    {
        public override void Operate(ProgramArrangement programs)
        {
            programs.Rotate(Rotation);
        }
        public override void OperateReverse(ProgramArrangement programs)
        {
            programs.Rotate(-Rotation);
        } 

        public static SpinOperation Parse(SpanString raw)
        {
            if (raw[0] is not 's')
                return null;

            var rotation = raw[1..].ParseInt32();

            return new SpinOperation(rotation);
        }
    }

    private abstract record DanceMove
    {
        public abstract void Operate(ProgramArrangement arrangement);
        public virtual void OperateReverse(ProgramArrangement arrangement) => Operate(arrangement);

        public static DanceMove ParseMove(SpanString raw)
        {
            return ExchangeMove.Parse(raw)
                ?? PartnerOperation.Parse(raw)
                ?? SpinOperation.Parse(raw)
                as DanceMove;
        }
    }
}
