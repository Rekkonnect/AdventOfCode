using Garyon.Extensions.ArrayExtensions;
using System;

namespace AdventOfCode.Problems
{
    public class ComputerProgram
    {
        // The design is such that optimizations reduce the number of instructions, replacing the empty elements with NOP
        private ComputerInstruction[] sourceInstructions;
        private ComputerInstruction[] optimizedInstructions;

        public ComputerInstruction[] Instructions
        {
            set
            {
                sourceInstructions = value.CopyArray();
                ResetOptimizations();
                // TODO: Uncomment this when the optimization system is set
                //ReoptimizeInstructions();
            }
        }
        public int Length => sourceInstructions.Length;

        public ComputerProgram(ComputerInstruction[] instructions) => Instructions = instructions;

        public void ResetOptimizations()
        {
            optimizedInstructions = sourceInstructions.CopyArray();
        }
        public void ReoptimizeInstructions()
        {
            ResetOptimizations();
            OptimizeInstructions();
        }
        public void OptimizeInstructions()
        {
            // Quite a WIP; avoid using for now
            for (int i = 0; i < optimizedInstructions.Length; i++)
            {
                var instruction = optimizedInstructions[i];
                // Just in case there is another jump instruction that can be optimized
                if (!instruction.Operator.IsJump())
                    continue;

                if (instruction.Operator is not ComputerOperator.JumpIfNotZero)
                    continue;

                // Must have a non-constant exit condition value
                bool isConstant = instruction.Argument(0, out _, out char loopedRegister);
                if (isConstant)
                    continue;

                // Must have a constant jump offset
                bool isConstantJumpOffset = instruction.Argument(1, out int jumpOffset, out _);
                if (!isConstantJumpOffset)
                    continue;

                // Must have a negative jump offset to classify as a loop
                if (jumpOffset >= 0)
                    continue;

                int loopStart = i + jumpOffset;

                int instructionCount = -jumpOffset;
                var replacedInstructions = new ComputerInstruction[instructionCount];
                replacedInstructions.Fill(ComputerInstruction.NoOperation);

                bool validOptimization = true;

                bool negatedRegister = false;
                var segment = new ArraySegment<ComputerInstruction>(optimizedInstructions, loopStart, instructionCount);

                // Find the instruction that handles the looped register
                foreach (var loopedInstruction in segment)
                {
                    bool isConstant0 = loopedInstruction.Argument(0, out int constant0, out char register0);

                    if (isConstant0)
                        continue;

                    switch (loopedInstruction.Operator)
                    {

                    }

                    if (register0 == loopedRegister)
                    {
                        negatedRegister = loopedInstruction.Operator is ComputerOperator.Increase;
                        break;
                    }
                }

                int replacedInstructionIndex = 0;
                foreach (var loopedInstruction in segment)
                {
                    bool isConstant0 = loopedInstruction.Argument(0, out int constant0, out char register0);
                    bool isConstant1 = loopedInstruction.Argument(1, out int constant1, out char register1);

                    if (register0 == loopedRegister)

                    switch (loopedInstruction.Operator)
                    {
                        case ComputerOperator.NoOperation:
                            continue;

                        case ComputerOperator.Increase:
                        case ComputerOperator.Decrease:
                            if (register0 == loopedRegister)
                                continue;
                            break;
                    }

                    switch (loopedInstruction.Operator)
                    {

                        case ComputerOperator.Add:
                            break;

                        case ComputerOperator.Subtract:
                            break;

                        default:
                            // Such a solution would require much more sophisticated structures, and providing accurate index mapping
                            // Which would rely on grouping the instructions
                            validOptimization = false;
                            continue;
                    }

                    replacedInstructionIndex++;
                }

                if (!validOptimization)
                    continue;

                // Perform the replacement if the optimization is valid - NOPs will have been added
                Array.Copy(replacedInstructions, 0, optimizedInstructions, loopStart, instructionCount);
            }
        }

        public void ToggleInstruction(int toggledIndex)
        {
            if (toggledIndex < 0 || toggledIndex >= Length)
                return;

            var toggledInstruction = sourceInstructions[toggledIndex];
            sourceInstructions[toggledIndex] = toggledInstruction with { Operator = GetToggledOperator(toggledInstruction.Operator) };
            ReoptimizeInstructions();
        }

        public ComputerInstruction this[int index] => optimizedInstructions[index];

        private static ComputerOperator GetToggledOperator(ComputerOperator op)
        {
            return (op.GetArgumentCount(), op) switch
            {
                (2, ComputerOperator.JumpIfNotZero) => ComputerOperator.Copy,
                (2, _) => ComputerOperator.JumpIfNotZero,
                (1, ComputerOperator.Increase) => ComputerOperator.Decrease,
                (1, _) => ComputerOperator.Increase,
                _ => ComputerOperator.NoOperation,
            };
        }
    }
}
