namespace AdventOfCode.Utilities
{
    public record MinMaxResult<T>(T Min, T Max)
    {
        public static MinMaxResult<T> Default => new MinMaxResult<T>(default, default);

        public void Deconstruct(out T min, out T max)
        {
            min = Min;
            max = Max;
        }
    }
}