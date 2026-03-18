using System;

namespace Project_5010.Models
{
    public class Workout
    {
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public int DurationMinutes { get; set; }
        public int Calories { get; set; }
        public string Notes { get; set; } = "";

        public string Icon
        {
            get
            {
                return Type switch
                {
                    "Cardio" => "🏃",
                    "Strength" => "🏋",
                    "HIIT" => "🔥",
                    "Flexibility" => "🧘",
                    _ => "💪"
                };
            }
        }

        public string DisplayDateTime => Date.ToString("yyyy-MM-dd HH:mm");
        public string DurationDisplay => $"{DurationMinutes} min";
        public string CaloriesDisplay => Calories > 0 ? $"{Calories} kcal" : "";
    }
}
