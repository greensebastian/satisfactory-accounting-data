using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Model
{
    public class RecipeBuilder
    {
        private Recipe _recipe = new()
        { 
            Ingredients = new List<ItemRate>(),
            Product = new List<ItemRate>(),
            ManufactoringDuration = 60
        };

        private List<Action> _actions = new ();

        public RecipeBuilder WithIngredient(string name, double amount)
        {
            _actions.Add(() => _recipe.Ingredients.Add(new ItemRate
            {
                ClassName = name,
                Amount = amount
            }));
            return this;
        }

        public RecipeBuilder WithProduct(string name, double amount)
        {
            _actions.Add(() => _recipe.Product.Add(new ItemRate
            {
                ClassName = name,
                Amount = amount
            }));
            return this;
        }

        public RecipeBuilder WithManufacturingDuration(double duration)
        {
            _actions.Add(() => _recipe.ManufactoringDuration = duration);
            return this;
        }

        public Recipe Build()
        {
            foreach (var action in _actions)
            {
                action();
            }
            return _recipe;
        }
    }
}
