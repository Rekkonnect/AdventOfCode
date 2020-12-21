using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using Garyon.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day21 : Problem<int, string>
    {
        private FoodCollection foodCollection;

        public override int SolvePart1()
        {
            // Intentionally left commented out because of the bug occurrence
            //foodCollection.GetCorrelations();
            var nonAllergen = foodCollection.NonAllergenIngredients;

            int count = 0;
            foreach (var food in foodCollection)
                foreach (var ingredient in food.Ingredients)
                    if (nonAllergen.Contains(ingredient))
                        count++;
            
            return count;
        }
        public override string SolvePart2()
        {
            return foodCollection.IngredientAllergenCorrelations.Select(kvp => kvp.Value).Combine(',');
        }

        protected override void LoadState()
        {
            foodCollection = new FoodCollection(FileLines.Select(Food.Parse));
        }
        protected override void ResetState()
        {
            foodCollection = null;
        }

        private class IngredientAllergenCorrelationDictionary : FlexibleDictionary<string, ValueCounterDictionary<string>>
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

            protected override ValueCounterDictionary<string> GetDefaultValue() => new ValueCounterDictionary<string>();
        }

        private class FoodCollection : IEnumerable<Food>
        {
            private HashSet<string> associatedIngredients = new();
            private SortedDictionary<string, string> ingredientAllergenCorrelations = new();

            public List<Food> Foods { get; init; }
            public int Count => Foods.Count;

            public SortedDictionary<string, string> IngredientAllergenCorrelations => ingredientAllergenCorrelations;

            public HashSet<string> AllIngredients => new HashSet<string>(Foods.Select(s => s.Ingredients).Flatten());
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
        private class Food
        {
            private string[] ingredients;
            private string[] allergens;

            public IEnumerable<string> Ingredients => ingredients;
            public IEnumerable<string> Allergens => allergens;

            public Food(string[] foodIngredients, string[] foodAllergens)
            {
                ingredients = foodIngredients;
                allergens = foodAllergens;
            }

            public static Food Parse(string raw)
            {
                var split = raw[..^1].Split(" (contains ");
                var ingredients = split[0].Split(' ');
                var allergens = split[1].Split(", ");
                return new(ingredients, allergens);
            }
        }
    }
}
