using AdventOfCode.Utilities;
using System.Threading;
using static System.Convert;

namespace AdventOfCode.Problems.Year2019;

public class Day14 : Problem<long>
{
    public override long SolvePart1() => General(true, 0, Part1Returner);
    [PartSolution(PartSolutionStatus.Refactoring)]
    public override long SolvePart2() => General(false, 1_000_000_000_000, Part2Returner);

    private long Part1Returner(ChemicalResourceBank bank, long min) => min;
    private long Part2Returner(ChemicalResourceBank bank, long min) => bank.ProduceAllCapableFuel();

    private long General(bool canProduceORE, long startingORE, Returner returner)
    {
        var lines = FileLines;
        var chemicals = new KeyedObjectDictionary<string, Chemical>();
        var reactions = new ChemicalReaction[lines.Length];
        for (int i = 0; i < lines.Length; i++)
            reactions[i] = ChemicalReaction.ParseReaction(lines[i], chemicals);
        ChemicalReaction.ParseReaction("1 ORE => 1 ORE", chemicals);

        var banks = ChemicalResourceBank.GetBanks(chemicals.Values.ToArray(), canProduceORE);
        long min = long.MaxValue;
        ChemicalResourceBank bank = null;
        foreach (var b in banks)
        {
            b.CurrentORE = startingORE;
            var ore = b.ProduceFuel();
            if (ore < min)
            {
                bank = b;
                min = ore;
            }
        }
        bank.CurrentORE = startingORE;
        return returner(bank, min);
    }

    public delegate long Returner(ChemicalResourceBank bank, long min);

    public class Chemical : IKeyedObject<string>
    {
        public readonly string Name;

        public readonly List<ChemicalReaction> InvolvedIngredients = new List<ChemicalReaction>();
        public readonly List<ChemicalReaction> InvolvedProducts = new List<ChemicalReaction>();

        public string Key => Name;

        public Chemical(string name) => Name = name;

        public ProducantChemical[] GetProductionReactionIngredients()
        {
            var ingredients = new ProducantChemical[InvolvedProducts.Count];
            for (int i = 0; i < InvolvedProducts.Count; i++)
                ingredients[i] = new ProducantChemical(0, this, i);
            return ingredients;
        }

        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name;
    }

    public class ChemicalResourceBank : KeyedObjectDictionary<Chemical, ProducantChemical>
    {
        private readonly List<ChemicalReaction> reactions = new();
        private long totalORE;

        public bool CanProduceORE { get; private set; }

        public long CurrentORE
        {
            get => this["ORE"].Amount;
            set => this["ORE"].Amount = value;
        }

        public ChemicalResourceBank(bool canProduceORE = true)
            : base()
        {
            CanProduceORE = canProduceORE;
        }
        public ChemicalResourceBank(KeyedObjectDictionary<Chemical, ProducantChemical> d, bool canProduceORE = true)
            : base(d)
        {
            CanProduceORE = canProduceORE;
        }

        public ChemicalResourceBank Clone() => new(this);

        public long ProduceAllCapableFuel()
        {
            // Produce FUEL once and find out how many resources are left
            long startingORE = CurrentORE;
            ProduceFuel();
            long consumedORE = startingORE - CurrentORE;
            long remainingIterations = CurrentORE / consumedORE;
            long totalIterations = remainingIterations + 1;
            CurrentORE -= remainingIterations * consumedORE;
            foreach (var v in this)
                if (v.Chemical.Name != "ORE")
                    v.Amount *= totalIterations;

            // Can this be optimized further?
            while (ProduceFuel() != -2) ;
            //PrintCurrentBankState(null, null);
            return this["FUEL"].Amount;
        }
        public long ProduceFuel() => ProduceFuel(1);
        public long ProduceFuel(int amount) => ProduceIngredient(new ChemicalIngredient(amount, Values.Where(i => i.Chemical.Name == "FUEL").First().Chemical));
        public long ProduceIngredient(ChemicalIngredient ingredient)
        {
            if (ingredient.Amount <= 0)
                return -1;

            var bankChemical = this[ingredient.Chemical];
            int operations = bankChemical.GetRequiredOperationsToProduceAmount(ingredient.Amount);

            if (bankChemical.Chemical.Name == "ORE")
            {
                if (!CanProduceORE)
                    return -2;
                totalORE += ingredient.Amount;
            }
            else
            {
                foreach (var i in bankChemical.ProductionReaction.Ingredients)
                {
                    var requiredBankChemical = this[i.Chemical];
                    long requiredAmount = i.Amount * operations;
                    long requiredToProduce = requiredAmount - requiredBankChemical.Amount;
                    if (ProduceIngredient(new ChemicalIngredient(requiredToProduce, i.Chemical)) == -2)
                        return -2;
                    requiredBankChemical.Amount -= requiredAmount;
                }
            }

            bankChemical.IncreaseAmountByOperations(operations);

            return totalORE;
        }

        public void PrintReactionList()
        {
            Console.WriteLine();
            foreach (var r in reactions)
                Console.WriteLine(r);
        }

        public ProducantChemical this[string chemicalName] => Values.Where(v => v.Chemical.Name == chemicalName).First();

        private void PrintCurrentBankState(ProducantChemical produced, List<ProducantChemical> consumed)
        {
            Console.SetCursorPosition(0, startingCursorTopPosition);
            foreach (var c in this)
            {
                if (produced?.Chemical == c.Chemical)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (consumed?.Find(ch => ch.Chemical == c.Chemical) != null)
                    Console.ForegroundColor = ConsoleColor.Red;
                else
                    Console.ResetColor();

                Console.WriteLine(c.AsIngredient().ToPaddedString());
            }
            Console.ResetColor();
            Thread.Sleep(5);
            //Console.ReadKey(true);
        }
        private readonly int startingCursorTopPosition = Console.CursorTop + 1;

        public static List<ChemicalResourceBank> GetBanks(Chemical[] chemicals, bool canProduceORE)
        {
            // This looks like code made by my 15-year-old self
            var banks = new List<ChemicalResourceBank>();
            var productionDictionary = new Dictionary<Chemical, ProducantChemical[]>(chemicals.Length);
            var productionIndices = new Dictionary<Chemical, int>(chemicals.Length);
            foreach (var i in chemicals)
            {
                productionDictionary.Add(i, i.GetProductionReactionIngredients());
                productionIndices.Add(i, 0);
            }

            while (productionIndices[chemicals[0]] < chemicals[0].InvolvedProducts.Count)
            {
                var bank = new ChemicalResourceBank(canProduceORE);
                foreach (var i in chemicals)
                    bank.Add(productionDictionary[i][productionIndices[i]]);

                banks.Add(bank);

                productionIndices[chemicals.Last()]++;

                for (int i = chemicals.Length - 1; i > 0; i--)
                    if (productionIndices[chemicals[i]] >= chemicals[i].InvolvedProducts.Count)
                    {
                        productionIndices[chemicals[i]] = 0;
                        productionIndices[chemicals[i - 1]]++;
                    }
                    else
                        break;
            }

            return banks;
        }
    }
    public class ChemicalReaction
    {
        public ChemicalIngredient[] Ingredients;
        public ChemicalIngredient Product;

        public ChemicalReaction(ChemicalIngredient[] ingredients, ChemicalIngredient product)
        {
            (Ingredients, Product) = (ingredients, product);
            foreach (var i in ingredients)
                i.Chemical.InvolvedIngredients.Add(this);
            product.Chemical.InvolvedProducts.Add(this);
        }

        public static ChemicalReaction ParseReaction(string s, KeyedObjectDictionary<string, Chemical> chemicals)
        {
            var split = s.Split(" => ");
            var ingredients = split[0].Split(", ").Select(i => ChemicalIngredient.ParseIngredient(i, chemicals)).ToArray();
            var product = ChemicalIngredient.ParseIngredient(split[1], chemicals);
            return new ChemicalReaction(ingredients, product);
        }

        public static ChemicalReaction operator *(ChemicalReaction reaction, int amount) => new ChemicalReaction(reaction.Ingredients.Select(i => i * amount).ToArray(), reaction.Product * amount);
        public static ChemicalReaction operator *(int amount, ChemicalReaction reaction) => new ChemicalReaction(reaction.Ingredients.Select(i => i * amount).ToArray(), reaction.Product * amount);

        public override string ToString() => $"{Ingredients.Select(i => i.ToString()).Aggregate((a, b) => $"{a}, {b}")} => {Product}";
    }
    public class ChemicalIngredient : IKeyedObject<Chemical>
    {
        public long Amount;
        public Chemical Chemical;

        public Chemical Key => Chemical;
        public int InvolvedProductsCount => Chemical.InvolvedProducts.Count;

        public ChemicalIngredient(long amount, Chemical chemical) => (Amount, Chemical) = (amount, chemical);

        public static ChemicalIngredient ParseIngredient(string s, KeyedObjectDictionary<string, Chemical> chemicals)
        {
            var split = s.Split(' ');
            int amount = ToInt32(split[0]);
            string name = split[1];
            chemicals.TryGetValue(name, out var chemical);
            if (chemical == null)
                chemicals.Add(chemical = new Chemical(name));
            return new ChemicalIngredient(amount, chemical);
        }

        public static ChemicalIngredient operator *(ChemicalIngredient ingredient, int amount) => new ChemicalIngredient(ingredient.Amount * amount, ingredient.Chemical);
        public static ChemicalIngredient operator *(int amount, ChemicalIngredient ingredient) => new ChemicalIngredient(amount * ingredient.Amount, ingredient.Chemical);

        public override string ToString() => $"{Amount} {Chemical}";
        public string ToPaddedString() => $"{Amount,10} {Chemical}";
    }
    public class ProducantChemical : IKeyedObject<Chemical>
    {
        public long Amount;
        public Chemical Chemical;
        public int ProductionReactionIndex;

        public ChemicalReaction ProductionReaction => ProductionReactionIndex > -1 ? Chemical.InvolvedProducts[ProductionReactionIndex] : null;
        public long ProducedAmountPerOperation => ProductionReaction.Product.Amount;

        public Chemical Key => Chemical;

        public ProducantChemical(long amount, Chemical chemical, int productionReactionIndex = -1) => (Amount, Chemical, ProductionReactionIndex) = (amount, chemical, productionReactionIndex);

        public int GetRequiredOperationsToProduceAmount(long amount)
        {
            int operations = (int)(amount / ProducedAmountPerOperation);
            if (amount % ProducedAmountPerOperation > 0)
                operations++;
            return operations;

            // TODO: Remove
            return (int)Math.Ceiling((double)amount / ProducedAmountPerOperation);
        }
        public long IncreaseAmountByOperations(int operations) => Amount += operations * ProducedAmountPerOperation;
        public ChemicalIngredient AsIngredient() => new ChemicalIngredient(Amount, Chemical);

        public override string ToString() => $"{Amount} {Chemical} | {ProductionReaction}";
    }
}
