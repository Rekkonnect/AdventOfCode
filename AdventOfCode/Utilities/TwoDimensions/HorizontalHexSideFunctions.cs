namespace AdventOfCode.Utilities.TwoDimensions;

public static class HorizontalHexSideFunctions
{
    public static Location2D GetOffset(this HorizontalHexSide side)
    {
        return side switch
        {
            HorizontalHexSide.South => (0, -2),
            HorizontalHexSide.SouthEast => (1, -1),
            HorizontalHexSide.SouthWest => (-1, -1),
            HorizontalHexSide.North => (0, 2),
            HorizontalHexSide.NorthWest => (-1, 1),
            HorizontalHexSide.NorthEast => (1, 1),
            _ => default,
        };
    }

    public static HorizontalHexSide ParseHexSide(string originalString)
    {
        int index = 0;
        return ParseHexSide(originalString, ref index);
    }
    public static HorizontalHexSide ParseHexSide(string originalString, ref int index)
    {
        char c0 = originalString[index];

        if (index + 1 >= originalString.Length)
            goto invalidSecond;

        char c1 = originalString[index + 1];
        if (c1 is 's' or 'n')
            goto invalidSecond;

        index++;

        return (c0, c1) switch
        {
            ('s', 'e') => HorizontalHexSide.SouthEast,
            ('s', 'w') => HorizontalHexSide.SouthWest,
            ('n', 'e') => HorizontalHexSide.NorthEast,
            ('n', 'w') => HorizontalHexSide.NorthWest,
        };

    invalidSecond:
        return c0 switch
        {
            's' => HorizontalHexSide.South,
            'n' => HorizontalHexSide.North,
        };
    }
}
