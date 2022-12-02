namespace AdventOfCode.Problems.Year2022;

public class Day2 : Problem<int>
{
    public override int SolvePart1()
    {
        return SolvePart(RPSGameInfo.ParseLinePart1);
    }
    public override int SolvePart2()
    {
        return SolvePart(RPSGameInfo.ParseLinePart2);
    }

    private int SolvePart(Func<string, RPSGameInfo> lineSelector)
    {
        return FileLines.Select(lineSelector).Sum(r => r.TotalScore);
    }

    private record struct RPSGameInfo(Choice Opponent, Choice Me, GameResult Result)
    {
        public int ResultScore => Result switch
        {
            GameResult.Loss => 0,
            GameResult.Draw => 3,
            GameResult.Win => 6,
        };

        public int RPSScore => Me switch
        {
            Choice.Rock => 1,
            Choice.Paper => 2,
            Choice.Scissors => 3,
        };

        public int TotalScore => ResultScore + RPSScore;

        public static RPSGameInfo ParseLinePart1(string line)
        {
            char opponentCode = line[0];
            char meCode = line[2];

            var opponent = ParseComputerChoice(opponentCode);

            var me = meCode switch
            {
                'X' => Choice.Rock,
                'Y' => Choice.Paper,
                'Z' => Choice.Scissors,
            };

            return CalculateForLinePart1(opponent, me);
        }
        public static RPSGameInfo ParseLinePart2(string line)
        {
            char opponentCode = line[0];
            char targetResultCode = line[2];

            var opponent = ParseComputerChoice(opponentCode);

            var result = targetResultCode switch
            {
                'X' => GameResult.Loss,
                'Y' => GameResult.Draw,
                'Z' => GameResult.Win,
            };

            return CalculateForLinePart2(opponent, result);
        }

        private static Choice ParseComputerChoice(char opponentCode) => opponentCode switch
        {
            'A' => Choice.Rock,
            'B' => Choice.Paper,
            'C' => Choice.Scissors,
        };

        private static RPSGameInfo CalculateForLinePart1(Choice opponent, Choice me)
        {
            if (opponent == me)
            {
                return Target(GameResult.Draw);
            }

            return (opponent, me) switch
            {
                (Choice.Paper, Choice.Scissors) or
                (Choice.Scissors, Choice.Rock) or
                (Choice.Rock, Choice.Paper) => Target(GameResult.Win),

                _ => Target(GameResult.Loss),
            };

            RPSGameInfo Target(GameResult result) => new(opponent, me, result);
        }
        private static RPSGameInfo CalculateForLinePart2(Choice opponent, GameResult result)
        {
            if (result is GameResult.Draw)
            {
                return Target(opponent);
            }

            return (opponent, result) switch
            {
                (Choice.Rock, GameResult.Win) or
                (Choice.Scissors, GameResult.Loss) => Target(Choice.Paper),

                (Choice.Scissors, GameResult.Win) or
                (Choice.Paper, GameResult.Loss) => Target(Choice.Rock),

                _ => Target(Choice.Scissors),
            };

            RPSGameInfo Target(Choice me) => new(opponent, me, result);
        }
    }

    private enum Choice
    {
        Rock,
        Paper,
        Scissors,
    }
    private enum GameResult
    {
        Loss,
        Draw,
        Win,
    }
}
