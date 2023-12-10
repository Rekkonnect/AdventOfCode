namespace AdventOfCode.Problems.Year2023;

public class Day9 : Problem<long>
{
    private Line[] _lines;

    public override long SolvePart1()
    {
        return _lines
            .Select(l => l.PredictNextValue())
            .Sum();
    }
    public override long SolvePart2()
    {
        return _lines
            .Select(l => l.PredictPreviousValue())
            .Sum();
    }

    protected override void LoadState()
    {
        _lines = ParsedFileLines((SpanStringSelector<Line>)ParseLine);
    }
    protected override void ResetState()
    {
        _lines = null;
    }

    private static Line ParseLine(SpanString span)
    {
        var values = Parsing.ParseAllInt64(span, true);
        return new(values);
    }

    private class Line
    {
        private readonly ImmutableArray<long> _values;

        public Line(ImmutableArray<long> values)
        {
            _values = values;
        }

        public long PredictNextValue()
        {
            return PredictNextValue(_values.AsSpan());
        }
        private static long PredictNextValue(ReadOnlySpan<long> values)
        {
            Span<long> diffs = stackalloc long[values.Length - 1];
            CalculateValues(values, diffs, out var allZero);

            if (allZero)
            {
                return values[0];
            }

            long nextDiff = PredictNextValue(diffs);
            return values[^1] + nextDiff;
        }

        public long PredictPreviousValue()
        {
            return PredictPreviousValue(_values.AsSpan());
        }
        private static long PredictPreviousValue(ReadOnlySpan<long> values)
        {
            Span<long> diffs = stackalloc long[values.Length - 1];
            CalculateValues(values, diffs, out var allZero);

            if (allZero)
            {
                return values[0];
            }

            long previousDiff = PredictPreviousValue(diffs);
            return values[0] - previousDiff;
        }

        private static void CalculateValues(ReadOnlySpan<long> values, Span<long> diffs, out bool allZero)
        {
            allZero = true;
            for (int i = 0; i < diffs.Length; i++)
            {
                var diff = values[i + 1] - values[i];
                diffs[i] = diff;
                if (diff is not 0)
                {
                    allZero = false;
                }
            }
        }
    }
}
