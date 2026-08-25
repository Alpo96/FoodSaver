using System;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace UnitTest
{
    public class FoodSaverUnitTest
    {
        public readonly Entities entities = new Entities(new DbContextOptionsBuilder<Entities>().UseInMemoryDatabase("foods").Options);
        
        [Theory]
        [InlineData("BannanaImage", "Budapest", "Banana", 1100, 90)]
        [InlineData("AppleaImage", "Eger", "Apple", 750, 75)]
        [InlineData("CarrotImage", "Vác", "Carrot", 220, 45)]
        public void Put_Product_InMemoryDatabase_Then_RemoveFromIt(string Image, string Location, string FoodName, int Value, int FoodQuality)
        {
            Foods fooditem = new Foods(Image, Location, FoodName, Value, FoodQuality, Guid.NewGuid());
            fooditem.FoodList.Add(fooditem);
            entities.foods.Add(fooditem);
            entities.SaveChanges();

            Foods FindFood = entities.foods.Find(fooditem.Id);
            entities.foods.Any(food => food.Id == fooditem.Id).Should().BeTrue();

            if (FindFood != null)
            {
               var DeleteFood = entities.foods.Remove(fooditem);
               entities.SaveChanges();
               entities.foods.Should().BeEmpty();
            }
        }
    }
}
