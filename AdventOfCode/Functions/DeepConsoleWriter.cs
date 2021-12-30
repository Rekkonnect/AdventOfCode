using AdventOfCSharp.Utilities;

namespace AdventOfCode.Functions;

public class DeepConsoleWriter
{
    private readonly Stack<int> tops = new();

    public DeepConsoleWriter()
    {
        PushLevel();
    }

    public void PushLevel()
    {
        tops.Push(Console.CursorTop + 1);
    }
    public void PopLevel()
    {
        if (tops.Count <= 1)
            return;

        int removed = tops.Pop();
        ConsolePrinting.ClearUntilCursorReposition(0, tops.Peek());
    }

    public void Write(object value)
    {
        Console.CursorTop = tops.Peek();
        Console.Write(value);
    }
    public void WriteLine(object value)
    {
        Write($"{value}\n");
    }
}
