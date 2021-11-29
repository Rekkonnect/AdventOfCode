using System.Runtime.CompilerServices;

namespace AdventOfCode.Utilities.TwoDimensions;

[SkipLocalsInit]
public class LocationWithDirection
{
    private Location2D location;
    private DirectionalLocation facedDirection;

    public Location2D Location
    {
        get => location;
        set => location = value;
    }
    public DirectionalLocation FacedDirection
    {
        get => facedDirection;
        set => facedDirection = value;
    }

    public LocationWithDirection(Direction startingDirection)
        : this(Location2D.Zero, new DirectionalLocation(startingDirection)) { }
    public LocationWithDirection(DirectionalLocation startingDirection)
        : this(Location2D.Zero, startingDirection) { }
    public LocationWithDirection(Location2D startingLocation, Direction startingDirection)
        : this(startingLocation, new DirectionalLocation(startingDirection)) { }
    public LocationWithDirection(Location2D startingLocation, DirectionalLocation startingDirection)
    {
        location = startingLocation;
        facedDirection = startingDirection;
    }

    public void TurnLeft() => facedDirection.TurnLeft();
    public void TurnRight() => facedDirection.TurnRight();

    public void TurnLeft(int times) => facedDirection.TurnLeft(times);
    public void TurnRight(int times) => facedDirection.TurnRight(times);

    public override string ToString() => $"{Location} - {FacedDirection}";
}
