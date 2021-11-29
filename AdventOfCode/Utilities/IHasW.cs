namespace AdventOfCode.Utilities;

public interface IHasW
{
    int W { get; set; }

    IHasW InvertW { get; }
}
