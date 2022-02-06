namespace SatisfactoryAccountingData.Client.Model.Simple
{
    public class UnlimitedItemSource : BaseItemSource
    {
        public UnlimitedItemSource(Guid id) : base(id)
        {
        }

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
