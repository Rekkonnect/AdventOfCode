namespace AdventOfCode.Utilities
{
    public interface IHasX
    {
        int X { get; set; }

        IHasX InvertX { get; }
    }
}
