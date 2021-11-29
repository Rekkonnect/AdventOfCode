namespace AdventOfCode.Utilities.TwoDimensions;

public static class VerticalHexSideFunctions
{
    public static Location2D GetOffset(this VerticalHexSide side)
    {
        return side switch
        {
            VerticalHexSide.East => (2, 0),
            VerticalHexSide.SouthEast => (1, -1),
            VerticalHexSide.SouthWest => (-1, -1),
            VerticalHexSide.West => (-2, 0),
            VerticalHexSide.NorthWest => (-1, 1),
            VerticalHexSide.NorthEast => (1, 1),
            _ => default,
        };
    }

    public static VerticalHexSide ParseHexSide(string originalString)
    {
        int index = 0;
        return ParseHexSide(originalString, ref index);
    }
    public static VerticalHexSide ParseHexSide(string originalString, ref int index)
    {
        char c0 = originalString[index];
        if (c0 is 's' or 'n')
        {
            index++;
            char c1 = originalString[index];
            return (c0, c1) switch
            {
                ('s', 'e') => VerticalHexSide.SouthEast,
                ('s', 'w') => VerticalHexSide.SouthWest,
                ('n', 'e') => VerticalHexSide.NorthEast,
                ('n', 'w') => VerticalHexSide.NorthWest,
            };
        }
        return c0 switch
        {
            'e' => VerticalHexSide.East,
            'w' => VerticalHexSide.West,
        };
    }
}
