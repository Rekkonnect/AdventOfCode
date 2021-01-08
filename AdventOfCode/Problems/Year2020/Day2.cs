using Garyon.Extensions;

namespace AdventOfCode.Problems.Year2020
{
    public class Day2 : Problem2<int>
    {
        public override int SolvePart1()
        {
            int validPasswords = 0;
            foreach (var l in FileLines)
            {
                var split = l.Split(' ');
                var splitRange = split[0].Split('-');
                int min = int.Parse(splitRange[0]);
                int max = int.Parse(splitRange[1]);
                char matchingCharacter = split[1][0];
                var password = split[2];
                int occurrences = password.GetCharacterOccurences(matchingCharacter);
                if (min <= occurrences && occurrences <= max)
                    validPasswords++;
            }

            return validPasswords;
        }
        public override int SolvePart2()
        {
            int validPasswords = 0;
            foreach (var l in FileLines)
            {
                var split = l.Split(' ');
                var splitRange = split[0].Split('-');
                int a = int.Parse(splitRange[0]) - 1;
                int b = int.Parse(splitRange[1]) - 1;
                char matchingCharacter = split[1][0];
                var password = split[2];
                int occurrences = password.GetCharacterOccurences(matchingCharacter);
                if ((password[a] == matchingCharacter) ^ (password[b] == matchingCharacter))
                    validPasswords++;
            }

            return validPasswords;
        }
    }
}
