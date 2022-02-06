using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Model.Simple
{
    public class RecipeItemProducer : BaseItemSource
    {
        public RecipeItemProducer(Guid id) : base(id)
        {
        }

        public Recipe? Recipe
        {
            get => _recipe;
            set
            {
                _recipe = value;
                RecomputeProducts();
            }
        }

        public double ClockSpeed
        {
            get => _clockSpeed;
            set
            {
                _clockSpeed = value;
                RecomputeProducts();
            }
        }

        private readonly List<(ISimpleItemSource source, IItemRateList request)> _activeRequests = new();
        private double _clockSpeed = 1d;
        private Recipe? _recipe;

        protected override void ComputeResult()
        {
            if (Recipe == null) return;

            var totalRequestedProducts = new ItemRateList().AsDictionary();
            foreach (var request in Requests.SelectMany(req => req.DeepCopy()))
            {
                totalRequestedProducts.CreateOrAdd(request.ClassName, request.Amount);
            }

            var totalDesiredIngredients = ComputeDesiredIngredients(new ItemRateList(totalRequestedProducts));
            var availableIngredients = RequestIngredients(totalDesiredIngredients);
            var availableProductionRatio = ComputeProductionRatio(availableIngredients);
            var availableProducts = ComputeProduction(availableProductionRatio);

            // Update requests after learning limits of sources
            var optimizedDesiredIngredients = ComputeDesiredIngredients(availableProducts);
            var usedIngredients = RequestIngredients(optimizedDesiredIngredients);
            ProtectedConsumedIngredients.AddRange(new ItemRateList(usedIngredients));
            var productionRatio = ComputeProductionRatio(usedIngredients);
            var products = ComputeProduction(productionRatio).AsDictionary();

            foreach (var request in Requests)
            {
                foreach (var missingItem in MissingFromRequest(request))
                {
                    var provided = products.Subtract(missingItem.ClassName, missingItem.Amount);
                    if (provided > 0)
                    {
                        AddToProduct(request,
                            new ItemRateList { new() { ClassName = missingItem.ClassName, Amount = provided } });
                    }
                }
            }

            foreach (var (className, amount) in products)
            {
                if (amount > 0) LeftoverItems.Add(new ItemRate { ClassName = className, Amount = amount });
            }
        }

        private IDictionary<string, double> RequestIngredients(IDictionary<string, double> totalDesiredIngredients)
        {
            var receivedIngredients = new ItemRateList().AsDictionary();

            // Clear previous requests
            foreach (var (source, request) in _activeRequests)
            {
                source.RemoveRequest(request);
            }

            foreach (var source in ProtectedSources)
            {
                if (totalDesiredIngredients.Values.All(amount => amount == 0)) continue;

                var ingredientRequest = new ItemRateList(totalDesiredIngredients);
                var producedBySource = source.AddRequest(ingredientRequest);
                _activeRequests.Add((source, ingredientRequest));

                // Subtract any actually produced ingredients from the desired list
                foreach (var ingredient in producedBySource)
                {
                    if (!totalDesiredIngredients.ContainsKey(ingredient.ClassName)) continue;
                    var desiredProductAmount = totalDesiredIngredients[ingredient.ClassName];

                    var consumed = Math.Min(desiredProductAmount, ingredient.Amount);
                    consumed = Math.Max(consumed, 0);
                    totalDesiredIngredients[ingredient.ClassName] -= consumed;

                    // Add realized ingredient to output
                    receivedIngredients.CreateOrAdd(ingredient.ClassName, ingredient.Amount);
                }
            }

            return receivedIngredients;
        }

        private ItemRateList ComputeProduction(double productionRatio)
        {
            if (Recipe == null) return new ItemRateList();

            var optimizedDesiredProducts =
                new ItemRateList(Recipe.ProductPerMinute.Select(product =>
                    new ItemRate { ClassName = product.ClassName, Amount = product.Amount * productionRatio }));
            return optimizedDesiredProducts;
        }

        private double ComputeProductionRatio(IDictionary<string, double> receivedIngredients)
        {
            if (Recipe == null) return 0;
            if (!receivedIngredients.Any()) return 0;

            var lowestRatioToRequired = new ItemRateList(receivedIngredients)
                .Min(receivedIngredient =>
                {
                    var desired = Recipe.IngredientsPerMinute.FirstOrDefault(ingredient =>
                        ingredient.ClassName == receivedIngredient.ClassName);

                    if (desired == null || desired.Amount == 0) return ClockSpeed;

                    return Math.Min(receivedIngredient.Amount / desired.Amount, ClockSpeed);
                });
            return lowestRatioToRequired;
        }

        private IDictionary<string, double> ComputeDesiredIngredients(IItemRateList requestedProducts)
        {
            if (Recipe == null) return new ItemRateList().AsDictionary();

            // Current recipe must be able to make the product
            // Use recipe as truth to limit production rate
            var possibleProducts = Recipe.ProductPerMinute.Where(product =>
                requestedProducts.Any(requestedProduct => requestedProduct.ClassName == product.ClassName));

            // Compile complete list of desired ingredients
            var totalDesiredIngredients = new ItemRateList().AsDictionary();
            var realizedProducts = new ItemRateList().AsDictionary();
            foreach (var recipeProduct in possibleProducts)
            {
                // Compute needed amount, while taking into account it may already have been added
                var requestedAmountByConsumer =
                    requestedProducts.First(product => product.ClassName == recipeProduct.ClassName).Amount;
                var desiredProductAmount = Math.Min(recipeProduct.Amount * ClockSpeed, requestedAmountByConsumer);

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
                    var desiredIngredientAmount = desiredIngredient.Amount * productionRatio;

                    totalDesiredIngredients.CreateOrAdd(desiredIngredient.ClassName, desiredIngredientAmount);
                }
            }

            return totalDesiredIngredients;
        }
    }
}
