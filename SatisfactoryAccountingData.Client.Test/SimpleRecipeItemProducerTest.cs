using System.Linq;
using SatisfactoryAccountingData.Client.Model.Simple;
using SatisfactoryAccountingData.Shared.Model;
using Shouldly;
using Xunit;

namespace SatisfactoryAccountingData.Client.Test
{
    public class SimpleRecipeItemProducerTest
    {
        [Fact]
        public void RecipeItemProducer_OneInputOneOutput_ShouldHaveProducts()
        {
            var recipe = new Recipe
            {
                Ingredients = new ItemRateListBuilder()
                    .WithItem("Apple", 1)
                    .BuildList(),
                Product = new ItemRateListBuilder()
                    .WithItem("Banana", 2)
                    .BuildList(),
                ManufactoringDuration = 60
            };

            var producer = new RecipeItemProducer
            {
                Recipe = recipe
            };

            var request = new ItemRateListBuilder()
                .WithItem("Banana", 4)
                .Build();

            producer.AddRequest(request);

            producer.Products.ShouldBeEmpty();

            producer.AddSource(new UnlimitedItemSource());

            producer.Products[request].Single().ClassName.ShouldBe("Banana");
            producer.Products[request].Single().Amount.ShouldBe(2);
            producer.LeftoverProducts.ShouldBeEmpty();
        }

        [Fact]
        public void RecipeItemProducer_ChainOfTwo_ShouldHaveProducts()
        {

            var bananaRecipe = new Recipe
            {
                Ingredients = new ItemRateListBuilder()
                    .WithItem("Apple", 1)
                    .BuildList(),
                Product = new ItemRateListBuilder()
                    .WithItem("Banana", 2)
                    .BuildList(),
                ManufactoringDuration = 60
            };

            var bananaProducer = new RecipeItemProducer
            {
                Recipe = bananaRecipe
            };

            bananaProducer.AddSource(new UnlimitedItemSource());
            
            var clementineRecipe = new Recipe
            {
                Ingredients = new ItemRateListBuilder().WithItem("Banana", 2).BuildList(),
                Product = new ItemRateListBuilder().WithItem("Clementine", 4).BuildList(),
                ManufactoringDuration = 60
            };

            var clementineProducer = new RecipeItemProducer
            {
                Recipe = clementineRecipe
            };

            clementineProducer.AddSource(bananaProducer);

            clementineProducer.Products.ShouldBeEmpty();

            var request = new ItemRateListBuilder()
                .WithItem("Clementine", 500)
                .Build();

            clementineProducer.AddRequest(request);

            clementineProducer.Products[request].Single().ClassName.ShouldBe("Clementine");
            clementineProducer.Products[request].Single().Amount.ShouldBe(4);
        }

        [Fact]
        public void RecipeItemProducer_OneInputOneOutput_ShouldBeRequestLimited()
        {
            var recipe = new Recipe
            {
                Ingredients = new ItemRateListBuilder()
                    .WithItem("Apple", 1)
                    .BuildList(),
                Product = new ItemRateListBuilder()
                    .WithItem("Banana", 2)
                    .BuildList(),
                ManufactoringDuration = 60
            };

            var producer = new RecipeItemProducer
            {
                Recipe = recipe
            };

            var request = new ItemRateListBuilder()
                .WithItem("Banana", 1)
                .Build();

            producer.AddRequest(request);

            producer.Products.ShouldBeEmpty();

            producer.AddSource(new UnlimitedItemSource());

            producer.Products[request].Single().ClassName.ShouldBe("Banana");
            producer.Products[request].Single().Amount.ShouldBe(1);
            producer.LeftoverProducts.ShouldBeEmpty();
        }

        [Fact]
        public void RecipeItemProducer_TwoInputsOneOutput_ShouldOptimize()
        {
            var producer = RecipeItemProducerFactory.FromSingleOutputRecipe("Output", 10, ("A", 10), ("B", 20));
            var aProducer = RecipeItemProducerFactory.FromSingleOutput("A", 10);
            var bProducer = RecipeItemProducerFactory.FromSingleOutput("B", 10);
            producer.AddSource(aProducer);
            producer.AddSource(bProducer);

            var request = new ItemRateListBuilder()
                .WithItem("Output", 10)
                .Build();

            producer.AddRequest(request);

            producer.Products[request].Single().ClassName.ShouldBe("Output");
            producer.Products[request].Single().Amount.ShouldBe(5);

            aProducer.Products.Values.Single().Single().ClassName.ShouldBe("A");
            aProducer.Products.Values.Single().Single().Amount.ShouldBe(5);

            bProducer.Products.Values.Single().Single().ClassName.ShouldBe("B");
            bProducer.Products.Values.Single().Single().Amount.ShouldBe(10);
        }

        [Fact]
        public void RecipeItemProducer_OneInputOneOutputClockSpeed_ShouldWork()
        {
            var producer = RecipeItemProducerFactory.FromSingleOutputRecipe("Banana", 10, ("Apple", 10));
            producer.AddSource(new UnlimitedItemSource());

            var request = new ItemRateListBuilder()
                .WithItem("Banana", 20)
                .Build();

            producer.AddRequest(request);

            producer.Products[request].Single().ClassName.ShouldBe("Banana");
            producer.Products[request].Single().Amount.ShouldBe(10);
            producer.LeftoverProducts.ShouldBeEmpty();

            producer.ClockSpeed = 1.5;

            producer.Products[request].Single().ClassName.ShouldBe("Banana");
            producer.Products[request].Single().Amount.ShouldBe(15);
            producer.LeftoverProducts.ShouldBeEmpty();

            producer.ClockSpeed = 0.5;

            producer.Products[request].Single().ClassName.ShouldBe("Banana");
            producer.Products[request].Single().Amount.ShouldBe(5);
            producer.LeftoverProducts.ShouldBeEmpty();

            producer.ClockSpeed = 2.5;

            producer.Products[request].Single().ClassName.ShouldBe("Banana");
            producer.Products[request].Single().Amount.ShouldBe(20);
            producer.LeftoverProducts.ShouldBeEmpty();
        }
    }
}
