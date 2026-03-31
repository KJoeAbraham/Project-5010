using System;

namespace Project_5010.Services
{
    public static class CalorieCalculator
    {
        public static double CalculateBmr(double weightKg, double heightCm, int age, string sex)
        {
            if (sex == "Female")
                return 10 * weightKg + 6.25 * heightCm - 5 * age - 161;
            return 10 * weightKg + 6.25 * heightCm - 5 * age + 5;
        }

        public static double GetActivityFactor(string activityLevel) => activityLevel switch
        {
            "Sedentary"   => 1.2,
            "Light"       => 1.375,
            "Active"      => 1.55,
            "Very Active" => 1.725,
            _             => 1.465
        };

        public static int CalculateDailyGoal(double weightKg, double heightCm, int age, string sex, string activityLevel, string goalType)
        {
            double bmr = CalculateBmr(weightKg, heightCm, age, sex);
            double maintenance = bmr * GetActivityFactor(activityLevel);
            double goal = goalType switch
            {
                "Lose Weight" => maintenance - 400,
                "Gain Weight" => maintenance + 300,
                _             => maintenance
            };
            return (int)Math.Round(Math.Max(1200, goal));
        }
    }
}
