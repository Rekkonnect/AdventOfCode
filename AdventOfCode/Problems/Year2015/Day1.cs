namespace AdventOfCode.Problems.Year2015
{
    public class Day1 : Problem<int>
    {
        private InstructionString instructionString;

        public override int SolvePart1()
        {
            return instructionString.FinalFloor;
        }
        public override int SolvePart2()
        {
            return instructionString.BasementEntranceInstructionPosition;
        }

        protected override void ResetState()
        {
            instructionString = null;
        }
        protected override void LoadState()
        {
            instructionString = new(FileContents);
        }

        private class InstructionString
        {
            // Analyzed data
            public int FinalFloor { get; private set; }
            public int BasementEntranceInstructionPosition { get; private set; }

            public string Instructions { get; }

            public InstructionString(string instructions)
            {
                Instructions = instructions;
                Analyze();
            }

            private void Analyze()
            {
                int current = 0;
                for (int i = 0; i < Instructions.Length; i++)
                {
                    char instruction = Instructions[i];
                    current += instruction switch
                    {
                        '(' => 1,
                        ')' => -1,
                    };

                    if (BasementEntranceInstructionPosition > 0)
                        continue;

                    if (current == -1)
                        BasementEntranceInstructionPosition = i + 1;
                }
                FinalFloor = current;
            }
        }
    }
}
