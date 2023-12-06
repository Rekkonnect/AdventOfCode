namespace AdventOfCode.Problems.Year2023;

public class Day6 : Problem<long>
{
    private ImmutableArray<Race> _races;
    private Race _realRace;

    public override long SolvePart1()
    {
        long product = 1;
        for (int i = 0; i < _races.Length; i++)
        {
            product *= _races[i].RecordBeatingWays();
        }
        return product;
    }
    public override long SolvePart2()
    {
        return _realRace.RecordBeatingWays();
    }

    protected override void LoadState()
    {
        var lines = FileLines;
        lines[0].AsSpan().SplitOnce(':', out _, out var timesSpan);
        lines[1].AsSpan().SplitOnce(':', out _, out var distancesSpan);
        var times = Parsing.ParseAllInt32(timesSpan);
        var distances = Parsing.ParseAllInt32(distancesSpan);

        var races = ImmutableArray.CreateBuilder<Race>(times.Length);
        for (int i = 0; i < times.Length; i++)
        {
            var race = new Race(times[i], distances[i]);
            races.Add(race);
        }
        _races = races.ToImmutable();

        _realRace = GetRealRace();
    }
    protected override void ResetState()
    {
        _races = default;
    }

    private Race GetRealRace()
    {
        long time = 0;
        long distance = 0;
        for (int i = 0; i < _races.Length; i++)
        {
            var race = _races[i];
            var rtime = (int)race.Time;
            var rdistance = (int)race.Distance;
            int timeDigits = rtime.GetDigitCount();
            int distanceDigits = rdistance.GetDigitCount();

            long timeMultiplier = MathFunctions.MultipleOfTen(timeDigits);
            long distanceMultiplier = MathFunctions.MultipleOfTen(distanceDigits);

            time *= timeMultiplier;
            distance *= distanceMultiplier;

            time += rtime;
            distance += rdistance;
        }
        return new(time, distance);
    }

    private readonly record struct Race(long Time, long Distance)
    {
        public long RecordBeatingWays()
        {
            // Velocity = StartHold
            // TravelTime = Time - StartHold
            // TravelDistance = TravelTime * Velocity
            // TravelDistance = (Time - StartHold) * StartHold
            // TravelDistance = Time * StartHold - StartHold * StartHold
            // Find (Distance = Time * StartHold - StartHold * StartHold)
            // - StartHold^2 + Time * StartHold - Distance = 0
            // StartHold^2 - Time * StartHold + Distance = 0
            // a = 1 | b = -Time | c = Distance
            // D = b^2 - 4ac
            // D = Time^2 - 4 * Distance
            // StartHold = (-b +/- sqrt(D)) / 2a
            // StartHold = (Time +/- sqrt(D)) / 2

            double d = Time * Time - 4 * Distance;
            var sqrtd = Math.Sqrt(d);

            var startHoldUpper = (Time + sqrtd) / 2;
            var floorStartHoldUpper = (long)Math.Floor(startHoldUpper);
            if (floorStartHoldUpper == startHoldUpper)
                floorStartHoldUpper--;

            var startHoldLower = (Time - sqrtd) / 2;
            var ceilingStartHoldLower = (long)Math.Ceiling(startHoldLower);
            if (ceilingStartHoldLower == startHoldLower)
                ceilingStartHoldLower++;
            return floorStartHoldUpper - ceilingStartHoldLower + 1;
        }
    }
}
