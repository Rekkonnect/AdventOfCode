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
            {
                Assert.That(i.GetDigitCount(), Is.EqualTo(currentDigitCount), message: i.ToString());
            }
            previous = next;
            currentDigitCount++;
        }

        Assert.That(456100.GetDigitCount(), Is.EqualTo(6));
        Assert.That(2456100.GetDigitCount(), Is.EqualTo(7));
        Assert.That(99999999.GetDigitCount(), Is.EqualTo(8));
    }
}
