namespace Project_5010.Models
{
    public class UserSettings
    {
        public string DisplayName { get; set; } = "Athlete";
        public double HeightCm { get; set; } = 170;
        public double WeightKg { get; set; } = 70;
        public int Age { get; set; } = 25;
        public string Sex { get; set; } = "Male";
        public string ActivityLevel { get; set; } = "Moderate";
        public string GoalType { get; set; } = "Maintain";

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
