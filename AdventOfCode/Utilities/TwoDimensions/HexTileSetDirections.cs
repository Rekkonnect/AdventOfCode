namespace AdventOfCode.Utilities.TwoDimensions;

public abstract class HexTileSetDirections<T>
    where T : struct, Enum
{
    private readonly List<T> directions;
    private HexTileLocation furthestLocation;

    public HexTileLocation Location { get; private init; }
    public IEnumerable<T> Directions => directions;

    public HexTileLocation FurthestLocation => furthestLocation;

    public HexTileSetDirections(IEnumerable<T> hexTileSetDirections)
    {
        directions = new(hexTileSetDirections);
        Location = HexTileLocation.WithFurthestFromCenter(hexTileSetDirections, out furthestLocation);
    }

    public override string ToString() => Location.ToString();

    protected static List<T> ParseDelimitedDirections(string rawDirections, string delimiter, HexSideParser parser)
    {
        var split = rawDirections.Split(delimiter);
        var directions = new List<T>(split.Length);
        foreach (string side in split)
            directions.Add(parser(side));
        return directions;
    }
    protected static List<T> ParseConcatenatedDirections(string rawDirections, HexSideConcatenatedParser parser)
    {
        var directions = new List<T>();
        for (int i = 0; i < rawDirections.Length; i++)
            directions.Add(parser(rawDirections, ref i));
        return directions;
    }

    protected delegate T HexSideParser(string side);
    protected delegate T HexSideConcatenatedParser(string side, ref int index);
}
