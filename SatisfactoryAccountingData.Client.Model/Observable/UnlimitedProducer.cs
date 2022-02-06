using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Model.Observable
{
    public class UnlimitedProducer : BaseProducer
    {
        protected override void UpdateSourceConsumption()
        {
            Ingredients = new ItemRateList();
        }

        protected override IItemRateList ComputeProducts()
        {
            return DesiredProducts;
        }

        protected override IItemRateList ComputeProductEfficiencies()
        {
            var ratios = new ItemRateList();
            foreach (var desiredProduct in DesiredProducts)
            {
                var product =
                    CurrentProducts.FirstOrDefault(product => product.ClassName == desiredProduct.ClassName);

                if (product == null)
                {
                    ratios.Add(new ItemRate { Amount = 0, ClassName = desiredProduct.ClassName });
                }
                else
                {
                    ratios.Add(new ItemRate
                        { Amount = product.Amount / desiredProduct.Amount, ClassName = desiredProduct.ClassName });
                }
            }

            return ratios;
        }
    }
}
