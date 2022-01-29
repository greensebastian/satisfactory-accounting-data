using System.Collections;
using System.Collections.Immutable;
using SatisfactoryAccountingData.Shared.Model;
using System.Linq;

namespace SatisfactoryAccountingData.Client.Model
{
    public interface IProducer
    {
        IItemRateList DesiredProducts { get; set; }
        IObservable<IItemRateList> Products { get; }
        IItemRateList CurrentProducts { get; }
        IObservable<IItemRateList> ProductionRatios { get; }
        IItemRateList CurrentProductionRatios { get; }
        IObservable<IItemRateList> Efficiency { get; }
        IReadOnlySet<IProducer> Sources { get; set; }
    }

    public interface IItemRateList : IReadOnlyList<ItemRate>
    {
        IItemRateList DeepCopy();
        IDictionary<string, double> AsDictionary();
    }

    public class ItemRateList : List<ItemRate>, IItemRateList
    {
        public ItemRateList() {}
        public ItemRateList(IEnumerable<ItemRate> items) : base(items) {}
        public ItemRateList(IDictionary<string, double> items) : base(items.Select(item =>
            new ItemRate { Amount = item.Value, ClassName = item.Key })) {}

        public IItemRateList DeepCopy()
        {
            return new ItemRateList(this.Select(item => new ItemRate
                { Amount = item.Amount, ClassName = item.ClassName }));
        }

        public IDictionary<string, double> AsDictionary() => this.ToDictionary(item => item.ClassName, item => item.Amount);
    }

    public interface IRecipeProducer : IProducer
    {
        Recipe Recipe { get; set; }
    }
}