using AdventOfCode.Functions;
using AdventOfCode.Problems.Utilities;
using Garyon.Functions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode.Problems.Year2016
{
    public class Day5 : Problem<string>
    {
        private static readonly Random random = new();

        private string secretKey;

        public override string SolvePart1()
        {
            return HackPassword<PasswordHackerPart1>();
        }
        public override string SolvePart2()
        {
            return HackPassword<PasswordHackerPart2>();
        }

        // I am proud that I converted an one-liner into a console animation function
        private string HackPassword<T>()
            where T : PasswordHacker, new()
        {
            var currentPassword = new string(default, 8);

            var hacker = new T();
            hacker.PasswordUpdated += newPassword => currentPassword = newPassword;

            var animationTask = CinematicHackerAnimation();
            var result = hacker.IdentifyPassword(secretKey);
            Task.WaitAll(animationTask);
            return result;

            async Task CinematicHackerAnimation()
            {
                Console.CursorVisible = false;
                Console.WriteLine();
                while (currentPassword.Any(c => c == default))
                {
                    Write(currentPassword);
                    await Task.Delay(16);
                }
                Write(currentPassword);
                Console.WriteLine("\n");
                Console.CursorVisible = true;
            }
            static void Write(string password)
            {
                Console.CursorLeft = 0;
                for (int i = 0; i < password.Length; i++)
                {
                    char character = password[i];
                    var color = ConsoleColor.Green;
                    if (character == '\0')
                    {
                        character = random.Next(0, 16).ToHexChar();
                        color = ConsoleColor.Red;
                    }

                    ConsoleUtilities.WriteWithColor(character.ToString(), color);
                }
            }
        }

        protected override void ResetState()
        {
            secretKey = null;
        }
        protected override void LoadState()
        {
            secretKey = FileContents;
        }

        private sealed class PasswordHackerPart1 : PasswordHacker
        {
            protected override bool UpdatePassword(byte[] hash, char[] password, int index)
            {
                password[index] = (hash[2] & 0xF).ToHexChar();
                return true;
            }
        }

        private sealed class PasswordHackerPart2 : PasswordHacker
        {
            protected override bool UpdatePassword(byte[] hash, char[] password, int index)
            {
                index = hash[2] & 0xF;
                if (index > 7)
                    return false;

                if (password[index] != default)
                    return false;

                password[index] = ((hash[3] & 0xF0) >> 4).ToHexChar();
                return true;
            }
        }

        private abstract class PasswordHacker : MD5HashBruteForcer
        {
            public event Action<string> PasswordUpdated;

            public string IdentifyPassword(string secretKey)
            {
                int current = 1;
                char[] password = new char[8];
                for (int foundCharacters = 0; foundCharacters < 8;)
                {
                    current = FindSuitableHash(secretKey, current, out var hash) + 1;

                    if (UpdatePassword(hash, password, foundCharacters))
                    {
                        foundCharacters++;
                        Task.Run(() => PasswordUpdated?.Invoke(new(password)));
                    }
                }
                return new(password);
            }

            protected abstract bool UpdatePassword(byte[] hash, char[] password, int index);

            protected override bool DetermineHashValidity(byte[] hash) => ValidHash(hash);

            private static bool ValidHash(byte[] hash)
            {
                for (int i = 0; i < 2; i++)
                    if (hash[i] > 0)
                        return false;

                return hash[2] < 0x10;
            }
        }
    }
}