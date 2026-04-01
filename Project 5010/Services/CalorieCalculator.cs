// CalorieCalculator.cs
// This class calculates how many calories the user needs per day
// using the Mifflin-St Jeor formula based on their body stats.
// It also adjusts the result depending on the user's fitness goal.

using System;

namespace Project_5010.Services
{
    public static class CalorieCalculator
    {
        // Calculates Basal Metabolic Rate (BMR) — the calories your body
        // burns just to stay alive (breathing, heartbeat, etc.)
        // Formula: Mifflin-St Jeor equation
        public static double CalculateBmr(double weightKg, double heightCm, int age, string sex)
        {
            if (sex == "Female")
                return 10 * weightKg + 6.25 * heightCm - 5 * age - 161;
            return 10 * weightKg + 6.25 * heightCm - 5 * age + 5;
        }

        // Returns a multiplier based on how active the user is.
        // A sedentary person burns fewer calories than someone who exercises daily.
        public static double GetActivityFactor(string activityLevel) => activityLevel switch
        {
            "Sedentary"    => 1.2,
            "Light"        => 1.375,
            "Active"       => 1.55,
            "Very Active"  => 1.725,
            _              => 1.465   // Moderate (default)
        };

        // Returns the full breakdown: BMR, Maintenance, and Goal calories.
        // This lets the UI show all three values to the user so they
        // understand where the number comes from.
        public static (int Bmr, int Maintenance, int Goal) CalculateBreakdown(
            double weightKg, double heightCm, int age, string sex,
            string activityLevel, string goalType)
        {
            if (age <= 0 || weightKg <= 0 || heightCm <= 0)
                return (0, 2000, 2000);

            double bmr = CalculateBmr(weightKg, heightCm, age, sex);
            double maintenance = bmr * GetActivityFactor(activityLevel);

            // Adjust based on goal:
            //   Lose Weight     → eat less than maintenance (-400 kcal)
            //   Maintain        → eat at maintenance
            //   Gain Weight     → eat more than maintenance (+300 kcal)
            //   Improve Fitness → small surplus for performance (+150 kcal)
            double goal = goalType switch
            {
                "Lose Weight"      => maintenance - 400,
                "Gain Weight"      => maintenance + 300,
                "Improve Fitness"  => maintenance + 150,
                _                  => maintenance
            };

            // Never go below 1200 kcal — that's the safe minimum
            goal = Math.Max(1200, goal);

            return (
                (int)Math.Round(bmr),
                (int)Math.Round(maintenance),
                (int)Math.Round(goal)
            );
        }

        // Simple version that just returns the daily goal number.
        // Used by the dashboard and other places that don't need the full breakdown.
        public static int CalculateDailyGoal(double weightKg, double heightCm, int age, string sex, string activityLevel, string goalType)
        {
            var (_, _, goal) = CalculateBreakdown(weightKg, heightCm, age, sex, activityLevel, goalType);
            return goal;
        }
    }
}
