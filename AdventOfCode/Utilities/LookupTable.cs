namespace AdventOfCode.Utilities
{
    public class LookupTable<T>
    {
        protected readonly int Offset;
        protected readonly T[] Values;

        public LookupTable(int start, int end)
        {
            Offset = start;
            Values = new T[end - start + 1];
        }
        public virtual T this[int index]
        {
            get => Values[index - Offset];
            set => Values[index - Offset] = value;
        }
    }
}
