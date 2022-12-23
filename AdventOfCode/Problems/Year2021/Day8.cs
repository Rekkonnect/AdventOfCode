#nullable enable

using AdventOfCode.Functions;
using System.Numerics;

namespace AdventOfCode.Problems.Year2021;

public class Day8 : Problem<int>
{
    private DisplayEntries? entries;

    public override int SolvePart1()
    {
        return entries!.SimpleDisplayedDigits;
    }
    public override int SolvePart2()
    {
        return entries!.DisplayedNumberSum;
    }

    protected override void LoadState()
    {
        entries = DisplayEntries.Parse(FileLines);
    }
    protected override void ResetState()
    {
        entries = null;
    }

    private class DigitSignalMapper
    {
        private Count5SignalContainer count5Signals;
        private Count6SignalContainer count6Signals;
        private SignalPattern digit1, digit4, digit7, digit8;

        public int GetDisplayedValueMap(SignalPattern[] shuffledPatterns, SignalPattern[] displayedPatterns)
        {
            Map(shuffledPatterns);
            return GetDisplayedValue(displayedPatterns);
        }
        public int GetDisplayedValue(SignalPattern[] displayedPatterns)
        {
            int result = 0;
            int multiplier = 1;
            // There could as well be the general case of using other than just 4 digits
            for (int i = 1; i <= displayedPatterns.Length; i++, multiplier *= 10)
                result += DigitFromPattern(displayedPatterns[^i]) * multiplier;
            return result;
        }

        public void Map(SignalPattern[] shuffledPatterns)
        {
            var patterns = new List<SignalPattern>(shuffledPatterns);
            RegisterSimpleDigits(patterns);
            SplitBySignalCount(patterns, out var count5, out var count6);
            RegisterSegmentCount6Digits(count6);
            RegisterSegmentCount5Digits(count5);
        }
        private void SetNonEncapsulating(List<SignalPattern> patterns, int encapsulatorDigit, int encapsulatedDigit)
        {
            var encapsulatedPattern = PatternForDigit(this, encapsulatedDigit);
            ref var encapsulatorPattern = ref PatternForDigit(this, encapsulatorDigit);
            for (int i = 0; i < patterns.Count; i++)
            {
                var pattern = patterns[i];
                if (!pattern.Encapsulates(encapsulatedPattern))
                {
                    patterns.RemoveAt(i);
                    encapsulatorPattern = pattern;
                    return;
                }
            }
        }
        private void SetEncapsulated(List<SignalPattern> patterns, int encapsulatorDigit, int encapsulatedDigit)
        {
            ref var encapsulatedPattern = ref PatternForDigit(this, encapsulatedDigit);
            var encapsulatorPattern = PatternForDigit(this, encapsulatorDigit);
            for (int i = 0; i < patterns.Count; i++)
            {
                var pattern = patterns[i];
                if (encapsulatorPattern.Encapsulates(pattern))
                {
                    patterns.RemoveAt(i);
                    encapsulatedPattern = pattern;
                    return;
                }
            }
        }
        private void RegisterSegmentCount6Digits(List<SignalPattern> patterns)
        {
            SetNonEncapsulating(patterns, 6, 1);
            SetNonEncapsulating(patterns, 0, 4);
            SetDigit(9, patterns[0]);
        }
        private void RegisterSegmentCount5Digits(List<SignalPattern> patterns)
        {
            SetEncapsulated(patterns, 6, 5);
            SetEncapsulated(patterns, 9, 3);
            SetDigit(2, patterns[0]);
        }
        private static void SplitBySignalCount(List<SignalPattern> all, out List<SignalPattern> count5, out List<SignalPattern> count6)
        {
            count6 = new();
            for (int i = 0; i < all.Count; i++)
            {
                var pattern = all[i];
                int signalCount = pattern.SignalCount;
                if (signalCount is 5)
                    continue;

                count6.Add(pattern);
                all.RemoveAtDecrement(ref i);
            }
            count5 = all;
        }
        private void RegisterSimpleDigits(List<SignalPattern> patterns)
        {
            for (int i = 0; i < patterns.Count; i++)
            {
                int digit = DigitForDefinitivePattern(patterns[i]);
                if (digit < 0)
                    continue;

                SetDigit(digit, patterns[i]);
                patterns.RemoveAtDecrement(ref i);
            }
        }

        private static int DigitForDefinitivePattern(SignalPattern pattern)
        {
            return pattern.SignalCount switch
            {
                2 => 1,
                3 => 7,
                4 => 4,
                7 => 8,
                _ => -1,
            };
        }
        private int DigitFromPattern(SignalPattern pattern)
        {
            return pattern.SignalCount switch
            {
                5 => count5Signals.DigitFrom(pattern),
                6 => count6Signals.DigitFrom(pattern),
                _ => DigitForDefinitivePattern(pattern),
            };
        }

        private void SetDigit(int digit, SignalPattern pattern)
        {
            PatternForDigit(this, digit) = pattern;
        }

        private static ref SignalPattern PatternForDigit(DigitSignalMapper mapper, int digit)
        {
            switch (digit)
            {
                case 1:
                    return ref mapper.digit1;
                case 4:
                    return ref mapper.digit4;
                case 7:
                    return ref mapper.digit7;
                case 8:
                    return ref mapper.digit8;

                case 2 or 3 or 5:
                    return ref Count5SignalContainer.PatternForDigit(ref mapper.count5Signals, digit);
                case 0 or 6 or 9:
                    return ref Count6SignalContainer.PatternForDigit(ref mapper.count6Signals, digit);
            }
            throw null;
        }

        private struct Count6SignalContainer
        {
            public SignalPattern Digit0, Digit6, Digit9;

            public static ref SignalPattern PatternForDigit(ref Count6SignalContainer container, int digit)
            {
                switch (digit)
                {
                    case 0:
                        return ref container.Digit0;
                    case 6:
                        return ref container.Digit6;
                    case 9:
                        return ref container.Digit9;
                }
                throw null;
            }
            public int DigitFrom(SignalPattern pattern)
            {
                if (pattern == Digit0)
                    return 0;
                if (pattern == Digit6)
                    return 6;
                if (pattern == Digit9)
                    return 9;

                return -1;
            }
        }
        private struct Count5SignalContainer
        {
            public SignalPattern Digit2, Digit3, Digit5;

            public static ref SignalPattern PatternForDigit(ref Count5SignalContainer container, int digit)
            {
                switch (digit)
                {
                    case 2:
                        return ref container.Digit2;
                    case 3:
                        return ref container.Digit3;
                    case 5:
                        return ref container.Digit5;
                }
                throw null;
            }
            public int DigitFrom(SignalPattern pattern)
            {
                if (pattern == Digit2)
                    return 2;
                if (pattern == Digit3)
                    return 3;
                if (pattern == Digit5)
                    return 5;

                return -1;
            }
        }
    }
    // TODO: Migrate to some random useless utilities 
    private struct Square8BitArray
    {
        private ulong bits;

        public Square8BitArray(ulong initialBits)
        {
            bits = initialBits;
        }

        public int this[int x, int y]
        {
            get => Convert.ToInt32((bits & BitMask(x, y)) != 0);
            set
            {
                ulong mask = BitMask(x, y);
                bits = value switch
                {
                    0 => Unset(bits, mask),
                    1 => Set(bits, mask),
                };
            }
        }

        private static ulong BitMask(int x, int y) => 1UL << BitIndex(x, y);
        private static int BitIndex(int x, int y) => y * 8 + x;
        private static ulong Unset(ulong value, ulong mask) => value & ~mask;
        private static ulong Set(ulong value, ulong mask) => value | mask;
    }

    private readonly struct SignalPattern
    {
        private readonly byte segments;
        private readonly byte signalCount;

        public int Segments => segments;
        public int SignalCount => signalCount;

        // Hacky resolution of 1, 4, 7, 8
        public bool IsSimpleDigit => signalCount is 2 or 3 or 4 or 7;

        public SignalPattern(uint bitSegments)
            : this((byte)bitSegments, BitOperations.PopCount(bitSegments)) { }
        private SignalPattern(byte bitSegments, int litBits)
        {
            segments = bitSegments;
            signalCount = (byte)litBits;
        }

        public bool Encapsulates(SignalPattern enclosed)
        {
            return (segments & enclosed.segments) == enclosed.segments;
        }

        public static bool operator ==(SignalPattern left, SignalPattern right) => left.segments == right.segments;
        public static bool operator !=(SignalPattern left, SignalPattern right) => left.segments != right.segments;

        public static SignalPattern Parse(string pattern)
        {
            byte segments = 0;
            for (int i = 0; i < pattern.Length; i++)
                segments |= (byte)(1 << (pattern[i] - 'a'));
            return new(segments, pattern.Length);
        }
    }

    private class DisplayEntry
    {
        private readonly SignalPattern[] patterns;
        private readonly SignalPattern[] displayedDigits;

        public int SimpleDisplayedDigits => displayedDigits.Count(displayed => displayed.IsSimpleDigit);
        public int DisplayedNumber => new DigitSignalMapper().GetDisplayedValueMap(patterns, displayedDigits);

        private DisplayEntry(SignalPattern[] registeredPatterns, SignalPattern[] displayedDigitPatterns)
        {
            patterns = registeredPatterns;
            displayedDigits = displayedDigitPatterns;
        }

        // This parsing could become even more efficient
        // But it's not my job to juice out the most performance I can
        public static DisplayEntry Parse(string rawEntry)
        {
            var split = rawEntry.Split(" | ");
            var registeredString = split[0];
            var displayedString = split[1];

            var registeredPatterns = ParsePatternString(registeredString);
            var displayedPatterns = ParsePatternString(displayedString);

            return new(registeredPatterns, displayedPatterns);

            static SignalPattern[] ParsePatternString(string patternString)
            {
                return patternString.Split(' ').Select(SignalPattern.Parse).ToArray();
            }
        }
    }

    private class DisplayEntries
    {
        private readonly DisplayEntry[] entries;

        public int SimpleDisplayedDigits => entries.Sum(entry => entry.SimpleDisplayedDigits);
        public int DisplayedNumberSum => entries.Sum(entry => entry.DisplayedNumber);

        private DisplayEntries(DisplayEntry[] displayEntries)
        {
            entries = displayEntries;
        }

        public static DisplayEntries Parse(string[] rawEntryLines)
        {
            return new(rawEntryLines.Select(DisplayEntry.Parse).ToArray());
        }
    }
}
