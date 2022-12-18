namespace AdventOfCode.Utilities.TwoDimensions;

public enum Direction
{
    Up,
    Down,
    Left,
    Right,

    North = Up,
    South = Down,
    East = Right,
    West = Left,
}

public static class DirectionExtensions
{
    public static Direction Inverse(this Direction direction)
    {
        return direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Down => Direction.Up,
            Direction.Up => Direction.Down,
            _ => default,
        };
    }
}
