using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace AdventOfCode.Utilities
{
    public interface IKeyedObject<T>
    {
        T Key { get; }
    }
}
