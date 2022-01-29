using SatisfactoryAccountingData.Shared.Model;
namespace SatisfactoryAccountingData.Client.Model
{
    public class RecipeProducer : BaseProducer
    {
        private Recipe _recipe;
        private double _clockSpeed;

        public RecipeProducer(Recipe recipe, double clockSpeed = 1d)
        {
            _recipe = recipe;
            _clockSpeed = clockSpeed;
        }

        public double ClockSpeed
        {
            get => _clockSpeed;
            set
            {
                _clockSpeed = value;
                OnInputChanged();
            }
        }

        public Recipe Recipe
        {
            get => _recipe;
            set
            {
                _recipe = value;
                OnInputChanged();
            }
        }

        protected override void UpdateSourceConsumption()
        {
            // Current recipe must be able to make the product
            // Use recipe as truth to limit production rate
            var possibleProducts = Recipe.ProductPerMinute.Where(product =>
                DesiredProducts.Any(desiredProduct => desiredProduct.ClassName == product.ClassName));

            // Compile complete list of desired ingredients
            var totalDesiredIngredients = new ItemRateList().AsDictionary();
            var realizedProducts = new ItemRateList().AsDictionary();
            foreach (var recipeProduct in possibleProducts)
            {
                // Compute needed amount, while taking into account it may already have been added
                var desiredAmountByConsumer =
                    DesiredProducts.First(product => product.ClassName == recipeProduct.ClassName).Amount;
                var desiredProductAmount = Math.Min(recipeProduct.Amount * ClockSpeed, desiredAmountByConsumer);

                if (desiredProductAmount == 0) continue; // Avoid division by zero

                var productionRatio = desiredProductAmount / recipeProduct.Amount;
                if (realizedProducts.ContainsKey(recipeProduct.ClassName))
                {
                    var alreadyProduced = realizedProducts[recipeProduct.ClassName];
                    var toProduce = Math.Max(0, desiredProductAmount - alreadyProduced);
                    productionRatio = Math.Min(toProduce / recipeProduct.Amount, productionRatio);
                    realizedProducts[recipeProduct.ClassName] += toProduce;
                }
                else
                {
                    realizedProducts[recipeProduct.ClassName] = desiredProductAmount;
                }

                // Compute and add all required ingredients for the realize product amount
                // Use recipe as truth to limit production rate
                var desiredIngredients = new ItemRateList(Recipe.IngredientsPerMinute);
                foreach (var desiredIngredient in desiredIngredients)
                {
                    var desiredIngredientAmount = desiredIngredient.Amount * ClockSpeed * productionRatio;

                    if (!totalDesiredIngredients.ContainsKey(desiredIngredient.ClassName))
                    {
                        totalDesiredIngredients[desiredIngredient.ClassName] = 0;
                    }

                    totalDesiredIngredients[desiredIngredient.ClassName] += desiredIngredientAmount;
                }
            }

            // Overflow sources to gather all ingredients
            foreach (var producer in Sources)
            {
                producer.DesiredProducts = new ItemRateList(totalDesiredIngredients);

                // Subtract any actually produced ingredients from the desired list
                foreach (var producedProduct in producer.CurrentProducts)
                {
                    if (!totalDesiredIngredients.ContainsKey(producedProduct.ClassName)) continue;
                    var desiredProductAmount = totalDesiredIngredients[producedProduct.ClassName];

                    var consumed = Math.Min(desiredProductAmount, producedProduct.Amount);
                    consumed = Math.Max(consumed, 0);
                    totalDesiredIngredients[producedProduct.ClassName] -= consumed;
                }

                // Should maybe change desired product on the source here as we now know what it can produce.
                // Changing will produce a re-compute of that entire tree though, so skipping for now.
            }
        }

        protected override IItemRateList ComputeProducts()
        {
            var receivedIngredients = Sources.Aggregate(new ItemRateList().AsDictionary(), (acc, source) =>
            {
                foreach (var producedIngredient in source.CurrentProducts)
                {
                    if (!acc.ContainsKey(producedIngredient.ClassName))
                    {
                        acc[producedIngredient.ClassName] = 0d;
                    }

                    acc[producedIngredient.ClassName] += producedIngredient.Amount;
                }

                return acc;
            });

            if (!receivedIngredients.Any()) return new ItemRateList();

            var lowestRatioToRequired = new ItemRateList(receivedIngredients)
                .Min(receivedIngredient =>
            {
                var desired = Recipe.IngredientsPerMinute.FirstOrDefault(ingredient =>
                    ingredient.ClassName == receivedIngredient.ClassName);

                if (desired == null || desired.Amount == 0) return ClockSpeed;

                return Math.Min(receivedIngredient.Amount / desired.Amount, ClockSpeed);
            });

            return new ItemRateList(Recipe.ProductPerMinute
                .Select(product => 
                    new ItemRate { ClassName = product.ClassName, Amount = product.Amount * lowestRatioToRequired }));
        }
    }
}
