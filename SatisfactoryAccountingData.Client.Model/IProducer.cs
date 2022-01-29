using System.Collections.Immutable;
using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Model
{
    public interface IProducer
    {
        IItemRateSet DesiredProducts { get; set; }
        IObservable<IItemRateSet> Products { get; }
        IObservable<IItemRateSet> ProductionRatios { get; }
        IObservable<IItemRateSet> Efficiency { get; }
        IReadOnlySet<IProducer> Sources { get; set; }
    }

    public interface IItemRateSet : IReadOnlySet<ItemRate> { }

    public class ItemRateSet : HashSet<ItemRate>, IItemRateSet { }

    public interface IRecipeProducer : IProducer
    {
        Recipe Recipe { get; set; }
    }
}