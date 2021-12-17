using AdventOfCode.Functions;
using AdventOfCSharp.Extensions;

namespace AdventOfCode.Problems.Year2018;

public class Day24 : Problem<int>
{
    private InfectionCombat combat;

    public override int SolvePart1()
    {
        return GetCombatUnits(combat);
    }
    public override int SolvePart2()
    {
        int maxBoost = 1;
        InfectionCombat targetEmboostedCombat = combat;

        while (true)
        {
            if (combat.SufficientlyEmboostsImmuneSystem(maxBoost, ref targetEmboostedCombat))
                break;

            maxBoost *= 2;
        }

        int minBoost = 0;

        while (minBoost < maxBoost)
        {
            int midBoost = (minBoost + maxBoost) / 2;

            if (combat.SufficientlyEmboostsImmuneSystem(midBoost, ref targetEmboostedCombat))
            {
                maxBoost = midBoost;
            }
            else
            {
                minBoost = midBoost + 1;
            }
        }

        return GetCombatUnits(targetEmboostedCombat);
    }

    private static int GetCombatUnits(InfectionCombat combat)
    {
        return combat.PlayCombat().AliveUnits;
    }

    protected override void LoadState()
    {
        combat = InfectionCombat.Parse(NormalizedFileContents.Trim());
    }

#nullable enable

    private enum DamageModification
    {
        Weakness = 2,
        Normal = 1,
        Immunity = 0,
    }

    private sealed class DamageModificationRules
    {
        private static readonly Regex modificationDescriptionPattern = new(@"(?'modification'\w*) to (?'types'.*)");

        private readonly string[] weaknesses;
        private readonly string[] immunities;

        public static DamageModificationRules Empty = new();

        private DamageModificationRules()
            : this(Array.Empty<string>(), Array.Empty<string>()) { }
        private DamageModificationRules(string[] weakDamageTypes, string[] immuneDamageTypes)
        {
            weaknesses = weakDamageTypes;
            immunities = immuneDamageTypes;
        }

        public DamageModification ModificationFor(string type)
        {
            if (weaknesses.Contains(type))
                return DamageModification.Weakness;

            if (immunities.Contains(type))
                return DamageModification.Immunity;

            return DamageModification.Normal;
        }

        public static DamageModificationRules Parse(Group capturedGroup)
        {
            if (!capturedGroup.Success)
                return Empty;

            return Parse(capturedGroup.Value);
        }
        private static DamageModificationRules Parse(string rawDescription)
        {
            var weaknesses = Array.Empty<string>();
            var immunities = weaknesses;
            var split = rawDescription.Split(';');
            // Overly modular for more kinds of modifications
            foreach (var section in split)
                AssignModifications(section);
            
            return new(weaknesses, immunities);

            void AssignModifications(string rawList)
            {
                var groups = modificationDescriptionPattern.Match(rawList).Groups;
                string modification = groups["modification"].Value;
                ref var modificationArray = ref ModificationArray(modification, ref weaknesses, ref immunities);
                modificationArray = groups["types"].Value.Split(", ");
            }

            static ref string[] ModificationArray(string kind, ref string[] weaknesses, ref string[] immunities)
            {
                switch (kind)
                {
                    case "weak":
                        return ref weaknesses;
                    default:
                        return ref immunities;
                }
            }
        }
    }
    private sealed record ArmyGroup(int InitialUnitCount, int UnitHP, DamageModificationRules Modifications, Attack Attack, int Initiative)
    {
        private static readonly Regex groupPattern = new(@"(?'units'\d*) units each with (?'hp'\d*) hit points (\((?'modifications'[\w ,;]*)\) )?with an attack that does (?'attackDamage'\d*) (?'damageType'\w*) damage at initiative (?'initiative'\d*)");

        public int CurrentUnitCount { get; private set; } = InitialUnitCount;

        public Attack EffectiveAttack => Attack.ForUnits(CurrentUnitCount);
        public int EffectivePower => EffectiveAttack.Damage;

        public bool InCombat => CurrentUnitCount > 0;

        public ArmyGroup Emboosted(int boost) => this with { Attack = Attack.Emboosted(boost) };

        public void TakeDamage(Attack attack)
        {
            int casualties = Casualties(attack);
            // Do not drop unit count below 0
            CurrentUnitCount = Math.Max(0, CurrentUnitCount - casualties);
        }

        public int Casualties(Attack attack) => ReceivedDamage(attack) / UnitHP;
        public int ReceivedDamage(Attack attack)
        {
            int multiplier = (int)Modifications.ModificationFor(attack.DamageType);
            return attack.Damage * multiplier;
        }

        public void ResetArmy()
        {
            CurrentUnitCount = InitialUnitCount;
        }

        public override int GetHashCode()
        {
            return Initiative;
        }

        public static ArmyGroup Parse(string rawGroup)
        {
            var groups = groupPattern.Match(rawGroup).Groups;

            // The regex thing should come out soon
            int units = groups["units"].Value.ParseInt32();
            int hp = groups["hp"].Value.ParseInt32();
            var modifications = DamageModificationRules.Parse(groups["modifications"]);
            int attackDamage = groups["attackDamage"].Value.ParseInt32();
            var damageType = groups["damageType"].Value;
            int initiative = groups["initiative"].Value.ParseInt32();

            return new(units, hp, modifications, new(attackDamage, damageType), initiative);
        }

        public static int SelectionOrder(ArmyGroup left, ArmyGroup right)
        {
            int effectivePowerComparison = right.EffectivePower.CompareTo(left.EffectivePower);
            if (effectivePowerComparison is not 0)
                return effectivePowerComparison;

            return AttackingOrder(left, right);
        }
        public static int AttackingOrder(ArmyGroup left, ArmyGroup right)
        {
            return right.Initiative.CompareTo(left.Initiative);
        }
        public static int TargetingOrder(Attack effectiveAttack, ArmyGroup left, ArmyGroup right)
        {
            int receivedDamageComparison = right.ReceivedDamage(effectiveAttack).CompareTo(left.ReceivedDamage(effectiveAttack));
            if (receivedDamageComparison is not 0)
                return receivedDamageComparison;

            return SelectionOrder(left, right);
        }
    }
    private record struct Attack(int Damage, string DamageType)
    {
        public Attack ForUnits(int units) => this with { Damage = Damage * units };
        public Attack Emboosted(int boost) => this with { Damage = Damage + boost };
    }

    private enum ArmySide
    {
        Infection,
        ImmuneSystem,
    }

    private sealed class Army
    {
        private readonly ArmyGroup[] groups;

        private HashSet<ArmyGroup> aliveGroups;

        public ArmySide Side { get; }

        public IEnumerable<ArmyGroup> AliveGroups => aliveGroups;
        public IEnumerable<ArmyGroup> OrderedForSelectionGroups => aliveGroups.ToArray().SortBy(ArmyGroup.SelectionOrder);

        public int AliveUnits => aliveGroups.Sum(group => group.CurrentUnitCount);

        public bool InCombat => aliveGroups.Any();

        private Army(ArmySide side, ArmyGroup[] armyGroups)
        {
            Side = side;
            groups = armyGroups;
        }

        public Army Emboosted(int boost) => new(Side, groups.SelectArray(group => group.Emboosted(boost)));

        public void UpdateAliveGroups()
        {
            aliveGroups.RemoveWhere(group => !group.InCombat);
        }
        private void ResetAliveStatus()
        {
            aliveGroups = new(groups);
        }

        public void Reset()
        {
            groups.ForEach(group => group.ResetArmy());
            ResetAliveStatus();
        }

        public static Army Parse(string[] rawGroups)
        {
            var side = ParseSide(rawGroups[0]);
            var groups = rawGroups.Skip(1).Select(ArmyGroup.Parse).ToArray();
            return new(side, groups);
        }
        private static ArmySide ParseSide(string armySide)
        {
            return armySide[..^1] switch
            {
                "Immune System" => ArmySide.ImmuneSystem,
                "Infection" => ArmySide.Infection,
            };
        }
    }

    private record struct ArmyGroupTarget(ArmyGroup Attacker, ArmyGroup Defender);

    private class ArmyGroupTargets
    {
        private readonly ArmyGroupTarget[] sortedTargets;

        public IEnumerable<ArmyGroupTarget> OrderedTargets => sortedTargets;

        public ArmyGroupTargets(IEnumerable<ArmyGroupTarget> targets)
        {
            sortedTargets = targets.ToArray().SortBy((left, right) => ArmyGroup.AttackingOrder(left.Attacker, right.Attacker));
        }
    }

    private class InfectionCombat
    {
        private readonly Army infection, immuneSystem;

        private InfectionCombat(Army infectionArmy, Army immuneSystemArmy)
        {
            infection = infectionArmy;
            immuneSystem = immuneSystemArmy;
        }

        public bool SufficientlyEmboostsImmuneSystem(int boost, ref InfectionCombat emboostedCombat)
        {
            var testingCombat = EmboostedImmuneSystem(boost);
            bool result = testingCombat.PlayCombat().Side is ArmySide.ImmuneSystem;

            if (result)
                emboostedCombat = testingCombat;

            return result;
        }
        public InfectionCombat EmboostedImmuneSystem(int boost) => new(infection, immuneSystem.Emboosted(boost));

        public void Reset()
        {
            infection.Reset();
            immuneSystem.Reset();
        }

        public Army PlayCombat()
        {
            Reset();
            while (true)
            {
                if (!infection.InCombat)
                    return immuneSystem;

                if (!immuneSystem.InCombat)
                    return infection;

                // This stalemate rule is not explained anywhere
                // Bless this Reddit thread:
                // https://www.reddit.com/r/adventofcode/comments/a9heik/2018_day_24_stalemate_part_1/
                bool stalemate = true;

                var targets = new List<ArmyGroupTarget>();

                RegisterTargets(targets, infection, immuneSystem);
                RegisterTargets(targets, immuneSystem, infection);

                var orderedTargets = new ArmyGroupTargets(targets).OrderedTargets;

                // Groups will never deal negative damage, and ones with 0 units will simply deal 0
                foreach (var target in orderedTargets)
                {
                    int previousUnits = target.Defender.CurrentUnitCount;
                    target.Defender.TakeDamage(target.Attacker.EffectiveAttack);
                    int newUnits = target.Defender.CurrentUnitCount;
                    if (newUnits != previousUnits)
                        stalemate = false;
                }

                if (stalemate)
                    return infection;

                infection.UpdateAliveGroups();
                immuneSystem.UpdateAliveGroups();
            }
        }

        private static void RegisterTargets(IList<ArmyGroupTarget> targets, Army attacker, Army defender)
        {
            var availableDefenders = defender.AliveGroups.ToList();
            foreach (var orderedAttacker in attacker.OrderedForSelectionGroups)
            {
                var effectiveAttack = orderedAttacker.EffectiveAttack;
                var chosen = availableDefenders.Min((left, right) => ArmyGroup.TargetingOrder(effectiveAttack, left, right));

                // Massive deadlock :(
                if (chosen.ReceivedDamage(effectiveAttack) is 0)
                    continue;

                availableDefenders.Remove(chosen);

                targets.Add(new(orderedAttacker, chosen));

                if (!availableDefenders.Any())
                    return;
            }
        }

        public static InfectionCombat Parse(string normalizedRawInfo)
        {
            var sections = normalizedRawInfo.Split("\n\n");
            var armies = sections.SelectArray(army => army.GetLines(false)).SelectArray(Army.Parse)!;
            Army infection = null!;
            Army immuneSystem = null!;

            SetCorrectSlot(0);
            SetCorrectSlot(1);

            return new(infection, immuneSystem);

            void SetCorrectSlot(int armyIndex)
            {
                var army = armies[armyIndex];
                ArmyReference(army, ref infection, ref immuneSystem) = army;
            }

            static ref Army ArmyReference(Army army, ref Army infection, ref Army immuneSystem)
            {
                switch (army.Side)
                {
                    case ArmySide.ImmuneSystem:
                        return ref immuneSystem;
                    default:
                        return ref infection;
                }
            }
        }
    }
}
