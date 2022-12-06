using AdventOfCode.Utilities;

namespace AdventOfCode.Problems.Year2016;

public partial class Day21 : Problem<string>
{
    private PasswordScrambler scrambler;

    public override string SolvePart1()
    {
        return scrambler.Scramble("abcdefgh");
    }
    public override string SolvePart2()
    {
        return scrambler.Unscramble("fbgdceah");
    }

    protected override void LoadState()
    {
        scrambler = new(ParsedFileLines(Operation.ParseOperation));
    }
    protected override void ResetState()
    {
        scrambler = null;
    }

    private class PasswordScrambler
    {
        private readonly Operation[] operations;

        public PasswordScrambler(Operation[] scrambleOperations) => operations = scrambleOperations;

        public string Scramble(string initial)
        {
#if DEBUG
            Console.WriteLine();
            Console.WriteLine(initial);
            Console.WriteLine();
#endif

            var constructable = new ConstructableArray<char>(initial.ToCharArray());

            foreach (var operation in operations)
            {
#if DEBUG
                Console.WriteLine(operation);
#endif
                operation.Operate(constructable);
#if DEBUG
                Console.WriteLine(new string(constructable.ConstructArray()));
                Console.WriteLine();
#endif
            }

            return new(constructable.ConstructArray());
        }
        public string Unscramble(string scrambled)
        {
#if DEBUG
            Console.WriteLine();
            Console.WriteLine(scrambled);
            Console.WriteLine();
#endif

            var constructable = new ConstructableArray<char>(scrambled.ToCharArray());

            foreach (var operation in operations.Reverse())
            {
#if DEBUG
                Console.WriteLine(operation);
#endif
                operation.OperateReverse(constructable);
#if DEBUG
                Console.WriteLine(new string(constructable.ConstructArray()));
                Console.WriteLine();
#endif
            }

            return new(constructable.ConstructArray());
        }
    }

    private sealed partial record SwapPositionOperation(int X, int Y) : TwoPositionArgumentOperation(X, Y)
    {
        private static readonly Regex swapPositionPattern = SwapPositionRegex();

        [GeneratedRegex("swap position (?'x'\\d) with position (?'y'\\d)", RegexOptions.Compiled)]
        private static partial Regex SwapPositionRegex();

        public override void Operate(ConstructableArray<char> scrambledPassword)
        {
            scrambledPassword.SwapPosition(X, Y);
        }

        public static SwapPositionOperation Parse(string raw)
        {
            bool success = ParseArguments(raw, swapPositionPattern, out int x, out int y);
            if (!success)
                return null;

            return new SwapPositionOperation(x, y);
        }
    }
    private sealed partial record SwapLetterOperation(char X, char Y) : Operation
    {
        private static readonly Regex swapLetterPattern = SwapLetterRegex();

        public override void Operate(ConstructableArray<char> scrambledPassword)
        {
            scrambledPassword.SwapItem(X, Y);
        }

        public static SwapLetterOperation Parse(string raw)
        {
            var match = swapLetterPattern.Match(raw);

            if (!match.Success)
                return null;

            var groups = match.Groups;
            char x = groups["x"].Value[0];
            char y = groups["y"].Value[0];
            return new SwapLetterOperation(x, y);
        }

        [GeneratedRegex("swap letter (?'x'\\w) with letter (?'y'\\w)", RegexOptions.Compiled)]
        private static partial Regex SwapLetterRegex();
    }
    private sealed partial record RotateOperation(int Rotation) : Operation
    {
        private static readonly Regex rotatePattern = RotateRegex();

        [GeneratedRegex("rotate (?'direction'\\w*) (?'x'\\d) step", RegexOptions.Compiled)]
        private static partial Regex RotateRegex();

        public override void Operate(ConstructableArray<char> scrambledPassword)
        {
            scrambledPassword.Rotate(Rotation);
        }
        public override void OperateReverse(ConstructableArray<char> scrambledPassword)
        {
            scrambledPassword.Rotate(-Rotation);
        }

        public static RotateOperation Parse(string raw)
        {
            var match = rotatePattern.Match(raw);

            if (!match.Success)
                return null;

            var groups = match.Groups;
            bool left = groups["direction"].Value is "left";
            int rotation = groups["x"].Value.ParseInt32();
            if (left)
                rotation = -rotation;

            return new RotateOperation(rotation);
        }
    }
    private sealed partial record RotateBasedPositionOperation(char X) : Operation
    {
        private static readonly Regex rotateBasedPositionPattern = RotateBasedPositionRegex();

        [GeneratedRegex("rotate based on position of letter (?'x'\\w)", RegexOptions.Compiled)]
        private static partial Regex RotateBasedPositionRegex();

        private static readonly FlexibleInitializableValueDictionary<int, RotateBasedPositionInversionDictionary> rotationMappings = new();

        public override void Operate(ConstructableArray<char> scrambledPassword)
        {
            RegisterRotationMappings(scrambledPassword.Length);

            int index = scrambledPassword.IndexOf(X);
#if DEBUG
            Console.WriteLine($"Index {index}");
#endif

            int rotation = GetRotation(index);
#if DEBUG
            Console.WriteLine($"Rotation {rotation}");
#endif

            scrambledPassword.Rotate(rotation);
        }
        public override void OperateReverse(ConstructableArray<char> scrambledPassword)
        {
            // Well played, Eric
            RegisterRotationMappings(scrambledPassword.Length);

            int rotatedIndex = scrambledPassword.IndexOf(X);
#if DEBUG
            Console.WriteLine($"Rotated Index {rotatedIndex}");
#endif

            int length = scrambledPassword.Length;
            int initialIndex = rotationMappings[length][rotatedIndex].Initial;
#if DEBUG
            Console.WriteLine($"Initial Index {initialIndex}");
#endif
            int rotation = initialIndex - rotatedIndex;
#if DEBUG
            Console.WriteLine($"Rotation {rotation}");
#endif
            scrambledPassword.Rotate(initialIndex - rotatedIndex);
        }

        private static void RegisterRotationMappings(int length)
        {
            if (rotationMappings[length].Any())
                return;

            for (int i = 0; i < length; i++)
            {
                int rotation = GetRotation(i);
                var mapping = GetRotationMapping(i, rotation, length);
                rotationMappings[length].Add(mapping);
            }
        }

        private static int GetRotation(int index)
        {
            int rotation = index + 1;
            if (index > 3)
                rotation++;
            return rotation;
        }
        private static RotateBasedPositionIndexMapping GetRotationMapping(int index, int rotation, int length)
        {
            return new(index, (index + rotation) % length);
        }

        public static RotateBasedPositionOperation Parse(string raw)
        {
            var match = rotateBasedPositionPattern.Match(raw);

            if (!match.Success)
                return null;

            var groups = match.Groups;
            char x = groups["x"].Value[0];
            return new RotateBasedPositionOperation(x);
        }
    }
    private sealed partial record ReversePositionsOperation(int X, int Y) : TwoPositionArgumentOperation(X, Y)
    {
        private static readonly Regex reversePositionsPattern = ReversePositionsRegex();

        [GeneratedRegex("reverse positions (?'x'\\d) through (?'y'\\d)", RegexOptions.Compiled)]
        private static partial Regex ReversePositionsRegex();

        public override void Operate(ConstructableArray<char> scrambledPassword)
        {
            scrambledPassword.ReverseOrder(X, Y);
        }

        public static ReversePositionsOperation Parse(string raw)
        {
            bool success = ParseArguments(raw, reversePositionsPattern, out int x, out int y);
            if (!success)
                return null;

            return new ReversePositionsOperation(x, y);
        }
    }
    private sealed partial record MovePositionOperation(int X, int Y) : TwoPositionArgumentOperation(X, Y)
    {
        private static readonly Regex movePositionPattern = MovePositionRegex();

        [GeneratedRegex("move position (?'x'\\d) to position (?'y'\\d)", RegexOptions.Compiled)]
        private static partial Regex MovePositionRegex();

        public override void Operate(ConstructableArray<char> scrambledPassword)
        {
            scrambledPassword.Move(X, Y);
        }
        public override void OperateReverse(ConstructableArray<char> scrambledPassword)
        {
            scrambledPassword.Move(Y, X);
        }

        public static MovePositionOperation Parse(string raw)
        {
            bool success = ParseArguments(raw, movePositionPattern, out int x, out int y);
            if (!success)
                return null;

            return new MovePositionOperation(x, y);
        }
    }
    private abstract record TwoPositionArgumentOperation(int X, int Y) : Operation
    {
        protected static bool ParseArguments(string raw, Regex pattern, out int x, out int y)
        {
            x = 0;
            y = 0;

            var match = pattern.Match(raw);

            if (!match.Success)
                return false;

            var groups = match.Groups;
            x = groups["x"].Value.ParseInt32();
            y = groups["y"].Value.ParseInt32();
            return true;
        }
    }

    private abstract record Operation
    {
        public abstract void Operate(ConstructableArray<char> scrambledPassword);
        public virtual void OperateReverse(ConstructableArray<char> scrambledPassword) => Operate(scrambledPassword);

        public static Operation ParseOperation(string raw)
        {
            return SwapPositionOperation.Parse(raw)
                ?? SwapLetterOperation.Parse(raw)
                ?? RotateOperation.Parse(raw)
                ?? RotateBasedPositionOperation.Parse(raw)
                ?? ReversePositionsOperation.Parse(raw)
                ?? MovePositionOperation.Parse(raw)
                as Operation;
        }
    }

    private class RotateBasedPositionInversionDictionary : KeyedObjectDictionary<int, RotateBasedPositionIndexMapping> { }

    private readonly struct RotateBasedPositionIndexMapping : IKeyedObject<int>
    {
        int IKeyedObject<int>.Key => Rotated;

        public int Initial { get; }
        public int Rotated { get; }

        public int Rotation => Rotated - Initial;

        public RotateBasedPositionIndexMapping(int initial, int rotated)
        {
            (Initial, Rotated) = (initial, rotated);
        }

        public override string ToString()
        {
            return $"{Initial} > {Rotated}";
        }
    }
}
