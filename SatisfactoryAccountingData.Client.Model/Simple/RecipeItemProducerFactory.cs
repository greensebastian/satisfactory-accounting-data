using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Model.Simple
{
    public class RecipeItemProducerFactory
    {
        public static RecipeItemProducer FromSingleOutput(string name, double amount)
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

            var producer = new RecipeItemProducer
            {
                Recipe = recipe
            };
            producer.AddSource(new UnlimitedItemSource());

            return producer;
        }

        public static RecipeItemProducer FromSingleOutputRecipe(string productName, double productAmount, params (string Name, double Amount)[] ingredients)
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

            var producer = new RecipeItemProducer
            {
                Recipe = recipe
            };

            return producer;
        }
    }
}
