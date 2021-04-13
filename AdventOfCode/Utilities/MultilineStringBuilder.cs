using System.Collections.Generic;
using System.Text;

namespace AdventOfCode.Utilities
{
    public class MultilineStringBuilder
    {
        private readonly List<StringBuilder> builders;

        public int Count => builders.Count;

        public MultilineStringBuilder(int lines)
        {
            builders = new(lines);
            for (int i = 0; i < lines; i++)
                builders.Add(new());
        }

        public StringBuilder this[int index] => builders[index];

        public override string ToString() => string.Join("\n", builders);
    }
}
