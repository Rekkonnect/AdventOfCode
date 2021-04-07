using Garyon.DataStructures;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2015
{
    public class Day24 : Problem<ulong>
    {
        private int[] packageWeights;

        public override ulong SolvePart1()
        {
            int targetWeight = packageWeights.Sum() / 3;
            int minGroupSize = GetMinGroupSize(targetWeight);

            // Start finding the groups
            int group1Size = minGroupSize;

            PackageGroup bestGroup = null;
            ulong bestGroupQE = ulong.MaxValue;

            var packageWeightsSet = new HashSet<int>(packageWeights);
            var groupDictionary = new FlexibleDictionary<int, HashSet<int>[]>();

            while (true)
            {
                foreach (var group1 in GetGroups(group1Size))
                {
                    var remainingWeights = new HashSet<int>(packageWeightsSet);
                    remainingWeights.ExceptWith(group1);
                    int maxGroup2Size = remainingWeights.Count - minGroupSize;

                    for (int group2Size = minGroupSize; group2Size < maxGroup2Size; group2Size++)
                    {
                        if (GetGroups(group2Size).Any(group => !remainingWeights.Overlaps(group)))
                        {
                            EvaluateBestGroup(new(group1), ref bestGroup, ref bestGroupQE);
                            break;
                        }
                    }
                }

                if (bestGroup is not null)
                    return bestGroupQE;

                group1Size++;
            }

            IEnumerable<HashSet<int>> GetGroups(int size)
            {
                if (!groupDictionary.ContainsKey(size))
                    groupDictionary[size] = FindGroups(size, targetWeight, packageWeights);
                return groupDictionary[size];
            }
        }
        public override ulong SolvePart2()
        {
            // 10.3+ seconds it is
            // It used to be 94s, then 52s, then 36s, then 11.7s, I think I can live with that much optimization
            // Although I suspect there is some sort of genius mathematical optimization due to the input being full of primes
            int targetWeight = packageWeights.Sum() / 4;
            int minGroupSize = GetMinGroupSize(targetWeight);

            int group1Size = minGroupSize;

            PackageGroup bestGroup = null;
            ulong bestGroupQE = ulong.MaxValue;

            var packageWeightsSet = new HashSet<int>(packageWeights);
            var groupDictionary = new FlexibleDictionary<int, HashSet<int>[]>();

            while (true)
            {
                foreach (var group1 in GetGroups(group1Size))
                {
                    var remainingGroup2Weights = new HashSet<int>(packageWeightsSet);
                    remainingGroup2Weights.ExceptWith(group1);
                    int maxGroup2Size = remainingGroup2Weights.Count - minGroupSize;

                    for (int group2Size = minGroupSize; group2Size < maxGroup2Size; group2Size++)
                    {
                        foreach (var group2 in GetGroups(group2Size))
                        {
                            if (group1 == group2)
                                continue;

                            if (group1.Overlaps(group2))
                                continue;
                            
                            var remainingGroup3Weights = new HashSet<int>(remainingGroup2Weights);
                            remainingGroup3Weights.ExceptWith(group2);
                            int maxGroup3Size = remainingGroup3Weights.Count - minGroupSize;

                            for (int group3Size = minGroupSize; group3Size < maxGroup3Size; group3Size++)
                            {
                                if (GetGroups(group3Size).Any(group => !remainingGroup3Weights.Overlaps(group)))
                                {
                                    EvaluateBestGroup(new(group1), ref bestGroup, ref bestGroupQE);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (bestGroup is not null)
                    return bestGroupQE;

                group1Size++;
            }

            IEnumerable<HashSet<int>> GetGroups(int size)
            {
                if (!groupDictionary.ContainsKey(size))
                    groupDictionary[size] = FindGroups(size, targetWeight, packageWeights);
                return groupDictionary[size];
            }
        }

        private int GetMinGroupSize(int targetWeight)
        {
            int currentBestWeight = 0;

            int minGroupSize = 1;
            for (; currentBestWeight < targetWeight; minGroupSize++)
                currentBestWeight += packageWeights[^minGroupSize];
            return minGroupSize - 1;
        }

        private static void EvaluateBestGroup(PackageGroup packageGroup, ref PackageGroup bestGroup, ref ulong bestGroupQE)
        {
            ulong packageGroupQE = packageGroup.QuantumEntanglement;
            if (packageGroupQE < bestGroupQE)
            {
                bestGroup = packageGroup;
                bestGroupQE = packageGroupQE;
            }
        }

        private static HashSet<int>[] FindGroups(int size, int targetWeight, int[] packageWeights)
        {
            var result = new List<HashSet<int>>(size * packageWeights.Length);

            int[] weights = new int[size];
            FindGroups(packageWeights.Length - 1, 0, targetWeight);

            return result.ToArray();

            void FindGroups(int maxIndex, int depth, int remaining)
            {
                if (depth == size - 1)
                {
                    if (Array.BinarySearch(packageWeights, 0, maxIndex, remaining) < 0)
                        return;

                    weights[depth] = remaining;
                    result.Add(new(weights));
                    return;
                }

                for (int i = maxIndex; i >= size - depth; i--)
                {
                    weights[depth] = packageWeights[i];
                    FindGroups(i - 1, depth + 1, remaining - weights[depth]);
                }
            }
        }

        protected override void LoadState()
        {
            // Already sorted
            packageWeights = FileNumbersInt32;
        }
        protected override void ResetState()
        {
            packageWeights = null;
        }

        private class PackageGroup
        {
            public IEnumerable<int> PackageWeights { get; }

            public ulong QuantumEntanglement
            {
                get
                {
                    ulong result = 1;
                    foreach (int weight in PackageWeights)
                        result *= (ulong)weight;
                    return result;
                }
            }

            public PackageGroup(IEnumerable<int> packageWeights) => PackageWeights = packageWeights;
        }
    }
}
