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
            filePath = Path.Combine(dataFolder, "exercises.json");
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
            // Small starter set — you can expand later.
            return new List<Exercise>
            {
                new Exercise { Name="Bench Press", Category="Push", Muscle="Chest", Equipment="Barbell" },
                new Exercise { Name="Overhead Press", Category="Push", Muscle="Shoulders", Equipment="Barbell" },
                new Exercise { Name="Tricep Pushdown", Category="Push", Muscle="Triceps", Equipment="Cable" },

                new Exercise { Name="Pull-Up", Category="Pull", Muscle="Back", Equipment="Bodyweight" },
                new Exercise { Name="Barbell Row", Category="Pull", Muscle="Back", Equipment="Barbell" },
                new Exercise { Name="Bicep Curl", Category="Pull", Muscle="Biceps", Equipment="Dumbbell" },

                new Exercise { Name="Squat", Category="Legs", Muscle="Quads", Equipment="Barbell" },
                new Exercise { Name="RDL", Category="Legs", Muscle="Hamstrings", Equipment="Barbell" },
                new Exercise { Name="Calf Raise", Category="Legs", Muscle="Calves", Equipment="Machine" },

                new Exercise { Name="Running", Category="Cardio", Muscle="—", Equipment="—" },
                new Exercise { Name="Cycling", Category="Cardio", Muscle="—", Equipment="—" },
            };
        }
    }
}