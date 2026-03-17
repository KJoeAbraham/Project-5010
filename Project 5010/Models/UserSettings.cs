namespace Project_5010.Models
{
    public class UserSettings
    {
        public string DisplayName { get; set; } = "Athlete";
        public double HeightCm { get; set; } = 170;
        public double WeightKg { get; set; } = 70;

        // Main stored property
        public string SplitPlanId { get; set; } = "PPL";

        // Compatibility aliases so older code like settings.SplitPlan still works
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