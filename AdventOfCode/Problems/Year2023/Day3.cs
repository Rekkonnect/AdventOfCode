using System.Buffers;

namespace AdventOfCode.Problems.Year2023;

public class Day3 : Problem<int>
{
    private static readonly SearchValues<char> _nonSymbols
        = SearchValues.Create(".0123456789");

    private readonly LineParserPart1 _part1Parser = new([]);
    private readonly LineParserPart2 _part2Parser = new([]);

    public override int SolvePart1()
    {
        return SolvePart(_part1Parser);
    }
    public override int SolvePart2()
    {
        return SolvePart(_part2Parser);
    }

    private static int SolvePart(LineParser parser)
    {
        parser.Reinitialize();
        int sum = 0;
        while (true)
        {
            var next = parser.GetNextSymbolSum();
            if (next < 0)
                return sum;

            sum += next;
        }
    }

    protected override void LoadState()
    {
        var lines = FileLines;
        SetParserLines(lines);
    }
    protected override void ResetState()
    {
        SetParserLines([]);
    }

    private void SetParserLines(string[] lines)
    {
        _part1Parser.SetLines(lines);
        _part2Parser.SetLines(lines);
    }

#nullable enable

    private abstract class LineParser
    {
        protected string[] Lines;
        protected readonly ThreeLineSegmentator Segmentator = new();
        protected int LineIndex;
        protected int ColumnIndex;

        protected LineParser(string[] lines)
        {
            SetLines(lines);
        }

        public void SetLines(string[] lines)
        {
            Lines = lines;
            Reinitialize();
        }

        public void Reinitialize()
        {
            Reset();
            ConsumeLine();
            ConsumeLine();
        }

        private void Reset()
        {
            LineIndex = 0;
            ColumnIndex = 0;
            Segmentator.Clear();
        }

        public abstract int GetNextSymbolSum();

        protected static int ConsumeNumber(Line line, int index)
        {
            if (line.IsInvalid)
                return 0;

            var source = line.Source;
            if (!source[index].IsDigit())
                return 0;

            // Do not consume the index again
            if (line.ConsumedIndices[index])
                return 0;

            int start = index;
            int end = index + 1;

            while (start > 0)
            {
                if (!source[start - 1].IsDigit())
                    break;

                start--;
            }
            while (end < source.Length)
            {
                if (!source[end].IsDigit())
                    break;

                end++;
            }

            int number = source.AsSpan()[start..end].ParseInt32();
            line.ConsumeRange(start, end);
            return number;
        }

        protected Line CurrentLine()
        {
            return Segmentator.MiddleLine;
        }

        protected void ConsumeLine()
        {
            var currentLine = Lines.AtOrDefault(LineIndex);
            Segmentator.FeedNext(currentLine);
            LineIndex++;
            ColumnIndex = 0;
        }

        protected void FindNextSymbolIndex()
        {
            while (true)
            {
                var middle = CurrentLine();
                if (middle.IsInvalid)
                    return;

                var span = middle.Source.AsSpan()[ColumnIndex..];
                int nextIndex = GetNextSymbolIndex(span);
                if (nextIndex >= 0)
                {
                    ColumnIndex += nextIndex + 1;
                    break;
                }

                ConsumeLine();
            }
        }

        protected abstract int GetNextSymbolIndex(SpanString span);
    }

    private sealed class LineParserPart1(string[] lines)
        : LineParser(lines)
    {
        public override int GetNextSymbolSum()
        {
            FindNextSymbolIndex();
            var currentLine = CurrentLine();
            if (currentLine.IsInvalid)
                return -1;

            int columnIndex = ColumnIndex - 1;

            var topLine = Segmentator.TopLine;
            var middleLine = currentLine;
            var bottomLine = Segmentator.BottomLine;

            return ConsumeNumberInternal(topLine, columnIndex, false)
                + ConsumeNumberInternal(middleLine, columnIndex, true)
                + ConsumeNumberInternal(bottomLine, columnIndex, false)
                ;

            static int ConsumeNumberInternal(Line line, int center, bool isMiddle)
            {
                int centerValue = 0;
                if (!isMiddle)
                {
                    centerValue = ConsumeNumber(line, center);
                }
                return ConsumeNumber(line, center - 1)
                    + centerValue
                    + ConsumeNumber(line, center + 1);
            }
        }

        protected override int GetNextSymbolIndex(SpanString span)
        {
            return span.IndexOfAnyExcept(_nonSymbols);
        }
    }

    private sealed class LineParserPart2(string[] lines)
        : LineParser(lines)
    {
        private readonly GearValueStorage _gearValues = new();

        public override int GetNextSymbolSum()
        {
            FindNextSymbolIndex();
            var currentLine = CurrentLine();
            if (currentLine.IsInvalid)
                return -1;

            int columnIndex = ColumnIndex - 1;

            var topLine = Segmentator.TopLine;
            var middleLine = currentLine;
            var bottomLine = Segmentator.BottomLine;

            _gearValues.Clear();

            TryConsumeNumber(topLine, columnIndex - 1);
            TryConsumeNumber(middleLine, columnIndex - 1);
            TryConsumeNumber(bottomLine, columnIndex - 1);

            TryConsumeNumber(topLine, columnIndex);
            TryConsumeNumber(bottomLine, columnIndex);

            TryConsumeNumber(topLine, columnIndex + 1);
            TryConsumeNumber(middleLine, columnIndex + 1);
            TryConsumeNumber(bottomLine, columnIndex + 1);

            return _gearValues.TotalValue;

            void ConsumeNumberInternal(Line line, int index)
            {
                int number = ConsumeNumber(line, index);
                _gearValues.SetValue(number);
            }
            void TryConsumeNumber(Line line, int index)
            {
                if (_gearValues.Stop)
                    return;

                ConsumeNumberInternal(line, index);
            }
        }

        protected override int GetNextSymbolIndex(SpanString span)
        {
            return span.IndexOf('*');
        }

        private class GearValueStorage
        {
            public int Left, Right, Excess;
            public bool Stop;

            public int TotalValue => Excess > 0 ? 0 : Left * Right;

            public void SetValue(int value)
            {
                if (Left is 0)
                {
                    Left = value;
                }
                else if (Right is 0)
                {
                    Right = value;
                }
                else if (Excess is 0)
                {
                    Excess = value;
                    Stop = value > 0;
                }
            }

            public void Clear()
            {
                Left = 0;
                Right = 0;
                Excess = 0;
                Stop = false;
            }
        }
    }

    // More performance improvements if we only utilize 3 bool[] at a time
    private class ThreeLineSegmentator
    {
        public Line TopLine { get; private set; }
        public Line MiddleLine { get; private set; }
        public Line BottomLine { get; private set; }

        public void Clear()
        {
            TopLine = default;
            MiddleLine = default;
            BottomLine = default;
        }

        public void FeedNext(string? line)
        {
            var next = Line.CreateForLine(line);
            TopLine = MiddleLine;
            MiddleLine = BottomLine;
            BottomLine = next;
        }
    }

    private readonly record struct Line(string Source, bool[] ConsumedIndices)
    {
        public bool IsInvalid => Source is null;

        public void ConsumeRange(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                ConsumedIndices[i] = true;
            }
        }

        public static Line CreateForLine(string? source)
        {
            if (source is null)
                return default;

            var numbers = new bool[source.Length];
            return new(source, numbers);
        }
    }
}
