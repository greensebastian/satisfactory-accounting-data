using System.Linq;
using SatisfactoryAccountingData.Client.Model;
using SatisfactoryAccountingData.Client.Model.Observable;
using SatisfactoryAccountingData.Client.Model.Observables;
using Shouldly;
using Xunit;

namespace SatisfactoryAccountingData.Client.Test
{
    public class ConstantProducerTest
    {
        [Fact]
        public void ConstantProducer_OneItem_Emits()
        {
            var producer = new UnlimitedProducer();

            var updateCount = 0;

            using var subscription = producer.Products.Subscribe(products =>
            {
                updateCount++;
            });

            updateCount.ShouldBe(0);

            producer.DesiredProducts = new ItemRateList
            {
                new() {Amount = 1, ClassName = "Banana"}
            };

            updateCount.ShouldBe(1);

            producer.CurrentProducts.Single().ClassName.ShouldBe("Banana");

            updateCount.ShouldBe(1);

            subscription.Dispose();

            producer.DesiredProducts = new ItemRateList
            {
                new() {Amount = 2, ClassName = "Apple"}
            };

            producer.CurrentProducts.Single().ClassName.ShouldBe("Apple");

            updateCount.ShouldBe(1);
        }

        [Fact]
        public void ConstantProducer_TwoItems_ComputesRatio()
        {
            var producer = new UnlimitedProducer
            {
                DesiredProducts = new ItemRateList
                {
                    new() {Amount = 1, ClassName = "Banana"},
                    new() {Amount = 2, ClassName = "Apple"}
                }
            };

            producer.CurrentProductEfficiencies.First().Amount.ShouldBe(1);
            producer.CurrentProductEfficiencies.Last().Amount.ShouldBe(1);
        }
    }
}