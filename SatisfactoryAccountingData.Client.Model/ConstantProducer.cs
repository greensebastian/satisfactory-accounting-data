namespace SatisfactoryAccountingData.Client.Model
{
    public class ConstantProducer : BaseProducer
    {
        protected override IItemRateSet ComputeProducts()
        {
            return DesiredProducts;
        }
    }
}
