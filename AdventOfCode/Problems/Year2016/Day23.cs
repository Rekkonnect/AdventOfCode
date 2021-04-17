using Garyon.Mathematics;

namespace AdventOfCode.Problems.Year2016
{
    // Massive thanks to u/aceshades for the discovery
    public class Day23 : Problem<int>
    {
        private ComputerInstruction[] instructions;
        private GenericComputer computer = new();
        private int constant;

        public override int SolvePart1()
        {
            return RunFactorial(7);
        }
        public override int SolvePart2()
        {
            return RunFactorial(12);
        }

        protected override void LoadState()
        {
            instructions = ParsedFileLines(i => ComputerInstruction.Parse(i));
            int result = RunProgram(7);
            constant = result - RunFactorial(7);
        }
        protected override void ResetState()
        {
            computer = null;
            constant = 0;
        }

        private int RunFactorial(int factorial)
        {
            return (int)GeneralMath.Factorial(factorial) + constant;
        }

        private int RunProgram(int initialRegisterValue)
        {
            computer.Instructions = instructions;
            computer.ResetRegisters();
            computer.SetRegisterValue('a', initialRegisterValue);
            computer.RunProgram();
            return computer.GetRegisterValue('a');
        }
    }
}
