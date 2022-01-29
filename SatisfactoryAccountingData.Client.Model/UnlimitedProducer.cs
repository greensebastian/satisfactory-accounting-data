namespace SatisfactoryAccountingData.Client.Model
{
    public class UnlimitedProducer : BaseProducer
    {
        protected override void UpdateSourceConsumption()
        {
        }

        protected override IItemRateList ComputeProducts()
        {
            return DesiredProducts;
        }
    }
}
