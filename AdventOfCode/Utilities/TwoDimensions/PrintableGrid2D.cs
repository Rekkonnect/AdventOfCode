﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AdventOfCode.Utilities.TwoDimensions
{
    public abstract class PrintableGrid2D<T> : Grid2D<T>, IPrintableGrid<T>
    {
        private Dictionary<T, char> printableCharacters;

        private PrintableGrid2D(int width, int height, T defaultValue, bool initializeValueCounters)
            : base(width, height, defaultValue, initializeValueCounters)
        {
            printableCharacters = GetPrintableCharacters();
        }

        public PrintableGrid2D(int both) : this(both, both, default) { }
        public PrintableGrid2D(int both, T defaultValue) : this(both, both, defaultValue) { }
        public PrintableGrid2D(int width, int height) : this(width, height, default) { }
        public PrintableGrid2D(int width, int height, T defaultValue) : this(width, height, defaultValue, true) { }
        public PrintableGrid2D(PrintableGrid2D<T> other)
            : base(other)
        {
            printableCharacters = other.printableCharacters;
        }

        public virtual void PrintGrid() => Console.WriteLine(ToString());

        protected abstract Dictionary<T, char> GetPrintableCharacters();
        protected virtual string FinalizeResultingString(StringBuilder builder) => builder.ToString();

        public sealed override string ToString()
        {
            var builder = new StringBuilder();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                    builder.Append(printableCharacters[Values[x, y]]);
                builder.AppendLine();
            }
            return FinalizeResultingString(builder);
        }
    }
}