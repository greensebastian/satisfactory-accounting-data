using System;
using System.Collections.Generic;
using System.Linq;
using SatisfactoryAccountingData.Client.Model;
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

            var producer = new RecipeItemProducer(Guid.NewGuid())
            {
                Recipe = recipe
            };

            var request = new ItemRateListBuilder()
                .WithItem("Banana", 4)
                .Build();

            producer.AddRequest(request);

            producer.Products.ShouldBeEmpty();

            producer.AddSource(new UnlimitedItemSource(Guid.NewGuid()));

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

            var bananaProducer = new RecipeItemProducer(Guid.NewGuid())
            {
                Recipe = bananaRecipe
            };

            bananaProducer.AddSource(new UnlimitedItemSource(Guid.NewGuid()));
            
            var clementineRecipe = new Recipe
            {
                Ingredients = new ItemRateListBuilder().WithItem("Banana", 2).BuildList(),
                Product = new ItemRateListBuilder().WithItem("Clementine", 4).BuildList(),
                ManufactoringDuration = 60
            };

            var clementineProducer = new RecipeItemProducer(Guid.NewGuid())
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

            var producer = new RecipeItemProducer(Guid.NewGuid())
            {
                Recipe = recipe
            };

            var request = new ItemRateListBuilder()
                .WithItem("Banana", 1)
                .Build();

            producer.AddRequest(request);

            producer.Products.ShouldBeEmpty();

            producer.AddSource(new UnlimitedItemSource(Guid.NewGuid()));

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
            producer.AddSource(new UnlimitedItemSource(Guid.NewGuid()));

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

        [Fact]
        public void RecipeItemProducer_TwoConsumersOneSource_ShouldSplit()
        {
            var producer = RecipeItemProducerFactory.FromSingleOutput("A", 10);
            var c1 = RecipeItemProducerFactory.FromSingleOutputRecipe("B", 10, ("A", 10));
            var c2 = RecipeItemProducerFactory.FromSingleOutputRecipe("B", 10, ("A", 10));

            var r1 = new ItemRateListBuilder()
                .WithItem("B", 10)
                .Build();
            var r2 = new ItemRateListBuilder()
                .WithItem("B", 10)
                .Build();

            c1.AddSource(producer);
            c2.AddSource(producer);

            c1.AddRequest(r1);
            c2.AddRequest(r2);

            c1.Products.Values.Single().Single().ClassName.ShouldBe("B");
            c1.Products.Values.Single().Single().Amount.ShouldBe(10);

            c2.Products.ShouldBeEmpty();
        }

        [Fact]
        public void RecipeItemProducer_BypassSplit_ShouldSplit()
        {
            var producer = RecipeItemProducerFactory.FromSingleOutput("A", 10);
            var middleman = RecipeItemProducerFactory.FromSingleOutputRecipe("B", 10, ("A", 8));
            var consumer = RecipeItemProducerFactory.FromSingleOutputRecipe("C", 10, ("A", 5), ("B", 10));

            var expectedProductionRatio = (10 - 8) / 5d;

            var request = new ItemRateListBuilder()
                .WithItem("C", 10)
                .Build();

            middleman.AddSource(producer);

            consumer.AddSource(middleman);
            consumer.AddSource(producer);

            consumer.AddRequest(request);

            middleman.Products.Values.Single().Single().ClassName.ShouldBe("B");
            middleman.Products.Values.Single().Single().Amount.ShouldBe(expectedProductionRatio * 10);

            consumer.Products.Values.Single().Single().ClassName.ShouldBe("C");
            consumer.Products.Values.Single().Single().Amount.ShouldBe(expectedProductionRatio * 10);
        }

        [Fact]
        public void RecipeItemProducer_FromFactoryPlan_ShouldWork()
        {
            var unlimitedSourceId = Guid.NewGuid();
            var copperProducerId = Guid.NewGuid();
            var wireProducerId = Guid.NewGuid();

            var plan = new FactoryPlan
            {
                Id = Guid.Empty,
                Components = new List<FactoryComponent>
                {
                    new()
                    {
                        Id = wireProducerId,
                        RecipeName = "Recipe_Wire_C",
                        Sources = new List<Guid>
                        {
                            copperProducerId
                        },
                        Type = ComponentType.Builder
                    },
                    new()
                    {
                        Id = copperProducerId,
                        RecipeName = "Recipe_CopperIngot_C",
                        Sources = new List<Guid>
                        {
                            unlimitedSourceId
                        },
                        Type = ComponentType.Builder
                    },
                    new()
                    {
                        Id = unlimitedSourceId,
                        Type = ComponentType.Source
                    }
                }
            };

            var wireRecipe = new Recipe
            {
                ClassName = "Recipe_Wire_C",
                Ingredients = new List<ItemRate>
                {
                    new()
                    {
                        ClassName = "Desc_CopperIngot_C",
                        Amount = 1
                    }
                },
                Product = new List<ItemRate>
                {
                    new()
                    {
                        ClassName = "Desc_Wire_C",
                        Amount = 2
                    }
                },
                ManufactoringDuration = 4
            };

            var copperIngotRecipe = new Recipe
            {
                ClassName = "Recipe_CopperIngot_C",
                Ingredients = new List<ItemRate>
                {
                    new()
                    {
                        ClassName = "Desc_OreCopper_C",
                        Amount = 1
                    }
                },
                Product = new List<ItemRate>
                {
                    new()
                    {
                        ClassName = "Desc_CopperIngot_C",
                        Amount = 1
                    }
                },
                ManufactoringDuration = 2
            };

            var recipes = new List<Recipe>
            {
                copperIngotRecipe,
                wireRecipe
            };

            var components = RecipeItemProducerFactory.FromFactoryPlan(plan, recipes);

            var wireProducer = components.Single(c => c.Id == wireProducerId);

            var request = new ItemRateListBuilder()
                .WithItem("Desc_Wire_C", 60)
                .Build();

            wireProducer.AddRequest(request);

            wireProducer.Products.Values.Single().Single().ClassName.ShouldBe("Desc_Wire_C");
            wireProducer.Products.Values.Single().Single().Amount.ShouldBe(30);
        }
    }
}
