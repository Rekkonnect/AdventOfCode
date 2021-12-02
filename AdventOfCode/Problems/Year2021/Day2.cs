using Garyon.Extensions;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2021;

public class Day2 : Problem<int>
{
    private Command[] commands;

    public override int SolvePart1()
    {
        return SolvePart(CommandContext.Hypothetical);
    }
    public override int SolvePart2()
    {
        return SolvePart(CommandContext.Real);
    }

    private int SolvePart(CommandContext context)
    {
        var position = new SubmarineStats();
        foreach (var command in commands)
            command.ApplyCommand(context, position);

        return position.PositionProduct;
    }

    protected override void LoadState()
    {
        commands = ParsedFileLines(Command.Parse);
    }
    protected override void ResetState()
    {
        commands = null;
    }

    private enum CommandContext
    {
        Hypothetical,
        Real,
    }

    private abstract record Command(int Value)
    {
        private static readonly Regex commandPattern = new(@"(?'kind'\w*) (?'value'\d*)");

        public void ApplyCommand(CommandContext commandContext, SubmarineStats stats)
        {
            switch (commandContext)
            {
                case CommandContext.Hypothetical:
                    ApplyHypotheticalCommand(stats);
                    break;
                case CommandContext.Real:
                    ApplyRealCommand(stats);
                    break;
            }
        }

        public abstract void ApplyHypotheticalCommand(SubmarineStats stats);
        public abstract void ApplyRealCommand(SubmarineStats stats);

        public static Command Parse(string command)
        {
            var match = commandPattern.Match(command);

            int value = match.Groups["value"].Value.ParseInt32();
            var commandKind = match.Groups["kind"].Value;
            return commandKind switch
            {
                "forward" => new ForwardCommand(value),
                "down" => new DownCommand(value),
                "up" => new UpCommand(value),
            };
        }
    }

    private sealed record ForwardCommand(int Value)
        : Command(Value)
    {
        public override void ApplyHypotheticalCommand(SubmarineStats stats)
        {
            stats.HorizontalPosition += Value;
        }
        public override void ApplyRealCommand(SubmarineStats stats)
        {
            stats.HorizontalPosition += Value;
            stats.Depth += stats.Aim * Value;
        }
    }
    private abstract record DepthCommand(int Value)
        : Command(Value)
    {
        protected abstract int Adjustment { get; }

        public sealed override void ApplyHypotheticalCommand(SubmarineStats stats)
        {
            stats.Depth += Adjustment;
        }
        public sealed override void ApplyRealCommand(SubmarineStats stats)
        {
            stats.Aim += Adjustment;
        }
    }
    private sealed record DownCommand(int Value)
        : DepthCommand(Value)
    {
        protected override int Adjustment => Value;
    }
    private sealed record UpCommand(int Value)
        : DepthCommand(Value)
    {
        protected override int Adjustment => -Value;
    }

    private sealed class SubmarineStats
    {
        public int Depth { get; set; }
        public int HorizontalPosition { get; set; }
        public int Aim { get; set; }

        public int PositionProduct => Depth * HorizontalPosition;
    }
}
