namespace AdventOfCode.Problems.Year2017;

public class Day5 : Problem<int>
{
    private JumpMaze maze;

    public override int SolvePart1()
    {
        return PerformJumps(JumpMaze.Part1JumpAdjustment);
    }
    public override int SolvePart2()
    {
        return PerformJumps(JumpMaze.Part2JumpAdjustment);
    }

    protected override void LoadState()
    {
        maze = new(ParsedFileLinesEnumerable(int.Parse));
    }
    protected override void ResetState()
    {
        maze = null;
    }

    private int PerformJumps(JumpAdjustment jumpAdjustment) => new JumpMaze(maze).StepsToExit(jumpAdjustment);

    private class JumpMaze
    {
        private int[] jumpOffsets;

        public JumpMaze(IEnumerable<int> jumps)
        {
            jumpOffsets = jumps.ToArray();
        }
        public JumpMaze(JumpMaze other)
            : this(other.jumpOffsets) { }

        public int StepsToExit(JumpAdjustment jumpAdjustment)
        {
            int current = 0;
            int steps = 0;

            do
            {
                int jump = jumpOffsets[current];
                jumpOffsets[current] += jumpAdjustment(jump);
                current += jump;

                steps++;
            }
            while (current >= 0 && current < jumpOffsets.Length);

            return steps;
        }

        public static int Part1JumpAdjustment(int jump) => 1;
        public static int Part2JumpAdjustment(int jump) => jump switch
        {
            >= 3 => -1,
            _ => 1
        };
    }

    private delegate int JumpAdjustment(int initialJump);
}
