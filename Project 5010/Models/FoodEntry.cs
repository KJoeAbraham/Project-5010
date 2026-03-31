using System;

namespace Project_5010.Models
{
    public class FoodEntry
    {
        public string Name { get; set; } = "";
        public int Calories { get; set; }
        public double Quantity { get; set; } = 1;
        public string Unit { get; set; } = "serving";
        public DateTime Date { get; set; } = DateTime.Today;
        public string MealType { get; set; } = "Breakfast";
    }
}
