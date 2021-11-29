using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2016;

public class Day3 : Problem<int>
{
    private TriangleSides[] triangles;

    public override int SolvePart1()
    {
        return triangles.Count(t => t.ValidTriangle);
    }
    public override int SolvePart2()
    {
        var transformedTriangles = TransformTriangles();
        return transformedTriangles.Count(t => t.ValidTriangle);

        IEnumerable<TriangleSides> TransformTriangles()
        {
            for (int i = 0; i < triangles.Length; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    int a = triangles[i].GetSide(j);
                    int b = triangles[i + 1].GetSide(j);
                    int c = triangles[i + 2].GetSide(j);
                    yield return new(a, b, c);
                }
            }
        }
    }

    protected override void ResetState()
    {
        triangles = null;
    }
    protected override void LoadState()
    {
        triangles = ParsedFileLines(TriangleSides.Parse);
    }

    private record TriangleSides(int A, int B, int C)
    {
        private static readonly Regex sidesPattern = new(@"\s*(?'a'\d*)\s*(?'b'\d*)\s*(?'c'\d*)", RegexOptions.Compiled);

        public bool ValidTriangle
        {
            get
            {
                return A < B + C
                    && B < A + C
                    && C < A + B;
            }
        }

        public int GetSide(int index) => index switch
        {
            0 => A,
            1 => B,
            2 => C,
        };

        public static TriangleSides Parse(string raw)
        {
            var groups = sidesPattern.Match(raw).Groups;
            int a = groups["a"].Value.ParseInt32();
            int b = groups["b"].Value.ParseInt32();
            int c = groups["c"].Value.ParseInt32();
            return new(a, b, c);
        }
    }
}
