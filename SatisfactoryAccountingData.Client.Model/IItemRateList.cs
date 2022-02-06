using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Model;

public interface IItemRateList : IReadOnlyList<ItemRate>
{
    IItemRateList DeepCopy();
    IDictionary<string, double> AsDictionary();
}