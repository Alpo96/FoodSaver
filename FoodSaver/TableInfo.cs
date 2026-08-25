using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodSaver
{
    public class FoodShopRelationInfo
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int FoodValue { get; set; }
        public string FoodValueCoin => FoodValue + " Coin";
        public int Quality { get; set; }
        public string QualityPercent => Quality + "%";
    }

    public class FoodInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ShopInfo
    {
        public int Id { get; set; }
        public string Location { get; set; }
    }

    public class ImageInfo
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
    }

    public class ValueInfo
    {
        public int Id { get; set; }
        public int FoodValue { get; set; }
        public string FoodValueCoin => FoodValue + " Coin";
    }

    public class QualityInfo
    {
        public int Id { get; set; }
        public int Quality { get; set; }
        public string QualityPercent => Quality + "%";
    }

    public class HistoryInfo
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int FoodValue { get; set; }
        public string FoodValueCoin => FoodValue + " Coin";
        public int Quality { get; set; }
        public string QualityPercent => Quality + "%";
        public string Username { get; set; }
        public string Refund { get; set; }
        public DateTime Time { get; set; }
        public string TimeForm => Time.ToString("yyyy.MM.dd HH:mm:ss");
    }
}
