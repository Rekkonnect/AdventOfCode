using System.Collections.Generic;

namespace AdventOfCode.Utilities.TwoDimensions
{
    public class HorizontalHexTileSetDirections : HexTileSetDirections<HorizontalHexSide>
    {
        public HorizontalHexTileSetDirections(IEnumerable<HorizontalHexSide> hexTileSetDirections)
            : base(hexTileSetDirections) { }

        public static HorizontalHexTileSetDirections ParseDelimited(string raw, string delimiter)
        {
            return new(ParseDelimitedDirections(raw, delimiter, HorizontalHexSideFunctions.ParseHexSide));
        }
        public static HorizontalHexTileSetDirections ParseConcatenated(string raw)
        {
            return new(ParseConcatenatedDirections(raw, HorizontalHexSideFunctions.ParseHexSide));
        }
    }
}
