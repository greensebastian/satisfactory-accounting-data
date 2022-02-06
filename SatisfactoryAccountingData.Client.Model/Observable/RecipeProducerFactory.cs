using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Model.Observable
{
    public class RecipeProducerFactory
    {
        public static RecipeProducer FromSingleOutput(string name, double amount)
        {
            var recipe = new Recipe
            {
                ManufactoringDuration = 60,
                Ingredients = new ItemRateList
                {
                    new() { Amount = amount, ClassName = name }
                },
                Product = new ItemRateList
                {
                    new() { Amount = amount, ClassName = name }
                }
            };

            var producer = new RecipeProducer(recipe)
            {
                Sources = new HashSet<IProducer>(new IProducer[] { new UnlimitedProducer() })
            };

            return producer;
        }

        public static RecipeProducer FromSingleOutputRecipe(string productName, double productAmount, params (string Name, double Amount)[] ingredients)
        {
            var recipe = new Recipe
            {
                ManufactoringDuration = 60,
                Ingredients = new ItemRateList(ingredients.Select(ingredient => new ItemRate { Amount = ingredient.Amount, ClassName = ingredient.Name })),
                Product = new ItemRateList
                {
                    new() { Amount = productAmount, ClassName = productName }
                }
            };

            var producer = new RecipeProducer(recipe);

            return producer;
        }
    }
}
