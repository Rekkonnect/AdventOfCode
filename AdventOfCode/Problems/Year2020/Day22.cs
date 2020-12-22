//#define DEBUGLOGGING

#if DEBUG && DEBUGLOGGING
    //#define DEBUGLOGGING_PLAYERSTATES
    //#define DEBUGLOGGING_GAMEINITIALIZATIONS
    //#define DEBUGLOGGING_INFINITELOOPDETECTIONS
#endif

using AdventOfCode.Functions;
using Garyon.Extensions;
using Garyon.Objects.Enumerators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day22 : Problem2<int>
    {
        private List<Player> players;

        public override int SolvePart1()
        {
            return new CombatGame(players).PlayToEnd().Score;
        }
        public override int SolvePart2()
        {
            return new RecursiveCombatGame(players).PlayToEnd().Score;
        }

        protected override void LoadState()
        {
            players = new List<Player>();

            var rawPlayers = NormalizedFileContents.Split("\n\n");
            foreach (var rawPlayer in rawPlayers)
            {
                var lines = rawPlayer.GetLines(false);
                int playerID = lines[0][("Player ".Length)..^1].ParseInt32();
                var cards = lines.Skip(1).Select(v => new Card(v.ParseInt32()));
                players.Add(new Player(playerID, cards));
            }
        }
        protected override void ResetState()
        {
            players = null;
        }

        private class PlayerDeckState
        {
            public Player Player { get; }
            public Deck Deck { get; }

            public PlayerDeckState(Player player)
            {
                Player = player;
                Deck = new(player.Deck);
            }

            public bool Equals(PlayerDeckState state) => Player.PlayerID == state.Player.PlayerID && Deck.Equals(state.Deck);
            public override bool Equals(object obj) => obj is PlayerDeckState state && Equals(state);
            public override int GetHashCode() => HashCode.Combine(Player, Deck);
        }
        private class RoundState : IEnumerable<PlayerDeckState>
        {
            public IEnumerable<PlayerDeckState> PlayerDeckStates { get; init; }

            public RoundState(IEnumerable<Player> players)
            {
                var list = (PlayerDeckStates = new List<PlayerDeckState>()) as List<PlayerDeckState>;
                foreach (var p in players)
                    list.Add(new PlayerDeckState(p));
            }

            public bool Equals(RoundState state)
            {
                if (PlayerDeckStates.Count() != state.PlayerDeckStates.Count())
                    return false;

                var parallel = new ParallellyEnumerable<PlayerDeckState, PlayerDeckState>(this, state);

                foreach (var (s1, s2) in parallel)
                    if (!s1.Equals(s2))
                        return false;
                return true;
            }
            public override bool Equals(object obj) => obj is RoundState state && Equals(state);

            public IEnumerator<PlayerDeckState> GetEnumerator() => PlayerDeckStates.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class RecursiveCombatGame : CombatGame
        {
            private List<RoundState> previousRoundStates = new();

            public RecursiveCombatGame(IEnumerable<Player> players)
                : base(players)
            {
            }

            protected override Player DetermineRoundWinner(SortedDictionary<Card, Player> playedCardDictionary)
            {
                foreach (var c in playedCardDictionary)
                {
                    int cardValue = c.Key.Value;
                    var player = c.Value;
                    if (player.Deck.Count < cardValue)
                        return base.DetermineRoundWinner(playedCardDictionary);
                }

#if DEBUGLOGGING_PLAYERSTATES
                Console.WriteLine($"Playing a sub-game to determine the winner...");
#endif
                // If the players can recurse a new game given their current decks, then so be it
                var newGamePlayers = new List<Player>();
                foreach (var c in playedCardDictionary)
                    newGamePlayers.Add(c.Value.Clone(c.Key.Value));
                newGamePlayers.Sort((a, b) => a.PlayerID.CompareTo(b.PlayerID));

                var winner = new RecursiveCombatGame(newGamePlayers).PlayToEnd();

#if DEBUGLOGGING_GAMEINITIALIZATIONS
                Console.WriteLine($"Returning back to the original game...\n\n=== Previous Game ===\n");
#endif

                return winner;
            }
            protected override void SortWinningCards(List<Card> cards, Player winningPlayer, Card winningPlayerCard)
            {
                cards.Remove(winningPlayerCard);
                cards.InsertAtStart(winningPlayerCard);
            }
            protected override bool DeterminePrematureGameTermination()
            {
                var currentState = CurrentRoundState;

                var index = previousRoundStates.FindIndex(s => s.Equals(currentState));
                if (index > -1)
                {
#if DEBUGLOGGING_INFINITELOOPDETECTIONS
                    Console.WriteLine($"Index: {index}\n===");
                    foreach (var state in currentState.PlayerDeckStates)
                        Console.WriteLine($"Player {state.Player.PlayerID} card count: {state.Deck.Count}");
                    Console.WriteLine();
#endif
                    return true;
                }
                previousRoundStates.Add(currentState);
                return false;
            }
        }
        private class CombatGame
        {
            private List<Player> alivePlayers;

            public IEnumerable<Player> Players { get; private init; }
            public int PlayerCount { get; }

            public bool IsOver => alivePlayers.Count == 1;
            public Player Winner { get; private set; }

            public RoundState CurrentRoundState => new(Players);

            public CombatGame(IEnumerable<Player> players)
            {
                Players = players;
                alivePlayers = new(players);
                PlayerCount = alivePlayers.Count;
            }

            public Player PlayToEnd()
            {
#if DEBUGLOGGING_GAMEINITIALIZATIONS
                Console.WriteLine($"\n=== New Game ===\n");
#endif
                while (!IsOver)
                    PlayRound();
                return Winner;
            }

            public void PlayRound()
            {
#if DEBUGLOGGING_PLAYERSTATES
                Console.WriteLine("-- New Round --");
                foreach (var p in Players)
                    Console.WriteLine($"Player {p.PlayerID}'s deck: {p.Deck}");
#endif

                var playedCardDictionary = new SortedDictionary<Card, Player>();
                foreach (var p in alivePlayers)
                {
                    var drawnCard = p.DrawTopCard();
                    playedCardDictionary.Add(drawnCard, p);
#if DEBUGLOGGING_PLAYERSTATES
                    Console.WriteLine($"Player {p.PlayerID} plays: {drawnCard}");
#endif
                }

                if (DeterminePrematureGameTermination())
                {
                    var winner = GetPrematureTerminationWinner();
                    alivePlayers.Clear();
                    alivePlayers.Add(Winner = winner);
#if DEBUGLOGGING_GAMEINITIALIZATIONS
                    Console.WriteLine($"The winner of the current game is player {winner.PlayerID}");
#endif
                    return;
                }
                
                var cardList = new List<Card>(PlayerCount);
                foreach (var card in playedCardDictionary)
                    cardList.Add(card.Key);
                cardList.Reverse();

                var roundWinner = DetermineRoundWinner(playedCardDictionary);
                var winningPlayer = alivePlayers.Single(p => p.PlayerID == roundWinner.PlayerID);
                SortWinningCards(cardList, roundWinner, playedCardDictionary.Single(kvp => kvp.Value.PlayerID == winningPlayer.PlayerID).Key);
                winningPlayer.ClaimWinningCards(cardList);

                alivePlayers.RemoveAll(p => p.HasLost);

                if (IsOver)
                {
                    Winner = alivePlayers.First();
#if DEBUGLOGGING_GAMEINITIALIZATIONS
                    Console.WriteLine($"The winner of the current game is player {Winner.PlayerID}");
#endif
                }
            }

            protected virtual Player GetPrematureTerminationWinner()
            {
                return Players.Single(p => p.PlayerID == 1);
            }
            protected virtual bool DeterminePrematureGameTermination()
            {
                return false;
            }
            protected virtual Player DetermineRoundWinner(SortedDictionary<Card, Player> playedCardDictionary)
            {
                return playedCardDictionary.Last().Value;
            }
            protected virtual void SortWinningCards(List<Card> cards, Player winningPlayer, Card winningPlayerCard) { }
        }

        private class Player
        {
            public Deck Deck { get; }

            public int PlayerID { get; init; }

            public int Score
            {
                get
                {
                    var cards = Deck.CardArray;
                    int score = 0;
                    for (int i = 1; i <= cards.Length; i++)
                        score += cards[^i].Value * i;
                    return score;
                }
            }
            public bool HasLost => Deck.IsEmpty;

            public Player(int playerID, IEnumerable<Card> startingCards)
            {
                PlayerID = playerID;
                Deck = new Deck(startingCards);
            }

            public Player Clone() => Clone(Deck.Count);
            public Player Clone(int firstCards) => new(PlayerID, Deck.Take(firstCards));

            public Card DrawTopCard() => Deck.DrawTopCard();
            public void ClaimWinningCards(IEnumerable<Card> winningCards)
            {
                Deck.ClaimWinningCards(winningCards);
            }
        }
        private class Deck : IEnumerable<Card>
        {
            private Queue<Card> cards;

            public bool IsEmpty => !cards.Any();
            public Card[] CardArray => cards.ToArray();
            public int Count => cards.Count;

            public Deck(IEnumerable<Card> startingCards)
            {
                cards = new(startingCards);
            }

            public Card DrawTopCard() => cards.Dequeue();
            public void ClaimWinningCards(IEnumerable<Card> winningCards)
            {
                cards.EnqueueRange(winningCards);
            }

            public IEnumerator<Card> GetEnumerator() => cards.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool Equals(Deck deck)
            {
                if (cards.Count != deck.cards.Count)
                    return false;

                var parallel = new ParallellyEnumerable<Card, Card>(this, deck);
                foreach (var (c1, c2) in parallel)
                    if (c1 != c2)
                        return false;
                
                return true;
            }
            public override bool Equals(object obj) => obj is Deck deck && Equals(deck);
            public override int GetHashCode()
            {
                var hashCode = new HashCode();
                foreach (var c in cards)
                    hashCode.Add(c);
                return hashCode.ToHashCode();
            }
            public override string ToString() => this.Select(s => s.ToString()).Combine(", ");
        }
        private sealed record Card(int Value) : IComparable<Card>, IEquatable<Card>
        {
            public int CompareTo(Card other) => Value.CompareTo(other.Value);
            public bool Equals(Card other) => Value == other.Value;
            public override int GetHashCode() => Value.GetHashCode();
            public override string ToString() => Value.ToString();

            public static int AscendingValueComparer(Card a, Card b) => a.CompareTo(b);
            public static int DescendingValueComparer(Card a, Card b) => b.CompareTo(a);
        }
    }
}
