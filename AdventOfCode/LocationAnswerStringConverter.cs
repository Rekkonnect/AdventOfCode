using AdventOfCode.Utilities;
using AdventOfCSharp;

namespace AdventOfCode;

public sealed class LocationAnswerStringConverter : AnswerStringConverter<ILocation>
{
    public override string Convert(ILocation value)
    {
        var result = value.GetAtDimension(0).Value.ToString();

        int dimension = 1;
        while (true)
        {
            int? dimensionValue = value.GetAtDimension(dimension);
            if (dimensionValue is null)
                break;

            result += $",{dimensionValue}";
            dimension++;
        }

        return result;
    }
}
