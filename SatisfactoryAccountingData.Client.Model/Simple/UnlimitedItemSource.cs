namespace SatisfactoryAccountingData.Client.Model.Simple
{
    public class UnlimitedItemSource : BaseItemSource
    {
        protected override void ComputeResult()
        {
            foreach (var request in Requests)
            {
                var result = request.DeepCopy();
                AddToProduct(request, result);
            }
        }
    }
}
