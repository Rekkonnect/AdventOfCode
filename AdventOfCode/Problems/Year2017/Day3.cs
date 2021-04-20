//#define PRINT

using AdventOfCode.Functions;
using AdventOfCode.Utilities.TwoDimensions;
using Garyon.Extensions;
using System;
using System.Threading;

namespace AdventOfCode.Problems.Year2017
{
    public class Day3 : Problem<int>
    {
        private int targetValue;

        public override int SolvePart1()
        {
            return SpiralGrid.ManhattanDistanceForIndex(targetValue);
        }
        public override int SolvePart2()
        {
            return SpiralGrid.FromEstimatedRadiusIndexingFirstLargerValue(targetValue).FirstLargerValue(targetValue);
        }

        protected override void LoadState()
        {
            targetValue = FileContents.ParseInt32();
        }
        protected override void ResetState()
        {
            targetValue = 0;
        }

        private class SpiralGrid : Grid2D<int>
        {
            public SpiralGrid(int radius)
                : base(radius * 2 + 1, false)
            {
                this[Center] = 1;
            }

            public int FirstLargerValue(int targetValue)
            {
                int currentRadius = 1;
                Location2D currentStart = Center + (1, 0);

#if PRINT
                int targetValueDigits = targetValue.GetDigitCount();
                int initialCursorTop = Console.CursorTop;

                Console.CursorVisible = false;
                PrintValue(this[Center], Center.X, Center.Y);
#endif

                int sum = 0;

                while (true)
                {
                    var (x, y) = currentStart;
                    int sideLength = currentRadius * 2;

                    // Evaluation legend:
                    /*
                     * # = Evaluated
                     * - = Current
                     * . = Ignored
                     */

                    // I'm so tempted to abstract this

                    /*
                     * Upward Expansion
                     * #..
                     * #-.
                     * ##.
                     */
                    for (int i = 0; i < sideLength - 1; i++)
                    {
                        sum = Values[x, y];
                        if (sum is 0)
                        {
                            sum = Values[x - 1, y - 1]
                                + Values[x - 1, y]
                                + Values[x - 1, y + 1]
                                + Values[x, y + 1];

                            Values[x, y] = sum;
                        }

#if PRINT
                        PrintValue(sum, x, y);
#endif
                        if (sum > targetValue)
                            goto end;

                        y--;
                    }

                    /*
                     * Leftward Expansion
                     * ...
                     * .-#
                     * ###
                     */
                    for (int i = 0; i < sideLength; i++)
                    {
                        sum = Values[x, y];
                        if (sum is 0)
                        {
                            sum = Values[x - 1, y + 1]
                                + Values[x, y + 1]
                                + Values[x + 1, y + 1]
                                + Values[x + 1, y];

                            Values[x, y] = sum;
                        }

#if PRINT
                        PrintValue(sum, x, y);
#endif
                        if (sum > targetValue)
                            goto end;

                        x--;
                    }

                    /*
                     * Downward Expansion
                     * .##
                     * .-#
                     * ..#
                     */
                    for (int i = 0; i < sideLength; i++)
                    {
                        sum = Values[x, y];
                        if (sum is 0)
                        {
                            sum = Values[x, y - 1]
                                + Values[x + 1, y - 1]
                                + Values[x + 1, y]
                                + Values[x + 1, y + 1];

                            Values[x, y] = sum;
                        }

#if PRINT
                        PrintValue(sum, x, y);
#endif
                        if (sum > targetValue)
                            goto end;

                        y++;
                    }

                    /*
                     * Rightward Expansion
                     * ###
                     * #-.
                     * ...
                     */
                    for (int i = 0; i < sideLength + 1; i++)
                    {
                        sum = Values[x, y];
                        if (sum is 0)
                        {
                            sum = Values[x - 1, y - 1]
                                + Values[x - 1, y]
                                + Values[x, y - 1]
                                + Values[x + 1, y - 1];

                            Values[x, y] = sum;
                        }

#if PRINT
                        PrintValue(sum, x, y);
#endif
                        if (sum > targetValue)
                            goto end;

                        x++;
                    }

                    // Iterate next
                    currentRadius++;
                    currentStart += (1, 1);
                }

            end:
#if PRINT
                Console.CursorVisible = true;
                Console.SetCursorPosition(0, initialCursorTop + Height);
#endif

                return sum;


#if PRINT
                void PrintValue(int value, int x, int y)
                {
                    const int sleep = 20;

                    Console.SetCursorPosition(x * (targetValueDigits + 1), initialCursorTop + y);
                    Console.Write(value.ToString().PadLeft(targetValueDigits));
                    Thread.Sleep(sleep);
                }
#endif
            }

            public static SpiralGrid FromEstimatedRadiusIndexingFirstLargerValue(int targetValue)
            {
                // Overshoot the radius but not too much
                // How is this valid? Fuck if I know

                int estimatedRadius = (int)Math.Ceiling(Math.Pow(targetValue, 1 / 11d)) + 1;
                return new SpiralGrid(estimatedRadius);
            }

            public static int ManhattanDistanceForIndex(int index)
            {
                if (index is 1)
                    return 0;

                int currentIndex = 1;
                int currentBase = 3;

                while (true)
                {
                    int nextIndex = currentBase * currentBase;
                    if (nextIndex > index)
                        break;

                    // Just in case, but I doubt
                    if (nextIndex == index)
                        return currentBase - 1;

                    currentIndex = nextIndex;
                    currentBase += 2;
                }

                int radius = currentBase / 2;
                int sideLength = 2 * radius;

                // Ignore the current side that the desired index is on
                int indexOffset = (index - currentIndex) % sideLength;
                int centerDistance = indexOffset - radius;

                return radius + Math.Abs(centerDistance);
            }
        }
    }
}
