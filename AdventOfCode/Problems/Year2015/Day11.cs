namespace AdventOfCode.Problems.Year2015;

public class Day11 : Problem<string>
{
    private Password password;

    public override string SolvePart1()
    {
        return password.GetNextValidPassword();
    }
    public override string SolvePart2()
    {
        return password.GetNextValidPassword(2);
    }

    protected override void ResetState()
    {
        password = null;
    }
    protected override void LoadState()
    {
        password = new(FileContents);
    }

    private class Password
    {
        private string original;

        public Password(string originalPassword)
        {
            original = originalPassword;
        }

        public string GetNextValidPassword() => GetNextValidPassword(original);
        public string GetNextValidPassword(int count)
        {
            var current = original;
            for (int i = 0; i < count; i++)
                current = GetNextValidPassword(current);
            return current;
        }

        private static string GetNextValidPassword(string original)
        {
            var nextPassword = original.ToCharArray();
            IncrementLastCharacter(nextPassword);

            bool valid;
            do
            {
                valid = true;

                for (int i = 0; i < nextPassword.Length; i++)
                {
                    if (!IsValidCharacter(nextPassword[i]))
                    {
                        IncrementCharacter(nextPassword, i);
                        valid = false;
                        break;
                    }
                }

                if (!HasTwoPairs(nextPassword))
                {
                    IncrementLastCharacter(nextPassword);
                    valid = false;
                }

                if (!HasStraightCharacters(nextPassword))
                {
                    IncrementLastCharacter(nextPassword);
                    valid = false;
                }
            }
            while (!valid);

            return new(nextPassword);
        }

        private static bool HasTwoPairs(char[] chars)
        {
            char previous = chars[0];
            int consecutive = 1;
            int pairs = 0;
            for (int i = 1; i < chars.Length; i++)
            {
                if (chars[i] == previous)
                    consecutive++;
                else
                    consecutive = 1;

                if (consecutive == 2)
                    pairs++;

                if (pairs == 2)
                    return true;

                previous = chars[i];
            }
            return false;
        }

        private static bool HasStraightCharacters(char[] chars)
        {
            char previous = chars[0];
            int consecutive = 1;
            for (int i = 1; i < chars.Length; i++)
            {
                if (chars[i] == previous + 1)
                    consecutive++;
                else
                    consecutive = 1;

                if (consecutive == 3)
                    return true;

                previous = chars[i];
            }
            return false;
        }

        private static bool IsValidCharacter(char c)
        {
            return c switch
            {
                'i' or 'o' or 'l' => false,
                _ => true
            };
        }

        private static void IncrementLastCharacter(char[] chars)
        {
            IncrementCharacter(chars, chars.Length - 1);
        }
        private static void IncrementCharacter(char[] chars, int index)
        {
            bool reset = false;
            do
            {
                chars[index]++;
                index--;

                if (index < 0 || chars[index + 1] <= 'z')
                    break;

                reset = true;

                chars[index + 1] = 'a';
            }
            while (true);

            if (!reset)
                return;

            // Reset the other chars too
            for (int i = index + 2; i < chars.Length; i++)
                chars[i] = 'a';
        }

        public override string ToString() => original;
    }
}
