using System;
using System.Collections.Generic;

namespace Project_5010.Models
{
    public class UserProfile
    {
        public string UserId { get; set; } = "";
        public string DisplayName { get; set; } = "Athlete";
        public double HeightCm { get; set; } = 170;
        public double WeightKg { get; set; } = 70;
        public string Gender { get; set; } = "Not specified";
        // e.g. "Beginner", "Intermediate", "Advanced"
        public string FitnessLevel { get; set; } = "Intermediate";
        // "Metric" or "Imperial"
        public string PreferredUnits { get; set; } = "Metric";
        public double WeeklyGoalMinutes { get; set; } = 150;
        public int WeeklyGoalWorkouts { get; set; } = 4;
        // e.g. "Build strength", "Lose weight", "Build endurance"
        public string PrimaryGoal { get; set; } = "Build strength";
        public List<Goal> Goals { get; set; } = new();
        public DateTime? BirthDate { get; set; }
        public string Bio { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public double Bmi
        {
            get
            {
                double heightM = HeightCm / 100.0;
                return heightM > 0 ? WeightKg / (heightM * heightM) : 0;
            }
        }

        public int Age
        {
            get
            {
                if (!BirthDate.HasValue) return 0;
                int age = DateTime.Today.Year - BirthDate.Value.Year;
                if (BirthDate.Value.Date > DateTime.Today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}