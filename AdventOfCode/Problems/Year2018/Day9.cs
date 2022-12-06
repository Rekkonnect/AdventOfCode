using AdventOfCode.Utilities;
using System.Diagnostics;

namespace AdventOfCode.Problems.Year2018;

public partial class Day9 : Problem<long>
{
    private MarbleGameConfiguration config;

    public override long SolvePart1()
    {
        return PlayGame(config);
    }
    public override long SolvePart2()
    {
        return PlayGame(config with { LastMarble = config.LastMarble * 100 });
    }

    protected override void LoadState()
    {
        config = MarbleGameConfiguration.Parse(FileContents);
    }
    protected override void ResetState()
    {
        config = null;
    }

    private static long PlayGame(MarbleGameConfiguration configuration)
    {
        return new MarbleGame(configuration).PlayGame();
    }

    private partial record MarbleGameConfiguration(int PlayerCount, int LastMarble)
    {
        private static readonly Regex configurationPattern = ConfigurationRegex();

        public static MarbleGameConfiguration Parse(string raw)
        {
            var groups = configurationPattern.Match(raw).Groups;
            int players = groups["players"].Value.ParseInt32();
            int lastValue = groups["lastValue"].Value.ParseInt32();
            return new(players, lastValue);
        }

        [GeneratedRegex("(?'players'\\d*) players; last marble is worth (?'lastValue'\\d*) points")]
        private static partial Regex ConfigurationRegex();
    }

    private partial class MarbleGame
    {
        private readonly MarbleGameConfiguration configuration;

        public int PlayerCount => configuration.PlayerCount;
        public int LastMarble => configuration.LastMarble;

        public MarbleGame(MarbleGameConfiguration gameConfiguration)
        {
            configuration = gameConfiguration;
        }

        public long PlayGame()
        {
            var players = new Player[PlayerCount];
            var marbles = new CircularLinkedList<int>();
            marbles.Add(0);

            var selectedMarble = marbles.Head;

            for (int currentMarble = 1; currentMarble <= LastMarble; currentMarble++)
            {
                int currentPlayer = (currentMarble - 1) % players.Length;

                if (currentMarble % 23 is 0)
                {
                    var removed = selectedMarble.GetPrevious(7);
                    selectedMarble = removed.Next;
                    marbles.Remove(removed);
                    players[currentPlayer].RegisterMarble(currentMarble, removed.Value);
                }
                else
                {
                    selectedMarble = marbles.InsertAfter(selectedMarble.Next, currentMarble);
                }
            }

            return players.Max(p => p.Score);
        }
    }

    private struct Player
    {
        public long Score { get; private set; }

        public void RegisterMarble(int keptMarble, int removedMarble)
        {
            Debug.Assert(keptMarble % 23 is 0);
            Score += keptMarble + removedMarble;
        }
    }
}
