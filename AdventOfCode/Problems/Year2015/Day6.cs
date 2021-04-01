using AdventOfCode.Utilities.TwoDimensions;
using Garyon.Functions.IntrinsicsHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2015
{
    public class Day6 : Problem<int>
    {
        private Instruction[] instructions;

        public override int SolvePart1()
        {
            var optimizedGrid = new LightGrid();
            optimizedGrid.ApplyInstructions(instructions);
            return optimizedGrid.GetTurnedOnLightCount();
        }
        public override int SolvePart2()
        {
            var grid = new UnoptimizedLightGrid();
            grid.ApplyInstructionsPart2(instructions);
            return grid.GetTotalBrightness();
        }

        protected override void ResetState()
        {
            instructions = null;
        }
        protected override void LoadState()
        {
            if (instructions != null)
                return;

            instructions = FileLines.Select(Instruction.Parse).ToArray();
        }

        private class UnoptimizedLightGrid
        {
            private int[,] lights = new int[1000, 1000];
            
            public int GetTotalBrightness()
            {
                int totalBrightness = 0;
                for (int x = 0; x < 1000; x++)
                    for (int y = 0; y < 1000; y++)
                        totalBrightness += lights[x, y];
                return totalBrightness;
            }

            public void ApplyInstructionsPart1(IEnumerable<Instruction> instructions)
            {
                foreach (var instruction in instructions)
                    ApplyInstructionPart1(instruction);
            }
            public void ApplyInstructionsPart2(IEnumerable<Instruction> instructions)
            {
                foreach (var instruction in instructions)
                    ApplyInstructionPart2(instruction);
            }

            public void ApplyInstructionPart1(Instruction instruction)
            {
                ApplyInstruction(instruction, AdjustLightPart1);
            }
            public void ApplyInstructionPart2(Instruction instruction)
            {
                ApplyInstruction(instruction, AdjustLightPart2);
            }

            private void ApplyInstruction(Instruction instruction, LightAdjuster adjuster)
            {
                for (int x = instruction.Start.X; x <= instruction.End.X; x++)
                    for (int y = instruction.Start.Y; y <= instruction.End.Y; y++)
                        lights[x, y] = adjuster(lights[x, y], instruction.Action);
            }

            private int AdjustLightPart1(int brightness, LightAction action)
            {
                return action switch
                {
                    LightAction.TurnOn => 1,
                    LightAction.Toggle => brightness == 1 ? 0 : 1,
                    LightAction.TurnOff => 0,
                };
            }
            private int AdjustLightPart2(int brightness, LightAction action)
            {
                return action switch
                {
                    LightAction.TurnOn => brightness + 1,
                    LightAction.Toggle => brightness + 2,
                    LightAction.TurnOff => Math.Max(0, brightness - 1),
                };
            }

            private delegate int LightAdjuster(int previousBrightness, LightAction action);
        }

        private unsafe class LightGrid
        {
            private const int dimension = 1000;
            private const int usedBytes = dimension * dimension / 8;

            // + vector byte count to avoid memory corruptions and access violations
            private byte[] bits = new byte[usedBytes + Vector256<byte>.Count];

            public int GetTurnedOnLightCount()
            {
                ulong totalOn = 0;
                fixed (byte* bitArray = bits)
                    for (int i = 0; i < usedBytes; i += sizeof(ulong))
                        totalOn += Popcnt.X64.PopCount(*(ulong*)(bitArray + i));
                return (int)totalOn;
            }

            private bool[,] GetGridPortion(Location2D start, Location2D size)
            {
                var grid = new bool[size.X, size.Y];
                for (int x = 0; x < size.X; x++)
                    for (int y = 0; y < size.Y; y++)
                        grid[x, y] = this[x + start.X, y + start.Y];
                return grid;
            }

            public void ApplyInstructions(IEnumerable<Instruction> instructions)
            {
                foreach (var instruction in instructions)
                    ApplyInstruction(instruction);
            }

            // The toggle instruction is broken for unknown reasons
            public void ApplyInstruction(Instruction instruction)
            {
                int byteCount = Vector256<byte>.Count;

                // Find the masks that will be used
                int startOffset = instruction.Start.Y;
                int endOffset = instruction.End.Y;

                var startMask = Vector256<byte>.AllBitsSet;
                var endMask = Vector256<byte>.AllBitsSet;
                var intermediateMask = Vector256<byte>.AllBitsSet;

                int startOffsetByte = Math.DivRem(startOffset, 8, out int startOffsetBitIndex);
                int endOffsetByte = Math.DivRem(endOffset, 8, out int endOffsetBitIndex);
                int offsetByteDifference = endOffsetByte - startOffsetByte;

                *(byte*)&startMask = (byte)(0xFF >> (startOffsetBitIndex));

                byte* adjustedMaskBytePtr, maskEndPtr;
                if (offsetByteDifference < byteCount)
                {
                    adjustedMaskBytePtr = (byte*)&startMask;
                    maskEndPtr = (byte*)&startMask;
                }
                else
                {
                    adjustedMaskBytePtr = (byte*)&endMask;
                    maskEndPtr = (byte*)&endMask;
                }

                adjustedMaskBytePtr += offsetByteDifference % byteCount;
                maskEndPtr += byteCount;

                *adjustedMaskBytePtr = (byte)(*adjustedMaskBytePtr & (0xFF << (7 - endOffsetBitIndex)));
                for (byte* ptr = adjustedMaskBytePtr + 1; ptr < maskEndPtr; ptr++)
                    *ptr = 0;
                
                if (instruction.Action is LightAction.TurnOff)
                {
                    startMask = AVXHelper.NOTVector256(startMask);
                    endMask = AVXHelper.NOTVector256(endMask);
                    intermediateMask = AVXHelper.NOTVector256(intermediateMask);
                }

                // Apply instruction for each X
                int startX = instruction.Start.X;
                int endX = instruction.End.X;
                fixed (byte* bitArray = bits)
                {
                    const int arrayAdvancement = dimension / 8;
                    byte* byteOffset = bitArray + startX * arrayAdvancement;
                    for (int x = startX; x <= endX; x++, byteOffset += arrayAdvancement)
                    {
                        Vector256<byte> usedMask = startMask;

                        for (int yByte = startOffsetByte; yByte <= endOffsetByte; yByte += byteCount)
                        {
                            if (yByte > startOffsetByte && yByte + byteCount > endOffsetByte)
                                usedMask = endMask;

                            var address = byteOffset + yByte;
                            var previousBytes = Avx.LoadVector256(address);

                            var result = instruction.Action switch
                            {
                                LightAction.TurnOn => Avx2.Or(previousBytes, usedMask),
                                LightAction.Toggle => Avx2.Xor(previousBytes, usedMask),
                                LightAction.TurnOff => Avx2.And(previousBytes, usedMask),
                            };

                            Avx.Store(address, result);

                            usedMask = intermediateMask;
                        }
                    }
                }
            }

            public bool this[int x, int y]
            {
                get
                {
                    if (x >= 1000 || y >= 1000)
                        return false;

                    int linearIndex = x * dimension + y;
                    int byteIndex = Math.DivRem(linearIndex, 8, out int bitShift);
                    return (bits[byteIndex] & (0x80 >> bitShift)) != 0;
                }
            }
        }

        private record Instruction(LightAction Action, Location2D Start, Location2D End)
        {
            private static Regex parsePattern = new(@"(.*) (\d*),(\d*) through (\d*),(\d*)", RegexOptions.Compiled);

            public Location2D RectangleSize => End - Start;

            public static Instruction Parse(string s)
            {
                var match = parsePattern.Match(s);
                var groups = match.Groups;

                var action = ParseAction(groups[1].Value);
                int startX = int.Parse(groups[2].Value);
                int startY = int.Parse(groups[3].Value);
                int endX = int.Parse(groups[4].Value);
                int endY = int.Parse(groups[5].Value);

                return new Instruction(action, new(startX, startY), new(endX, endY));
            }

            private static LightAction ParseAction(string s)
            {
                return s switch
                {
                    "turn on" => LightAction.TurnOn,
                    "toggle" => LightAction.Toggle,
                    "turn off" => LightAction.TurnOff,
                };
            }
        }

        private enum LightAction
        {
            TurnOn,
            Toggle,
            TurnOff
        }
    }
}
