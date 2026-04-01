// FoodEntry.cs
// Represents one food item the user logged (e.g. "Chicken Breast, 300 kcal, Lunch").
// These are saved to food.json and used to track daily calorie intake.

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

        // Macronutrients (grams)
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }
    }
}
