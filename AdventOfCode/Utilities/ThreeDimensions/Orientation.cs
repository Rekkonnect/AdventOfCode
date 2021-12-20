using System.Collections.Specialized;

namespace AdventOfCode.Utilities.ThreeDimensions;

public enum AxesOrder
{
    XYZ,
    XZY,
    YXZ,
    YZX,
    ZXY,
    ZYX,
}

public enum Axis
{
    X,
    Y,
    Z,
}

public enum Sign
{
    Negative = -1,
    Zero = 0,
    Positive = 1,
}

public static class OrientationEnumeration
{
    private static readonly AxesOrder[] orders = new[]
    {
        AxesOrder.XYZ,
        AxesOrder.XZY,
        AxesOrder.YXZ,
        AxesOrder.YZX,
        AxesOrder.ZXY,
        AxesOrder.ZYX,
    };
    private const int signVariants = 0b111 + 1;

    public static Orientation[] EnumerateAllOrientations()
    {
        var result = new Orientation[orders.Length * signVariants];
        for (int i = 0; i < orders.Length; i++)
        {
            EnumerateAllOrientations(orders[i]).CopyTo(result, i * signVariants);
        }
        return result;
    }
    public static unsafe Orientation[] EnumerateAllOrientations(AxesOrder axesOrder)
    {
        var orientation = Orientation.AllPositiveXYZ with { AxesOrder = axesOrder };

        var result = new Orientation[signVariants]
        {
            orientation with { Signs = ( 1,  1,  1) },
            orientation with { Signs = ( 1,  1, -1) },
            orientation with { Signs = ( 1, -1,  1) },
            orientation with { Signs = ( 1, -1, -1) },
            orientation with { Signs = (-1,  1,  1) },
            orientation with { Signs = (-1,  1, -1) },
            orientation with { Signs = (-1, -1,  1) },
            orientation with { Signs = (-1, -1, -1) },
        };
        return result;
    }
}

// TODO: Bloat with equality
public struct Orientation
{
    private static readonly BitVector32.Section orientationSection;
    private static readonly int xBit, yBit, zBit;

    public static readonly Orientation AllPositiveXYZ = new();

    static Orientation()
    {
        orientationSection = BitVector32.CreateSection(0b111);
        xBit = BitVector32.CreateMask(1 << 2);
        yBit = BitVector32.CreateMask(xBit);
        zBit = BitVector32.CreateMask(yBit);
    }

    private BitVector32 bits;

    public AxesOrder AxesOrder
    {
        get => (AxesOrder)bits[orientationSection];
        set => bits[orientationSection] = (int)value;
    }
    public Sign XSign
    {
        get => SignFromBool(bits[xBit]);
        set => bits[xBit] = BoolFromSign(value);
    }
    public Sign YSign
    {
        get => SignFromBool(bits[yBit]);
        set => bits[yBit] = BoolFromSign(value);
    }
    public Sign ZSign
    {
        get => SignFromBool(bits[zBit]);
        set => bits[zBit] = BoolFromSign(value);
    }

    public Location3D Signs
    {
        get => new((int)XSign, (int)YSign, (int)ZSign);
        set
        {
            XSign = (Sign)value.X;
            YSign = (Sign)value.Y;
            ZSign = (Sign)value.Z;
        }
    }

    public Orientation(AxesOrder order)
        : this()
    {
        AxesOrder = order;
    }
    public Orientation(AxesOrder order, Location3D signs)
        : this(order)
    {
        Signs = signs;
    }
    public Orientation(AxesOrder order, bool xSign, bool ySign, bool zSign)
        : this(order)
    {
        XSign = SignFromBool(xSign);
        YSign = SignFromBool(ySign);
        ZSign = SignFromBool(zSign);
    }

    /*
     * The inner product of orientations A, B, resulting in R is the following:
     * - R.Signs = A.Signs * B.Signs
     * - R.AxesOrder[X] = B.AxesOrder[A.AxesOrder[X]]
     * - same for Y, Z
     */
    public Orientation InnerProduct(Orientation b)
    {
        var a = this;
        var signs = a.Signs * b.Signs;
        var aAxes = AxesOrderAsTuple(a.AxesOrder);
        var bAxes = AxesOrderAsTuple(b.AxesOrder);

        var rX = InnerAxalResult(aAxes, bAxes, Axis.X);
        var rY = InnerAxalResult(aAxes, bAxes, Axis.Y);
        var rZ = InnerAxalResult(aAxes, bAxes, Axis.Z);
        var rAxesOrder = AxesTupleAsOrder((rX, rY, rZ));
        return new(rAxesOrder, signs);

        static Axis InnerAxalResult((Axis, Axis, Axis) a, (Axis, Axis, Axis) b, Axis axis)
        {
            return ElementAtAxis(b, ElementAtAxis(a, axis));
        }
    }

    // Holy shit
    private static T ElementAtAxis<T>((T X, T Y, T Z) axalTuple, Axis axis)
    {
        return axis switch
        {
            Axis.X => axalTuple.X,
            Axis.Y => axalTuple.Y,
            Axis.Z => axalTuple.Z,
        };
    }
    private static AxesOrder AxesTupleAsOrder((Axis, Axis, Axis) axes)
    {
        return axes switch
        {
            (Axis.X, Axis.Y, Axis.Z) => AxesOrder.XYZ,
            (Axis.X, Axis.Z, Axis.Y) => AxesOrder.XZY,
            (Axis.Y, Axis.X, Axis.Z) => AxesOrder.YXZ,
            (Axis.Y, Axis.Z, Axis.X) => AxesOrder.YZX,
            (Axis.Z, Axis.X, Axis.Y) => AxesOrder.ZXY,
            (Axis.Z, Axis.Y, Axis.X) => AxesOrder.ZYX,
        };
    }
    private static (Axis, Axis, Axis) AxesOrderAsTuple(AxesOrder order)
    {
        return order switch
        {
            AxesOrder.XYZ => (Axis.X, Axis.Y, Axis.Z),
            AxesOrder.XZY => (Axis.X, Axis.Z, Axis.Y),
            AxesOrder.YXZ => (Axis.Y, Axis.X, Axis.Z),
            AxesOrder.YZX => (Axis.Y, Axis.Z, Axis.X),
            AxesOrder.ZXY => (Axis.Z, Axis.X, Axis.Y),
            AxesOrder.ZYX => (Axis.Z, Axis.Y, Axis.X),
        };
    }

    public override string ToString()
    {
        return $"{AxesOrder} - {Signs}";
    }

    private static bool BoolFromSign(Sign sign) => sign is Sign.Negative;
    private static Sign SignFromBool(bool value) => value ? Sign.Negative : Sign.Positive;
}
