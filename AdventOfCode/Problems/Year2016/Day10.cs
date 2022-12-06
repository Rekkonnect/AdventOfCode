using AdventOfCode.Utilities;

namespace AdventOfCode.Problems.Year2016;

public partial class Day10 : Problem<int>
{
    private BotHandlingMachine machine;

    public override int SolvePart1()
    {
        return machine.IdentifyBotMatchingChips(17, 61);
    }
    public override int SolvePart2()
    {
        machine.IterateAll();

        int result = 1;
        for (int i = 0; i < 3; i++)
            result *= (machine.GetContainer(new(ContainerType.Output, i)) as OutputChipBin).ChipValue;
        return result;
    }

    protected override void ResetState()
    {
        machine = null;
    }
    protected override void LoadState()
    {
        machine = new(ParsedFileLines(BotInstruction.Parse));
    }

    private class BotHandlingMachine
    {
        private readonly List<ValueAssignmentInstruction> valueAssignmentInstructions = new();
        private readonly KeyedObjectDictionary<int, ComparisonInstruction> comparisonInstructions = new();

        private readonly KeyedObjectDictionary<ContainerID, MicrochipContainer> containers = new();
        private readonly Stack<Bot> doubleChipBots = new();

        private Bot CurrentDoubleChipBot => doubleChipBots.Peek();

        public BotHandlingMachine(IEnumerable<BotInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                switch (instruction)
                {
                    case ValueAssignmentInstruction assignmentInstruction:
                        valueAssignmentInstructions.Add(assignmentInstruction);
                        containers.TryAddPreserve(new Bot(assignmentInstruction.BotID), out var container);

                        var botContainer = container as Bot;
                        botContainer.TakeChip(assignmentInstruction.ChipValue);
                        if (botContainer.CanGiveChips)
                            doubleChipBots.Push(botContainer);

                        break;

                    case ComparisonInstruction comparisonInstruction:
                        comparisonInstructions.Add(comparisonInstruction);
                        containers.TryAddPreserve(MicrochipContainer.From(comparisonInstruction.Low));
                        containers.TryAddPreserve(MicrochipContainer.From(comparisonInstruction.High));
                        containers.TryAddPreserve(new Bot(comparisonInstruction.SourceBotID));
                        break;
                }
            }
        }

        public MicrochipContainer GetContainer(ContainerID containerID) => containers[containerID];

        public void IterateAll()
        {
            while (doubleChipBots.Any())
                IterateBotChipHandling();
        }
        public int IdentifyBotMatchingChips(int low, int high)
        {
            do
            {
                if (CurrentDoubleChipBot.MatchChips(low, high))
                    return CurrentDoubleChipBot.ContainerID.ID;

                IterateBotChipHandling();
            }
            while (doubleChipBots.Any());

            return -1;
        }

        private void IterateBotChipHandling()
        {
            var currentBotInstruction = comparisonInstructions[CurrentDoubleChipBot.ContainerID.ID];
            var lowContainer = containers[currentBotInstruction.Low];
            var highContainer = containers[currentBotInstruction.High];
            CurrentDoubleChipBot.GiveChips(lowContainer, highContainer);

            doubleChipBots.Pop();

            SetNextDoubleChipBot(lowContainer);
            SetNextDoubleChipBot(highContainer);
        }

        private void SetNextDoubleChipBot(MicrochipContainer container)
        {
            if (container is Bot botContainer && botContainer.CanGiveChips)
                doubleChipBots.Push(botContainer);
        }
    }

    private sealed class Bot : MicrochipContainer
    {
        public int LowChip { get; private set; }
        public int HighChip { get; private set; }

        public int ChipCount => Convert.ToInt32(LowChip > 0) + Convert.ToInt32(HighChip > 0);
        public bool CanGiveChips => ChipCount == 2;

        public override ContainerType ContainerType => ContainerType.Bot;

        public Bot(int id)
            : base(id) { }

        public bool MatchChips(int low, int high)
        {
            return LowChip == low && HighChip == high;
        }

        public void GiveChips(MicrochipContainer lowReceiver, MicrochipContainer highReceiver)
        {
            lowReceiver.TakeChip(LowChip);
            highReceiver.TakeChip(HighChip);
            LowChip = HighChip = 0;
        }

        public override void TakeChip(int chipValue)
        {
            if (LowChip == 0)
                LowChip = chipValue;
            else
            {
                if (chipValue < LowChip)
                {
                    HighChip = LowChip;
                    LowChip = chipValue;
                }
                else
                    HighChip = chipValue;
            }
        }

        public override string ToString()
        {
            return $"{ContainerID} - {LowChip} {HighChip}";
        }
    }
    private sealed class OutputChipBin : MicrochipContainer
    {
        public int ChipValue { get; private set; }

        public override ContainerType ContainerType => ContainerType.Output;

        public OutputChipBin(int id)
            : base(id) { }

        public override void TakeChip(int chipValue)
        {
            ChipValue = chipValue;
        }
    }
    private abstract class MicrochipContainer : IKeyedObject<ContainerID>
    {
        public ContainerID ContainerID { get; }

        public abstract ContainerType ContainerType { get; }

        ContainerID IKeyedObject<ContainerID>.Key => ContainerID;

        protected MicrochipContainer(int id)
        {
            ContainerID = new(ContainerType, id);
        }

        public abstract void TakeChip(int chipValue);

        public static MicrochipContainer From(ContainerID containerID)
        {
            return containerID.ContainerType switch
            {
                ContainerType.Bot => new Bot(containerID.ID),
                ContainerType.Output => new OutputChipBin(containerID.ID),
            };
        }
    }

    private readonly struct ContainerID
    {
        public ContainerType ContainerType { get; }
        public int ID { get; }

        public ContainerID(ContainerType containerType, int id)
        {
            ContainerType = containerType;
            ID = id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ContainerType, ID);
        }
        public override string ToString() => $"{ContainerType} {ID}";

        public static ContainerID Parse(string rawContainerType, string rawID)
        {
            return new(ParseContainerType(rawContainerType), rawID.ParseInt32());
        }
        private static ContainerType ParseContainerType(string raw)
        {
            return raw switch
            {
                "bot" => ContainerType.Bot,
                "output" => ContainerType.Output,
            };
        }
    }

    private enum ContainerType
    {
        Bot,
        Output,
    }

    private sealed record ComparisonInstruction(int SourceBotID, ContainerID Low, ContainerID High) : BotInstruction, IKeyedObject<int>
    {
        public override BotInstructionOperation Operation => BotInstructionOperation.Comparison;

        int IKeyedObject<int>.Key => SourceBotID;
    }
    private sealed record ValueAssignmentInstruction(int ChipValue, int BotID) : BotInstruction
    {
        public override BotInstructionOperation Operation => BotInstructionOperation.ValueAssignment;
    }

    private abstract partial record BotInstruction
    {
        private static readonly Regex comparisonPattern = ComparisonRegex();
        private static readonly Regex valueAssignmentPattern = ValueAssignmentRegex();

        [GeneratedRegex("bot (?'sourceID'\\d*) gives low to (?'lowType'\\w*) (?'lowID'\\d*) and high to (?'highType'\\w*) (?'highID'\\d*)", RegexOptions.Compiled)]
        private static partial Regex ComparisonRegex();
        [GeneratedRegex("value (?'value'\\d*) goes to bot (?'botID'\\d*)", RegexOptions.Compiled)]
        private static partial Regex ValueAssignmentRegex();

        public abstract BotInstructionOperation Operation { get; }

        public static BotInstruction Parse(string raw)
        {
            var match = comparisonPattern.Match(raw);
            if (match.Success)
            {
                var groups = match.Groups;
                int sourceID = groups["sourceID"].Value.ParseInt32();
                var low = ContainerID.Parse(groups["lowType"].Value, groups["lowID"].Value);
                var high = ContainerID.Parse(groups["highType"].Value, groups["highID"].Value);
                return new ComparisonInstruction(sourceID, low, high);
            }

            match = valueAssignmentPattern.Match(raw);
            if (match.Success)
            {
                var groups = match.Groups;
                int value = groups["value"].Value.ParseInt32();
                int botID = groups["botID"].Value.ParseInt32();
                return new ValueAssignmentInstruction(value, botID);
            }

            return default;
        }
    }

    private enum BotInstructionOperation
    {
        Comparison,
        ValueAssignment,
    }
}
