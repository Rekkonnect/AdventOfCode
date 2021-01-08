using System;
using System.Linq;

namespace AdventOfCode.Problems.Year2020.Utilities
{
    public class ConsoleSimulator
    {
        private bool enabledInstructionExecutionRecording;
        private bool[] executedInstructions;

        #region State
        private int currentInstructionIndex;
        #endregion

        #region Variables
        public int Accumulator { get; set; }
        #endregion

        public readonly ConsoleSimulatorInstruction[] Instructions;

        public int TotalInstructions => Instructions.Length;
        public bool EnabledInstructionExecutionRecording
        {
            get => enabledInstructionExecutionRecording;
            set
            {
                if (enabledInstructionExecutionRecording == value)
                    return;

                if (enabledInstructionExecutionRecording = value)
                    executedInstructions = new bool[TotalInstructions];
                else
                    executedInstructions = null;
            }
        }

        public ConsoleSimulator(string[] rawInstructions, bool recordInstructionExecution = false)
        {
            Instructions = rawInstructions.Select(r => ConsoleSimulatorInstruction.Parse(r)).ToArray();
            EnabledInstructionExecutionRecording = recordInstructionExecution;
        }

        public bool RunUntilExecutedInstruction()
        {
            bool forciblyTerminated = false;
            while (!forciblyTerminated && currentInstructionIndex < TotalInstructions)
                forciblyTerminated = !ExecuteCurrentInstruction(true);
            return forciblyTerminated;
        }
        public void RunToEnd()
        {
            while (currentInstructionIndex < TotalInstructions)
                ExecuteCurrentInstruction();
        }

        public void Reset()
        {
            currentInstructionIndex = 0;
            Array.Clear(executedInstructions, 0, executedInstructions.Length);
            Accumulator = 0;
        }

        public bool ExecuteCurrentInstruction(bool terminateIfAlreadyExecuted = false)
        {
            var instruction = Instructions[currentInstructionIndex];
            if (EnabledInstructionExecutionRecording)
            {
                if (terminateIfAlreadyExecuted && executedInstructions[currentInstructionIndex])
                    return false;
                executedInstructions[currentInstructionIndex] = true;
            }

            ExecuteInstruction(instruction);

            currentInstructionIndex++;
            return true;
        }

        private void ExecuteInstruction(ConsoleSimulatorInstruction instruction)
        {
            switch (instruction.Operation)
            {
                case ConsoleSimulatorOperation.AccumulatorIncrement:
                    Accumulator += instruction.ArgumentAt(0);
                    break;
                case ConsoleSimulatorOperation.Jump:
                    currentInstructionIndex += instruction.ArgumentAt(0) - 1;
                    break;
                case ConsoleSimulatorOperation.NoOperation:
                    break;
            }
        }
    }
}
