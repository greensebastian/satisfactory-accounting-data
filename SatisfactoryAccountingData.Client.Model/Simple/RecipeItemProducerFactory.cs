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

            var producer = new RecipeItemProducer(Guid.NewGuid())
            {
                Recipe = recipe
            };
            producer.AddSource(new UnlimitedItemSource(Guid.NewGuid()));

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

            var producer = new RecipeItemProducer(Guid.NewGuid())
            {
                Recipe = recipe
            };

            return producer;
        }

        public static IList<ISimpleItemSource> FromFactoryPlan(FactoryPlan plan, List<Recipe> recipes, bool requestRoots = false)
        {
            var builderComponentBuilder = new RecursiveFromFactoryPlanBuilder(plan, recipes, requestRoots);

            return builderComponentBuilder.BuildAndGetRoots();
        }

        public class RecursiveFromFactoryPlanBuilder
        {
            private readonly List<ISimpleItemSource> _realizedComponents = new();
            private readonly FactoryPlan _plan;
            private readonly List<Recipe> _recipes;
            private readonly bool _requestRoots;

            public RecursiveFromFactoryPlanBuilder(FactoryPlan plan, List<Recipe> recipes, bool requestRoots)
            {
                _plan = plan;
                _recipes = recipes;
                _requestRoots = requestRoots;
            }

            public List<ISimpleItemSource> BuildAndGetRoots()
            {
                var rootComponents = _plan.GetRootComponents();

                foreach (var root in rootComponents)
                {
                    AddBuilderComponent(root, true);
                }

                return _realizedComponents
                    .Where(component => 
                        rootComponents
                            .Select(c => c.Id)
                            .Contains(component.Id))
                    .ToList();
            }

            private ISimpleItemSource AddBuilderComponent(FactoryComponent component, bool isRoot)
            {
                var existingComponent = _realizedComponents.SingleOrDefault(c => c.Id == component.Id);
                if (existingComponent != null) return existingComponent;

                if (component.Type == ComponentType.Source)
                {
                    var sourceComponent = new UnlimitedItemSource(component.Id);
                    _realizedComponents.Add(sourceComponent);
                    return sourceComponent;
                }

                var recipe = _recipes.Find(recipe => recipe.ClassName == component.RecipeName);
                if (recipe == null)
                    throw new ArgumentException("Recipe does not exist", nameof(FactoryComponent.RecipeName));

                var realizedComponent = new RecipeItemProducer(component.Id)
                {
                    Recipe = recipe,
                    ClockSpeed = component.ClockSpeed
                };
                _realizedComponents.Add(realizedComponent);

                foreach (var sourceId in component.Sources)
                {
                    var childComponent = _plan.Components.Find(c => c.Id == sourceId);
                    if (childComponent == null)
                        throw new ArgumentException($"Source ${sourceId} for item ${component.Id} does not exist.",
                            nameof(FactoryComponent.Sources));
                    realizedComponent.AddSource(AddBuilderComponent(childComponent, false));
                }

                if (isRoot && _requestRoots)
                {
                    var request = new ItemRateList(recipe.ProductPerMinute);
                    realizedComponent.AddRequest(request);
                }

                return realizedComponent;
            }
        }
    }
}
