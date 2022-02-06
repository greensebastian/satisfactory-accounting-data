using System.Collections.Generic;
using SatisfactoryAccountingData.Client.Model;
using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Test;

public class ItemRateListBuilder
{
    private readonly ItemRateList _list = new();

    public ItemRateListBuilder WithItem(string name, double amount)
    {
        _list.Add(new ItemRate{ClassName = name, Amount = amount});
        return this;
    }

    public IItemRateList Build() => _list.DeepCopy();
    public List<ItemRate> BuildList() => new (Build());
}