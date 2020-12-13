using AdventOfCode.Functions;
using Garyon.DataStructures;
using System;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day13 : Problem<int, long>
    {
        private int earliestTimestamp;
        private int[] busIDs;
        
        public override int SolvePart1()
        {
            int minimumAwait = int.MaxValue;
            int chosenBusID = 0;

            foreach (var id in busIDs)
            {
                if (id == 0) // Out of order
                    continue;

                int awaitingTime = id - (earliestTimestamp % id);
                if (awaitingTime == id)
                    awaitingTime = 0;

                if (awaitingTime < minimumAwait)
                {
                    minimumAwait = awaitingTime;
                    chosenBusID = id;
                }
            }

            return chosenBusID * minimumAwait;
        }
        public override long SolvePart2()
        {
            var indexedIDs = new FlexibleDictionary<int, int>();

            for (int i = 0; i < busIDs.Length; i++)
            {
                if (busIDs[i] == 0)
                    continue;

                indexedIDs[busIDs[i]] = i;
            }

            var sortedIDs = busIDs.Where(id => id > 0).ToArray();
            Array.Sort(sortedIDs, CompareDescending);
            var sortedIDOffsets = sortedIDs.Select(id => indexedIDs[id]).ToArray();

            long max0 = sortedIDs[0];

            long velocity = max0;
            long currentTimestamp = 0;

            int lockedMultipliers = 1;

            int offsetDiff = sortedIDOffsets[lockedMultipliers] - sortedIDOffsets[0];

            while (lockedMultipliers < sortedIDs.Length)
            {
                currentTimestamp += velocity;

                while ((currentTimestamp + offsetDiff) % sortedIDs[lockedMultipliers] == 0)
                {
                    // Register another locked multiplier
                    velocity = MathFunctions.LCM(velocity, sortedIDs[lockedMultipliers]);

                    lockedMultipliers++;
                    if (lockedMultipliers >= sortedIDs.Length)
                        break;

                    offsetDiff = sortedIDOffsets[lockedMultipliers] - sortedIDOffsets[0];
                }
            }

            currentTimestamp -= sortedIDOffsets[0];

            while (true)
            {
                if (currentTimestamp % busIDs[0] == 0)
                    return currentTimestamp;

                currentTimestamp += velocity;
            }
        }

        private static int CompareDescending<T>(T a, T b)
            where T : IComparable<T>
        {
            return b.CompareTo(a);
        }

        protected override void ResetState()
        {
            busIDs = null;
        }
        protected override void LoadState()
        {
            var lines = FileLines;
            earliestTimestamp = lines[0].ParseInt32();
            busIDs = lines[1].Replace('x', '0').Split(',').Select(int.Parse).ToArray();
        }
    }
}
