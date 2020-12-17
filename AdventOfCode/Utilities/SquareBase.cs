namespace AdventOfCode.Utilities
{
    // This should be probably inherit Grid2D
    public class SquareBase<T>
    {
        protected T[,] Values;
        public int Size { get; }

        public SquareBase(int size, T defaultValue)
        {
            Size = size;
            Values = new T[size, size];

            if (!defaultValue.Equals(default(T)))
            {
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                        Values[i, j] = defaultValue;
            }
        }
        public SquareBase(SquareBase<T> other)
        {
            for (int i = 0; i < other.Size; i++)
                for (int j = 0; j < other.Size; j++)
                    Values[i, j] = other.Values[i, j];
        }

        public virtual T this[int x, int y]
        {
            get => Values[x, y];
            set => Values[x, y] = value;
        }
    }
}
