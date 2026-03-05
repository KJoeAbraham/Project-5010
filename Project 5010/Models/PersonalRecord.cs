using System;

namespace Project_5010.Models
{
    public class PersonalRecord
    {
        public string ExerciseName { get; set; } = "";
        public double Weight { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Notes { get; set; } = "";
    }
}