namespace AdventOfCode.Utilities.TwoDimensions;

public class AdjacentValueSlots<T>
{
    private T top, bottom, left, right;
    private byte availableSlots;

    // Should we not get something like property models?
    /*

    private prop model T Adjacent(ref T field, Direction respectiveDirection)
    {
        // As an added bonus, make the field argument implicit if internally used
        get => field;
        set
        {
            field = value;
            SetAvailableSlot(respectiveDirection);
        }
    }
    private Adjacent T Top(ref top, Direction.Up);
    private Adjacent T Bottom(ref bottom, Direction.Down);
    private Adjacent T Left(ref left, Direction.Left);
    private Adjacent T Right(ref right, Direction.Right);

    */

    public T Top
    {
        get => top;
        set
        {
            top = value;
            SetAvailableSlot(Direction.Up);
        }
    }
    public T Bottom
    {
        get => bottom;
        set
        {
            bottom = value;
            SetAvailableSlot(Direction.Down);
        }
    }
    public T Left
    {
        get => left;
        set
        {
            left = value;
            SetAvailableSlot(Direction.Left);
        }
    }
    public T Right
    {
        get => right;
        set
        {
            right = value;
            SetAvailableSlot(Direction.Right);
        }
    }

    public AdjacentValueSlots<T> Clone()
    {
        return new()
        {
            top = top,
            bottom = bottom,
            left = left,
            right = right,
            availableSlots = availableSlots,
        };
    }
    public void Reset()
    {
        availableSlots = 0;
    }

    private T ValueOnSlot(Direction direction)
    {
        return direction switch
        {
            Direction.Up => top,
            Direction.Down => bottom,
            Direction.Left => left,
            Direction.Right => right,
        };
    }
    public T ValueOnSlot(Direction direction, out bool hasValue)
    {
        hasValue = HasSlot(direction);
        return ValueOnSlot(direction);
    }

    public bool HasSlot(Direction direction) => (availableSlots & SlotMask(direction)) != 0;
    private void SetAvailableSlot(Direction direction)
    {
        availableSlots |= SlotMask(direction);
    }
    private static byte SlotMask(Direction direction) => (byte)(1 << (int)direction);
}
