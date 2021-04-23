using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2017
{
    public class Day11 : Problem<int, int>
    {
        private HorizontalHexTileSetDirections directions;

        public override int SolvePart1()
        {
            return directions.Location.StepsFromCenter;
        }
        public override int SolvePart2()
        {
            return directions.FurthestLocation.StepsFromCenter;
        }

        protected override void LoadState()
        {
            directions = HorizontalHexTileSetDirections.ParseDelimited(FileContents, ",");
        }
        protected override void ResetState()
        {
            directions = null;
        }
    }
}
