namespace AdventOfCode.Utilities
{
    public class BoolLookupTable : LookupTable<bool>
    {
        public int Count { get; private set; }

        public BoolLookupTable(int start, int end)
            : base(start, end) { }

        public void Unset(int index) => this[index] = false;
        public void Set(int index) => this[index] = true;

        public void Reset()
        {
            for (int i = 0; i < Values.Length; i++)
                Values[i] = false;
            Count = 0;
        }

        public override bool this[int index]
        {
            set
            {
                var old = this[index];
                if (old == value)
                    return;

                Values[index - Offset] = value;

                if (old && !value)
                    Count--;
                if (!old && value)
                    Count++;
            }
        }
    }
}
