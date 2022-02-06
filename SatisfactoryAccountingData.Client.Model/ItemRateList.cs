using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Model;

public class ItemRateList : List<ItemRate>, IItemRateList
{
    public ItemRateList() {}
    public ItemRateList(IEnumerable<ItemRate> items) : base(items) {}
    public ItemRateList(IDictionary<string, double> items) : base(items
        .Select(item => new ItemRate { Amount = item.Value, ClassName = item.Key })
        .Aggregate(new List<ItemRate>(), (addedItems, currentItem) =>
        {
            var existing = addedItems.FirstOrDefault(item => item.ClassName == currentItem.ClassName);
            if (existing != null) existing.Amount += currentItem.Amount;
            else addedItems.Add(currentItem);
            return addedItems;
        })) {}

    public IItemRateList DeepCopy()
    {
        return new ItemRateList(this.Select(item => new ItemRate
            { Amount = item.Amount, ClassName = item.ClassName }));
    }

    public IDictionary<string, double> AsDictionary() => this.ToDictionary(item => item.ClassName, item => item.Amount);
}