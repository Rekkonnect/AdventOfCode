using AdventOfCode.Functions;
using Garyon.Objects;

namespace AdventOfCode.Problems.Year2015;

public class Day9 : Problem<int>
{
    private LocationSystem locations;

    public override int SolvePart1()
    {
        return locations.GetShortestDistance();
    }
    public override int SolvePart2()
    {
        return locations.GetLongestDistance();
    }

    protected override void ResetState()
    {
        locations = null;
    }
    protected override void LoadState()
    {
        locations = new(ParsedFileLines(Trip.Parse));
    }

    private class LocationSystem
    {
        private DestinationDictionary locations = new();

        public LocationSystem(IEnumerable<Trip> destinationTrips)
        {
            foreach (var trip in destinationTrips)
            {
                locations[trip.LocationA].RegisterTrip(trip);
                locations[trip.LocationB].RegisterTrip(trip);
            }
        }

        public int GetShortestDistance()
        {
            return GetDesiredDistance(ComparisonResult.Less, int.MaxValue);
        }
        public int GetLongestDistance()
        {
            return GetDesiredDistance(ComparisonResult.Greater, int.MinValue);
        }
        private int GetDesiredDistance(ComparisonResult desiredComparison, int startingDistance)
        {
            var destinations = locations.Values.ToAvailabilityDictionary();
            int desiredDistance = startingDistance;

            foreach (var d in locations.Values)
                Iterate(d, 0, locations.Count - 1);

            return desiredDistance;

            // It's sad that the best algorithm is a dynamic one with time complexity O(n^2 * 2^n)
            // With the brute force one being O(n!), being the implementation below
            void Iterate(Destination currentDestination, int currentDistance, int remaining)
            {
                if (remaining == 0)
                {
                    var result = currentDistance.GetComparisonResult(desiredDistance);
                    if (result == desiredComparison)
                        desiredDistance = currentDistance;

                    return;
                }

                destinations[currentDestination] = false;
                foreach (var availability in destinations)
                {
                    if (!availability.Value)
                        continue;

                    var targetDestination = availability.Key;
                    var trip = locations[currentDestination.Name].GetTrip(targetDestination);
                    Iterate(targetDestination, currentDistance + trip.Distance, remaining - 1);
                }
                destinations[currentDestination] = true;
            }
        }
    }

    private class Destination
    {
        private FlexibleDictionary<string, Trip> trips = new();

        public string Name { get; }

        public Destination(string name)
        {
            Name = name;
        }

        public void RegisterTrip(Trip trip)
        {
            var otherLocation = trip.LocationB;
            if (otherLocation == Name)
                otherLocation = trip.LocationA;
            trips.Add(otherLocation, trip);
        }

        public Trip GetTrip(Destination destination) => trips[destination.Name];
        public Trip GetTrip(string destinationName) => trips[destinationName];
    }

    private record Trip(string LocationA, string LocationB, int Distance)
    {
        private static readonly Regex tripPattern = new(@"(\w*) to (\w*) = (\d*)", RegexOptions.Compiled);

        public static Trip Parse(string s)
        {
            var match = tripPattern.Match(s);
            var startingDestination = match.Groups[1].Value;
            var targetDestination = match.Groups[2].Value;
            int distance = int.Parse(match.Groups[3].Value);
            return new(startingDestination, targetDestination, distance);
        }

        public override string ToString() => $"{LocationA} to {LocationB} = {Distance}";
    }

    private class DestinationDictionary : FlexibleDictionary<string, Destination>
    {
        public override Destination this[string key]
        {
            get
            {
                if (!Dictionary.ContainsKey(key))
                    Dictionary.Add(key, new(key));
                return Dictionary[key];
            }
        }
    }
}
