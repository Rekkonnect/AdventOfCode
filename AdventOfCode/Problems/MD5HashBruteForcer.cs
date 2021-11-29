using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AdventOfCode.Problems.Utilities;

public abstract class MD5HashBruteForcer
{
    protected readonly MD5 Hasher = MD5.Create();

    protected int CurrentIndex;

    protected int FindSuitableHash(string secretKey, out byte[] hash) => FindSuitableHash(secretKey, CurrentIndex, out hash);
    protected int FindSuitableHash(string secretKey, int start, out byte[] hash)
    {
        for (CurrentIndex = start; ; CurrentIndex++)
        {
            hash = Hasher.ComputeHash(Encoding.UTF8.GetBytes(secretKey + CurrentIndex));
            if (DetermineHashValidity(hash))
                return CurrentIndex++;
        }
    }

    protected abstract bool DetermineHashValidity(byte[] hash);

    protected static string StringifyHash(byte[] hash) => string.Concat(hash.Select(b => b.ToString("x2")));
}
