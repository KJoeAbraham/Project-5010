using System;

namespace Project_5010.Models
{
    public class PersonalRecord
    {
        public string ExerciseName { get; set; } = "";
        public double Weight { get; set; }
        public int Reps { get; set; } = 1;               // Added Reps
        public DateTime Date { get; set; } = DateTime.Now;
        public string Notes { get; set; } = "";
    }
}