namespace AdventOfCode.Utilities;

public interface IHasY
{
    int Y { get; set; }

    IHasY InvertY { get; }
}
