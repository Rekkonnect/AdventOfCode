namespace AdventOfCode.Problems.Year2021;

public partial class Day21 : Problem<int, ulong>
{
    private PlayerContainer container;

    public override int SolvePart1()
    {
        var game = new DiracDiceGame(container);
        game.PlayToEnd();
        return game.Loser.Score * game.TotalDieRolls;
    }
    public override ulong SolvePart2()
    {
        var evaluator = new QuantumGameEvaluator();
        return evaluator.EvaluateWins(new(container)).MaxWins;
    }

    protected override void LoadState()
    {
        container = PlayerContainer.Parse(FileLines);
    }
    protected override void ResetState()
    {
        container = null;
    }

#nullable enable

    // NextPlayerID would result in a binding failure
    // The binder does not try to bind to a method whose name matches one of an
    // available from a nesting parent member
    // TODO: Create proposal/report
    private static int GetNextPlayerID(int id)
    {
        return (id + 1) % 2;
    }

    private class QuantumGameEvaluator
    {
        private readonly QuantumGameStateCache cache = new();

        public PlayerWins EvaluateWins(PlayerContainerState players)
        {
            return EvaluateWins(0, players);
        }
        public PlayerWins EvaluateWins(int advancingPlayerID, PlayerContainerState players)
        {
            ref var cached = ref cache.StateFor(advancingPlayerID, players);
            if (cached.IsValid)
                return cached.PlayerWins;

            return EvaluateIterate(advancingPlayerID, players);
        }
        private PlayerWins EvaluateIterate(int advancingPlayerID, PlayerContainerState players)
        {
            ref var stateRef = ref cache.StateFor(advancingPlayerID, players);

            int nextPlayerID = GetNextPlayerID(advancingPlayerID);
            var wins = new PlayerWins();

            for (int die1 = 1; die1 <= 3; die1++)
            {
                for (int die2 = 1; die2 <= 3; die2++)
                {
                    for (int die3 = 1; die3 <= 3; die3++)
                    {
                        int moves = die1 + die2 + die3;
                        var advanced = players.WithAdvancedPlayer(advancingPlayerID, moves);
                        wins += EvaluateWinning(nextPlayerID, advanced);
                    }
                }
            }

            return SetState(ref stateRef, new(players, wins)).PlayerWins;
        }
        private PlayerWins EvaluateWinning(int advancingPlayerID, PlayerContainerState players)
        {
            int winningPlayerID = players.WinningPlayerID(21);
            if (winningPlayerID > -1)
            {
                return PlayerWins.WinFor(winningPlayerID);
            }

            return EvaluateWins(advancingPlayerID, players);
        }

        private static ref CachedQuantumPlayerState SetState(ref CachedQuantumPlayerState stateRef, CachedQuantumPlayerState setState)
        {
            stateRef = setState;
            return ref stateRef;
        }
    }

    private sealed class QuantumGameStateCache
    {
        private readonly StartingPlayerTable startingPlayerTable;

        public QuantumGameStateCache()
        {
            startingPlayerTable = new();
        }

        public ref CachedQuantumPlayerState StateFor(int startingPlayerID, PlayerContainerState players)
        {
            return ref startingPlayerTable.StateFor(startingPlayerID, players);
        }

        private sealed class StartingPlayerTable
        {
            private readonly ScoreTable player1, player2;

            public StartingPlayerTable()
            {
                player1 = new();
                player2 = new();
            }

            private ScoreTable TableFor(int startingPlayerID)
            {
                return startingPlayerID switch
                {
                    0 => player1,
                    1 => player2,
                };
            }
            public ref CachedQuantumPlayerState StateFor(int startingPlayerID, PlayerContainerState players)
            {
                return ref TableFor(startingPlayerID)[players];
            }

            private sealed class ScoreTable
            {
                private const int maxScore = 21;

                private readonly PositionTable[,] scores = new PositionTable[maxScore, maxScore];

                public ScoreTable()
                {
                    for (int i = 0; i < maxScore; i++)
                        for (int j = 0; j < maxScore; j++)
                            scores[i, j] = new();
                }

                private PositionTable PositionTableForPlayers(PlayerContainerState players)
                {
                    return scores[players.Player1State.Score, players.Player2State.Score];
                }
                public ref CachedQuantumPlayerState this[PlayerContainerState players]
                {
                    get => ref PositionTableForPlayers(players)[players];
                }

                private sealed class PositionTable
                {
                    private const int maxPosition = 10;

                    private readonly CachedQuantumPlayerState[,] positions = new CachedQuantumPlayerState[maxPosition, maxPosition];

                    public ref CachedQuantumPlayerState this[PlayerContainerState players]
                    {
                        get => ref positions[players.Player1State.CurrentPosition - 1, players.Player2State.CurrentPosition - 1];
                    }
                }
            }
        }
    }

    private record struct PlayerWins(ulong Player1, ulong Player2)
    {
        public ulong MaxWins => Math.Max(Player1, Player2);

        public static PlayerWins operator +(PlayerWins left, PlayerWins right)
        {
            return new(left.Player1 + right.Player1, right.Player2 + left.Player2);
        }

        public static PlayerWins WinFor(int playerID) => playerID switch
        {
            0 => new(1, 0),
            1 => new(0, 1),
        };
    }

    private record struct CachedQuantumPlayerState(PlayerContainerState State, PlayerWins PlayerWins)
    {
        public bool IsValid => State.IsValid;
    }
    private record struct PlayerContainerState(PlayerState Player1State, PlayerState Player2State)
    {
        public bool IsValid => Player1State.IsValid;

        public PlayerContainerState(PlayerContainer container)
            : this(container[0].PlayerState, container[1].PlayerState) { }

        public int WinningPlayerID(int winningScore)
        {
            if (Player1State.Score >= winningScore)
                return 0;
            if (Player2State.Score >= winningScore)
                return 1;

            return -1;
        }

        public PlayerContainerState WithAdvancedPlayer(int advancingPlayerID, int moves)
        {
            return advancingPlayerID switch
            {
                0 => this with { Player1State = Player1State.AdvancedBy(moves) },
                1 => this with { Player2State = Player2State.AdvancedBy(moves) },
            };
        }
    }

    private sealed record PlayerContainer
    {
        private readonly Player[] players = new Player[2];

        private PlayerContainer() { }

        public PlayerContainer CloneInitial()
        {
            var result = new PlayerContainer();
            for (int i = 0; i < players.Length; i++)
                result.players[i] = players[i].CloneInitial();
            return result;
        }

        public Player this[int index] => players[index];

        public static PlayerContainer Parse(string[] rawPlayers)
        {
            var container = new PlayerContainer();
            ParsePlayer(0);
            ParsePlayer(1);
            return container;

            void ParsePlayer(int rawIndex)
            {
                var parsed = Player.Parse(rawPlayers[rawIndex]);
                container.players[parsed.ID - 1] = parsed;
            }
        }
    }

    private class DiracDiceGame
    {
        private readonly PlayerContainer players;
        private readonly DeterministicDie die = new();
        private int currentPlayerID = 0;

        public Player CurrentPlayer => players[currentPlayerID];
        public Player NextPlayer => players[NextPlayerID()];

        public Player? Winner { get; private set; }
        public Player? Loser => IsAlive ? null : players[GetNextPlayerID(Winner!.ID - 1)];

        public int TotalDieRolls => die.TotalRolls;

        public bool IsAlive => Winner is null;

        public const int WinnerScore = 1000;

        public DiracDiceGame(PlayerContainer playerContainer)
        {
            players = playerContainer.CloneInitial();
        }

        public Player PlayToEnd()
        {
            while (IsAlive)
                PlayRound();
            return Winner!;
        }

        public void PlayRound()
        {
            int moves = die.RollNext3();
            CurrentPlayer.Advance(moves);
            if (CurrentPlayer.Score >= WinnerScore)
            {
                Winner = CurrentPlayer;
            }

            AdvanceCurrentPlayer();
        }
        private void AdvanceCurrentPlayer()
        {
            currentPlayerID = NextPlayerID();
        }
        private int NextPlayerID()
        {
            return GetNextPlayerID(currentPlayerID);
        }
    }

    private class DeterministicDie : Die
    {
        private const int sides = 100;
        private int current;

        public override int Sides => sides;

        public DeterministicDie()
        {
            current = 1;
        }

        public int RollNext3()
        {
            RegisterRolls(3);
            return ConsumeDieSide()
                 + ConsumeDieSide()
                 + ConsumeDieSide();
        }
        private int ConsumeDieSide()
        {
            int result = current;
            current++;
            if (current > sides)
                current = 1;
            return result;
        }
    }
    private abstract class Die
    {
        public abstract int Sides { get; }
        public int TotalRolls { get; private set; }
        
        protected void RegisterRolls(int rolls)
        {
            TotalRolls += rolls;
        }
    }

    private record struct PlayerState(int CurrentPosition, int Score)
    {
        public bool IsValid => CurrentPosition > 0;

        public PlayerState(int startingPosition)
            : this(startingPosition, 0) { }

        public PlayerState AdvancedBy(int moves)
        {
            int advancedIndex = CurrentPosition + moves - 1;
            int nextPosition = advancedIndex % 10 + 1;
            return new(nextPosition, Score + nextPosition);
        }
    }

    private partial class Player
    {
        private static readonly Regex playerPattern = PlayerRegex();

        public int ID { get; }

        public int Score => PlayerState.Score;
        public PlayerState PlayerState { get; private set; }

        public Player(int id, int startingPosition)
        {
            ID = id;
            PlayerState = new(startingPosition);
        }

        public Player CloneInitial()
        {
            return new Player(ID, PlayerState.CurrentPosition);
        }

        public void Advance(int moves)
        {
            PlayerState = PlayerState.AdvancedBy(moves);
        }

        public static Player Parse(string rawPlayer)
        {
            var groups = playerPattern.Match(rawPlayer).Groups;
            int playerID = groups["id"].Value.ParseInt32();
            int startingPosition = groups["cards"].Value.ParseInt32();
            return new(playerID, startingPosition);
        }

        [GeneratedRegex("Player (?'id'\\d) starting position: (?'cards'\\d*)")]
        private static partial Regex PlayerRegex();
    }
}
