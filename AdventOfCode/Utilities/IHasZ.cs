namespace AdventOfCode.Utilities;

public interface IHasZ
{
    int Z { get; set; }

    IHasZ InvertZ { get; }
}
