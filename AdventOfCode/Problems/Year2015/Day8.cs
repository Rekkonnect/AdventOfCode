using AdventOfCSharp;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AdventOfCode.Problems.Year2015;

public class Day8 : Problem<int>
{
    private CodeString[] strings;

    public override int SolvePart1()
    {
        return strings.Sum(s => s.DecodingLengthDifference);
    }
    public override int SolvePart2()
    {
        return strings.Sum(s => s.EncodingLengthDifference);
    }

    protected override void ResetState()
    {
        strings = null;
    }
    protected override void LoadState()
    {
        strings = ParsedFileLines(CodeString.Parse);
    }

    private class CodeString
    {
        private string original;
        private string decoded;
        private string encoded;

        public int OriginalStringLength => original.Length;
        public int DecodedStringLength => decoded.Length;
        public int EncodedStringLength => encoded.Length;

        public int DecodingLengthDifference => OriginalStringLength - DecodedStringLength;
        public int EncodingLengthDifference => EncodedStringLength - OriginalStringLength;

        public CodeString(string s)
        {
            original = s;
            decoded = DecodeString(s);
            encoded = EncodeString(s);
        }

        private static string EncodeString(string s)
        {
            var builder = new StringBuilder("\"", s.Length * 2);

            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '\\':
                    case '"':
                        builder.Append('\\');
                        goto default;

                    default:
                        builder.Append(s[i]);
                        break;
                }
            }

            return builder.Append('"').ToString();
        }
        private static string DecodeString(string s)
        {
            var builder = new StringBuilder(s.Length);

            bool isEscaped = false;
            for (int i = 1; i < s.Length - 1; i++)
            {
                switch (s[i])
                {
                    case '\\':
                        isEscaped = !isEscaped;
                        if (isEscaped)
                            continue;

                        goto default;

                    case 'x' when isEscaped:
                        int codePoint = int.Parse(s[(i + 1)..(i + 3)], NumberStyles.AllowHexSpecifier);
                        builder.Append((char)codePoint);
                        i += 2;
                        goto unescape;

                    default:
                        builder.Append(s[i]);
                    unescape:
                        isEscaped = false;
                        break;
                }
            }

            return builder.ToString();
        }

        public static CodeString Parse(string s) => new(s);
    }
}
