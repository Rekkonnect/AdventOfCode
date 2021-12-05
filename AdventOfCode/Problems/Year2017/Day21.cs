using AdventOfCode.Utilities.TwoDimensions;
using AdventOfCSharp;
using AdventOfCSharp.Extensions;
using Garyon.DataStructures;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2017;

public class Day21 : Problem<int>
{
    private EnchantmentRuleSystem particleSystem;
    private PixelGrid expanded5;

    public override int SolvePart1()
    {
        return expanded5.ValueCounters[PixelState.On];
    }
    public override int SolvePart2()
    {
        // Is it just me or does that one take too long?
        return particleSystem.Expand(expanded5, 18 - 5).ValueCounters[PixelState.On];
    }

    protected override void LoadState()
    {
        particleSystem = new(ParsedFileLines(EnchantmentRule.Parse));
        expanded5 ??= particleSystem.Expand(PixelGrid.StartingGrid, 5);
    }
    protected override void ResetState()
    {
        particleSystem = null;
        expanded5 = null;
    }

    private class EnchantmentRuleSystem
    {
        private readonly EnchantmentRule[] rules;
        private readonly Dictionary<int, int> inputRuleMatches;

        public EnchantmentRuleSystem(EnchantmentRule[] enchantmentRules)
        {
            rules = enchantmentRules;
            inputRuleMatches = new(rules.Length);

            for (int i = 0; i < rules.Length; i++)
            {
                var rule = rules[i];
                var codes = rule.Input.GetInputPatternCodes();
                foreach (var code in codes)
                    inputRuleMatches.TryAdd(code, i);
            }
        }

        public PixelGrid Expand(PixelGrid grid, int times)
        {
            var current = grid;
            for (int i = 0; i < times; i++)
                current = Expand(current);
            return current;
        }

        public PixelGrid Expand(PixelGrid grid)
        {
            var patternCodes = grid.GetGroupPatternCodes();
            return PixelGrid.FromGroupedGrids(patternCodes.SelectArray(code => rules[inputRuleMatches[code]].Output));
        }
    }

    private record EnchantmentRule(PixelGrid Input, PixelGrid Output)
    {
        private static readonly Regex rulePattern = new(@"(?'input'[\.\#\/]*) => (?'output'[\.\#\/]*)", RegexOptions.Compiled);

        public static EnchantmentRule Parse(string raw)
        {
            var groups = rulePattern.Match(raw).Groups;
            var input = PixelGrid.Parse(groups["input"].Value);
            var output = PixelGrid.Parse(groups["output"].Value);
            return new(input, output);
        }
    }

    private class PixelGrid : SquareGrid2D<PixelState>
    {
        public const string RawStartingGrid = ".#./..#/###";
        public static readonly PixelGrid StartingGrid = Parse(RawStartingGrid);

        private PixelGrid(int size, ValueCounterDictionary<PixelState> valueCounters)
            : base(size, default, valueCounters) { }

        public PixelGrid(int size)
            : base(size) { }

        public int[,] GetGroupPatternCodes()
        {
            int groupSize = (Size % 2) switch
            {
                0 => 2,
                _ => 3,
            };

            int groupCount = Size / groupSize;
            int[,] result = new int[groupCount, groupCount];

            for (int x = 0; x < groupCount; x++)
            {
                for (int y = 0; y < groupCount; y++)
                {
                    result[x, y] = GetGroupPatternCode(x, y, groupSize);
                }
            }

            return result;
        }

        public int GetPatternCode()
        {
            return GetGroupPatternCode(0, 0, Size);
        }
        public int GetGroupPatternCode(int groupX, int groupY, int groupSize)
        {
            int result = 0;

            for (int x = 0; x < groupSize; x++)
            {
                for (int y = 0; y < groupSize; y++)
                {
                    if (this[groupX, groupY, x, y, groupSize] is PixelState.On)
                        result |= 1;

                    result <<= 1;
                }
            }

            return result;
        }

        public HashSet<int> GetInputPatternCodes()
        {
            var set = new HashSet<int>();

            var baseGrids = new[]
            {
                    this,
                    FlipHorizontally() as PixelGrid,
                    FlipVertically() as PixelGrid,
                };

            foreach (var baseGrid in baseGrids)
            {
                set.Add(baseGrid.GetPatternCode());

                var current = baseGrid;

                for (int i = 0; i < 3; i++)
                {
                    current = current.RotateClockwise() as PixelGrid;
                    set.Add(current.GetPatternCode());
                }
            }

            return set;
        }

        protected override PixelGrid InitializeClone()
        {
            return new(Size, ValueCounters);
        }

        public PixelState this[int groupX, int groupY, int x, int y, int groupSize]
        {
            get => this[groupX * groupSize + x, groupY * groupSize + y];
            set => this[groupX * groupSize + x, groupY * groupSize + y] = value;
        }

        public static PixelGrid FromGroupedGrids(PixelGrid[,] grids)
        {
            int groupSize = grids[0, 0].Size;
            int groupCount = grids.GetLength(0);
            int totalSize = groupSize * groupCount;
            var result = new PixelGrid(totalSize);

            for (int groupX = 0; groupX < groupCount; groupX++)
            {
                for (int groupY = 0; groupY < groupCount; groupY++)
                {
                    for (int x = 0; x < groupSize; x++)
                    {
                        for (int y = 0; y < groupSize; y++)
                        {
                            result[groupX, groupY, x, y, groupSize] = grids[groupX, groupY][x, y];
                        }
                    }
                }
            }

            return result;
        }

        public static PixelGrid Parse(string raw)
        {
            var split = raw.Split('/');
            var result = new PixelGrid(split.Length);
            for (int x = 0; x < result.Size; x++)
            {
                for (int y = 0; y < result.Size; y++)
                {
                    result[x, y] = ParsePixel(split[y][x]);
                }
            }
            return result;
        }

        private static PixelState ParsePixel(char c) => c switch
        {
            '#' => PixelState.On,
            _ => PixelState.Off,
        };
    }
    private enum PixelState
    {
        Off,
        On
    }
}
