using AdventOfCode.Functions;
using AdventOfCSharp;
using Garyon.DataStructures;
using Garyon.Extensions;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2015;

public class Day22 : Problem<int>
{
    private BossStats bossStats;

    public override int SolvePart1()
    {
        return FindMinWinnableManaCost(FightDifficulty.Easy);
    }
    public override int SolvePart2()
    {
        return FindMinWinnableManaCost(FightDifficulty.Hard);
    }

    protected override void LoadState()
    {
        bossStats = BossStats.Parse(FileContents);
    }
    protected override void ResetState()
    {
        bossStats = null;
    }

    private int FindMinWinnableManaCost(FightDifficulty difficulty)
    {
        int minCost = int.MaxValue;
        ForAllFightPermutations(IterateFightPermutation, difficulty);
        return minCost;

        bool IterateFightPermutation(Fight fight)
        {
            if (fight.PlayerManaCost >= minCost)
                return false;

            if (fight.PlayerWon)
                minCost = fight.PlayerManaCost;
            return !fight.Over;
        }
    }

    private void ForAllFightPermutations(Predicate<Fight> stateUpdate, FightDifficulty difficulty)
    {
        var player = new Player();
        var boss = bossStats.CreateBoss();
        var fight = new Fight(player, boss);

        Iterate(fight);

        void Iterate(Fight fight)
        {
            foreach (var spell in AvailableSpells.Spells)
            {
                var clonedFight = fight.Clone();

                if (!clonedFight.PlayTurn(spell, difficulty))
                    continue;

                if (!stateUpdate(clonedFight))
                    continue;

                Iterate(clonedFight);
            }
        }
    }

    private enum WinningCharacter
    {
        None,
        Player,
        Boss,
    }
    private enum FightDifficulty
    {
        Easy,
        Hard,
    }

    private class Fight
    {
        public Player Player { get; }
        public Boss Boss { get; }

        public FlexibleDictionary<Type, Effect> Effects { get; private init; } = new();

        public int PlayerManaCost { get; private set; }

        public WinningCharacter Winner { get; private set; }
        public bool Over => Winner is not WinningCharacter.None;
        public bool PlayerWon => Winner is WinningCharacter.Player;

        public Fight(Player player, Boss boss)
        {
            Player = player;
            Boss = boss;
        }

        public Fight Clone()
        {
            return new(Player.Clone(), Boss.Clone())
            {
                Winner = Winner,
                Effects = new(Effects.Select(kvp => kvp.WithValue(kvp.Value?.Clone()))),
                PlayerManaCost = PlayerManaCost,
            };
        }

        public bool PlayTurn(Spell playerSpell, FightDifficulty difficulty)
        {
            return difficulty switch
            {
                FightDifficulty.Easy => PlayTurnEasy(playerSpell),
                FightDifficulty.Hard => PlayTurnHard(playerSpell),
            };
        }
        public bool PlayTurnEasy(Spell playerSpell)
        {
            return PlayTurn(playerSpell, () => true);
        }
        public bool PlayTurnHard(Spell playerSpell)
        {
            return PlayTurn(playerSpell, PrepareTurnExecution);

            bool PrepareTurnExecution()
            {
                Player.ApplyTurnHPReduction();

                if (DeclareBossWinner())
                    return false;

                return true;
            }
        }
        private bool PlayTurn(Spell playerSpell, Func<bool> prepareTurnExecution)
        {
            if (!prepareTurnExecution())
                return false;

            ApplyEffects();

            if (!BeforePlayTurn(playerSpell))
                return false;

            ExecuteTurnCommand(playerSpell);
            return true;
        }

        private bool BeforePlayTurn(Spell playerSpell)
        {
            if (!Player.CanCastSpells)
            {
                Winner = WinningCharacter.Boss;
                return false;
            }

            if (!playerSpell.CanBeCast(this))
                return false;

            return true;
        }
        private void ExecuteTurnCommand(Spell playerSpell)
        {
            if (DeclarePlayerWinner())
                return;

            Player.CastSpell(playerSpell);
            playerSpell.Cast(this);
            PlayerManaCost += playerSpell.ManaCost;

            if (DeclarePlayerWinner())
                return;

            ApplyEffects();

            if (DeclarePlayerWinner())
                return;

            Boss.AttackPhysical(Player);

            if (DeclareBossWinner())
                return;
        }

        private void ApplyEffects()
        {
            foreach (var effect in Effects.Values)
                effect?.ApplyEffect(this);
        }

        private bool DeclarePlayerWinner()
        {
            return DeclareWinner(Boss, WinningCharacter.Player);
        }
        private bool DeclareBossWinner()
        {
            return DeclareWinner(Player, WinningCharacter.Boss);
        }
        private bool DeclareWinner(Character losingCharacter, WinningCharacter winningCharacter)
        {
            bool declared = !losingCharacter.Alive;
            if (declared)
                Winner = winningCharacter;
            return declared;
        }
    }

    private abstract class Character
    {
        public int MaxHP { get; protected set; }
        public int HP { get; protected set; }
        public int Damage { get; set; }
        public int Armor { get; set; }

        public bool Alive => HP > 0;

        protected Character(int hp, int damage, int armor)
        {
            MaxHP = HP = hp;
            Damage = damage;
            Armor = armor;
        }

        public void Heal(int healing) => HP += healing;

        public void AttackPhysical(Character other)
        {
            other.ReduceHP(GetPhysicalDamageAgainst(other));
        }
        public void AttackMagic(Character other, int damage)
        {
            other.ReduceHP(GetMagicDamageAgainst(other, damage));
        }

        public int GetPhysicalDamageAgainst(Character other)
        {
            return Math.Max(Damage - other.Armor, 1);
        }
        public static int GetMagicDamageAgainst(Character other, int damage)
        {
            return Math.Max(damage, 1);
        }

        public void ApplyTurnHPReduction() => HP--;
        private void ReduceHP(int reduction) => HP -= reduction;

        public virtual void Reset()
        {
            HP = MaxHP;
        }

        public override string ToString()
        {
            return $"HP: {HP} - Damage: {Damage} - Armor: {Armor}";
        }
    }
    private sealed class Boss : Character
    {
        public Boss(int hp, int damage)
            : base(hp, damage, 0) { }

        public Boss Clone()
        {
            return new(MaxHP, Damage)
            {
                HP = HP,
            };
        }
    }
    private sealed class Player : Character
    {
        public const int InitialMana = 500;

        public int Mana { get; private set; } = InitialMana;
        public bool CanCastSpells => Mana >= AvailableSpells.MinimumSpellCost;

        public Player()
            : base(50, 0, 0) { }

        public override void Reset()
        {
            base.Reset();
            Mana = InitialMana;
        }

        public Player Clone()
        {
            return new()
            {
                MaxHP = MaxHP,
                HP = HP,
                Damage = Damage,
                Armor = Armor,
                Mana = Mana,
            };
        }

        public void RechargeMana(int mana) => Mana += mana;

        public void CastSpell(Spell spell)
        {
            Mana -= spell.ManaCost;
        }

        public override string ToString()
        {
            return $"{base.ToString()} - Mana: {Mana}";
        }
    }

    private static class AvailableSpells
    {
        public static ImmutableArray<Spell> Spells = ImmutableArray.Create(new Spell[]
        {
                new MagicMissile(),
                new Drain(),
                new Shield(),
                new Poison(),
                new Recharge(),
        });

        public static int MinimumSpellCost { get; }

        static AvailableSpells()
        {
            MinimumSpellCost = Spells.Min(s => s.ManaCost);
        }
    }

    private abstract class Spell
    {
        public abstract int ManaCost { get; }

        public virtual bool CanBeCast(Fight fight) => fight.Player.Mana >= ManaCost;

        public void Cast(Fight fight)
        {
            ApplyInitialSpellAction(fight);
            AfterCast(fight);
        }

        protected abstract void ApplyInitialSpellAction(Fight fight);
        protected virtual void AfterCast(Fight fight) { }

        protected static void DealMagicDamage(Fight fight, int damage)
        {
            fight.Player.AttackMagic(fight.Boss, damage);
        }
    }
    private abstract class EffectSpell<T> : Spell
        where T : Effect
    {
        protected abstract T CreateNewEffect(Fight fight);

        public sealed override bool CanBeCast(Fight fight) => base.CanBeCast(fight) && fight.Effects[typeof(T)] is null;

        protected sealed override void ApplyInitialSpellAction(Fight fight) { }

        protected sealed override void AfterCast(Fight fight) => fight.Effects[typeof(T)] = CreateNewEffect(fight);
    }

    private sealed class MagicMissile : Spell
    {
        public override int ManaCost => 53;

        protected override void ApplyInitialSpellAction(Fight fight)
        {
            DealMagicDamage(fight, 4);
        }
    }
    private sealed class Drain : Spell
    {
        public override int ManaCost => 73;

        protected override void ApplyInitialSpellAction(Fight fight)
        {
            DealMagicDamage(fight, 2);
            fight.Player.Heal(2);
        }
    }
    private sealed class Shield : EffectSpell<ShieldEffect>
    {
        public override int ManaCost => 113;

        protected sealed override ShieldEffect CreateNewEffect(Fight fight) => new(fight);
    }
    private sealed class Poison : EffectSpell<PoisonEffect>
    {
        public override int ManaCost => 173;

        protected sealed override PoisonEffect CreateNewEffect(Fight fight) => new(fight);
    }
    private sealed class Recharge : EffectSpell<RechargeEffect>
    {
        public override int ManaCost => 229;

        protected sealed override RechargeEffect CreateNewEffect(Fight fight) => new(fight);
    }

    private abstract class Effect
    {
        public int RemainingTurns { get; private set; }

        protected abstract int MaxTurns { get; }

        protected Effect(int remainingTurns)
        {
            RemainingTurns = remainingTurns;
        }
        protected Effect(Fight fight)
        {
            RemainingTurns = MaxTurns;
            OnEffectInitialization(fight);
        }

        // The true problems of abstraction - I can only blame the language for being insufficient for this
        public abstract Effect Clone();

        public void ApplyEffect(Fight fight)
        {
            ApplyEffectInternal(fight);
            RemainingTurns--;
            if (RemainingTurns <= 0)
            {
                fight.Effects[GetType()] = null;
                OnEffectWearOff(fight);
            }
        }

        protected virtual void OnEffectInitialization(Fight fight) { }
        protected virtual void ApplyEffectInternal(Fight fight) { }
        protected virtual void OnEffectWearOff(Fight fight) { }
    }
    private sealed class ShieldEffect : Effect
    {
        protected override int MaxTurns => 6;

        private ShieldEffect(int remainingTurns)
            : base(remainingTurns) { }
        public ShieldEffect(Fight fight)
            : base(fight) { }

        public override Effect Clone() => new ShieldEffect(RemainingTurns);

        protected override void OnEffectInitialization(Fight fight)
        {
            fight.Player.Armor += 7;
        }
        protected override void OnEffectWearOff(Fight fight)
        {
            fight.Player.Armor -= 7;
        }
    }
    private sealed class PoisonEffect : Effect
    {
        protected override int MaxTurns => 6;

        private PoisonEffect(int remainingTurns)
            : base(remainingTurns) { }
        public PoisonEffect(Fight fight)
            : base(fight) { }

        public override Effect Clone() => new PoisonEffect(RemainingTurns);

        protected override void ApplyEffectInternal(Fight fight)
        {
            fight.Player.AttackMagic(fight.Boss, 3);
        }
    }
    private sealed class RechargeEffect : Effect
    {
        protected override int MaxTurns => 5;

        private RechargeEffect(int remainingTurns)
            : base(remainingTurns) { }
        public RechargeEffect(Fight fight)
            : base(fight) { }

        public override Effect Clone() => new RechargeEffect(RemainingTurns);

        protected override void ApplyEffectInternal(Fight fight)
        {
            fight.Player.RechargeMana(101);
        }
    }

    private record BossStats(int HP, int Damage)
    {
        private static readonly Regex statPattern = new(@"Hit Points: (?'hp'\d*)\s*Damage: (?'damage'\d*)", RegexOptions.Compiled);

        public Boss CreateBoss() => new(HP, Damage);

        public static BossStats Parse(string s)
        {
            var groups = statPattern.Match(s).Groups;
            int hp = groups["hp"].Value.ParseInt32();
            int damage = groups["damage"].Value.ParseInt32();
            return new(hp, damage);
        }
    }
}
