using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2015;

public class Day21 : Problem<int>
{
    private BossStats playerStats;

    public override int SolvePart1()
    {
        int minCost = int.MaxValue;
        ForAllItems(IterateItemPermutation);
        return minCost;

        void IterateItemPermutation(Player player, Boss boss)
        {
            if (player.ItemCost >= minCost)
                return;

            if (player.InitiateFight(boss))
                minCost = player.ItemCost;
        }
    }
    public override int SolvePart2()
    {
        int maxCost = int.MinValue;
        ForAllItems(IterateItemPermutation);
        return maxCost;

        void IterateItemPermutation(Player player, Boss boss)
        {
            if (player.ItemCost <= maxCost)
                return;

            if (!player.InitiateFight(boss))
                maxCost = player.ItemCost;
        }
    }

    protected override void LoadState()
    {
        playerStats = BossStats.Parse(FileContents);
    }
    protected override void ResetState()
    {
        playerStats = null;
    }

    private void ForAllItems(Action<Player, Boss> fightPreparation)
    {
        var player = new Player();
        var boss = playerStats.CreateBoss();

        // How the fuck else?
        foreach (var weapon in AvailableItems.Weapons)
        {
            player.Weapon = weapon;

            foreach (var armor in AvailableItems.Armor.ConcatSingleValue(null))
            {
                player.ArmorClothing = armor;

                var rings = AvailableItems.Rings;
                for (int i = 0; i <= rings.Length; i++)
                {
                    player.Ring0 = i < rings.Length ? rings[i] : null;

                    for (int j = i + 1; j <= rings.Length; j++)
                    {
                        player.Ring1 = j < rings.Length ? rings[j] : null;
                        fightPreparation(player, boss);
                    }
                }
            }
        }
    }

    private abstract class Character
    {
        // This was proven to be extremely useless, assuming that the boss's HP is always 100 (but why?)
        public int HP { get; protected set; } = 100;
        public abstract int Damage { get; }
        public abstract int Armor { get; }

        public void Attack(Character other)
        {
            other.HP -= GetDamageAgainst(other);
        }
        public int GetDamageAgainst(Character other)
        {
            return Math.Max(Damage - other.Armor, 1);
        }
        public bool InitiateFight(Character other)
        {
            int againstOther = GetDamageAgainst(other);
            int againstMe = other.GetDamageAgainst(this);
            int beatOtherRounds = (other.HP + againstOther - 1) / againstOther;
            int beatMeRounds = (HP + againstMe - 1) / againstMe;
            return beatOtherRounds <= beatMeRounds;
        }

        public override string ToString()
        {
            return $"HP: {HP} - Damage: {Damage} - Armor: {Armor}";
        }
    }
    private sealed class Boss : Character
    {
        public override int Damage { get; }
        public override int Armor { get; }

        public Boss(int damage, int armor) => (Damage, Armor) = (damage, armor);
    }
    private class Player : Character
    {
        private int? damage;
        private int? armor;

        private Weapon weapon;
        private ArmorClothing armorClothing;
        private Ring ring0;
        private Ring ring1;

        public int ItemCost { get; private set; }

        public override int Damage
        {
            get
            {
                if (damage is null)
                    damage = ItemDamage(weapon) + ItemDamage(ring0) + ItemDamage(ring1);
                return damage.Value;
            }
        }
        public override int Armor
        {
            get
            {
                if (armor is null)
                    armor = ItemArmor(armorClothing) + ItemArmor(ring0) + ItemArmor(ring1);
                return armor.Value;
            }
        }

        public Weapon Weapon
        {
            get => weapon;
            set => UpdateItemSlot(ref weapon, value);
        }
        public ArmorClothing ArmorClothing
        {
            get => armorClothing;
            set => UpdateItemSlot(ref armorClothing, value);
        }
        public Ring Ring0
        {
            get => ring0;
            set => UpdateItemSlot(ref ring0, value);
        }
        public Ring Ring1
        {
            get => ring1;
            set => UpdateItemSlot(ref ring1, value);
        }

        private static int ItemDamage(Item item) => (item?.Damage).GetValueOrDefault();
        private static int ItemArmor(Item item) => (item?.Armor).GetValueOrDefault();

        private void UpdateItemSlot<T>(ref T itemSlot, T newItem)
            where T : Item
        {
            int previousCost = itemSlot?.Cost ?? 0;
            int newCost = newItem?.Cost ?? 0;
            ItemCost += newCost - previousCost;
            itemSlot = newItem;
            damage = null;
            armor = null;
        }
    }

    private static class AvailableItems
    {
        private static Dictionary<string, Item> itemNames = new();

        public static ImmutableArray<Weapon> Weapons { get; }
        public static ImmutableArray<ArmorClothing> Armor { get; }
        public static ImmutableArray<Ring> Rings { get; }

        static AvailableItems()
        {
            Weapons = ImmutableArray.Create(new Weapon[]
            {
                new("Dagger", 8, 4),
                new("Shortsword", 10, 5),
                new("Warhammer", 25, 6),
                new("Longsword", 40, 7),
                new("Greataxe", 74, 8),
            });
            Armor = ImmutableArray.Create(new ArmorClothing[]
            {
                new("Leather", 13, 1),
                new("Chainmail", 31, 2),
                new("Splintmail", 53, 3),
                new("Bandedmail", 75, 4),
                new("Platemail", 102, 5),
            });
            Rings = ImmutableArray.Create(new Ring[]
            {
                new DamageRing("Damage +1", 25, 1),
                new DamageRing("Damage +2", 50, 2),
                new DamageRing("Damage +3", 100, 3),
                new ArmorRing("Armor +1", 20, 1),
                new ArmorRing("Armor +2", 40, 2),
                new ArmorRing("Armor +3", 80, 3),
            });

            itemNames = Weapons.AsEnumerable<Item>().ConcatMultiple(Armor, Rings).ToDictionary(i => i.Name);
        }
    }

    // mmm yes records
    private abstract record Item(string Name, int Cost, int Damage, int Armor);
    private abstract record DamageItem(string Name, int Cost, int Damage) : Item(Name, Cost, Damage, 0);
    private abstract record ArmorItem(string Name, int Cost, int Armor) : Item(Name, Cost, 0, Armor);

    private sealed record Weapon(string Name, int Cost, int Damage) : DamageItem(Name, Cost, Damage);
    private sealed record ArmorClothing(string Name, int Cost, int Armor) : ArmorItem(Name, Cost, Armor);

    private abstract record Ring(string Name, int Cost, int Damage, int Armor) : Item(Name, Cost, Damage, Armor);
    private sealed record DamageRing(string Name, int Cost, int Damage) : Ring(Name, Cost, Damage, 0);
    private sealed record ArmorRing(string Name, int Cost, int Armor) : Ring(Name, Cost, 0, Armor);

    private record BossStats(int HP, int Damage, int Armor)
    {
        private static readonly Regex statPattern = new(@"Hit Points: (?'hp'\d*)\s*Damage: (?'damage'\d*)\s*Armor: (?'armor'\d*)", RegexOptions.Compiled);

        public Boss CreateBoss() => new(Damage, Armor);

        public static BossStats Parse(string s)
        {
            var groups = statPattern.Match(s).Groups;
            int hp = groups["hp"].Value.ParseInt32();
            int damage = groups["damage"].Value.ParseInt32();
            int armor = groups["armor"].Value.ParseInt32();
            return new(hp, damage, armor);
        }
    }
}
