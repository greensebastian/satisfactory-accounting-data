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
        }

        private class ItemRateListBuilder
        {
            private readonly ItemRateList _list = new();

            public ItemRateListBuilder WithItem(string name, double amount)
            {
                _list.Add(new ItemRate{ClassName = name, Amount = amount});
                return this;
            }

            public IItemRateList Build() => _list.DeepCopy();
            public List<ItemRate> BuildList() => new (Build());
        }
    }
}