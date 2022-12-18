using AdventOfCode.Utilities.ThreeDimensions;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems;

public static class CommonParsing
{
    public static Location2D ParseLocation2D(SpanString span)
    {
        span.SplitOnceSpan(',', out var xSpan, out var ySpan);
        int x = xSpan.ParseInt32();
        int y = ySpan.TrimStart().ParseInt32();
        return (x, y);
    }
    public static Location3D ParseLocation3D(SpanString raw)
    {
        raw.SplitOnceSpan(',', out var xSpan, out var yzSpan);
        yzSpan.SplitOnceSpan(',', out var ySpan, out var zSpan);

        int x = xSpan.ParseInt32();
        int y = ySpan.TrimStart().ParseInt32();
        int z = zSpan.TrimStart().ParseInt32();
        return (x, y, z);
    }
}
