namespace AdventOfCode.Utilities.TwoDimensions;

public class VerticalHexTileSetDirections : HexTileSetDirections<VerticalHexSide>
{
    public VerticalHexTileSetDirections(IEnumerable<VerticalHexSide> hexTileSetDirections)
        : base(hexTileSetDirections) { }

    public static VerticalHexTileSetDirections ParseDelimited(string raw, string delimiter)
    {
        return new(ParseDelimitedDirections(raw, delimiter, VerticalHexSideFunctions.ParseHexSide));
    }
    public static VerticalHexTileSetDirections ParseConcatenated(string raw)
    {
        return new(ParseConcatenatedDirections(raw, VerticalHexSideFunctions.ParseHexSide));
    }
}
