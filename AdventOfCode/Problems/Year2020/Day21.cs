using AdventOfCode.Utilities;
using AdventOfCSharp.Utilities;
using System.Collections;

namespace AdventOfCode.Problems.Year2020;

public partial class Day21 : Problem<int, DelimitedList<string>>
{
    private FoodCollection foodCollection;

    public override int SolvePart1()
    {
        var nonAllergen = foodCollection.NonAllergenIngredients;
        return foodCollection.SelectMany(f => f.Ingredients).Count(nonAllergen.Contains);
    }
    public override DelimitedList<string> SolvePart2()
    {
        return foodCollection.IngredientAllergenCorrelations.Select(kvp => kvp.Value).ToDelimitedList(",");
    }

    protected override void LoadState()
    {
        foodCollection = new FoodCollection(ParsedFileLinesEnumerable(Food.Parse));
    }
    protected override void ResetState()
    {
        foodCollection = null;
    }

    private class IngredientAllergenCorrelationDictionary : FlexibleInitializableValueDictionary<string, NextValueCounterDictionary<string>>
    {
        public IngredientAllergenCorrelationDictionary()
            : base() { }
        public IngredientAllergenCorrelationDictionary(FoodCollection foodCollection)
            : base()
        {
            AddFoods(foodCollection);
        }

        public void AddFoods(FoodCollection foodCollection)
        {
            foreach (var food in foodCollection)
                Add(food);
        }
        public void Add(Food food)
        {
            foreach (var ingredient in food.Ingredients)
                foreach (var allergen in food.Allergens)
                    this[allergen].Add(ingredient);
        }
    }

    private class FoodCollection : IEnumerable<Food>
    {
        private readonly HashSet<string> associatedIngredients = new();
        private readonly SortedDictionary<string, string> ingredientAllergenCorrelations = new();

        public List<Food> Foods { get; init; }
        public int Count => Foods.Count;

        public SortedDictionary<string, string> IngredientAllergenCorrelations => ingredientAllergenCorrelations;

        public HashSet<string> AllIngredients => new(Foods.SelectMany(s => s.Ingredients));
        public HashSet<string> NonAllergenIngredients
        {
            get
            {
                var all = AllIngredients;
                all.ExceptWith(associatedIngredients);
                return all;
            }
        }

        public FoodCollection(IEnumerable<Food> foods)
        {
            Foods = foods.ToList();
            GetCorrelations();
        }

        private void GetCorrelations()
        {
            var correlations = new IngredientAllergenCorrelationDictionary(this);

            var allergenList = correlations.Keys.ToList();
            while (allergenList.Count > 0)
            {
                for (int i = 1; i <= allergenList.Count; i++)
                {
                    var allergen = allergenList[^i];

                    // Remove already associated ingredients
                    foreach (var associatedIngredient in associatedIngredients)
                        correlations[allergen][associatedIngredient] = 0;

                    var mostOccurrentIngredient = correlations[allergen].Max().Key;
                    if (mostOccurrentIngredient == null)
                        continue;

                    ingredientAllergenCorrelations.Add(allergen, mostOccurrentIngredient);
                    associatedIngredients.Add(mostOccurrentIngredient);
                    allergenList.RemoveAt(^i);
                }
            }
        }

        public IEnumerator<Food> GetEnumerator() => Foods.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    private partial record Food(IEnumerable<string> Ingredients, IEnumerable<string> Allergens)
    {
        private readonly static Regex foodPattern = FoodRegex();

        public static Food Parse(string raw)
        {
            var groups = foodPattern.Match(raw).Groups;
            var ingredients = groups["ingredients"].Value.Split(' ');
            var allergens = groups["allergens"].Value.Split(", ");
            return new(ingredients, allergens);
        }

        [GeneratedRegex("(?'ingredients'[\\w ]*) \\(contains (?'allergens'.*)\\)", RegexOptions.Compiled)]
        private static partial Regex FoodRegex();
    }
}
