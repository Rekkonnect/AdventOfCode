using AdventOfCode.Utilities;

namespace AdventOfCode.Problems.Year2017;

public partial class Day24 : Problem<int, int>
{
    private BridgeBuilder builder;

    public override int SolvePart1()
    {
        return builder.GetStrongestBridge();
    }
    public override int SolvePart2()
    {
        return builder.GetStrongestLongestBridge();
    }

    protected override void LoadState()
    {
        builder = new(ParsedFileLines(Component.Parse));
    }
    protected override void ResetState()
    {
        builder = null;
    }

    private class BridgeBuilder
    {
        private readonly ComponentDictionary componentDictionary;

        public IEnumerable<Component> Components => componentDictionary.Values.Flatten();

        public BridgeBuilder(IEnumerable<Component> components)
        {
            componentDictionary = new(components);
        }

        public int GetStrongestBridge()
        {
            return GetStrongestBridge(Evaluator);

            static void Evaluator(ref int strongest, ref int longest, int currentStrength, int currentLength)
            {
                if (currentStrength > strongest)
                {
                    strongest = currentStrength;
                }
            }
        }
        public int GetStrongestLongestBridge()
        {
            return GetStrongestBridge(Evaluator);

            static void Evaluator(ref int strongest, ref int longest, int currentStrength, int currentLength)
            {
                if (currentLength > longest)
                {
                    longest = currentLength;
                    strongest = currentStrength;
                }
                else if (currentLength == longest && currentStrength > strongest)
                {
                    strongest = currentStrength;
                }
            }
        }

        private int GetStrongestBridge(BridgeEvaluator evaluator)
        {
            int strongest = int.MinValue;
            int longest = int.MinValue;

            var currentBridge = new Stack<Component>();
            var remainingComponents = new ComponentDictionary(componentDictionary);

            Iterate(0, -1);

            return strongest;

            void Iterate(int currentStrength, int previousConnection)
            {
                var candidates = Array.Empty<Component>();

                int currentConnection;

                if (currentBridge.Count is 0)
                {
                    candidates = remainingComponents[0].ToArray();
                    currentConnection = 0;
                }
                else
                {
                    var (a, b) = currentBridge.Peek();
                    currentConnection = previousConnection == a ? b : a;

                    candidates = remainingComponents[currentConnection].ToArray();
                }

                // No more candidates = bridge is over
                if (candidates.Length is 0)
                {
                    evaluator(ref strongest, ref longest, currentStrength, currentBridge.Count);
                    return;
                }

                foreach (var candidate in candidates)
                {
                    remainingComponents.Remove(candidate);
                    currentBridge.Push(candidate);

                    Iterate(currentStrength + candidate.Strength, currentConnection);

                    currentBridge.Pop();
                    remainingComponents.Add(candidate);
                }
            }
        }

        private delegate void BridgeEvaluator(ref int strongest, ref int longest, int currentStrength, int currentLength);
    }

    private class ComponentDictionary : FlexibleHashSetDictionary<int, Component>
    {
        public ComponentDictionary(IEnumerable<Component> components)
            : base()
        {
            components.ForEach(Add);
        }
        public ComponentDictionary(ComponentDictionary other)
            : base(other) { }

        public void Add(Component component)
        {
            this[component.EndA].Add(component);
            this[component.EndB].Add(component);
        }
        public void Remove(Component component)
        {
            this[component.EndA].Remove(component);
            this[component.EndB].Remove(component);
        }
    }

    private partial struct Component : IEquatable<Component>
    {
        private static readonly Regex componentPattern = ComponentRegex();

        public int EndA { get; }
        public int EndB { get; }

        public int Strength => EndA + EndB;

        public Component(int a, int b) => (EndA, EndB) = (a, b);

        public bool ConnectableWith(Component other)
        {
            // Hopefully this can be further simplified in a future language version
            return other.EndA == EndA
                || other.EndA == EndB
                || other.EndB == EndA
                || other.EndB == EndB;
        }

        public void Deconstruct(out int a, out int b)
        {
            a = EndA;
            b = EndB;
        }

        public static Component Parse(string raw)
        {
            var groups = componentPattern.Match(raw).Groups;
            int a = groups["a"].Value.ParseInt32();
            int b = groups["b"].Value.ParseInt32();
            return new(a, b);
        }

        public bool Equals(Component other) => EndA == other.EndA && EndB == other.EndB;
        public override bool Equals(object obj) => obj is Component other && Equals(other);
        public override int GetHashCode() => EndA << 8 | EndB;
        public override string ToString() => $"{EndA}/{EndB}";
        [GeneratedRegex("(?'a'\\d*)/(?'b'\\d*)", RegexOptions.Compiled)]
        private static partial Regex ComponentRegex();
    }
}
