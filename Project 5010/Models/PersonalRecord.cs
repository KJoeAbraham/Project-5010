// PersonalRecord.cs
// Tracks the user's best performance for a specific exercise.
// For example: "Bench Press — 80 kg x 5 reps on March 15".

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