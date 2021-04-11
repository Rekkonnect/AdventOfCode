using Garyon.Extensions;
using Garyon.Mathematics;
using System.Linq;

namespace AdventOfCode.Problems.Year2015
{
    public class Day2 : Problem<int>
    {
        private PresentBox[] presentBoxes;

        public override int SolvePart1()
        {
            return presentBoxes.Sum(p => p.TotalWrappingPaper);
        }
        public override int SolvePart2()
        {
            return presentBoxes.Sum(p => p.TotalRibbon);
        }

        protected override void ResetState()
        {
            presentBoxes = null;
        }
        protected override void LoadState()
        {
            presentBoxes = ParsedFileLines(PresentBox.Parse);
        }

        private struct PresentBox
        {
            public int W { get; }
            public int H { get; }
            public int L { get; }

            public int TotalRibbon
            {
                get
                {
                    int max = GeneralMath.Max(W, H, L);
                    return 2 * (W + H + L - max) + W * H * L;
                }
            }
            public int TotalWrappingPaper
            {
                get
                {
                    int a = W * H;
                    int b = H * L;
                    int c = L * W;
                    int area = 2 * (a + b + c);
                    int smallestSide = GeneralMath.Min(a, b, c);
                    return area + smallestSide;
                }
            }

            public PresentBox(int w, int h, int l)
            {
                (W, H, L) = (w, h, l);
            }

            public static PresentBox Parse(string line)
            {
                var dimensions = line.Split('x').Select(int.Parse).ToArray();
                return new PresentBox(dimensions[0], dimensions[1], dimensions[2]);
            }
        }
    }
}
