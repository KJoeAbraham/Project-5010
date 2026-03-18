using System;

namespace Project_5010.Models
{
    public class Goal
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        // Allowed: "Minutes", "Weight", "Workouts"
        public string TargetType { get; set; } = "Minutes";
        public double TargetValue { get; set; } = 0;
        public double CurrentValue { get; set; } = 0;
        public string Unit { get; set; } = "min";
        public DateTime? StartDate { get; set; }
        public DateTime? TargetDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}