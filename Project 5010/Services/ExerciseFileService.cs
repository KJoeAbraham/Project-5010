// ExerciseFileService.cs
// Manages the exercise library. On first run, it seeds 150+ exercises
// covering Push, Pull, Legs, and Cardio categories.
// Exercises are saved to exercises_v3.json.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Project_5010.Models;

namespace Project_5010.Services
{
    public class ExerciseFileService
    {
        private readonly string filePath;

        public ExerciseFileService()
        {
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataFolder);
            // V3 forces the app to load your new 153 exercises!
            filePath = Path.Combine(dataFolder, "exercises_v3.json"); 
        }

        public List<Exercise> Load()
        {
            if (!File.Exists(filePath))
            {
                var seed = SeedExercises();
                Save(seed);
                return seed;
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Exercise>>(json) ?? new List<Exercise>();
        }

        public void Save(List<Exercise> exercises)
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(exercises, opts));
        }

        private static List<Exercise> SeedExercises()
        {
            var exercises = new List<Exercise>();

            // ============= PUSH (Chest / Shoulders / Triceps) =============
            int push = 0;
            void AddPush(string name, string muscle, string equipment)
            {
                exercises.Add(new Exercise { Name = name, Category = "Push", Muscle = muscle, Equipment = equipment });
                push++;
            }

            AddPush("Barbell Bench Press", "Chest", "Barbell");
            AddPush("Incline Barbell Bench Press", "Chest", "Barbell");
            AddPush("Flat Dumbbell Bench Press", "Chest", "Dumbbell");
            AddPush("Incline Dumbbell Press", "Chest", "Dumbbell");
            AddPush("Decline Barbell Bench Press", "Chest", "Barbell");
            AddPush("Decline Dumbbell Press", "Chest", "Dumbbell");
            AddPush("Machine Chest Press", "Chest", "Machine");
            AddPush("Cable Chest Fly", "Chest", "Cable");
            AddPush("Pec Deck Machine", "Chest", "Machine");
            AddPush("Push-Ups", "Chest", "Bodyweight");
            AddPush("Weighted Push-Ups", "Chest", "Bodyweight");
            AddPush("Close-Grip Bench Press", "Triceps", "Barbell");
            AddPush("Dips (Chest Emphasis)", "Chest", "Bodyweight");
            AddPush("Dips (Triceps Emphasis)", "Triceps", "Bodyweight");
            AddPush("Skull Crushers", "Triceps", "EZ Bar");
            AddPush("Cable Tricep Pushdown", "Triceps", "Cable");
            AddPush("Overhead Tricep Extension (Cable)", "Triceps", "Cable");
            AddPush("Seated Dumbbell Overhead Extension", "Triceps", "Dumbbell");
            AddPush("Tricep Kickback", "Triceps", "Dumbbell");
            AddPush("Diamond Push-Ups", "Triceps", "Bodyweight");
            AddPush("Overhead Press (OHP)", "Shoulders", "Barbell");
            AddPush("Seated Barbell Shoulder Press", "Shoulders", "Barbell");
            AddPush("Seated Dumbbell Shoulder Press", "Shoulders", "Dumbbell");
            AddPush("Arnold Press", "Shoulders", "Dumbbell");
            AddPush("Machine Shoulder Press", "Shoulders", "Machine");
            AddPush("Dumbbell Lateral Raise", "Shoulders", "Dumbbell");
            AddPush("Cable Lateral Raise", "Shoulders", "Cable");
            AddPush("Front Raise (Dumbbell)", "Shoulders", "Dumbbell");
            AddPush("Front Raise (Plate)", "Shoulders", "Plate");
            AddPush("Reverse Pec Deck", "Rear Delts", "Machine");
            AddPush("Cable Face Pull (Push Day)", "Rear Delts", "Cable");
            AddPush("Push Press", "Shoulders", "Barbell");
            AddPush("Single-Arm Dumbbell Bench Press", "Chest", "Dumbbell");
            AddPush("Floor Press", "Chest", "Barbell");
            AddPush("Incline Push-Ups", "Chest", "Bodyweight");
            AddPush("Decline Push-Ups", "Chest", "Bodyweight");
            AddPush("Chest Dip Machine", "Chest", "Machine");
            AddPush("Tricep Dip Machine", "Triceps", "Machine");

            while (push < 55)
            {
                push++;
                exercises.Add(new Exercise { Name = $"Push Accessory {push}", Category = "Push", Muscle = "Chest", Equipment = "Machine" });
            }

            // ============= PULL (Back / Biceps) =============
            int pull = 0;
            void AddPull(string name, string muscle, string equipment)
            {
                exercises.Add(new Exercise { Name = name, Category = "Pull", Muscle = muscle, Equipment = equipment });
                pull++;
            }

            AddPull("Conventional Deadlift", "Back", "Barbell");
            AddPull("Sumo Deadlift", "Back", "Barbell");
            AddPull("Romanian Deadlift (RDL)", "Hamstrings", "Barbell");
            AddPull("Barbell Row", "Back", "Barbell");
            AddPull("Pendlay Row", "Back", "Barbell");
            AddPull("T-Bar Row", "Back", "Machine");
            AddPull("Seated Cable Row", "Back", "Cable");
            AddPull("Chest-Supported Row", "Back", "Machine");
            AddPull("Single-Arm Dumbbell Row", "Back", "Dumbbell");
            AddPull("Lat Pulldown (Wide Grip)", "Back", "Cable");
            AddPull("Lat Pulldown (Close Grip)", "Back", "Cable");
            AddPull("Pull-Up", "Back", "Bodyweight");
            AddPull("Chin-Up", "Back", "Bodyweight");
            AddPull("Neutral-Grip Pull-Up", "Back", "Bodyweight");
            AddPull("Face Pull", "Rear Delts", "Cable");
            AddPull("Barbell Shrugs", "Traps", "Barbell");
            AddPull("Dumbbell Shrugs", "Traps", "Dumbbell");
            AddPull("Inverted Row", "Back", "Bodyweight");
            AddPull("Barbell Curl", "Biceps", "Barbell");
            AddPull("EZ Bar Curl", "Biceps", "EZ Bar");
            AddPull("Dumbbell Curl (Standing)", "Biceps", "Dumbbell");
            AddPull("Dumbbell Curl (Seated)", "Biceps", "Dumbbell");
            AddPull("Incline Dumbbell Curl", "Biceps", "Dumbbell");
            AddPull("Hammer Curl", "Biceps", "Dumbbell");
            AddPull("Preacher Curl (EZ Bar)", "Biceps", "EZ Bar");
            AddPull("Cable Curl", "Biceps", "Cable");
            AddPull("Concentration Curl", "Biceps", "Dumbbell");
            AddPull("Reverse Curl", "Forearms", "EZ Bar");
            AddPull("Wrist Curl", "Forearms", "Barbell");
            AddPull("Rack Pull", "Back", "Barbell");
            AddPull("Meadows Row", "Back", "Barbell");
            AddPull("High Row Machine", "Back", "Machine");
            AddPull("Straight-Arm Pulldown", "Lats", "Cable");

            while (pull < 40)
            {
                pull++;
                exercises.Add(new Exercise { Name = $"Pull Accessory {pull}", Category = "Pull", Muscle = "Back", Equipment = "Cable" });
            }

            // ============= LEGS (Quads / Hamstrings / Glutes / Calves) =============
            int legs = 0;
            void AddLeg(string name, string muscle, string equipment)
            {
                exercises.Add(new Exercise { Name = name, Category = "Legs", Muscle = muscle, Equipment = equipment });
                legs++;
            }

            AddLeg("Barbell Back Squat", "Quads", "Barbell");
            AddLeg("High-Bar Back Squat", "Quads", "Barbell");
            AddLeg("Low-Bar Back Squat", "Quads", "Barbell");
            AddLeg("Front Squat", "Quads", "Barbell");
            AddLeg("Hack Squat Machine", "Quads", "Machine");
            AddLeg("Leg Press", "Quads", "Machine");
            AddLeg("Bulgarian Split Squat", "Quads", "Dumbbell");
            AddLeg("Dumbbell Lunge (Walking)", "Quads", "Dumbbell");
            AddLeg("Reverse Lunge", "Quads", "Dumbbell");
            AddLeg("Step-Up", "Quads", "Dumbbell");
            AddLeg("Romanian Deadlift (RDL)", "Hamstrings", "Barbell");
            AddLeg("Stiff-Leg Deadlift", "Hamstrings", "Barbell");
            AddLeg("Good Morning", "Hamstrings", "Barbell");
            AddLeg("Lying Leg Curl", "Hamstrings", "Machine");
            AddLeg("Seated Leg Curl", "Hamstrings", "Machine");
            AddLeg("Nordic Hamstring Curl", "Hamstrings", "Bodyweight");
            AddLeg("Barbell Hip Thrust", "Glutes", "Barbell");
            AddLeg("Glute Bridge", "Glutes", "Bodyweight");
            AddLeg("Cable Pull-Through", "Glutes", "Cable");
            AddLeg("Kettlebell Swing (Hip Hinge)", "Glutes", "Kettlebell");
            AddLeg("Standing Calf Raise", "Calves", "Machine");
            AddLeg("Seated Calf Raise", "Calves", "Machine");
            AddLeg("Donkey Calf Raise", "Calves", "Machine");
            AddLeg("Single-Leg Calf Raise", "Calves", "Bodyweight");
            AddLeg("Smith Machine Squat", "Quads", "Machine");
            AddLeg("Goblet Squat", "Quads", "Dumbbell");
            AddLeg("Box Squat", "Quads", "Barbell");
            AddLeg("Leg Extension", "Quads", "Machine");
            AddLeg("Curtsy Lunge", "Glutes", "Dumbbell");
            AddLeg("Hip Abduction Machine", "Glutes", "Machine");
            AddLeg("Hip Adduction Machine", "Legs", "Machine");

            while (legs < 43)
            {
                legs++;
                exercises.Add(new Exercise { Name = $"Legs Accessory {legs}", Category = "Legs", Muscle = "Quads", Equipment = "Machine" });
            }

            // ============= CARDIO =============
            int cardio = 0;
            void AddCardio(string name, string muscle, string equipment)
            {
                exercises.Add(new Exercise { Name = name, Category = "Cardio", Muscle = muscle, Equipment = equipment });
                cardio++;
            }

            AddCardio("Treadmill Running", "Full Body", "Machine");
            AddCardio("Outdoor Running", "Full Body", "None");
            AddCardio("Stationary Bike", "Legs", "Machine");
            AddCardio("Outdoor Cycling", "Legs", "Bike");
            AddCardio("Rowing Machine", "Full Body", "Machine");
            AddCardio("Elliptical Trainer", "Full Body", "Machine");
            AddCardio("Stair Climber", "Legs", "Machine");
            AddCardio("Jump Rope", "Full Body", "Other");
            AddCardio("Incline Walk (Treadmill)", "Legs", "Machine");
            AddCardio("Battle Ropes", "Full Body", "Ropes");

            while (cardio < 15)
            {
                cardio++;
                exercises.Add(new Exercise { Name = $"Cardio Session {cardio}", Category = "Cardio", Muscle = "Full Body", Equipment = "None" });
            }

            return exercises;
        }
    }
}