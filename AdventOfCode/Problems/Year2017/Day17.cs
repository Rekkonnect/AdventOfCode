using AdventOfCode.Utilities;
using Garyon.Extensions;
using System;

namespace AdventOfCode.Problems.Year2017
{
    public class Day17 : Problem<int>
    {
        private Spinlock spinlock;

        public override int SolvePart1()
        {
            return spinlock.GetBuffer(2017).NodeOf(2017).Next.Value;
        }
        public override int SolvePart2()
        {
            return spinlock.GetValueAfterZero(50000000);
        }

        protected override void LoadState()
        {
            spinlock = new(FileContents.ParseInt32());
        }
        protected override void ResetState()
        {
            spinlock = null;
        }

        private class Spinlock
        {
            public int Step { get; }

            public Spinlock(int step)
            {
                Step = step;
            }

            public CircularLinkedList<int> GetBuffer(int max)
            {
                var buffer = new CircularLinkedList<int>(0);
                var current = buffer.Head;

                for (int i = 1; i <= max; i++)
                {
                    buffer.InsertAfter(current = buffer.GetNext(current, Step), i);
                    current = current.Next;
                }

                return buffer;
            }

            public int GetValueAfterZero(int max)
            {
                int secondValue = 1;
                int count = 2;
                int currentIndex = 1;

                while (true)
                {
                    if (count >= Step)
                    {
                        // Skip the entire iteration of the entire list if past 0
                        int remainingElements = count - 1 - currentIndex;
                        int addedElements = Math.DivRem(remainingElements, Step, out int remainder) + 1;
                        int nextIndex = Step - remainder;

                        count += addedElements;
                        currentIndex = nextIndex;
                    }
                    else
                    {
                        // Simply loop through the list and consider adding the element
                        currentIndex += Step;
                        currentIndex %= count;

                        count++;
                        currentIndex++;
                    }

                    if (count > max)
                        return secondValue;

                    // Must have adjusted count and currentIndex
                    if (currentIndex is 1)
                        secondValue = count - 1;
                }
            }
        }
    }
}
