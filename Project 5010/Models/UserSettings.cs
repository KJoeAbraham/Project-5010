// UserSettings.cs
// Stores the user's profile info, fitness preferences, and app settings.
// This data is saved as JSON and loaded every time the app starts.

namespace Project_5010.Models
{
    public class UserSettings
    {
        // Basic profile info
        public string DisplayName { get; set; } = "Athlete";
        public double HeightCm { get; set; } = 170;
        public double WeightKg { get; set; } = 70;
        public int Age { get; set; } = 25;
        public string Sex { get; set; } = "Male";

        // Fitness preferences
        public string ActivityLevel { get; set; } = "Moderate";
        public string GoalType { get; set; } = "Maintain";
        public int WorkoutsPerWeek { get; set; } = 3;
        public bool HasWorkedOutBefore { get; set; } = false;

        // Calorie tracking
        public int DailyCalorieTarget { get; set; } = 2000;

        // App state flags
        public bool IsOnboardingComplete { get; set; } = false;
        public string ThemePreference { get; set; } = "Light";

        // Training split selection
        public string SplitPlanId { get; set; } = "PPL";

        public string SplitPlan
        {
            get => SplitPlanId;
            set => SplitPlanId = string.IsNullOrWhiteSpace(value) ? "PPL" : value;
        }

        public string SplitId
        {
            get => SplitPlanId;
            set => SplitPlanId = string.IsNullOrWhiteSpace(value) ? "PPL" : value;
        }

        public string PreferredSplit
        {
            get => SplitPlanId;
            set => SplitPlanId = string.IsNullOrWhiteSpace(value) ? "PPL" : value;
        }

        public string TrainingSplit
        {
            get => SplitPlanId;
            set => SplitPlanId = string.IsNullOrWhiteSpace(value) ? "PPL" : value;
        }
    }
}
