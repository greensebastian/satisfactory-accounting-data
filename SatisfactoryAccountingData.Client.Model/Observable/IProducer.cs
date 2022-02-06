using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Model.Observable
{
    public interface IProducer
    {
        IItemRateList DesiredProducts { get; set; }
        IObservable<IItemRateList> Products { get; }
        IItemRateList CurrentProducts { get; }
        IItemRateList Ingredients { get; }
        IObservable<IItemRateList> ProductEfficiencies { get; }
        IItemRateList CurrentProductEfficiencies { get; }
        IObservable<IItemRateList> Efficiency { get; }
        IReadOnlySet<IProducer> Sources { get; set; }
    }

    public interface IRecipeProducer : IProducer
    {
        Recipe Recipe { get; set; }
    }
}