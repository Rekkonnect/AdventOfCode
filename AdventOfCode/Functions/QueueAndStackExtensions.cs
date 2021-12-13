namespace AdventOfCode.Functions;

// How did these miss being in Garyon?
public static class QueueAndStackExtensions
{
    public static IEnumerable<T> PopAll<T>(this Stack<T> stack) => stack.PopRange(stack.Count);
    public static IEnumerable<T> DequeueAll<T>(this Queue<T> queue) => queue.DequeueRange(queue.Count);
}
