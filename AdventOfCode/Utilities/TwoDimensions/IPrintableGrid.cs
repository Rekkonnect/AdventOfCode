namespace AdventOfCode.Utilities.TwoDimensions;

public interface IPrintableGrid<T>
{
    public abstract char GetPrintableCharacter(T value);

    public void PrintGrid();
}
