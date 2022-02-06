namespace SatisfactoryAccountingData.Client.Model.Simple
{
    public interface ISimpleItemSource
    {
        IDictionary<IItemRateList, IItemRateList> Products { get; }

        IItemRateList LeftoverProducts { get; }

        IItemRateList AddRequest(IItemRateList product);

        void RemoveRequest(IItemRateList product);

        void AddSource(ISimpleItemSource source);

        void RemoveSource(ISimpleItemSource source);
    }
}
