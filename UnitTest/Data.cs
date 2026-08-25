using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace UnitTest
{

    public class Foods
    {
        public List<Foods> FoodList { get; set; } = new List<Foods>();
        public string Image { get; set; }
        public string Location { get; set; }
        public string FoodName { get; set; }
        public int Value { get; set; }
        public int FoodQuality { get; set; }
        public Guid Id { get; set; }

        public Foods(string image, string location, string foodname, int value, int foodquality, Guid id)
        {
            this.Image = image;
            this.Location = location;
            this.FoodName = foodname;
            this.Value = value;
            this.FoodQuality = foodquality;
            this.Id = id;
        }
        public Foods()
        {

        }
    }
    public class Entities : DbContext
    {
        public DbSet<Foods> foods { get; set; }
        public Entities(DbContextOptions options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Foods>().HasKey(f => f.Id);
            base.OnModelCreating(modelBuilder);
        }
    }
}
