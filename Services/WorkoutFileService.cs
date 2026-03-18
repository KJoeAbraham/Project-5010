using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Project_5010.Models;

namespace Project_5010.Services
{
    public class WorkoutFileService
    {
        private readonly string filePath;

        public WorkoutFileService()
        {
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataFolder);
            filePath = Path.Combine(dataFolder, "workouts.json");
        }

        public List<Workout> LoadWorkouts()
        {
            if (!File.Exists(filePath))
            {
                return new List<Workout>();
            }

            string json = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Workout>();
            }

            return JsonSerializer.Deserialize<List<Workout>>(json) ?? new List<Workout>();
        }

        public void SaveWorkouts(List<Workout> workouts)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(workouts, options);
            File.WriteAllText(filePath, json);
        }
    }
}