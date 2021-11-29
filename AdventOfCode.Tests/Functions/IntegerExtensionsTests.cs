using AdventOfCode.Functions;
using NUnit.Framework;

namespace AdventOfCode.Tests.Functions;

public class IntegerExtensionsTests
{
    [Test]
    public void GetDigitCountTest()
    {
        int currentDigitCount = 1;
        int previous = 0;
        for (int next = 10; next < 100000; next *= 10)
        {
            for (int i = previous; i < next; i++)
                Assert.AreEqual(currentDigitCount, i.GetDigitCount(), i.ToString());
            previous = next;
            currentDigitCount++;
        }

        Assert.AreEqual(6, 456100.GetDigitCount());
        Assert.AreEqual(7, 2456100.GetDigitCount());
        Assert.AreEqual(8, 99999999.GetDigitCount());
    }
}
