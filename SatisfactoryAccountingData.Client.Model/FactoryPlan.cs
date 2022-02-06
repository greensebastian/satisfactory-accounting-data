namespace SatisfactoryAccountingData.Client.Model
{
    public class FactoryPlan
    {
        public Guid Id = Guid.NewGuid();
        public List<FactoryComponent> Components { get; set; } = new();
        public List<Guid> ParentPlan { get; set; } = new();
        public List<FactoryComponent> GetRootComponents()
        {
            var rootComponents = new List<FactoryComponent>(Components);

            foreach (var factoryComponent in Components)
            {
                foreach (var sourceId in factoryComponent.Sources)
                {
                    rootComponents.RemoveAll(component => component.Id == sourceId);
                }
            }

            return rootComponents;
        }
    }

    public class FactoryComponent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ComponentType Type { get; set; }
        public List<Guid> Sources { get; set; } = new();
        public string RecipeName { get; set; } = string.Empty;
        public double ClockSpeed { get; set; } = 1d;
    }

    public enum ComponentType
    {
        Source,
        Builder
    }
}
