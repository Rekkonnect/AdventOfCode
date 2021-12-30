namespace AdventOfCode.Utilities;

public class LineStringBuilder
{
    private readonly List<StringBuilder> lines = new();

    public int LineCount => lines.Count;

    public void AddLine(string line)
    {
        lines.Add(new(line));
    }
    public void AddRepeatedLine(string line, int times)
    {
        for (int i = 0; i < times; i++)
            AddLine(line);
    }
    public void AddLines(string[] lines)
    {
        foreach (var line in lines)
            AddLine(line);
    }

    public void Clear()
    {
        lines.Clear();
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        // TODO: AppendLines extension in Garyon
        foreach (var line in lines)
            builder.AppendLine(line);

        return builder.RemoveLast().ToString();
    }

    public char this[int line, int column]
    {
        get => lines[line][column];
        set => lines[line][column] = value;
    }
}
