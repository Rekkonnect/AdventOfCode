using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Functions;

public static class EnumerableProducts
{
    public static int Product(this IEnumerable<int> source)
    {
        int product = 1;
        foreach (int value in source)
            product *= value;

        return product;
    }
}
