#nullable enable

using AdventOfCSharp;
using Garyon.Extensions;
using System;
using System.Linq;

namespace AdventOfCode.Problems.Year2021;

public class Day6 : Problem<ulong>
{
    private ReproductionSystem? system;

    public override ulong SolvePart1()
    {
        return SolvePart(80);
    }
    public override ulong SolvePart2()
    {
        return SolvePart(256);
    }

    protected override void LoadState()
    {
        system = ReproductionSystem.Parse(FileContents.TrimEnd());
    }
    protected override void ResetState()
    {
        system = null;
    }

    private ulong SolvePart(int days)
    {
        return system!.Clone().TotalLanternfishAt(days);
    }

    private class ReproductionSystem
    {
        private readonly ulong[] timers;

        public ulong TotalLanternfish => timers.Sum();

        private ReproductionSystem(ulong[] reproductionTimers)
        {
            timers = reproductionTimers;
        }
        public ReproductionSystem Clone() => new(timers.ToArray());

        public ulong TotalLanternfishAt(int days)
        {
            Iterate(days);
            return TotalLanternfish;
        }
        private void Iterate(int days)
        {
            int doubleIterations = days / 2;
            for (int i = 0; i < doubleIterations; i++)
                IterateTwice();

            if (days % 2 is 1)
                Iterate();
        }
        private void Iterate()
        {
            ShiftLeft(timers.AsSpan(), out var reproduced);
            timers[6] += reproduced;
            timers[8] = reproduced;
        }
        private void IterateTwice()
        {
            ulong reproduced0 = timers[0];
            ulong reproduced1 = timers[1];
            ShiftLeftTwo(timers.AsSpan());
            timers[5] += reproduced0;
            timers[6] += reproduced1;
            timers[7] = reproduced0;
            timers[8] = reproduced1;
        }
        // No idea if you can further speed iteration up

        private static unsafe void ShiftLeft(Span<ulong> span, out ulong excluded)
        {
            excluded = span[0];
            for (int i = 0; i < span.Length - 1; i++)
                span[i] = span[i + 1];
        }
        private static unsafe void ShiftLeftTwo(Span<ulong> span)
        {
            for (int i = 0; i < span.Length - 2; i++)
                span[i] = span[i + 2];
        }

        public static ReproductionSystem Parse(string rawTimers)
        {
            return new(GetTimerCounters(rawTimers));
        }

        private static unsafe ulong[] GetTimerCounters(string rawTimers)
        {
            ulong[] result = new ulong[9];

            int length = rawTimers.Length;
            fixed (char* rawTimerChars = rawTimers)
                for (int i = 0; i < length; i += 2)
                    result[rawTimerChars[i].GetNumericValueInteger()]++;

            return result;
        }
    }
}
