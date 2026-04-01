// Exercise.cs
// Represents one exercise in the library (e.g. "Barbell Bench Press").
// Includes the category (Push/Pull/Legs/Cardio), equipment, and muscles worked.

namespace Project_5010.Models
{
    public class Exercise
    {
        // Standard Info
        public string Name { get; set; } = "";
        public string Category { get; set; } = "Other"; 
        public string Equipment { get; set; } = "";
        
        // Hevy UI Requirements
        public string ExerciseType { get; set; } = "Weight & Reps"; 
        public string Muscle { get; set; } = "";             // Primary Muscle
        public string SecondaryMuscles { get; set; } = "";   // Comma-separated list

        // Legacy 
        public string SubMuscle { get; set; } = "";
        public string SplitPlan { get; set; } = "";
        public string Difficulty { get; set; } = "";
    }
}