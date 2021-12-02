//#define PRINT

using AdventOfCode.Utilities;
using AdventOfCode.Utilities.TwoDimensions;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2016;

public class Day22 : Problem<int>
{
    private StorageCluster cluster;

    public override int SolvePart1()
    {
        return cluster.GetViablePairCount();
    }
    // WIP forever?
    [PartSolution(PartSolutionStatus.Uninitialized)]
    public override int SolvePart2()
    {
        // I don't like this
        return -1;
    }

    protected override void LoadState()
    {
        cluster = new(ParsedFileLines(StorageDisk.Parse, 2, 0));
    }
    protected override void ResetState()
    {
        cluster = null;
    }

    private class StorageCluster : Grid2D<StorageDisk>
    {
        public StorageCluster(IEnumerable<StorageDisk> disks)
            : base(31, initializeValueCounters: false)
        {
            foreach (var d in disks)
                Values[d.X, d.Y] = d;
        }

        public int GetViablePairCount()
        {
            var flattenedValues = new List<StorageDisk>(Values.Length);
            foreach (var v in Values)
                flattenedValues.Add(v);

            var sortedUsed = new SortedCollection<StorageDisk>(flattenedValues, StorageDisk.AscendingUsed);
            var sortedAvailable = new SortedCollection<StorageDisk>(flattenedValues, StorageDisk.AscendingAvailable);

#if PRINT
                Console.WriteLine("\nSorted by Used:\n");
                sortedUsed.ForEach(d => Console.WriteLine(d));
                Console.WriteLine("\nSorted by Available:\n");
                sortedAvailable.ForEach(d => Console.WriteLine(d));
#endif

            int count = 0;

            int availableIndex = 0;
            for (int usedIndex = 0; usedIndex < sortedUsed.Count; usedIndex++)
            {
                var currentUsed = sortedUsed[usedIndex];

                if (currentUsed.Used == 0)
                    continue;

                while (availableIndex < sortedAvailable.Count && currentUsed.Used > sortedAvailable[availableIndex].Available)
                {
                    availableIndex++;
                }

                if (availableIndex >= sortedAvailable.Count)
                    break;

                count += flattenedValues.Count - availableIndex;
                // Remove self-assignment
                if (currentUsed.Used <= currentUsed.Available)
                    count--;

#if PRINT
                    Console.WriteLine($"Used\t\t{usedIndex,3} | {currentUsed}\nAvailable\t{availableIndex,3} | {sortedAvailable[availableIndex]}\n{count}\n");
#endif
            }

            return count;
        }
    }

    private struct StorageDisk : IEquatable<StorageDisk>
    {
        private static readonly Regex diskPattern = new(@"node-x(?'x'\d*)-y(?'y'\d*)\s*(?'size'\d*)T\s*(?'used'\d*)T", RegexOptions.Compiled);

        public int X { get; }
        public int Y { get; }
        public int Size { get; }
        public int Used { get; private set; }
        public int Available => Size - Used;
        public double UseRatio => (double)Used / Size;

        public StorageDisk(int x, int y, int size, int used)
        {
            X = x;
            Y = y;
            Size = size;
            Used = used;
        }

        public bool TrySendData(StorageDisk other)
        {
            if (other.Available < Used)
                return false;

            other.Used += Used;
            Used = 0;
            return true;
        }

        public static StorageDisk Parse(string raw)
        {
            var groups = diskPattern.Match(raw).Groups;
            int x = groups["x"].Value.ParseInt32();
            int y = groups["y"].Value.ParseInt32();
            int size = groups["size"].Value.ParseInt32();
            int used = groups["used"].Value.ParseInt32();
            return new(x, y, size, used);
        }

        public bool Equals(StorageDisk other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is StorageDisk disk && Equals(disk);

        public static int AscendingUsed(StorageDisk left, StorageDisk right) => left.Used.CompareTo(right.Used);
        public static int AscendingAvailable(StorageDisk left, StorageDisk right) => left.Available.CompareTo(right.Available);
        public static int DescendingUsed(StorageDisk left, StorageDisk right) => right.Used.CompareTo(left.Used);
        public static int DescendingAvailable(StorageDisk left, StorageDisk right) => right.Available.CompareTo(left.Available);

        public override int GetHashCode() => (X << 5) | Y;
        public override string ToString()
        {
            return $"{X,2}, {Y,2} - Used {Used,3}T / {Size,3}T - Available {Available,3}T";
        }
    }
}
