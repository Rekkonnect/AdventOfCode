using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2017;

public class Day25 : FinalDay<int>
{
    private TuringMachine turingMachine;

    public override int SolvePart1()
    {
        return turingMachine.GetChecksum();
    }

    protected override void LoadState()
    {
        turingMachine = TuringMachine.Parse(NormalizedFileContents);
    }
    protected override void ResetState()
    {
        turingMachine = null;
    }

    private class TuringMachine
    {
        private static readonly Regex initialPattern = new(@"Begin in state (?'initial'\w)\.(\s*)Perform a diagnostic checksum after (?'steps'\d*) steps\.(\s*)(?'declarations'.*)", RegexOptions.Compiled | RegexOptions.Singleline);

        private int desiredSteps;
        private int currentStep;

        private readonly HashSet<int> trueValues = new();
        private char currentState;
        private int currentTapeIndex;

        private Dictionary<char, State> stateDictionary;

        private State CurrentState => stateDictionary[currentState];

        public TuringMachine(char initialState, int checksumSteps, IEnumerable<State> states)
        {
            currentState = initialState;
            desiredSteps = checksumSteps;
            stateDictionary = states.ToDictionary(s => s.Name);
        }

        public int GetChecksum()
        {
            for (; currentStep < desiredSteps; currentStep++)
                Iterate();

            return trueValues.Count;
        }
        private void Iterate()
        {
            var action = CurrentState.ActionFor(trueValues.Contains(currentTapeIndex));

            if (action.WrittenValue is 1)
                trueValues.Add(currentTapeIndex);
            else
                trueValues.Remove(currentTapeIndex);

            currentTapeIndex += action.SlotOffset;

            currentState = action.NextState;
        }

        public static TuringMachine Parse(string raw)
        {
            var initialGroups = initialPattern.Match(raw).Groups;
            var initialState = initialGroups["initial"].Value[0];
            int steps = initialGroups["steps"].Value.ParseInt32();
            var states = initialGroups["declarations"].Value.Split("\n\n").Select(State.Parse);
            return new(initialState, steps, states);
        }
    }

    private record StateAction(int WrittenValue, int SlotOffset, char NextState)
    {
        private static readonly Regex stateActionPattern = new(@"- Write the value (?'value'\d)\.(\s*)- Move one slot to the (?'direction'left|right)\.(\s*)- Continue with state (?'nextState'\w)", RegexOptions.Compiled);

        public static StateAction Parse(string raw)
        {
            var groups = stateActionPattern.Match(raw).Groups;
            int writtenValue = groups["value"].Value.ParseInt32();
            int slotOffset = groups["direction"].Value is "left" ? -1 : 1;
            char nextState = groups["nextState"].Value[0];
            return new(writtenValue, slotOffset, nextState);
        }
    }

    private record State(char Name, StateAction Action0, StateAction Action1)
    {
        private static readonly Regex statePattern = new(@"In state (?'name'\w):(\s*).*is 0:(\s*)(?'action0'.*)is 1:(?'action1'.*)", RegexOptions.Compiled | RegexOptions.Singleline);

        public StateAction ActionFor(bool value) => value ? Action1 : Action0;

        public static State Parse(string raw)
        {
            var groups = statePattern.Match(raw).Groups;
            char name = groups["name"].Value[0];
            var action0 = StateAction.Parse(groups["action0"].Value);
            var action1 = StateAction.Parse(groups["action1"].Value);
            return new(name, action0, action1);
        }
    }
}
