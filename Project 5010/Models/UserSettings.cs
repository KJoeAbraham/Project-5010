namespace Project_5010.Models
{
    public class UserSettings
    {
        public string DisplayName { get; set; } = "User";
        public int Age { get; set; } = 18;

        public double HeightCm { get; set; } = 170;
        public double WeightKg { get; set; } = 70;

        // Examples: "PPL", "UpperLower", "FullBody", "Bro"
        public string SplitPlan { get; set; } = "PPL";
    }
}