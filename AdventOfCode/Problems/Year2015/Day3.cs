using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2015;

public class Day3 : Problem<int>
{
    private DeliveryDirections directions;
    private AssistedDeliveryDirections assistedDirections;

    public override int SolvePart1()
    {
        return directions.PresentedHouses;
    }
    public override int SolvePart2()
    {
        return assistedDirections.PresentedHouses;
    }

    protected override void ResetState()
    {
        directions = null;
    }
    protected override void LoadState()
    {
        directions = new(FileContents);
        assistedDirections = new(FileContents);
    }

    private class AssistedDeliveryDirections : DeliveryDirections
    {
        public AssistedDeliveryDirections(string s)
            : base(s) { }

        protected override void AnalyzeDirections()
        {
            Location2D santaLocation = (0, 0);
            Location2D roboLocation = (0, 0);

            GivenPresents.Add(santaLocation);
            for (int i = 0; i < Directions.Length; i += 2)
            {
                var santaDirection = Directions[i];
                var roboDirection = Directions[i + 1];

                santaLocation.Forward(santaDirection);
                roboLocation.Forward(roboDirection);

                GivenPresents.Add(santaLocation);
                GivenPresents.Add(roboLocation);
            }
        }
    }

    private class DeliveryDirections
    {
        protected Direction[] Directions;
        protected ValueCounterDictionary<Location2D> GivenPresents = new();

        public int PresentedHouses => GivenPresents.Count;

        public DeliveryDirections(string s)
        {
            Directions = s.Select(CommonParsing.ParseDirectionArrow).ToArray();
            AnalyzeDirections();
        }

        protected virtual void AnalyzeDirections()
        {
            Location2D location = (0, 0);
            GivenPresents.Add(location);
            foreach (var d in Directions)
            {
                location.Forward(d);
                GivenPresents.Add(location);
            }
        }
    }
}
