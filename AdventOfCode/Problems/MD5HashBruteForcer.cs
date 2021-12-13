using System.Security.Cryptography;

namespace AdventOfCode.Problems;

public abstract class MD5HashBruteForcer
{
    private static readonly char[] hexChars;

    static MD5HashBruteForcer()
    {
        hexChars = new char[16];
        for (int i = 0; i < 10; i++)
            hexChars[i] = (char)('0' + i);

        for (int i = 0; i < 6; i++)
            hexChars[10 + i] = (char)('a' + i);
    }

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

    protected static string StringifyHash(byte[] hash)
    {
        var result = new char[hash.Length * 2];
        for (int i = 0; i < hash.Length; i++)
        {
            int resultIndexOffset = i * 2;
            byte b = hash[i];
            result[resultIndexOffset] = hexChars[b >> 4];
            result[resultIndexOffset + 1] = hexChars[b & 0x0F];
        }
        return new(result);
    }
}
