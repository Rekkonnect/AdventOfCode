using AdventOfCode.Utilities;
using AdventOfCSharp;
using Garyon.Extensions;
using System.Linq;

namespace AdventOfCode.Problems.Year2016;

public class Day19 : Problem<int>
{
    private int elves;

    public override int SolvePart1()
    {
        return new ElfStealingGame(elves).GetWinningElfPart1();
    }
    public override int SolvePart2()
    {
        return new ElfStealingGame(elves).GetWinningElfPart2();
    }

    protected override void LoadState()
    {
        elves = FileContents.ParseInt32();
    }

    private class ElfStealingGame
    {
        private CircularLinkedList<int> linkedList;

        public ElfStealingGame(int elves)
        {
            // This will be awful to start with
            // I wish I could avoid unnecessary allocations without copy-pasting the entire implementation
            linkedList = new(Enumerable.Range(1, elves));
        }

        public int GetWinningElfPart1()
        {
            var current = linkedList.Head;
            while (linkedList.Count > 1)
            {
                linkedList.Remove(current.Next);
                current = current.Next;
            }

            return current.Value;
        }
        public int GetWinningElfPart2()
        {
            var current = linkedList.GetNode(linkedList.Count / 2);
            bool skipNext = linkedList.Count % 2 is 1;
            while (linkedList.Count > 1)
            {
                var next = current.Next;
                if (skipNext)
                    next = next.Next;

                linkedList.Remove(current);
                current = next;

                skipNext = !skipNext;
            }

            return current.Value;
        }
    }
}
