using System.Collections.Generic;

namespace AdventOfCode.Functions
{
    public static class QueueExtensions
    {
        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> range)
        {
            foreach (var e in range)
                queue.Enqueue(e);
        }
        public static IEnumerable<T> DequeueRange<T>(this Queue<T> queue, int elements)
        {
            for (int i = 0; i < elements; i++)
                yield return queue.Dequeue();
        }
    }
}
