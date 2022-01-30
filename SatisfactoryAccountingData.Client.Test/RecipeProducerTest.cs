using System.Collections.Generic;
using SatisfactoryAccountingData.Client.Model;
using SatisfactoryAccountingData.Shared.Model;
using Shouldly;
using Xunit;

namespace SatisfactoryAccountingData.Client.Test
{
    public class RecipeProducerTest
    {
        [Fact]
        public void RecipeProducer_OneInputOneOutput_ShouldHaveProducts()
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

            var producer = new RecipeProducer(recipe);

            producer.CurrentProducts.ShouldBeEmpty();

            producer.Sources = new HashSet<IProducer>
            {
                new UnlimitedProducer()
            };

            producer.CurrentProducts.ShouldBeEmpty();

            producer.DesiredProducts = new ItemRateListBuilder()
                .WithItem("Banana", 500)
                .Build();

            producer.CurrentProducts.ShouldContain(product => product.ClassName == "Banana" && product.Amount == 2);
        }

        [Fact]
        public void RecipeProducer_ChainOfTwo_ShouldHaveProducts()
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

            var bananaProducer = new RecipeProducer(bananaRecipe);

            bananaProducer.CurrentProducts.ShouldBeEmpty();

            bananaProducer.Sources = new HashSet<IProducer>
            {
                new UnlimitedProducer()
            };

            bananaProducer.CurrentProducts.ShouldBeEmpty();

            bananaProducer.DesiredProducts = new ItemRateListBuilder()
                .WithItem("Banana", 500)
                .Build();

            bananaProducer.CurrentProducts.ShouldContain(product => product.ClassName == "Banana" && product.Amount == 2);

            var clementineRecipe = new Recipe
            {
                Ingredients = new ItemRateListBuilder().WithItem("Banana", 2).BuildList(),
                Product = new ItemRateListBuilder().WithItem("Clementine", 4).BuildList(),
                ManufactoringDuration = 60
            };

            var clementineProducer = new RecipeProducer(clementineRecipe);

            clementineProducer.Sources = new HashSet<IProducer>
            {
                bananaProducer
            };

            clementineProducer.CurrentProducts.ShouldBeEmpty();

            clementineProducer.DesiredProducts = new ItemRateListBuilder()
                .WithItem("Clementine", 500)
                .Build();

            clementineProducer.CurrentProducts.ShouldContain(product => product.ClassName == "Clementine" && product.Amount == 4);
        }

        [Fact]
        public void RecipeProducer_OneInputOneOutput_ShouldBeOutputLimited()
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

            var producer = new RecipeProducer(recipe);

            producer.CurrentProducts.ShouldBeEmpty();

            producer.Sources = new HashSet<IProducer>
            {
                new UnlimitedProducer()
            };

            producer.CurrentProducts.ShouldBeEmpty();

            producer.DesiredProducts = new ItemRateListBuilder()
                .WithItem("Banana", 500)
                .Build();

            producer.CurrentProducts.ShouldContain(product => product.ClassName == "Banana" && product.Amount == 2);

            producer.DesiredProducts = new ItemRateListBuilder()
                .WithItem("Banana", 1)
                .Build();
            
            producer.CurrentProducts.ShouldContain(product => product.ClassName == "Banana" && product.Amount == 1);
            producer.CurrentProducts.ShouldContain(product => product.ClassName == "Banana" && product.Amount == 100);
        }

        [Fact]
        public void RecipeProducer_TwoInputsOneOutput_ShouldOptimize()
        {
            var producer = RecipeProducerFactory.FromSingleOutputRecipe("Output", 10, ("A", 10), ("B", 20));
            var aProducer = RecipeProducerFactory.FromSingleOutput("A", 10);
            var bProducer = RecipeProducerFactory.FromSingleOutput("B", 10);
            producer.Sources = new HashSet<IProducer>
            {
                aProducer,
                bProducer
            };

            producer.DesiredProducts = new ItemRateListBuilder()
                .WithItem("Output", 10)
                .Build();

            producer.CurrentProducts.ShouldContain(product => product.ClassName == "Output" && product.Amount == 5);
            producer.CurrentProductEfficiencies.ShouldContain(product => product.ClassName == "Output" && product.Amount == 0.5);

            aProducer.CurrentProducts.ShouldContain(product => product.ClassName == "A" && product.Amount == 5);
            aProducer.CurrentProductEfficiencies.ShouldContain(product => product.ClassName == "A" && product.Amount == 0.5);

            bProducer.CurrentProducts.ShouldContain(product => product.ClassName == "B" && product.Amount == 10);
            bProducer.CurrentProductEfficiencies.ShouldContain(product => product.ClassName == "B" && product.Amount == 1);
        }

        [Fact]
        public void RecipeProducer_OneImpossibleOutput_ShouldNotFail()
        {
            var producer = RecipeProducerFactory.FromSingleOutput("A", 10);

            producer.DesiredProducts = new ItemRateListBuilder()
                .WithItem("B", 500)
                .Build();

            producer.CurrentProducts.ShouldBeEmpty();
        }

        [Fact]
        public void RecipeProducer_OneInputSourceChanges_ShouldUpdate()
        {
            var producer = RecipeProducerFactory.FromSingleOutput("A", 10);

            producer.DesiredProducts = new ItemRateListBuilder()
                .WithItem("A", 500)
                .Build();

            producer.CurrentProducts.ShouldContain(product => product.ClassName == "A" && product.Amount == 10);

            producer.Sources = new HashSet<IProducer>
            {
                RecipeProducerFactory.FromSingleOutput("A", 5)
            };

            producer.CurrentProducts.ShouldContain(product => product.ClassName == "A" && product.Amount == 5);
        }

        [Fact]
        public void RecipeProducer_OneInputHigherClock_ShouldWork()
        {
            var producer = RecipeProducerFactory.FromSingleOutput("A", 10);

            producer.DesiredProducts = new ItemRateListBuilder()
                .WithItem("A", 500)
                .Build();

            producer.CurrentProducts.ShouldContain(product => product.ClassName == "A" && product.Amount == 10);

            producer.Sources = new HashSet<IProducer>
            {
                RecipeProducerFactory.FromSingleOutput("A", 5)
            };

            producer.CurrentProducts.ShouldContain(product => product.ClassName == "A" && product.Amount == 5);
        }
    }
}