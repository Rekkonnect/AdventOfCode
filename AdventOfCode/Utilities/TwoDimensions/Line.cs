namespace AdventOfCode.Utilities.TwoDimensions
{
    public class Line
    {
        public Location2D Location;
        public DirectionalLocation Direction;
        public int Movement;

        public Line PreviousLine;

        public Location2D LocationOffset => Movement * Direction.LocationOffset;
        public Location2D EndingLocation => Location + LocationOffset;

        public Location2D ConstantDimension => (LocationOffset.X == 0 ? Location.X : 0, LocationOffset.Y == 0 ? Location.Y : 0);

        public Line(DirectionalLocation direction, int movement, Location2D location = default) => (Location, Direction, Movement) = (location, direction, movement);

        public void GetLocationFromPreviousLine(Line previousLine) => Location = (PreviousLine = previousLine).EndingLocation;
        public int GetTotalStepsToStart() => PreviousLine?.GetTotalStepsToEnd() ?? 0;
        public int GetTotalStepsToEnd() => (PreviousLine?.GetTotalStepsToEnd() ?? 0) + Movement;
        public int GetTotalStepsToLocation(Location2D location) => GetTotalStepsToStart() + Location.ManhattanDistance(location);

        public bool IntersectsWith(Line other)
        {
            if (Location.IsCenter || other.Location.IsCenter)
                return false;
            if (LocationOffset * other.LocationOffset != (0, 0))
                return false;
            if (LocationOffset.X != 0)
                return IsWithinX(other) && other.IsWithinY(this);
            return IsWithinY(other) && other.IsWithinX(this);
        }
        public Location2D? GetIntersectionWith(Line other)
        {
            if (IntersectsWith(other))
                return ConstantDimension + other.ConstantDimension;
            return null;
        }

        public bool IsWithinX(Line other)
        {
            if (LocationOffset.X > 0)
                return Location.X < other.Location.X && other.Location.X < EndingLocation.X;
            return EndingLocation.X < other.Location.X && other.Location.X < Location.X;
        }
        public bool IsWithinY(Line other)
        {
            if (LocationOffset.Y > 0)
                return Location.Y < other.Location.Y && other.Location.Y < EndingLocation.Y;
            else
                return EndingLocation.Y < other.Location.Y && other.Location.Y < Location.Y;
        }

        public static Line Parse(string line)
        {
            var direction = DirectionalLocation.Parse(line[0]);
            int movement = int.Parse(line.Substring(1));
            return new Line(direction, movement);
        }

        public override string ToString() => $"{Location} {Direction.ToString()[0]}{Movement}";
    }
}
