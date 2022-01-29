using System.Linq;
using SatisfactoryAccountingData.Client.Model;
using SatisfactoryAccountingData.Client.Model.Observables;
using SatisfactoryAccountingData.Shared.Model;
using Shouldly;
using Xunit;

namespace SatisfactoryAccountingData.Client.Test
{
    public class ConstantProducerTest
    {
        [Fact]
        public void ConstantProducer_OneItem_Emits()
        {
            var producer = new ConstantProducer();

            var updateCount = 0;

            producer.Products.Subscribe(products =>
            {
                updateCount++;
            });

            updateCount.ShouldBe(1);

            producer.DesiredProducts = new ItemRateSet
            {
                new() {Amount = 1, ClassName = "Banana"}
            };

            updateCount.ShouldBe(2);

            producer.CurrentProducts.Single().ClassName.ShouldBe("Banana");

            updateCount.ShouldBe(2);
        }
    }
}