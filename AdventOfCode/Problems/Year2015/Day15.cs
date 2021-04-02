using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2015
{
    public class Day15 : Problem<int>
    {
        private IEnumerable<Ingredient> ingredients;

        public override int SolvePart1()
        {
            return new Recipe(ingredients).GetHighestScore();
        }
        public override int SolvePart2()
        {
            return new CaloriedRecipe(ingredients).GetHighestScore();
        }

        protected override void ResetState()
        {
            ingredients = null;
        }
        protected override void LoadState()
        {
            ingredients = FileLines.Select(Ingredient.Parse).ToArray();
        }

        private class CaloriedRecipe : Recipe
        {
            public CaloriedRecipe(IEnumerable<Ingredient> ingredientCollection)
                : base(ingredientCollection) { }

            protected override bool ValidateCalories(int calories)
            {
                return calories == 500;
            }
        }
        private class Recipe
        {
            private readonly Ingredient[] ingredients;

            public Recipe(IEnumerable<Ingredient> ingredientCollection)
            {
                ingredients = ingredientCollection.ToArray();
            }

            public int GetHighestScore()
            {
                int highest = int.MinValue;

                var teaspoons = new int[ingredients.Length];

                for (int i = 0; i <= 100; i++)
                    Iterate(0, 100 - i);

                return highest;

                void Iterate(int ingredientIndex, int remainingTeaspoons)
                {
                    if (ingredientIndex == ingredients.Length - 1)
                    {
                        teaspoons[ingredientIndex] = remainingTeaspoons;

                        int capacity = 0;
                        int durability = 0;
                        int flavor = 0;
                        int texture = 0;
                        int calories = 0;

                        for (int i = 0; i < ingredients.Length; i++)
                        {
                            capacity += ingredients[i].Capacity * teaspoons[i];
                            durability += ingredients[i].Durability * teaspoons[i];
                            flavor += ingredients[i].Flavor * teaspoons[i];
                            texture += ingredients[i].Texture * teaspoons[i];
                            calories += ingredients[i].Calories * teaspoons[i];
                        }

                        if (capacity <= 0 || durability <= 0 || flavor <= 0 || texture <= 0)
                            return;

                        if (!ValidateCalories(calories))
                            return;

                        int total = capacity * durability * flavor * texture;
                        if (total > highest)
                            highest = total;

                        return;
                    }

                    for (int i = 0; i <= remainingTeaspoons; i++)
                    {
                        teaspoons[ingredientIndex] = i;
                        Iterate(ingredientIndex + 1, remainingTeaspoons - i);
                    }
                }
            }

            protected virtual bool ValidateCalories(int calories) => true;
        }

        private record Ingredient(string Name, int Capacity, int Durability, int Flavor, int Texture, int Calories)
        {
            private static readonly Regex ingredientPattern = new(@"(?'name'\w*): capacity (?'capacity'[-\d]*), durability (?'durability'[-\d]*), flavor (?'flavor'[-\d]*), texture (?'texture'[-\d]*), calories (?'calories'[-\d]*)", RegexOptions.Compiled);

            public static Ingredient Parse(string s)
            {
                var groups = ingredientPattern.Match(s).Groups;
                var name = groups["name"].Value;
                var capacity = groups["capacity"].Value.ParseInt32();
                int durability = groups["durability"].Value.ParseInt32();
                int flavor = groups["flavor"].Value.ParseInt32();
                int texture = groups["texture"].Value.ParseInt32();
                int calories = groups["calories"].Value.ParseInt32();
                return new(name, capacity, durability, flavor, texture, calories);
            }
        }
    }
}
