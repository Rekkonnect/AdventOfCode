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

    private record struct RPSGameInfo(RPSResult Opponent, RPSResult Me, GameResult Result)
    {
        public int ResultScore => Result switch
        {
            GameResult.Loss => 0,
            GameResult.Draw => 3,
            GameResult.Win => 6,
        };

        public int RPSScore => Me switch
        {
            RPSResult.Rock => 1,
            RPSResult.Paper => 2,
            RPSResult.Scissors => 3,
        };

        public int TotalScore => ResultScore + RPSScore;

        public static RPSGameInfo ParseLinePart1(string line)
        {
            char opponentCode = line[0];
            char meCode = line[2];

            var opponent = opponentCode switch
            {
                'A' => RPSResult.Rock,
                'B' => RPSResult.Paper,
                'C' => RPSResult.Scissors,
            };
            var me = meCode switch
            {
                'X' => RPSResult.Rock,
                'Y' => RPSResult.Paper,
                'Z' => RPSResult.Scissors,
            };

            return CalculateForLinePart1(opponent, me);
        }
        public static RPSGameInfo ParseLinePart2(string line)
        {
            char opponentCode = line[0];
            char targetResultCode = line[2];

            var opponent = opponentCode switch
            {
                'A' => RPSResult.Rock,
                'B' => RPSResult.Paper,
                'C' => RPSResult.Scissors,
            };
            var result = targetResultCode switch
            {
                'X' => GameResult.Loss,
                'Y' => GameResult.Draw,
                'Z' => GameResult.Win,
            };

            return CalculateForLinePart2(opponent, result);
        }

        private static RPSGameInfo CalculateForLinePart1(RPSResult opponent, RPSResult me)
        {
            if (opponent == me)
            {
                return Target(GameResult.Draw);
            }

            return (opponent, me) switch
            {
                (RPSResult.Paper, RPSResult.Scissors) or
                (RPSResult.Scissors, RPSResult.Rock) or
                (RPSResult.Rock, RPSResult.Paper) => Target(GameResult.Win),
                _ => Target(GameResult.Loss),
            };

            RPSGameInfo Target(GameResult result) => new(opponent, me, result);
        }
        private static RPSGameInfo CalculateForLinePart2(RPSResult opponent, GameResult result)
        {
            if (result is GameResult.Draw)
            {
                return Target(opponent);
            }

            return (opponent, result) switch
            {
                (RPSResult.Rock, GameResult.Win) or
                (RPSResult.Scissors, GameResult.Loss) => Target(RPSResult.Paper),
                (RPSResult.Scissors, GameResult.Win) or
                (RPSResult.Paper, GameResult.Loss) => Target(RPSResult.Rock),

                _ => Target(RPSResult.Scissors),
            };
            RPSGameInfo Target(RPSResult me) => new(opponent, me, result);
        }
    }

    private enum RPSResult
    {
        Rock,
        Paper,
        Scissors,
    }
    public enum GameResult
    {
        Loss,
        Draw,
        Win,
    }
}
