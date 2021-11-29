using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2015;

public class Day17 : Problem<int>
{
    private ContainerCollection containers;
    private int combinationCount;
    private int smallestCombinationCount;

    public override int SolvePart1()
    {
        return combinationCount;
    }
    public override int SolvePart2()
    {
        return smallestCombinationCount;
    }

    protected override void ResetState()
    {
        containers = null;
    }
    protected override void LoadState()
    {
        containers = new(FileNumbersInt32);
        containers.IterateCombinations(150, out combinationCount, out smallestCombinationCount);
    }

    private class ContainerCollection
    {
        private int[] containers;

        public ContainerCollection(IEnumerable<int> containerSizes)
        {
            containers = containerSizes.ToArray();
        }

        public void IterateCombinations(int size, out int combinationCount, out int smallestCombinationCount)
        {
            int combinations = 0;
            int smallestCombination = int.MaxValue;
            int smallestCount = 0;

            for (int i = 0; i < containers.Length; i++)
                Iterate(i, size - containers[i], 1);

            combinationCount = combinations;
            smallestCombinationCount = smallestCount;

            void Iterate(int index, int remainingSize, int depth)
            {
                if (remainingSize < 0)
                    return;

                if (remainingSize == 0)
                {
                    combinations++;

                    if (depth < smallestCombination)
                    {
                        smallestCombination = depth;
                        smallestCount = 1;
                    }
                    else if (depth == smallestCombination)
                        smallestCount++;

                    return;
                }

                depth++;
                for (int next = index + 1; next < containers.Length; next++)
                    Iterate(next, remainingSize - containers[next], depth);
            }
        }
    }

    private class Availability<T>
    {
        public T Value { get; }
        public bool Available { get; set; }

        public Availability(T value, bool available = true)
        {
            Value = value;
            Available = available;
        }
    }
}
