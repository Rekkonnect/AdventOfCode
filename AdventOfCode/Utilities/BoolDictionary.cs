namespace AdventOfCode.Utilities
{
    // TODO: Rename
    public class BoolDictionary
    {
        private int offset;
        private bool[] values;

        public int Count { get; private set; }

        public BoolDictionary(int start, int end)
        {
            offset = start;
            values = new bool[end - start + 1];
        }

        public void Unset(int index) => this[index] = false;
        public void Set(int index) => this[index] = true;

        public void Reset()
        {
            for (int i = 0; i < values.Length; i++)
                values[i] = false;
            Count = 0;
        }

        public bool this[int index]
        {
            get => values[index - offset];
            set
            {
                var old = this[index];
                if (old == value)
                    return;

                values[index - offset] = value;

                if (old && !value)
                    Count--;
                if (!old && value)
                    Count++;
            }
        }
    }
}
