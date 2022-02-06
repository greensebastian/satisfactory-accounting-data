namespace SatisfactoryAccountingData.Client.Model.Simple
{
    public interface ISimpleItemSource
    {
        Guid Id { get; }

        IDictionary<IItemRateList, IItemRateList> Products { get; }

        IItemRateList LeftoverProducts { get; }

        IItemRateList ConsumedIngredients { get; }

        IReadOnlyList<ISimpleItemSource> Sources { get; }

        IItemRateList AddRequest(IItemRateList product);

        void RemoveRequest(IItemRateList product);

        void AddSource(ISimpleItemSource source);

        void RemoveSource(ISimpleItemSource source);
    }
}
