using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using System.Collections.Immutable;
using System.Data;

namespace AdventOfCode.Problems.Year2022;

public partial class Day5 : Problem<string>
{
    private InstructionCrateConfiguration crateConfiguration;

    public override string SolvePart1()
    {
        return SolvePart<Part1CrateConfigurationTransformer>();
    }
    public override string SolvePart2()
    {
        return SolvePart<Part2CrateConfigurationTransformer>();
    }

    private string SolvePart<TTransformer>()
        where TTransformer : ICrateConfigurationTransformer
    {
        return crateConfiguration.ApplyInstructions<TTransformer>()
                                 .GetTopmostSignal();
    }

    protected override void LoadState()
    {
        var sections = NormalizedFileContents.TrimEnd().Split("\n\n");
        var configuration = CrateConfiguration.Parse(sections[0].GetLines());
        var instructions = sections[1].GetLines().Select(MovementInstruction.Parse).ToImmutableArray();
        crateConfiguration = new(configuration, instructions);
    }
    protected override void ResetState()
    {
        crateConfiguration = null;
    }

    private readonly partial record struct MovementInstruction(int Crates, int Origin, int Target)
    {
        private static readonly Regex instructionPattern = InstructionRegex();

        public static MovementInstruction Parse(string raw)
        {
            var match = instructionPattern.Match(raw);
            int crates = match.Groups["crates"].Value.ParseInt32();
            int origin = match.Groups["origin"].Value.ParseInt32();
            int target = match.Groups["target"].Value.ParseInt32();
            return new(crates, origin, target);
        }

        [GeneratedRegex("move (?'crates'\\d*) from (?'origin'\\d*) to (?'target'\\d*)")]
        private static partial Regex InstructionRegex();
    }

    private sealed class InstructionCrateConfiguration
    {
        private readonly ImmutableArray<MovementInstruction> instructions;

        public CrateConfiguration CrateConfiguration { get; }

        public InstructionCrateConfiguration(CrateConfiguration crateConfiguration, ImmutableArray<MovementInstruction> instructionArray)
        {
            CrateConfiguration = crateConfiguration;
            instructions = instructionArray;
        }

        public CrateConfiguration ApplyInstructions<TTransformer>()
            where TTransformer : ICrateConfigurationTransformer
        {
            var cloned = CrateConfiguration.Clone();
            var transformer = TTransformer.Create(cloned);
            transformer.ApplyInstructions(instructions);
            return cloned;
        }
    }

    private interface ICrateConfigurationTransformer
    {
        public static abstract CrateConfigurationTransformer Create(CrateConfiguration configuration);
    }
    private abstract class CrateConfigurationTransformer 
    {
        public CrateConfiguration Configuration { get; }

        protected CrateConfigurationTransformer(CrateConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ApplyInstructions(IEnumerable<MovementInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                ApplyInstruction(instruction);
            }
        }
        protected abstract void ApplyInstruction(MovementInstruction instruction);

    }
    private sealed class Part1CrateConfigurationTransformer : CrateConfigurationTransformer, ICrateConfigurationTransformer
    {
        public Part1CrateConfigurationTransformer(CrateConfiguration configuration)
            : base(configuration) { }

        protected override void ApplyInstruction(MovementInstruction instruction)
        {
            var origin = Configuration.StackWithLabel(instruction.Origin);
            var target = Configuration.StackWithLabel(instruction.Target);
            var popped = origin.PopRange(instruction.Crates).ToList();
            target.PushRange(popped);
        }

        public static CrateConfigurationTransformer Create(CrateConfiguration configuration)
        {
            return new Part1CrateConfigurationTransformer(configuration);
        }
    }
    private sealed class Part2CrateConfigurationTransformer : CrateConfigurationTransformer, ICrateConfigurationTransformer
    {
        public Part2CrateConfigurationTransformer(CrateConfiguration configuration)
            : base(configuration) { }

        protected override void ApplyInstruction(MovementInstruction instruction)
        {
            var origin = Configuration.StackWithLabel(instruction.Origin);
            var target = Configuration.StackWithLabel(instruction.Target);
            var popped = origin.PopRange(instruction.Crates).ToList();
            target.PushRangeReversed(popped);
        }

        public static CrateConfigurationTransformer Create(CrateConfiguration configuration)
        {
            return new Part2CrateConfigurationTransformer(configuration);
        }
    }

    private sealed class CrateConfiguration
    {
        private readonly FlexStack<Crate>[] stacks;

        private CrateConfiguration(FlexStack<Crate>[] stacks)
        {
            this.stacks = stacks;
        }
        private CrateConfiguration(int stackCount)
        {
            stacks = new FlexStack<Crate>[stackCount];
            for (int i = 0; i < stackCount; i++)
            {
                stacks[i] = new();
            }
        }

        public CrateConfiguration Clone()
        {
            var newStacks = stacks.Select(s => s.Clone()).ToArray();
            return new(newStacks);
        }

        public string GetBottommostSignal()
        {
            char[] buffer = new char[stacks.Length];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = stacks[i].Bottommost.Name;
            }

            return new(buffer);
        }

        public string GetTopmostSignal()
        {
            char[] buffer = new char[stacks.Length];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = stacks[i].Peek().Name;
            }

            return new(buffer);
        }

        public FlexStack<Crate> StackWithLabel(int label)
        {
            return stacks[label - 1];
        }

        public static CrateConfiguration Parse(string[] lines)
        {
            var configuration = new CrateConfiguration(9);
            var stacks = configuration.stacks;

            for (int i = 0; i < stacks.Length; i++)
            {
                stacks[i] = new(lines.Length - 1);
            }

            for (int height = lines.Length - 2; height >= 0; height--)
            {
                var line = lines[height];
                for (int stackIndex = 0; stackIndex < 9; stackIndex++)
                {
                    int column = ColumnIndex(stackIndex);
                    char name = line[column];
                    if (name is >= 'A' and <= 'Z')
                    {
                        stacks[stackIndex].Push(new(name));
                    }
                }
            }

            return configuration;

            static int ColumnIndex(int stackIndex)
            {
                return stackIndex * 4 + 1;
            }
        }
    }

    private readonly record struct Crate(char Name);
}
